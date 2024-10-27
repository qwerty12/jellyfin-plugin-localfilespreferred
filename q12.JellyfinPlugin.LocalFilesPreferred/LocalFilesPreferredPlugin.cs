using System;
using System.Collections.Generic;
using System.Globalization;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using q12.JellyfinPlugin.LocalFilesPreferred.Configuration;

namespace q12.JellyfinPlugin.LocalFilesPreferred;

public sealed class LocalFilesPreferredPlugin : BasePlugin<PluginConfiguration>, IHasWebPages, IDisposable
{
    private readonly RuntimePatcher _patcher;
    public static bool SendRealPath = true;

    public LocalFilesPreferredPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
        _patcher = new RuntimePatcher();
    }

    public void Dispose()
    {
        _patcher.Dispose();
    }

    public override string Name => "Local Files Preferred";

    public override Guid Id => Guid.Parse("2ad61cac-3e9d-48ba-bf00-e0d99dfc1165");

    public static LocalFilesPreferredPlugin? Instance { get; private set; }

    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace),
            }
        ];
    }
}
