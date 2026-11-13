# Installed pictures

This document describes how the installation stores the game's pre-coloured pictures (#1633, #1635, #1644, #1646): the
picture store that holds every picture's pixels once, each picture the box of what its frames show, the two kinds of map a
frame can show a picture with, each map stored once beside the store, and the sheet manifests that name where each frame's
picture lies in the store, where it lies in the frame, and which maps the frame shows it with. The import writes these files once. A player may edit them in place, and the game reads them as edited.

## Where the files are

Every sheet belongs to a family, and each family has its own directory under `game/`:

| Family directory | What it holds |
|---|---|
| `sprite-sheets/` | Samus, projectiles, the escape timer, and every screen's sprites and pictures |
| `enemy-sheets/` | the enemies and the objects they spawn |
| `room-block-sheets/` | room blocks, backgrounds, written pictures, the effect overlay and HUD band, message boxes and turning floors |
| `boss-body-sheets/` | the bosses' bodies |

A family directory holds each sheet's manifest, `game/<family>/<sheet>.json`, and no provenance manifest (#1693): the
sheet catalog, the tilesets or the family's own manifest name the sheets it holds. A family directory holds no pictures.

Every picture any sheet shows is in the picture store, `game/pictures/`:

| File | Contents |
|---|---|
| `game/pictures/pictures-NNNN.png` | a page of pictures: an 8-bit RGBA PNG, numbered from `0000` |
| `game/pictures/pictures-provenance.json` | the store's format revision, the SHA-256 of every page and map file, every picture's tag and every map's tag |
| `game/recolour-regions/pictures.regions` | every recolour-region map a frame shows a picture with, each once |
| `game/blend-maps/pictures.blendmap` | every blend map a frame shows a picture with, each once |

The maps live apart from the pages, so the store's folder holds only pictures. The store's provenance (format revision 4)
keeps `pageSha256` for the pages and `pictures` for every stored picture's place by its tag, and apart from them
`recolourRegionMaps` and `blendMaps`: each the map file's `sha256` and its `maps`, every map's tag with the size of the
picture it is for. A kind of map is `null` exactly when no frame shows a picture with one, and then no file is written.

## The picture store

A picture is the pixels one frame rectangle shows: its RGBA, including the RGB of pixels whose alpha is 0, and nothing
else. It is the box of what the rectangle shows (#1646): the smallest rectangle inside the frame's rectangle holding every
pixel that is not clear black `(0, 0, 0, 0)`. An opaque pixel is in the box, and so is a clear pixel that keeps a colour of
its own, since a layer drawn opaque shows that colour. Everything of the frame's rectangle outside the box is clear black,
in region 0 and not blending, and the frame names where in its rectangle the box lies, so every pixel lands where it lies
in the whole rectangle. Two frames show the same picture when their pixels match, pixel for pixel, wherever each lies in
its own rectangle: two frames that differ only in their clear margins show one picture. The store holds each picture once
per installation, however many frames of however many sheets show it: a message box that several tilesets show alike, for
example, is one picture that each of their `Messages.<tileset>` sheets names.

A turning floor the game repeats beyond its edges is stored whole: its picture is the floor's whole 1024 by 1024 pixels,
clear margin and all, because every pixel of the floor comes into view and the floor repeats with its whole size. These
are the floors whose scenes run with the cartridge's Mode 7 settings 0: the Ceres elevator shaft's floor
(`room-block-sheets/Floor.<tileset>` for the shaft's tilesets), the ending's two escape scenes and the planet's explosion
(`sprite-sheets/ending-escape-a-floor`, `ending-escape-b-floor` and `ending-planet-explosion-floor`). Every other turning
floor (the title's, the Ceres cinematics', the gunship's flyaway, the reward icon's and Ceres Ridley's getaway) runs with
settings `$80`, which show nothing beyond the floor, and is stored as the box of what it shows.

Each pixel's recolour region and whether it blends are not part of the picture. They belong to the frame that shows it:
the frame names the recolour-region map and the blend map it shows the picture with, and a frame needing neither names
none. Two sheets can show the same pixels with different maps, because each sheet numbers its own recolour regions. They
name one stored picture and different maps. Frames showing a picture with the same map name one stored map.

**Editing a picture on a page changes it for every frame of every sheet that shows it.** That is the point of the store:
one appearance, one picture. To change one sheet's copy only, give it a picture of its own: copy the picture to an unused
part of a page and point that sheet's frames at the copy. Editing a map likewise changes it for every frame that names it.

A frame rectangle that shows nothing at all (every pixel clear black) holds no picture, and its frame names none.

The import packs the pages in one fixed order, so every install writes the same bytes:

- the families in the order `sprite-sheets`, `room-block-sheets`, `boss-body-sheets`, `enemy-sheets`;
- each family's sheets by name, in ordinal order;
- each sheet's frame rectangles from the top, then from the left.

The first rectangle showing a picture's pixels stores it, and each map is tagged in the order rectangles first show it
(`R0000`, `R0001`, ... and `B0000`, `B0001`, ...). Pictures are packed onto shelves: each picture goes to the right of the
one before it, a picture that does not fit across the page starts a new shelf below the tallest picture of the last one,
and a picture that does not fit below the last shelf starts the next page. Every page is as wide as the widest picture,
but at least 1024 pixels, and at most 8192 pixels tall. Everything on a page outside its pictures is clear black.

The game assembles each sheet's image from the store when the sheet is first drawn. The image is the size its manifest
names. It starts clear: every pixel `(0, 0, 0, 0)`, in region 0 and not blending. Each frame's picture is then copied from
its page to the place in the frame's rectangle the frame names, and the maps the frame names are written over the same
place; the rest of the rectangle stays clear. A frame naming a page the store lacks, a rectangle off its page, a place that
does not lie inside its rectangle, or a map the store does not list at the picture's size fails to load. A
frame naming a stored picture that shows nothing (every pixel clear black) fails to load too, naming the frame: an
installed picture always shows something (#1636).

The store's pages are the installation's only picture files. When the store is opened, a `.png` anywhere under `game/`
outside `game/pictures/` (a character or tile page, an ink map, anything else) rejects the installation, and the
rejection names every such file.

## The picture

A picture is only its colours. Each pixel is RGBA:

- Alpha 255 means the pixel is opaque and shows its RGB.
- Alpha 0 means the pixel is clear.
- A clear pixel's RGB is the colour the pixel keeps of its own. A clear pixel that keeps no colour of its own is black,
  `(0, 0, 0, 0)`.

Whether a clear pixel shows is decided by the layer that draws it, not by the pixel:

- A layer drawn opaque shows a clear pixel's RGB exactly as it is stored. Gameplay's HUD band is drawn this way.
- A see-through layer shows whatever lies behind a clear pixel. The pause screen's HUD is drawn this way.

A clear pixel's colour is never recoloured: no shade names it, and it has no colour key.

No installed file names a colour by its place in the cartridge's colour table, and no file holds a cartridge colour word.

### Pictures whose RGB under transparency matters

These sheets are composed from four-colour character sets. Every one of their clear pixels keeps the first colour of its
colour set as its own, and their pictures on the store's pages keep it too:

| Sheet | Where it shows |
|---|---|
| `room-block-sheets/OverlayPictures.<tileset>` | each room's effect overlay pictures and gameplay's HUD band |
| `room-block-sheets/Messages.<tileset>` | the gameplay message boxes |
| `sprite-sheets/pause-hud` | the pause screen's HUD |
| `sprite-sheets/intro-narration-pictures` | the opening narration cards, the first card among them |
| `sprite-sheets/ending-wavy-samus-picture` | the ending's wavy Samus transformation |
| `sprite-sheets/area-map-background-<area>` | each area map's background |

Every other sheet's clear pixels are black and keep no colour of their own.

> **Warning.** The RGB of a clear pixel in the pictures of the sheets above is part of the picture. Anything that rewrites
> the colour of fully transparent pixels on a store page breaks these pictures where their layer is drawn opaque. This
> includes:
>
> - image editors that discard or flatten the colour under transparency when saving;
> - PNG optimisers that zero transparent pixels or reduce the image to indexed colour (for example "clean transparent
>   pixels", "alpha optimisation" or "remove colour under alpha");
> - Godot's texture importer with **Fix Alpha Border** enabled, and any premultiplied-alpha import or conversion, which
>   multiplies a clear pixel's RGB down to black.
>
> Edit the pages with a tool that keeps the colour of transparent pixels, and import them into an engine as straight (not
> premultiplied) alpha with alpha-border fixing turned off.

## The manifest

The manifest (`<sheet>.json`, revision 19) is camel-case JSON. Unknown fields are rejected. Every colour is written as
`#rrggbb`: two lowercase hexadecimal digits per 8-bit channel.

```json
{
  "version": 19,
  "size": { "x": 64, "y": 24 },
  "backdrop": { "colour": "#000000", "region": 0 },
  "shades": {
    "enemy-hit-flash/0": {
      "substitutions": [
        { "colour": "#ff6b00", "shows": "#ffffff" },
        { "colour": "#ff6b00", "region": 1, "shows": "#ff6b00" }
      ],
      "blends": true
    }
  },
  "frames": {
    "Idle": {
      "corner": { "x": 0, "y": 0 }, "size": { "x": 32, "y": 24 }, "origin": { "x": 16, "y": 12 }, "depth": "InFront",
      "blends": true,
      "picture": {
        "tag": "P3-0412", "page": 3, "corner": { "x": 512, "y": 1040 }, "size": { "x": 26, "y": 21 },
        "offset": { "x": 3, "y": 2 }, "recolourRegionMap": "R0042"
      }
    }
  },
  "blank": [ "Hidden" ]
}
```

The fields are:

- `version`: the manifest revision.
- `size`: the width (`x`) and height (`y`) of the sheet's image, which the game assembles from the store.
- `backdrop`: what a room tileset's scene shows where nothing is drawn, as a colour key. It is absent for every other
  sheet.
- `shades`: each shade the sheet is pre-coloured in, by shade name, as described in the next section. It is absent when
  the sheet has none.
- `frames`: each frame's rectangle on the sheet's image, its origin, its depth among the room's layers, and its picture.
  - `picture` names where the store holds the frame's picture: its `tag`, the page, counted from 0, the pixel of the
    page its top-left corner lies on, and its `size`, the box of what the frame shows (#1646). `offset` is where the
    picture's top-left pixel lies in the frame's rectangle, counted from the rectangle's top-left pixel as `origin` is; it
    is absent where the picture starts at that pixel. The picture lies wholly inside the frame's rectangle, and the rest
    of the rectangle is clear. Every frame showing the same pixels, on this sheet or any other, names the same place on
    its page, each with its own `offset`. `recolourRegionMap` and `blendMap` name the maps the frame shows the picture
    with, by tag; each is absent where the frame needs no such map. A frame whose rectangle shows nothing at all has no
    `picture`.
  - A frame drawn as a sprite whose every opaque pixel blends with what lies behind it has `"blends": true`. Otherwise
    `blends` is absent.
  - Only sheets drawn as sprites carry blending. These are the sheets of Samus, projectiles, enemies, the escape timer and
    every screen's sprites. A room, background, HUD, boss body or screen picture takes its blending from the layer that
    draws it.
- `blank`: the frames that show nothing, which the store holds no picture for. A picture of a room's or a screen's tiles
  is installed as its part behind (under its own name) and its part in front (under its name followed by `^front`); a
  part that shows nothing is not installed at all, neither as a frame nor in `blank`, and the game draws nothing there.
  Only such a picture neither part of which shows anything is listed in `blank`, under its own name (#1636).

### Colour keys and shades

A shade recolours a picture by colour. A colour key is a colour the picture's visible (opaque) pixels hold, or the
backdrop's colour, together with its recolour region: `{ "colour": "#rrggbb", "region": n }`. Clear pixels have no key.

- Region 0 is the usual case: every pixel of that colour recolours alike.
- A region number only orders the groups of one colour that recolour alike. It stands for nothing else.

The region a pixel lies in is the frame's: the recolour-region map the frame names says it, and a frame naming none holds
every pixel in region 0.

Each shade lists `substitutions`. A substitution `{ "colour": X, "region": n, "shows": Y }` means that the pixels of colour
X in region n show colour Y. `region` is omitted when it is 0. Pixels whose key a shade does not name keep the colour
they showed without it.

Shades are applied in the order the game names them. A later shade overwrites the colour of each key it names, whatever
an earlier shade showed there. A shade can therefore name a key with the key's own colour: that substitution restores
the colour over an earlier shade.

`blends` is true for a shade whose draws blend with what lies behind them, whatever the picture's own pixels say. It is
omitted when false.

## The recolour-region map

A frame names a recolour-region map only when it shows a pixel in a region other than 0: some of its sheet's opaque
pixels share a colour but some shade recolours them differently. For example, a Torizo's shell and body can share a
colour that its golden transition turns into two different colours. Regions are worked out over each sheet's opaque
pixels and backdrop only, so the same picture can lie in different regions on different sheets.

The file is `game/recolour-regions/pictures.regions`, format revision 2. It holds every map once, by its tag. It is JSON:

```json
{ "version": 2, "maps": { "R0000": { "size": { "x": 64, "y": 2 }, "rows": [ [0, 40, 1, 24], [0, 64] ] } } }
```

- Each map's `size` is the width (`x`) and height (`y`) of the picture it is for, which is the `size` every frame naming
  it names for its picture: the map covers the picture, placed where the frame places it.
- `rows` holds one entry per pixel row, from the top.
- Each row is a run-length list of `region, count` pairs, read from the left. A row's counts add up to exactly the map's
  width.
- Every opaque pixel's region applies to the pixel's own colour in its picture. Clear pixels are written as region 0 and
  are never recoloured.

## The blend map

A frame names a blend map only when it shows blending pixels: its sheet is drawn as sprites and has a frame that blends
in some opaque pixels and not others.

The file is `game/blend-maps/pictures.blendmap`, format revision 2. It holds every map once, by its tag. It is JSON:

```json
{ "version": 2, "maps": { "B0000": { "size": { "x": 32, "y": 2 }, "rows": [ [3, 5, 20, 2], [] ] } } }
```

- Each map's `size` is the width (`x`) and height (`y`) of the picture it is for, which is the `size` every frame naming
  it names for its picture: the map covers the picture, placed where the frame places it.
- `rows` holds one entry per pixel row, from the top.
- Each row lists its runs of blending pixels from the left as `start, count` pairs. The runs are in order, do not
  overlap, and stay inside the row.
- Every pixel outside a run does not blend.
