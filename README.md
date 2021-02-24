# ValheimUtils

Collection of various mods for Valheim.

ValheimUtils uses [MonoMod](https://github.com/MonoMod/MonoMod) as a mod framwework, and relies on [BiPinEx](https://github.com/BepInEx/BepInEx) for mods loading.

## Sections

* [Mods](#mods)
* [Prerequsites](#prerequsites)
* [Installation](#installation)

## Mods

* [SharedMap](#sharedmap)
* [NoServerPassword](#noserverpassword)

### SharedMap

Map sharing between all players in game (online and offline). You can use this mod retroactively, shared map will be updated once a player with explored map is connected to server.

Implemented features:
* Server-side explored map shared bewtween all players.
* Forced players public positions.

Planned features:
* Bosses position and players pins sharing.
* Keep explored map on server between restarts.

### NoServerPassword

Let you start public server without password. You probably want to add some SteamID to permittedlist.txt if you plan to do this.

Planned features:
* Ask password only for not whitelisted users.

## Prerequsites

* [Valheim](https://www.valheimgame.com/) of course :)
* [BiPinEx](https://github.com/BepInEx/BepInEx)
* [BepInEx.MelonLoader.Loader](https://github.com/BepInEx/BepInEx.MelonLoader.Loader)
* Some Mono asemblies from [Unity 2019.4.20f1](https://unity3d.com/unity/qa/lts-releases?version=2019.4)

## Installation

* Windows builds is available in [releases section](https://github.com/glcoder/ValheimUtils/releases).
* For Linux you can use same assemblies but you need BePinEx for Linux, which you can download manually [from here](https://github.com/BepInEx/BepInEx/releases).

Just unpack archive content over Valheim installation and you ready to go. You can always undo installation by removing `winhttp.dll` and restoring Valheim files. Probably it will be needed to repeat mod installation process after each Valheim update.
