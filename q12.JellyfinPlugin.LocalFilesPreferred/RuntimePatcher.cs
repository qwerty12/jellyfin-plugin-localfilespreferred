using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Emby.Server.Implementations.Library;
using HarmonyLib;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;

namespace q12.JellyfinPlugin.LocalFilesPreferred;

internal sealed class RuntimePatcher : IDisposable
{
    private Harmony? _harmony;
    private static readonly string? _harmonyId = typeof(RuntimePatcher).Namespace;

    public RuntimePatcher()
    {
        _harmony = new Harmony(_harmonyId);
        _harmony.PatchAll();
    }

    ~RuntimePatcher() => Release();

    private void Release()
    {
        if (_harmony is null)
        {
            return;
        }

        _harmony.UnpatchAll(_harmonyId);
        _harmony = null;
    }

    public void Dispose()
    {
        Release();
        GC.SuppressFinalize(this);
    }
}

[HarmonyPatch(typeof(CollectionFolder), "LoadLibraryOptions")]
public static class Patch_CollectionFolder_LoadLibraryOptions
{
    internal static readonly Dictionary<string, string> PathNetworkMap = [];
    internal static readonly MethodInfo GetLibraryOptionsPath = AccessTools.Method(typeof(CollectionFolder), "GetLibraryOptionsPath");

    public static void Postfix(LibraryOptions __result, string path)
    {
        if (__result.PathInfos.Length == 0)
        {
            return;
        }

        var inPathInfos = false;
        string? xmlPath = null;
        string? xmlNetworkPath = null;
        using var reader = XmlReader.Create((string)GetLibraryOptionsPath.Invoke(null, [path])!, new XmlReaderSettings { CheckCharacters = false, CloseInput = true, DtdProcessing = DtdProcessing.Ignore });
        while (reader.Read())
        {
            if (inPathInfos)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "Path":
                                reader.Read();
                                xmlPath = reader.Value;
                                break;
                            case "NetworkPath":
                                reader.Read();
                                xmlNetworkPath = reader.Value;
                                break;
                        }

                        break;
                    case XmlNodeType.EndElement:
                        switch (reader.Name)
                        {
                            case "MediaPathInfo":
                            {
                                if (xmlPath != null && xmlNetworkPath != null)
                                {
                                    PathNetworkMap.TryAdd(xmlPath, xmlNetworkPath);
                                }

                                xmlPath = null;
                                xmlNetworkPath = null;
                                break;
                            }

                            case "PathInfos":
                                return;
                        }

                        break;
                }
            }
            else if (reader is { NodeType: XmlNodeType.Element, Name: "PathInfos" })
            {
                inPathInfos = true;
            }
        }
    }
}

[HarmonyPatch(typeof(CollectionFolder), nameof(CollectionFolder.SaveLibraryOptions))]
public static class Patch_CollectionFolder_SaveLibraryOptions
{
    public static void Postfix(string path)
    {
        if (Patch_CollectionFolder_LoadLibraryOptions.PathNetworkMap.Count == 0)
        {
            return;
        }

        var libraryOptionsPath = (string)Patch_CollectionFolder_LoadLibraryOptions.GetLibraryOptionsPath.Invoke(null, [path])!;
        var xmlDoc = XDocument.Load(libraryOptionsPath);

        var pathInfosElement = xmlDoc.Descendants("PathInfos").FirstOrDefault();
        if (pathInfosElement == null)
        {
            return;
        }

        var changesMade = false;
        foreach (var mediaPathInfo in pathInfosElement.Elements("MediaPathInfo"))
        {
            var pathElement = mediaPathInfo.Element("Path");
            if (pathElement != null && Patch_CollectionFolder_LoadLibraryOptions.PathNetworkMap.TryGetValue(pathElement.Value, out var networkPath))
            {
                var networkPathElement = mediaPathInfo.Element("NetworkPath");
                if (networkPathElement == null || string.IsNullOrEmpty(networkPathElement.Value))
                {
                    if (networkPathElement == null)
                    {
                        networkPathElement = new XElement("NetworkPath");
                        mediaPathInfo.Add(networkPathElement);
                    }

                    networkPathElement.Value = networkPath;
                    changesMade = true;
                }
            }
        }

        if (changesMade)
        {
            using var stream = new FileStream(libraryOptionsPath, FileMode.Create, FileAccess.Write);
            using var writer = new StreamWriter(stream, bufferSize: IODefaults.StreamWriterBufferSize);
            using var textWriter = new XmlTextWriter(writer);
            textWriter.Formatting = Formatting.Indented;
            xmlDoc.Save(textWriter);
        }
    }
}

[HarmonyPatch(typeof(LibraryManager), nameof(LibraryManager.GetPathAfterNetworkSubstitution))]
public static class Patch_LibraryManager_GetPathAfterNetworkSubstitution
{
    /*private static readonly AccessTools.FieldRef<LibraryManager, IServerConfigurationManager> _configurationManagerRef =
        AccessTools.FieldRefAccess<LibraryManager, IServerConfigurationManager>("_configurationManager");*/

    public static bool Prefix(ref string __result, LibraryManager __instance, string path, BaseItem? ownerItem)
    {
        if (!LocalFilesPreferredPlugin.SendRealPath)
        {
            string? newPath;
            if (ownerItem is not null)
            {
                var libraryOptions = __instance.GetLibraryOptions(ownerItem);
                if (libraryOptions is not null)
                {
                    foreach (var pathInfo in libraryOptions.PathInfos)
                    {
                        if (!Patch_CollectionFolder_LoadLibraryOptions.PathNetworkMap.TryGetValue(pathInfo.Path, out var networkPath))
                        {
                            continue;
                        }

                        if (!path.TryReplaceSubPath(pathInfo.Path, networkPath, out newPath))
                        {
                            continue;
                        }

                        __result = newPath;
                        return false;
                    }
                }
            }

            // CBA to read system.xml for summat I don't use
            /*var _configurationManager = _configurationManagerRef(__instance);
            var metadataPath = _configurationManager.Configuration.MetadataPath;
            var metadataNetworkPath = _configurationManager.Configuration.MetadataNetworkPath;

            if (path.TryReplaceSubPath(metadataPath, metadataNetworkPath, out newPath))
            {
                __result = newPath;
                return false;
            }*/
        }

        return true;
    }
}
