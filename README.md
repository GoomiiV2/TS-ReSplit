# TS-ReSplit

ReSplit is a reverse engineering effort for the TimeSplitters games, currently focused on TimeSplitters 2.
Plan is to document the file formats and some of how the game's engine worked and to provide a library to interact with those formats.
Then on top of that to build a framework for loading and using those assets in a TimeSplitters like game using Unity as the base engine, like a PC port with some extra visual niceties.
**I don't and won't provide any TimeSplitters assets**, you will have to provide those yourself, think of this project like a replacement engine for the game, in the same vein as OpenMorrowwind and OpenRA.

This project is very much a work in progress.

# Progress
## File Format Parsing
- [x] PAK archive files for TS1/2/3 and Second Sight
- [x] PS2 TS2 Textures
- [x] TS2 Model format
- [ ] TS2 Animations (80% some animation types are yet to be worked out
- [ ] TS2 Level Data (70% Got enough to work with)
  - [x] Mesh Data
  - [x] Material listing
  - [x] Vis portals and doors
  - [x] Level segments
  - [ ] Other Data
- [x] PAD AI pathing
- [x] Vag sound files
- [x] MIB music files

# Visual Progress

[![](http://img.youtube.com/vi/jp1Slei3I4w/0.jpg)](https://www.youtube.com/watch?v=jp1Slei3I4w "")

[![](http://img.youtube.com/vi/5ahnKU03Lo8/0.jpg)](http://www.youtube.com/watch?v=5ahnKU03Lo8 "")

# Unity Assest Store Free Packs
Thanks to the creators of the below assest packs that were used.

You will need to get these from the assest store your self to build the project.

[Effect textures and prefabs](https://assetstore.unity.com/packages/vfx/particles/effect-textures-and-prefabs-109031) (placeholders hope to replace these)
