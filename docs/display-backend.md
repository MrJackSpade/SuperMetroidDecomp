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
| Image texture (`D3D11ImageAtlas`) | `Images`, t4 | Every installed image a scene draws: sprite frames, block pictures, background images, tile sheets, the message box. Each texel holds the image's pixel and its key word: for a clear pixel (alpha zero) only that it is clear; for a visible pixel the place of its colour key among the image's (`SpriteImage.ColourKeys`) and whether it blends where it is drawn as a sprite (its frame blends, or its sheet's blend map marks it). An image keeps its place while later scenes show it; a scene that does not fit empties the texture and lays its images tallest first on a skyline. |
| Floor image (`D3D11FloorImage`) | `Floor`, t5 | The perspective floor, 1024 pixels square, laid out from its tiles' pictures whenever the floor changes, with its first tile's picture beside it. |
| Scene colours (`D3D11SceneColourBuffer`) | `Colours`, t0 | The backdrop, then one colour table per image and colouring: the colour each of the image's colour keys shows under the colouring's shading, in the keys' order. |
| Sprite draws (`D3D11SpriteDrawBuffer`) | `SpriteDraws`, t1 | One record per whole-frame sprite draw (position, size, mirroring, depth among the room's layers, image place, colours), front to back: the paint's draws grouped by depth from the front back, then each sprite layer's. |
| Room levels (`D3D11RoomLevelBuffer`) | `RoomLevels`, t3 | The layers' block grids, each picture's place, size and colour slot, the pictures written over the layers and the HUD's at the pixels they show at, the effect overlay's placed picture, a boss body's placed pieces, the sky's bands, the door window and the floor's sheets (#1514). |
| Sprite layer | `SpriteLayer`, u1 / render target | Per pixel, the frontmost sprite pixel's colour and its draw's depth, with whether it blends. |
| Output | `Output`, u0 | The composed 256 by 224 frame, eight bits a channel. |

## Colour

A picture is only its colours (#1633): each pixel is an RGBA colour, alpha zero meaning clear. A visible pixel's colour
key is its colour with its recolour region, which the sheet's recolour-region map gives where pixels of one colour
recolour differently. A visible pixel shows the colour its key shows under its colour slot's shading (`TexelColour`): the
shading's shades overwrite the colours of the keys they name, then its colour effects change every key's colour, then its
shades over the effects overwrite again (`PictureShading.KeyColours`). This is how glows, fades and tints recolour
pre-coloured art. A clear pixel has no key and is never recoloured. A see-through layer leaves clear pixels out; a layer
drawn opaque (`HudLayer.DrawsOpaque`, `PicturedPlaneLayer.DrawsOpaque`) shows a clear pixel's stored colour as it is: the
colour the pixel keeps of its own (the HUD band and room-effect art), or black where it keeps none.

A sprite draw's pixels show the image's own colours, or, where the draw is shaded, the colour its shading gives each
pixel's key. A sprite pixel blends where its frame blends (`SpriteSheetFrame.Blends`), where its sheet's blend map marks
it (`SpriteImage.Blending`), or where a shade of the draw blends (`PictureRecolouring.Blends`). Only sprite draws read
blending from the picture; a picture drawn as a room, plane, HUD or boss source blends as its layer does.

The message box's picture is coloured from gameplay's message-box shading (`GameplayPictures.MessageBox`), as the room's
art is from the room's.

## Passes

Each pass runs over the screen, or over a band of lines, in the order the scene lists its layers.

- **Sprite layer** (`Sprites.hlsl`). Each sprite draw is an instanced quad over its image, drawn back to front into the
  sprite layer, so each pixel keeps the frontmost draw's opaque pixel. A gameplay frame's draws are packed by depth from
  the front back (#1514), so the frontmost draw is one at the frontmost depth, and each pixel records its draw's depth: a
  whole draw sits at one depth among the room's layers.
- **Backdrop.** Fills the screen with the backdrop colour.
- **Layer.** Draws a tile layer: a room plane through its block grid, the sky's rows, the effect overlay, a boss body, the
  X-ray reveal, a library background, or a frontend layer's cells. Each pixel of the layer shows its picture's pixel at
  the scrolled position. Options: only its pictures' part behind or only their part in front (#1634), drawn opaque
  (its clear pixels showing their stored colours), a scroll per line, a mosaic that groups screen pixels into blocks before the
  line's scroll, and words written over the layer drawn on top.
- **Room order.** A room composes, per band of lines with the same shown layers (`GameplaySceneLayer.Bands`), the
  gameplay scene's layers in `GameplaySceneLayer.Order` (#1514): the actors behind the room, the background's back tiles,
  the foreground's back tiles, the actors behind the front tiles, the background's front tiles, the foreground's front
  tiles and the actors in front; then the HUD, opaque, over the top 32 lines. The view names the room pixel each layer
  shows at the screen's top-left corner (`GameplaySceneView`), so every screen pixel shows the layer pixel that far from
  it, with no offset of the backend's own (#1514).
  colour to the screen's, or add a fixed colour to a sprite that blends.
- **Floor** (the perspective floor, `FloorView`). The display model describes the floor as an angle, a scale, a centre
  and a scroll; the backend turns the angle and scale into a matrix of four floats. For each screen pixel, the floor
  position is the centre plus that matrix applied to the pixel's distance from the centre once scrolled, rounded down to
  the floor pixel. The shader computes it with `precise` arithmetic, so every device lands each pixel on the same floor
  pixel. Outside the floor it shows nothing or repeats. Options: add the floor's colour to whichever of the floor and the sprites is in front
  (the floor shows over sprites drawn behind the room; a sprite that does not blend is left as it is), or subtract the
  sprite layer's colour from the floor.
- **Add colour.** Adds a fixed colour to every pixel, in the colour's thirty-two levels a channel, clamped. The display
  model describes colours as plain colours (`RgbColor`, 0-255 a channel), each on the game's fade scale; a colour reaches
  the tile shaders' colour maths as its own levels (`FadeLevelColour`, #1544), and one off the scale is rejected.
- **Line colour add.** Adds each line's colour within that line's window.
- **Add / subtract layer.** Adds or subtracts the room effect's overlay, each line at its own horizontal scroll, eight
  bits a channel, clamped. The overlay is a liquid or an atmosphere (`OverlayPicture`, #1514, #1706), each picture
  repeating across at its own width. An atmosphere (`AtmosphereOverlay`) is one picture repeating down at its height,
  scrolled line by line. A liquid (`LiquidOverlay`) is its surface's screen line, its surface strip drawn with the
  strip's origin row on that line and nothing above it, and its body repeating down from the strip's last row; while
  the surface lies at or above the screen's top line the strip does not show and the body fills every line at the place
  in its repeat it has below the strip. The room buffer holds the pictures; where they lie reaches each pass as its
  vertical scroll, so a moving surface or scroll repacks nothing.
- **Add layer behind.** Adds a frontend layer's colours where the layer covering it shows (and, optionally, where a
  sprite that blends shows over it), each line at its own scroll, in thirty-two levels a channel.
- **Message text.** Draws the message box's text image from its top line, within the reveal window's lines.
- **Blended room** (`Xray.hlsli`). Composes a room under a gameplay blend (`GameplayBlendLayer`: the X-ray, Phantoon,
  Fireflea, the waterfall rooms, the spores) in one pass: the foreground, the background and the actors in room order,
  the reveal shown inside the window and the foreground outside it, the layers each line shows, and outside the window
  the blend of the frontmost thing the blend names with the layers lying behind it (the background, the effect overlay,
  whose front tiles lie over the background, or both) or with the blend's colour where none lies behind: added or
  subtracted, halved where the blend halves, and the colour unhalved where the layers lying behind show nothing.
- **Gradient** (`ScreenGradient.hlsl`, #1468). A gradient is its stops and the band of lines they cover. Each line of
  the band takes the last stop beginning on or above it: its colour darkens the floor and the backdrop, or brightens
  them and the sprites that blend, as the stop's blend says.
- **Brightness and fades.** Scales every opaque pixel's channels by the level over fifteen, the scene's brightness first
  and then each fade in order, each rounding its own result.
- **Windowed scene.** Composes a child scene with its own resources and copies its window's rectangle into the screen.
- **Solid colour.** A packet that is one colour fills the screen with it, then applies its fades.

## Presentation

`Display.hlsl` scales the composed frame to the window at 4:3 with a whole-number vertical scale, nearest-neighbour,
reading the integer output texel by texel. Presentation, device-loss recovery and timing telemetry are unchanged by the
display model and live in `Presentation`, `Device` and `Telemetry`.
