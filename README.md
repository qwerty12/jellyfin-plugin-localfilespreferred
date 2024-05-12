# Local Files Preferred plugin for Jellyfin

A race condition-prone plugin to have Jellyfin return the return the real paths of items (i.e. "C:\...\1.mp4") when Kodi is not connected, instead of the set shared network folder path ("\\MyComputer\MyShare\1.mp4") when you've configured your libraries to include them.

This plugin was made for personal use with consideration shown only to my particular setup. In my case, I want the Kodi plugin to be able to play files via SMB, but want the Jellyfin MPV Shim, which I run on the same PC as Jellyfin, to play files directly instead of streaming through Jellyfin. You might also benefit from this plugin if you run Jellyfin on the same PC, have all your media on the same PC, primarily watch stuff on the same PC and have Kodi with the Jellyfin plugin only installed on one other device. As such:

* This plugin causes all `PathSubstitutions` in system.xml to be ignored (which was empty for me anyway)

* *Any* Kodi-running device disconnecting will cause local paths to be returned by JF's API, even if other Kodi devices are still connected. As I only use Kodi on one device, implementing tracking isn't worth my time

This uses [Harmony](https://github.com/pardeike/Harmony) to patch the internal method that returns paths at runtime; this is the quickest way and requires no constant changes to your libraries' configuration.

## Setting a shared network folder in Jellyfin 10.9

... as the ability to let you set a shared network folder from the web interface has been removed.

I do not know what incantation of the API is needed for this, so here's something easier involving editing Jellyfin setting files directly:

1. Shutdown Jellyfin

2. Open C:\ProgramData\Jellyfin\Server\root\default\

3. Go into the folder of the library you want to modify and open its options.xml

4. Inside the `<PathInfos>` -> `<MediaPathInfo>` element, add a corresponding `<NetworkPath>` element underneath the `<Path>` element.

    Look [here](https://jellyfin.org/docs/general/clients/kodi#native-mode) for information on the format. As this is XML, you might need to escape certain characters in your text.

5. Save and start Jellyfin again.

Example:

```xml
  <PathInfos>
    <MediaPathInfo>
      <Path>C:\1</Path>
      <NetworkPath>\\192.168.1.1\C$\1</NetworkPath>
    </MediaPathInfo>
    <MediaPathInfo>
      <Path>C:\2</Path>
      <NetworkPath>\\192.168.1.1\C$\2</NetworkPath>
    </MediaPathInfo>
  </PathInfos>
```

## Installation

1. Build the plugin in Release configuration

2. Shutdown Jellyfin

3. Make a q12.JellyfinPlugin.LocalFilesPreferred folder in C:\ProgramData\Jellyfin\Server\plugins\

4. Copy `q12.JellyfinPlugin.LocalFilesPreferred\bin\Release\net8.0\q12.JellyfinPlugin.LocalFilesPreferred.dll` into said folder

5. Start Jellyfin
