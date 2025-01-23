# Pulsar
 A successor to Super Smash Bros. Ultimate's mod manager Quasar, written in C# and Visual Studio.

 An important note: Tags are for users to define. Tags will be cleared when zipping a mod.

# Current Features

## Full One-Click install functionality.
 You'll need to run the setup.bat to enable one-click. Drag Pulsar's exe onto the script after downloading it. Even so, Gamebanana doesn't support it yet, but I'm working on it.

## Tabs:

### Mods

#### Package Builder.
 Package Installer from archive, generates Pulsar metadata from info.toml if found.

#### Package Zipper using 7z.

#### Search/Sort Installed Mods
 Additional sorting can be done through user-defined tags. These can be created when editing metadata.

#### Built-in Logging and Console.

#### File Conflict Management.

### Assign

#### The Costume Assigner
 Edits paramaters in Pulsar's metadata files to swap the costume that will be changed when deployed.

#### WIP ALERT
 The costume assigner is still a work in progress since I don't actually know how the game is layed out internally. All the mods I've tested it with work fine, but some things may fail. Please read the alert!

### Settings

#### Deploy Path
 This is where your mods will be sent when being deployed.

#### Default Preview Image Changer
 I had a couple images I liked so I just included them all anyway. Swap them out inside the settings window.

#### Blacklist
 Got those mods that not everyone needs to see? You can blacklist user-defined tags to have them not show up in your mod window. Bear in mind, if they were enabled before, they still are, and will be deployed!

## Possible features that may be added in the future

### Merging of certain files.

# Credits
## [Aemulus Mod Manager](https://github.com/TekkaGB/AemulusModManager)
 Aemulus' Parallel Logging feature was used, and is a huge inspiration for this project.

## [Quasar](https://github.com/Mowjoh/Quasar)
 Quasar, the origianl SSBU mod manager. I tried looking at its code but none of it made sense to me. Their logo is used for one of the default previews.

## [83](https://www.youtube.com/watch?v=e1xCOsgWG0M)
 They drew the Hatsune Miku image used in one of the default previews.

## [oceanstuck](https://github.com/oceanstuck)
 Based person who gave me the name for this.

## [Atmosphere-NX](https://github.com/Atmosphere-NX/Atmosphere)
 CFW for the Nintendo Switch. Their background is used for one of the default previews.

## Should any one of these people wish for me to remove their work from my project, I will happily oblige.

# Software Info

## Framework
 Runs in .NET Framework 4.7.2

## External Libraries

### [HTML Agility Pack](https://github.com/zzzprojects/html-agility-pack)

### [SevenZipSharp](https://www.nuget.org/packages/SevenZipSharp/0.64.0?_src=template)
 In hindsight I should use a more up-to-date fork. Might change it out later.

### [SevenZipSharp.Interop](https://github.com/luuksommers/SevenZipSharp.Interop/)

### [SevenZipExtractor](https://github.com/adoconnection/SevenZipExtractor)
 SevenZipSharp didn't support .rar files.

### [ImageSharp](https://github.com/SixLabors/ImageSharp)
