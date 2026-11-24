# The asset files

Everything of Super Metroid's the game shows, plays or reads as a number is an asset: a plain file under `assets/`,
written from the player's ROM by the asset extractor and never committed. `ContentLibrary` loads them by their path
in that tree. This says what kinds of file there are and where each kind's exact shape is written down. The files
the project names, one by one, are listed in [asset-manifest.md](asset-manifest.md), which
`tools/godot-rooms/asset_manifest.py` writes from the project's scenes and scripts.

## Kinds of file

Each kind is read by one type, and that type's remarks give its exact shape with an example. A data file is JSON
with comments allowed.

| Kind | Where | Read by | What it holds |
| --- | --- | --- | --- |
| A thing's data | `enemies/`, `bosses/`, `objects/`, `samus/` | `DataValues` for the thing that names it | The thing's values by name (each property marked `DataValue`), its `colliders` by the names of its collider nodes (`EntityNode`), its `timelines` (`TimelineSet`), its `shots` and whatever its own remarks list. |
| A timeline | wherever it is named | `FrameTimeline` | Steps, each with a picture, how long it shows and the cues it gives. |
| A shot | wherever it is named | `ShotType`, `ProjectileSettings` | What a projectile is: its hit, how it moves, how it meets terrain and targets, how it looks. |
| A room layout | `rooms/<area>/` | `TerrainLayout` | The room's tiles, its blocks, its animated tiles and its named patches. |
| A block type | `blocks/` | `BlockType` | The rules of one kind of reactive block. |
| An item type | `items/` | `ItemType` | What an item grants, its message and how it looks. |
| A liquid type | `liquids/` | `LiquidType` | How a liquid slows, what it takes and how it looks. |
| An overlay's look | `effects/` | `OverlayEffect` | The pictures laid over the whole view, how they move and how they are laid over the room. |
| A speed curve | wherever it is named | `SpeedCurve` | The marks of a curve of speeds. |
| Drops and pickups | wherever they are named | `DropTable`, `PickupType` | What an enemy leaves and what each pickup gives. |
| An area | `areas/` | `Area` | An area's name and map. |
| The game's words | the text folder the game definition names, one file for each language | `GameText` | Every message and label by name. |
| A demo | wherever it is named | `DemoRecording` | Recorded input. |
| A picture | anywhere | `ContentLibrary.LoadTexture` | A PNG, at the size it is drawn. |
| Sounds and music | the sound and music folders the game definition names | the audio player | One file for each name the game asks for. |

## Room layouts

A room's scene never holds its tiles. Each version of a room names a layout,
`rooms/<area>/<level>_in_<graphics set>.json`, and the layout is the old room level of that name as that area's rooms
use it, drawn with that set of graphics; a level several areas use, or one drawn with two sets, is written once for
each. Beyond the tiles themselves, a layout answers for these things the old code did with objects placed over the
level:

- **Doors.** The blocks of a door are empty: a room exit's region lies over them. A door's cap is a barrier placed
  in the scene, which draws itself shut, opening and open; so the four blocks of the level a cap stands on are
  empty too, and the layout draws nothing there.
- **Elevators.** The blocks an elevator's platform rests on are solid: the door blocks the old level marks under a
  platform that leads nowhere, and the blocks of the old elevator-platform object.
- **Hidden items.** Where the old code hides an item in a shot block, the layout has that block.
- **Patches.** Every change the old code made to a room's blocks during play is a patch of the layout, by the name
  the manifest lists for it: a bridge collapsing, a wall coming down, the way the animals leave by.
- **Backgrounds.** A background picture is coloured by its room's set of graphics, so it is kept once for each set
  that shows it, as `backdrops/{graphics}/<name>.png`. A room whose old background is part of its level is drawn by its layout. One with a background
  picture of its own has a backdrop in its scene, and the picture is an asset.

## Pictures for a set of graphics

A layout names the set of graphics it is drawn with, under "graphics". The old game draws some things into a room
with the room's own blocks: a door's cap, a gate, a station. Such a thing looks as the room's graphics draw those
blocks, so its pictures are written once for each set, in a folder named for the set, and its data names them all by
one path with `{graphics}` where the folder's name goes. A room reads its files through a `ContentLibrary` made for
its set, which puts the set's name there.

## Where a placed thing stands

The room scenes place each of the old code's enemies where the old code placed it. Each of its room objects, which
the old code placed on a block, stands on that block's top-left corner, and its data's colliders and pictures are
measured from there; an item stands on the middle of its block.
