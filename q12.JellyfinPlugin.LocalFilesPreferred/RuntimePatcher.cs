using System;
using Emby.Server.Implementations.Library;
using HarmonyLib;

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

[HarmonyPatch(typeof(LibraryManager), nameof(LibraryManager.GetPathAfterNetworkSubstitution))]
internal static class Patch_LibraryManager_GetPathAfterNetworkSubstitution
{
    private static bool Prefix(ref string __result, string path)
    {
        if (!LocalFilesPreferredPlugin.SendRealPath)
        {
            return true;
        }

        __result = path;
        return false;
    }
}
