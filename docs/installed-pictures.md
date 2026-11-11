# Installed pictures

This document describes how the installation stores the game's pre-coloured pictures (#1633, #1635): the picture store
that holds every picture once, the sheet manifests that name where each frame's picture lies in it, and the two kinds of
map a store page can have beside it. The import writes these files once. A player may edit them in place, and the game
reads them as edited.

## Where the files are

Every sheet belongs to a family, and each family has its own directory under `game/`:

| Family directory | What it holds |
|---|---|
| `sprite-sheets/` | Samus, projectiles, the escape timer, and every screen's sprites and pictures |
| `enemy-sheets/` | the enemies and the objects they spawn |
| `room-block-sheets/` | room blocks, backgrounds, written pictures, the effect overlay and HUD band, message boxes and turning floors |
| `boss-body-sheets/` | the bosses' bodies |

A family directory holds each sheet's manifest, `game/<family>/<sheet>.json`, and the family's provenance manifest (for
example `sprite-sheets.json`), which lists every sheet manifest's SHA-256. A family directory holds no pictures.

Every picture any sheet shows is in the picture store, `game/pictures/`:

| File | Contents |
|---|---|
| `game/pictures/pictures-NNNN.png` | a page of pictures: an 8-bit RGBA PNG, numbered from `0000` |
| `game/pictures/pictures-provenance.json` | the store's format revision and the SHA-256 of every page and map |
| `game/recolour-regions/pictures/pictures-NNNN.regions` | a page's recolour-region map, only where the page needs one |
| `game/blend-maps/pictures/pictures-NNNN.blendmap` | a page's blend map, only where the page needs one |

The maps live apart from the pages, so the store's folder holds only pictures. The store's provenance keeps three
separate lists: `pageSha256` for the pages, `recolourRegionSha256` for the recolour-region maps and `blendMapSha256` for
the blend maps. A page has a map exactly when the provenance lists one for it.

## The picture store

A picture is what one frame rectangle shows: its RGBA, including the RGB of pixels whose alpha is 0, together with each
pixel's recolour region and whether it blends. Two frames show the same picture only when all of this matches, pixel for
pixel. The store holds each picture once per installation, however many frames of however many sheets show it: a message
box that several tilesets show alike, for example, is one picture that each of their `Messages.<tileset>` sheets names.

**Editing a picture on a page changes it for every frame of every sheet that shows it.** That is the point of the store:
one appearance, one picture. To change one sheet's copy only, give it a picture of its own: copy the picture to an unused
part of a page and point that sheet's frames at the copy.

A frame rectangle that shows nothing at all (every pixel clear black, in region 0 and not blending) holds no picture, and
its frame names none.

The import packs the pages in one fixed order, so every install writes the same bytes:

- the families in the order `sprite-sheets`, `room-block-sheets`, `boss-body-sheets`, `enemy-sheets`;
- each family's sheets by name, in ordinal order;
- each sheet's frame rectangles from the top, then from the left.

The first rectangle showing a picture stores it. Pictures are packed onto shelves: each picture goes to the right of the
one before it, a picture that does not fit across the page starts a new shelf below the tallest picture of the last one,
and a picture that does not fit below the last shelf starts the next page. Every page is as wide as the widest picture,
but at least 1024 pixels, and at most 8192 pixels tall. Everything on a page outside its pictures is clear black.

The game assembles each sheet's image from the store when the sheet is first drawn. The image is the size its manifest
names. It starts clear: every pixel `(0, 0, 0, 0)`, in region 0 and not blending. Each frame's picture is then copied from
its page into the frame's own rectangle. A frame naming a page the store lacks, or a rectangle off its page, fails to load.

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

The manifest (`<sheet>.json`, revision 15) is camel-case JSON. Unknown fields are rejected. Every colour is written as
`#rrggbb`: two lowercase hexadecimal digits per 8-bit channel.

```json
{
  "version": 15,
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
      "blends": true, "picture": { "page": 3, "corner": { "x": 512, "y": 1040 } }
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
  - `picture` names where the store holds the frame's picture: the page, counted from 0, and the pixel of the page its
    top-left corner lies on. The picture is the frame's size. Every frame showing the same picture, on this sheet or any
    other, names the same place. A frame whose rectangle shows nothing at all has no `picture`.
  - A frame drawn as a sprite whose every opaque pixel blends with what lies behind it has `"blends": true`. Otherwise
    `blends` is absent.
  - Only sheets drawn as sprites carry blending. These are the sheets of Samus, projectiles, enemies, the escape timer and
    every screen's sprites. A room, background, HUD, boss body or screen picture takes its blending from the layer that
    draws it.
- `blank`: the frames that show nothing, which the store holds no picture for.

### Colour keys and shades

A shade recolours a picture by colour. A colour key is a colour the picture's visible (opaque) pixels hold, or the
backdrop's colour, together with its recolour region: `{ "colour": "#rrggbb", "region": n }`. Clear pixels have no key.

- Region 0 is the usual case: every pixel of that colour recolours alike.
- A region number only orders the groups of one colour that recolour alike. It stands for nothing else.

The region a pixel lies in is the store's: the recolour-region map of the page its picture is on says it.

Each shade lists `substitutions`. A substitution `{ "colour": X, "region": n, "shows": Y }` means that the pixels of colour
X in region n show colour Y. `region` is omitted when it is 0. Pixels whose key a shade does not name keep the colour
they showed without it.

Shades are applied in the order the game names them. A later shade overwrites the colour of each key it names, whatever
an earlier shade showed there. A shade can therefore name a key with the key's own colour: that substitution restores
the colour over an earlier shade.

`blends` is true for a shade whose draws blend with what lies behind them, whatever the picture's own pixels say. It is
omitted when false.

## The recolour-region map

A page has a recolour-region map only when one of its pictures has a pixel in a region other than 0: some of its sheet's
opaque pixels share a colour but some shade recolours them differently. For example, a Torizo's shell and body can share
a colour that its golden transition turns into two different colours. Regions are worked out over each sheet's opaque
pixels and backdrop only.

The file is `game/recolour-regions/pictures/pictures-NNNN.regions`, format revision 1. It is JSON:

```json
{ "version": 1, "size": { "x": 1024, "y": 2 }, "rows": [ [0, 40, 1, 24, 0, 960], [0, 1024] ] }
```

- `size` is the page's width (`x`) and height (`y`).
- `rows` holds one entry per pixel row, from the top.
- Each row is a run-length list of `region, count` pairs, read from the left. A row's counts add up to exactly `width`.
- Every opaque pixel's region applies to the pixel's own colour in its picture. Clear pixels, and everything outside the
  page's pictures, are written as region 0 and are never recoloured.

## The blend map

A page has a blend map only when one of its pictures has blending pixels: the picture comes from a sheet drawn as sprites
with a frame that blends in some opaque pixels and not others.

The file is `game/blend-maps/pictures/pictures-NNNN.blendmap`, format revision 1. It is JSON:

```json
{ "version": 1, "size": { "x": 1024, "y": 2 }, "rows": [ [3, 5, 20, 2], [] ] }
```

- `size` is the page's width (`x`) and height (`y`).
- `rows` holds one entry per pixel row, from the top.
- Each row lists its runs of blending pixels from the left as `start, count` pairs. The runs are in order, do not
  overlap, and stay inside the row.
- Every pixel outside a run does not blend, everything outside the page's pictures included.
