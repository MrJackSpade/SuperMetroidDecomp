# Installed pictures

This document describes how the installation stores the game's pre-coloured pictures (#1633): the sheet images, their
manifests, and the two kinds of map a sheet can have beside them. The import writes these files once. A player may edit
them in place, and the game reads them as edited.

## Where the files are

Every sheet belongs to a family, and each family has its own directory under `game/`:

| Family directory | What it holds |
|---|---|
| `sprite-sheets/` | Samus, projectiles, the escape timer, and every screen's sprites and pictures |
| `enemy-sheets/` | the enemies and the objects they spawn |
| `room-block-sheets/` | room blocks, backgrounds, written pictures, the effect overlay and HUD band, message boxes and turning floors |
| `boss-body-sheets/` | the bosses' bodies |

Each sheet named `<sheet>` has these files:

| File | Contents |
|---|---|
| `game/<family>/<sheet>.png` | the picture: an 8-bit RGBA PNG |
| `game/<family>/<sheet>.json` | the manifest: frames, shades, backdrop, and which maps the sheet has |
| `game/recolour-regions/<family>/<sheet>.regions` | the recolour-region map, only where the sheet needs one |
| `game/blend-maps/<family>/<sheet>.blendmap` | the blend map, only where the sheet needs one |

The maps live apart from the pictures, so a picture folder holds only pictures and manifests. Each family's provenance
manifest (for example `sprite-sheets.json`) lists every file's SHA-256. It keeps three separate lists, each counted
separately: `sha256` for the pictures and manifests, `recolourRegionSha256` for the recolour-region maps, and
`blendMapSha256` for the blend maps.

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
colour set as its own:

| Sheet | Where it shows |
|---|---|
| `room-block-sheets/OverlayPictures.<tileset>.png` | each room's effect overlay pictures and gameplay's HUD band |
| `room-block-sheets/Messages.<tileset>.png` | the gameplay message boxes |
| `sprite-sheets/pause-hud.png` | the pause screen's HUD |
| `sprite-sheets/intro-narration-pictures.png` | the opening narration cards, the first card among them |
| `sprite-sheets/ending-wavy-samus-picture.png` | the ending's wavy Samus transformation |
| `sprite-sheets/area-map-background-<area>.png` | each area map's background |

Every other sheet's clear pixels are black and keep no colour of their own.

> **Warning.** The RGB of a clear pixel in the sheets above is part of the picture. Anything that rewrites the colour of
> fully transparent pixels breaks these pictures where their layer is drawn opaque. This includes:
>
> - image editors that discard or flatten the colour under transparency when saving;
> - PNG optimisers that zero transparent pixels or reduce the image to indexed colour (for example "clean transparent
>   pixels", "alpha optimisation" or "remove colour under alpha");
> - Godot's texture importer with **Fix Alpha Border** enabled, and any premultiplied-alpha import or conversion, which
>   multiplies a clear pixel's RGB down to black.
>
> Edit these sheets with a tool that keeps the colour of transparent pixels, and import them into an engine as
> straight (not premultiplied) alpha with alpha-border fixing turned off.

## The manifest

The manifest (`<sheet>.json`, revision 14) is camel-case JSON. Unknown fields are rejected. Every colour is written as
`#rrggbb`: two lowercase hexadecimal digits per 8-bit channel.

```json
{
  "version": 14,
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
    "Idle": { "corner": { "x": 0, "y": 0 }, "size": { "x": 32, "y": 24 }, "origin": { "x": 16, "y": 12 }, "depth": "InFront", "blends": true }
  },
  "blank": [ "Hidden" ],
  "recolourRegions": true,
  "blendMap": true
}
```

The fields are:

- `version`: the manifest revision.
- `backdrop`: what a room tileset's scene shows where nothing is drawn, as a colour key. It is absent for every other
  sheet.
- `shades`: each shade the sheet is pre-coloured in, by shade name, as described in the next section. It is absent when
  the sheet has none.
- `frames`: each frame's rectangle on the image, its origin, and its depth among the room's layers.
  - A frame drawn as a sprite whose every opaque pixel blends with what lies behind it has `"blends": true`. Otherwise
    `blends` is absent.
  - Only sheets drawn as sprites carry blending. These are the sheets of Samus, projectiles, enemies, the escape timer and
    every screen's sprites. A room, background, HUD, boss body or screen picture takes its blending from the layer that
    draws it.
- `blank`: the frames that show nothing, which the image holds no picture for.
- `recolourRegions`, `blendMap`: present and true when the sheet has that map. A sheet whose manifest names a map it does
  not have fails to load. A sheet without a map has none written.

### Colour keys and shades

A shade recolours a picture by colour. A colour key is a colour the picture's visible (opaque) pixels hold, or the
backdrop's colour, together with its recolour region: `{ "colour": "#rrggbb", "region": n }`. Clear pixels have no key.

- Region 0 is the usual case: every pixel of that colour recolours alike.
- A region number only orders the groups of one colour that recolour alike. It stands for nothing else.

Each shade lists `substitutions`. A substitution `{ "colour": X, "region": n, "shows": Y }` means that the pixels of colour
X in region n show colour Y. `region` is omitted when it is 0. Pixels whose key a shade does not name keep the colour
they showed without it.

Shades are applied in the order the game names them. A later shade overwrites the colour of each key it names, whatever
an earlier shade showed there. A shade can therefore name a key with the key's own colour: that substitution restores
the colour over an earlier shade.

`blends` is true for a shade whose draws blend with what lies behind them, whatever the picture's own pixels say. It is
omitted when false.

## The recolour-region map

A sheet has a recolour-region map only when some of its opaque pixels share a colour but some shade recolours them
differently. For example, a Torizo's shell and body can share a colour that its golden transition turns into two
different colours. Regions are worked out over the opaque pixels and the backdrop only.

The file is `game/recolour-regions/<family>/<sheet>.regions`, format revision 1. It is JSON:

```json
{ "version": 1, "size": { "x": 64, "y": 2 }, "rows": [ [0, 40, 1, 24], [0, 64] ] }
```

- `size` is the image's width (`x`) and height (`y`).
- `rows` holds one entry per pixel row, from the top.
- Each row is a run-length list of `region, count` pairs, read from the left. A row's counts add up to exactly `width`.
- Every opaque pixel's region applies to the pixel's own colour in the picture. Clear pixels are written as region 0
  and are never recoloured.

## The blend map

A sheet has a blend map only when it is drawn as sprites and one of its frames blends in some opaque pixels and not in
others.

The file is `game/blend-maps/<family>/<sheet>.blendmap`, format revision 1. It is JSON:

```json
{ "version": 1, "size": { "x": 64, "y": 2 }, "rows": [ [3, 5, 20, 2], [] ] }
```

- `size` is the image's width (`x`) and height (`y`).
- `rows` holds one entry per pixel row, from the top.
- Each row lists its runs of blending pixels from the left as `start, count` pairs. The runs are in order, do not
  overlap, and stay inside the row.
- Every pixel outside a run does not blend.
