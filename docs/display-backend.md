# Display backend: composition rules (Direct3D 11)

The Direct3D 11 backend (`csharp/src/SuperMetroid.Rendering.Direct3D11`) draws a `DisplayPacket` and nothing else
(#1408). It knows no console memory image, palette memory, sprite table, register or bit depth: its inputs are the
packet's typed layers and effect descriptions, and the installed pre-coloured images the packet's layers and draws
name. `--packet-input` in `SuperMetroid.RenderVerification` checks that the renderer takes no other frame input and that
none of its code names a hardware type.

The backend is an intermediate step. v1.0.0 ports each rule below into Godot's own rendering, one by one; this page is
the list to port. Every rule reproduces a visible result of the game exactly: each was proven against the software
renderer before #1409 deleted it, and the GPU checks in `SuperMetroid.RenderVerification` now hold the hardware device
to the WARP reference.

## Resources

| Resource | Shader binding | What it holds |
|---|---|---|
| Image texture (`D3D11ImageAtlas`) | `Images`, t4 | Every installed image a scene draws: sprite frames, block pictures, background images, tile sheets, the message box. Each texel holds the image's pre-coloured pixel and its ink word: the ink, the pixel's priority, its part row, whether it is colour zero and whether colour zero keeps its group. An image keeps its place while later scenes show it; a scene that does not fit empties the texture and lays its images tallest first on a skyline. |
| Floor image (`D3D11FloorImage`) | `Floor`, t5 | The perspective floor, 1024 pixels square, laid out from its tiles' pictures whenever the floor changes, with its first tile's picture beside it. |
| Scene colours (`D3D11SceneColourBuffer`) | `Colours`, t0 | The backdrop, then one colour table per image and colouring: a bit per sixteen-ink group still showing the image's own colours, then the colour each ink shows now. |
| Sprite draws (`D3D11SpriteDrawBuffer`) | `SpriteDraws`, t1 | One record per whole-frame sprite draw (position, size, mirroring, image place, colours), front to back. |
| Room levels (`D3D11RoomLevelBuffer`) | `RoomLevels`, t3 | The layers' cells and block grids, each picture's place, size and colour slot, the overlays, sky rows, door window and the floor's sheets. |
| Sprite layer | `SpriteLayer`, u1 / render target | Per pixel, the frontmost sprite pixel's colour and its priority, with whether it blends. |
| Output | `Output`, u0 | The composed 256 by 224 frame, eight bits a channel. |

## Colour

A picture's pixel shows the image's own pre-coloured pixel while its ink's group still shows the image's own colours,
and otherwise the colour its ink shows now (`TexelColour`). The packet publishes what each ink shows
(`BackgroundColours`); the backend resolves it once per image per frame (`ShowsOwnColours`, `Colour`). This is how glows,
fades and tints recolour pre-coloured art. Colour zero is transparent, except where an image's colour zero keeps its group
(the HUD and room-effect art) and the layer shows colour zero: then it shows that group's colour zero.

A sprite draw's pixels show the image's own colours, or, where the draw is recoloured, the draw's colour for the pixel's
ink row or part row (`DrawColours`). A sprite pixel blends where its colours' row is one that blends.

The message box's text is coloured as the room's art is, with its two text colours in place of the two inks they replace
(`MessageBoxRenderLayer.ShownIn`).

## Passes

Each pass runs over the screen, or over a band of lines, in the order the scene lists its layers.

- **Sprite layer** (`Sprites.hlsl`). Each sprite draw is an instanced quad over its image, drawn back to front into the
  sprite layer, so each pixel keeps the frontmost draw's opaque pixel whatever its priority. A sprite hidden by one in
  front therefore never shows at another priority.
- **Backdrop.** Fills the screen with the backdrop colour.
- **Layer.** Draws a tile layer: a room plane through its block grid, the sky's rows, the layer-3 plane, a boss body, the
  X-ray reveal, a library background, or a frontend layer's cells. Each pixel of the layer shows its picture's pixel at
  the scrolled position. Options: a priority filter (only the low- or high-priority pixels), opaque colour zero, a scroll
  per line, a mosaic that groups screen pixels into blocks before the line's scroll, and words written over the layer
  drawn on top.
- **Room order.** A room composes, per band of lines with the same visible layers: sprites of priority 0 and 1, layer 2
  then layer 1 at low priority, sprites of priority 2, layer 2 then layer 1 at high priority, sprites of priority 3; then
  the HUD, layer 3, opaque, over the top 32 lines. Each scrolled layer shows the line one past its scroll on each screen
  row (`D3D11Screen.LayerLineOffset`).
- **Insert sprites.** Writes the sprite layer's pixels of one priority, or all, over the screen. Options: add the sprite's
  colour to the screen's, or add a fixed colour to a sprite that blends.
- **Floor** (the perspective floor). For each screen pixel, the floor position is the transform's matrix applied to the
  pixel, relative to its centre and scroll, with the screen's row counted from one. The products are truncated to whole
  sixty-fourths before they are summed and the sum is taken in 1/256ths, exactly as the game's floor shows; the centre and
  scroll are thirteen-bit signed values and the scroll's offset from the centre is ten-bit. Outside the floor it shows
  nothing, its first tile, or repeats. Options: add the floor's colour to whichever of the floor and the sprites is in front
  (the floor shows over priority-zero sprites; a sprite that does not blend is left as it is), or subtract the sprite
  layer's colour from the floor.
- **Add colour.** Adds a fixed colour to every pixel, in five-bit channels, clamped.
- **Line colour add.** Adds each line's colour within that line's window.
- **Add / subtract layer.** Adds or subtracts the room effect's layer-3 rows, each line at its own scroll, eight bits a
  channel, clamped.
- **Add layer behind.** Adds a frontend layer's colours where the layer covering it shows (and, optionally, where a
  sprite that blends shows over it), each line at its own scroll, in five-bit channels.
- **Message text.** Draws the message box's text image from its top line, within the reveal window's lines.
- **X-ray room** (`Xray.hlsli`). Composes a room under the X-ray in one pass: layers 1 and 2 and the sprites in room
  order, layer 2 hidden or shown inside the reveal window and layer 1 outside it, the layers each line shows, and outside
  the window the blend of the frontmost pixel with the fixed colour or with what lies behind (layer 2, layer 3 or both):
  add or subtract, halved where the blend halves.
- **Title gradient** (`TitleGradient.hlsl`). Adds or subtracts each line's colour to the pixels whose layer the line's
  blend names: the backdrop, or a sprite that blends.
- **Brightness and fades.** Scales every opaque pixel's channels by the level over fifteen, the scene's brightness first
  and then each fade in order, each rounding its own result.
- **Windowed scene.** Composes a child scene with its own resources and copies its window's rectangle into the screen.
- **Solid colour.** A packet that is one colour fills the screen with it, then applies its fades.

## Presentation

`Display.hlsl` scales the composed frame to the window at 4:3 with a whole-number vertical scale, nearest-neighbour,
reading the integer output texel by texel. Presentation, device-loss recovery and timing telemetry are unchanged by the
display model and live in `Presentation`, `Device` and `Telemetry`.
