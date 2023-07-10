# Local Files Preferred plugin for Jellyfin

A race condition-prone plugin to have Jellyfin return the return the real paths of items (i.e. "C:\...\1.mp4") when Kodi is not connected, instead of the set shared network folder path ("\\MyComputer\MyShare\1.mp4") when you've configured your libraries to include them.

This plugin was made for personal use with consideration shown only to my particular setup. In my case, I want the Kodi plugin to be able to play files via SMB but want the Jellyfin MPV Shim, which I run on the same PC as Jellyfin, to play files directly instead of streaming them through Jellyfin. You might also benefit from this plugin if you run Jellyfin on the same PC, have all your media on the same PC, primarily watch stuff on the same PC and have Kodi with the Jellyfin plugin only installed on one other device. As such:

* This plugin causes all `PathSubstitutions` in system.xml to be ignored (this was empty for me anyway)

* *Any* Kodi-running device disconnecting will cause local paths to be returned by JF's API, even if other Kodi devices are still connected. As I only use Kodi on one device, implementing tracking isn't worth my time

You probably want to have this plugin disabled when initially adding a Kodi client to Jellyfin, just in case. I have not checked if this plugin interferes with the operation of the Kodi Sync Queue plugin.

## Installation

1. Build the plugin in Release configuration

2. Shutdown Jellyfin

3. Make a q12.JellyfinPlugin.LocalFilesPreferred folder in C:\ProgramData\Jellyfin\Server\plugins\

4. Copy the following into said folder:

    * q12.JellyfinPlugin.LocalFilesPreferred.dll

    * 0Harmony.dll

    * Mono.Cecil.dll

    * MonoMod.Common.dll

5. Start Jellyfin

## TODOs

* a "configuration" option to grant a one-time reprieve until a Kodi device next reappears to force local path responses
