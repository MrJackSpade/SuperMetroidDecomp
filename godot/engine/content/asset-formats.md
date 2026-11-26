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
| A colour effect's steps | `colours/{graphics}/` | `ColourEffect` | The steps of a change of colours: how long each lasts, the colours it shows changed, listed place by place (a picture's path or a folder of pictures), each as that place's pictures have it and as it is shown, a colour changed in only one region of the pictures with the region's number after a stroke; and a sound it may ask for; and the step it goes back to after its last. One file for each set of graphics the effect is shown in, since a step names colours as that set's pictures have them. |
| The front part of a tile's picture | beside the picture, named as it is with `.front.png` in place of `.png` | `ContentLibrary.LoadFrontOf`, drawn by `Terrain` | The part of a picture of a room's tiles that lies in front of the things standing in the room, the size of the picture and clear elsewhere. The picture itself stays whole; the part is drawn again over the room's things. It may have a map of regions of its own. |
| A picture's map of regions | beside the picture, named as it is with `.regions.png` in place of `.png` | `ContentLibrary.LoadTexture`, read by `Recolouring` | Greys, the size of the picture: each pixel as far from white as the number of the region the picture's pixel there lies in, so the first region is white. A picture has one where a change of colours has to tell apart two parts of it that have one colour. |
| A speed curve | wherever it is named | `SpeedCurve` | The marks of a curve of speeds. |
| Drops and pickups | wherever they are named | `DropTable`, `PickupType` | What an enemy leaves and what each pickup gives. |
| An area | `areas/`, and wherever a screen with a map of its own names | `Area` | An area's name and map: its cells, its markers and the labels laid over it. One area may have a file for each screen that draws it in colours of its own. |
| The game's words | the text folder the game definition names, one file for each language | `GameText` | Every message and label by name. |
| A demo | wherever it is named | `DemoRecording` | Recorded input. |
| How messages are shown | the file the game definition names | `MessageBoxStyle` | For a game whose messages are pictures: their folder, the line the window opens about, its size and step, the waits before and after, when a question looks at the player's presses, and the buttons a message's picture can name. |
| A font | wherever it is named | `PixelFont` | A picture of a font's characters, each one cell, and the list of them. |
| A picture | anywhere | `ContentLibrary.LoadTexture` | A PNG, at the size it is drawn. |
| Sounds and music | the sound and music folders the game definition names | the audio player | One WAV file for each name the game asks for. One that goes on and on says in the file where it loops from, and goes on until something else is asked for on its channel. Such a file ends with the loop's first frame written once more, and gives that frame as the loop's end: Godot's player leaves out one frame at the turn, the end or the start, and this way every frame of the loop is played once a turn. |

## Sounds

A sound is named for the old game's sound it is: the library it is in ("samus_", "enemy_" or "room_") and its name
there, in lower case with underscores. A piece of music is named for the old track. The sound "silence" is a
moment of nothing: a channel sounds one thing at a time, so asking for it on the channel of a sound that goes on and
on is how that sound is stopped. `tools/godot-rooms/check_audio.py` lists any name the project asks for that is none
of these, and `check_words.py` does the same for the game's words.

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
