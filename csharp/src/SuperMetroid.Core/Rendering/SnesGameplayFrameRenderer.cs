using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Composes the currently modeled gameplay PPU layers into one desktop frame.</summary>
public static class SnesGameplayFrameRenderer
{
    public const int Width = 256;
    public const int Height = 224;
    public const int HudHeight = 32;

    /// <summary>
    /// Combines the scanline-switched BG3 HUD and OBJ table. Room BG1/BG2 pixels will be
    /// inserted between these layers once the scrolling viewport feeds VRAM tilemaps.
    /// </summary>
    public static Rgba32[] RenderHudAndObjs(
        SnesVram vram,
        SnesCgram cgram,
        OamBuffer oam,
        byte obsel = 0x03)
    {
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(cgram);
        ArgumentNullException.ThrowIfNull(oam);

        Rgba32[] output = CreateBackdrop(cgram);
        DrawHudAndObjects(output, vram, cgram, oam, obsel);
        return output;
    }

    /// <summary>
    /// Inserts a ROM-derived, host-decoded room crop behind the modeled PPU HUD and objects.
    /// This is the terrain-complete debugger view while live library-background composition
    /// remains incomplete; callers can still select the all-live PPU compositor separately.
    /// </summary>
    public static Rgba32[] RenderHudRoomAndObjs(
        SnesVram vram,
        SnesCgram cgram,
        OamBuffer oam,
        ReadOnlySpan<Rgba32> roomPixels,
        int roomWidth,
        int roomHeight,
        int cameraX,
        int cameraY,
        byte obsel = 0x03)
    {
        if (roomWidth <= 0 || roomHeight <= 0 || roomPixels.Length != checked(roomWidth * roomHeight))
            throw new ArgumentException("Room pixel buffer does not match its dimensions.", nameof(roomPixels));
        if (cameraX < 0 || cameraY < 0 || cameraX + Width > roomWidth || cameraY + Height > roomHeight)
            throw new ArgumentOutOfRangeException(nameof(cameraX), "Camera must contain the complete physical 256x224 viewport.");

        Rgba32[] output = CreateBackdrop(cgram);
        for (int screenY = HudHeight; screenY < Height; screenY++)
        {
            // The HUD does not create a new BG1 coordinate system. IRQ command 4 merely
            // hides BG1 for physical scanlines 0-31, then command 6 exposes it again.
            // Consequently the first visible terrain row at screen Y=32 samples world
            // cameraY+32. This is also what the live PPU renderer expresses by adding
            // HudHeight to BG1VOFS before rendering its 192-row diagnostic surface.
            int sourceY = cameraY + screenY;
            roomPixels.Slice(sourceY * roomWidth + cameraX, Width)
                .CopyTo(output.AsSpan(screenY * Width, Width));
        }

        DrawHudAndObjects(output, vram, cgram, oam, obsel);
        return output;
    }

    /// <summary>
    /// Uses the translated live BG1 tilemap for foreground pixels while retaining a decoded
    /// room image as a terrain-complete lower layer. This hybrid is an explicit diagnostic:
    /// Landing Site's live library-background composition is not complete yet.
    /// </summary>
    public static Rgba32[] RenderHudLiveBg1AndHostBackground(
        SnesVram vram,
        SnesCgram cgram,
        OamBuffer oam,
        ReadOnlySpan<Rgba32> roomPixels,
        int roomWidth,
        int roomHeight,
        int cameraX,
        int cameraY,
        ushort bg1HorizontalScroll,
        ushort bg1VerticalScroll,
        byte obsel = 0x03)
    {
        if (roomWidth <= 0 || roomHeight <= 0 || roomPixels.Length != checked(roomWidth * roomHeight))
            throw new ArgumentException("Room pixel buffer does not match its dimensions.", nameof(roomPixels));
        if (cameraX < 0 || cameraY < 0 || cameraX + Width > roomWidth || cameraY + Height > roomHeight)
            throw new ArgumentOutOfRangeException(nameof(cameraX));

        Rgba32[] output = CreateBackdrop(cgram);

        // Until BG2's scrolling-sky library background is translated, this crop supplies
        // only what should be visible through transparent BG1 pixels.
        for (int screenY = HudHeight; screenY < Height; screenY++)
        {
            // Keep the temporary host layer in the same physical-scanline coordinate
            // system as BG1. Screen line 32 therefore samples cameraY+32, not cameraY.
            int sourceY = cameraY + screenY;
            roomPixels.Slice(sourceY * roomWidth + cameraX, Width)
                .CopyTo(output.AsSpan(screenY * Width, Width));
        }

        Rgba32[] bg1 = SnesBgTilemapRenderer.Render4BppViewport(
            vram,
            cgram,
            tilemapBaseWord: 0x5000,
            characterBaseWord: 0,
            bg1HorizontalScroll,
            bg1VerticalScroll,
            Width,
            Height - HudHeight);
        for (int pixel = 0; pixel < bg1.Length; pixel++)
        {
            if (bg1[pixel].A != 0)
                output[HudHeight * Width + pixel] = bg1[pixel];
        }

        DrawHudAndObjects(output, vram, cgram, oam, obsel);
        return output;
    }

    /// <summary>
    /// Composes Landing Site entirely from modeled PPU state: scanline-scrolled BG2,
    /// streamed BG1, the IRQ-switched BG3 HUD, and OAM objects.
    /// </summary>
    public static Rgba32[] RenderHudLiveBackgroundsAndObjs(
        SnesVram vram,
        SnesCgram cgram,
        OamBuffer oam,
        ushort bg1HorizontalScroll,
        ushort bg1VerticalScroll,
        ushort bg2VerticalScroll,
        IReadOnlyList<ushort> bg2HorizontalScrollByLine,
        byte obsel = 0x03)
    {
        ArgumentNullException.ThrowIfNull(bg2HorizontalScrollByLine);
        if (bg2HorizontalScrollByLine.Count != Height - HudHeight)
            throw new ArgumentException("BG2 HDMA scrolls must cover all 192 gameplay lines.", nameof(bg2HorizontalScrollByLine));

        Rgba32[] output = CreateBackdrop(cgram);
        Rgba32[] bg2 = SnesBgTilemapRenderer.Render4BppViewport(
            vram,
            cgram,
            tilemapBaseWord: 0x4800,
            characterBaseWord: 0,
            horizontalScroll: 0,
            // IRQ command 4 reserves physical scanlines 0-31 for BG3. The PPU does not
            // restart BG scroll coordinates when command 6 restores gameplay layers at
            // scanline 32, so the first output row samples BG2VOFS + 32.
            verticalScroll: unchecked((ushort)(bg2VerticalScroll + HudHeight)),
            width: Width,
            height: Height - HudHeight,
            tilemapWidthInTiles: 32,
            // Setup ASM and the HDMA pre-instruction both write BG2SC=$4A. Its low bits
            // are %10: one 32x32 screen at $4800 above another at $4C00.
            tilemapHeightInTiles: 64,
            horizontalScrollByLine: bg2HorizontalScrollByLine);
        Rgba32[] bg1 = SnesBgTilemapRenderer.Render4BppViewport(
            vram,
            cgram,
            tilemapBaseWord: 0x5000,
            characterBaseWord: 0,
            bg1HorizontalScroll,
            // BG1 is subject to the same physical-scanline offset as BG2. Keeping this
            // adjustment in the compositor lets the register mirrors remain literal.
            unchecked((ushort)(bg1VerticalScroll + HudHeight)),
            Width,
            Height - HudHeight);

        for (int pixel = 0; pixel < bg2.Length; pixel++)
        {
            int framePixel = HudHeight * Width + pixel;
            if (bg2[pixel].A != 0)
                output[framePixel] = bg2[pixel];
            if (bg1[pixel].A != 0)
                output[framePixel] = bg1[pixel];
        }

        DrawHudAndObjects(output, vram, cgram, oam, obsel);
        return output;
    }

    /// <summary>
    /// Applies the translated power-bomb fixed-color window to a composed gameplay frame.
    /// </summary>
    /// <remarks>
    /// The PPU performs this during scanout through two indirect HDMA channels. Desktop
    /// rendering already owns a flat RGBA frame, so replaying the resulting per-scanline
    /// window is equivalent and keeps the ordinary BG/OBJ compositors independent of this
    /// one effect. Pre-scaled yellow/white phases read the cartridge's actual 192-byte
    /// shape records. The two continuously accelerated phases replay `$88:8D04`
    /// against the ROM's horizontal and vertical curve bytes at `$88:A266/A286`; this
    /// deliberately preserves the SNES routine's 8x8 multiply truncation and its
    /// one-scanline overlap between adjacent curve bands.
    /// </remarks>
    public static void ApplyPowerBombColorMath(
        Span<Rgba32> frame,
        ISnesAddressSpace bus,
        SamusPowerBombExplosionState explosion,
        ushort layer1X,
        ushort layer1Y)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(explosion);
        if (frame.Length != Width * Height)
            throw new ArgumentException("Power-bomb compositor requires one complete 256x224 frame.", nameof(frame));
        if (!explosion.IsActive)
            return;

        int centerX = unchecked((short)(explosion.XPosition - layer1X));
        int centerY = unchecked((short)(explosion.YPosition - layer1Y));
        byte addRed = ExpandFiveBit(explosion.FixedColorRed);
        byte addGreen = ExpandFiveBit(explosion.FixedColorGreen);
        byte addBlue = ExpandFiveBit(explosion.FixedColorBlue);

        for (int screenY = HudHeight; screenY < Height; screenY++)
        {
            int halfWidth = ReadPowerBombHalfWidth(bus, explosion, screenY - centerY);
            if (halfWidth < 0)
                continue;

            int left = Math.Max(0, centerX - halfWidth);
            int right = Math.Min(Width - 1, centerX + halfWidth);
            if (left > right)
                continue;

            int row = screenY * Width;
            for (int screenX = left; screenX <= right; screenX++)
            {
                Rgba32 source = frame[row + screenX];

                // The active room blending mode adds fixed COLDATA to the main-screen
                // layers with SNES saturation. Expanding five-bit components before the
                // addition produces the same 8-bit endpoint as the rest of this renderer.
                frame[row + screenX] = new Rgba32(
                    SaturatingAdd(source.R, addRed),
                    SaturatingAdd(source.G, addGreen),
                    SaturatingAdd(source.B, addBlue),
                    source.A);
            }
        }
    }

    private static int ReadPowerBombHalfWidth(
        ISnesAddressSpace bus,
        SamusPowerBombExplosionState explosion,
        int yFromCenter)
    {
        PowerBombExplosionPhase renderedPhase = explosion.RenderedPhase;
        if (renderedPhase == PowerBombExplosionPhase.Inactive)
        {
            // Spawn occurs during projectile processing, after the frame's HDMA-object
            // pass. Status is already active at that point, but no window table exists
            // until the next frame; the cartridge therefore displays no flash yet.
            return -1;
        }

        if (renderedPhase == PowerBombExplosionPhase.Afterglow)
        {
            // The final `$9E46` table has expanded beyond the 256-pixel viewport. Stage
            // five stops updating HDMA data and fades the already full-screen fixed color.
            return Width;
        }

        ushort shapePointer = explosion.RenderedShapeDefinitionPointer;
        if ((renderedPhase is PowerBombExplosionPhase.PreExplosionYellow or
             PowerBombExplosionPhase.ExplosionWhite) && shapePointer != 0)
        {
            // Each 192-byte record is a half-profile, not a 192-line top-to-bottom
            // bitmap. Byte zero is the widest center line; increasing indices travel
            // away from the center until the first zero terminates the native copier.
            int shapeLine = Math.Abs(yFromCenter);
            if ((uint)shapeLine >= 192)
                return -1;
            byte halfWidth = bus.ReadByte(0x880000 | unchecked((ushort)(shapePointer + shapeLine)));
            return halfWidth == 0 ? -1 : halfWidth;
        }

        int horizontalRadius = renderedPhase == PowerBombExplosionPhase.PreExplosionWhite
            ? explosion.RenderedPreExplosionRadius >> 8
            : explosion.RenderedExplosionRadius >> 8;
        int scanlineDistance = Math.Abs(yFromCenter);
        if (horizontalRadius == 0 || scanlineDistance >= 192)
            return -1;

        // `$88:8CC6/8D04/8D46` all build the same width profile and differ only in how
        // they clip left/right endpoints for an off-screen origin. The desktop renderer
        // performs that clipping after this method, so only the common profile builder
        // is needed here. `$88:A266` contains 32 increasing horizontal samples, while
        // `$88:A286` contains their decreasing vertical boundaries.
        const int HorizontalCurveAddress = 0x88A266;
        const int VerticalCurveAddress = 0x88A286;
        int currentOuterScanline =
            horizontalRadius * bus.ReadByte(VerticalCurveAddress) >> 8;
        if (scanlineDistance > currentOuterScanline)
            return -1;

        int selectedHalfWidth = -1;
        int finalHalfWidth = 0;
        for (int curveIndex = 0; curveIndex < 32; curveIndex++)
        {
            // The 65816 routine uses the high byte of an unsigned 8x8 product. An
            // ordinary integer multiply followed by `>> 8` is exactly that operation.
            int innerScanline = horizontalRadius * bus.ReadByte(VerticalCurveAddress + curveIndex) >> 8;
            int halfWidth = horizontalRadius * bus.ReadByte(HorizontalCurveAddress + curveIndex) >> 8;
            finalHalfWidth = halfWidth;

            // Native code fills both endpoints inclusively, then begins the next band
            // on the same endpoint. Assigning again on a shared boundary intentionally
            // lets the later, wider band win just as the original loop does.
            if (scanlineDistance >= innerScanline && scanlineDistance <= currentOuterScanline)
                selectedHalfWidth = halfWidth;

            currentOuterScanline = innerScanline;
        }

        // After the 32 curve bands, `$88:8DE9/90DF` fills every remaining line through
        // the center with the final (nearly full-radius) width.
        if (scanlineDistance <= currentOuterScanline)
            selectedHalfWidth = finalHalfWidth;

        return selectedHalfWidth;
    }

    private static byte ExpandFiveBit(byte value) =>
        (byte)(((value & 0x1f) << 3) | ((value & 0x1f) >> 2));

    private static byte SaturatingAdd(byte left, byte right) =>
        (byte)Math.Min(byte.MaxValue, left + right);

    private static Rgba32[] CreateBackdrop(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);

        // CGRAM color zero is the PPU backdrop. Starting with it (rather than transparent
        // host pixels) matches every screen location no translated layer has covered yet.
        var output = new Rgba32[Width * Height];
        Array.Fill(output, cgram.GetRgba(0));
        return output;
    }

    private static void DrawHudAndObjects(
        Rgba32[] output,
        SnesVram vram,
        SnesCgram cgram,
        OamBuffer oam,
        byte obsel)
    {
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(cgram);
        ArgumentNullException.ThrowIfNull(oam);

        // IRQ command 4 writes BG3SC=$5A and TM=$04 for scanlines 0-31. That exposes the
        // four HUD rows at tilemap word $5800 while disabling OBJ in the HUD band.
        Rgba32[] hud = SnesBgTilemapRenderer.Render2Bpp(
            vram,
            cgram,
            tilemapBaseWord: 0x5800,
            characterBaseWord: 0x4000,
            rowCount: 4);
        hud.CopyTo(output, 0);

        Rgba32[] objects = SnesObjRenderer.Render(oam, vram, cgram, obsel, Width, Height);
        for (int pixel = HudHeight * Width; pixel < output.Length; pixel++)
        {
            // OBJ palette index zero was emitted as alpha zero. In the modeled gameplay
            // region, any nonzero OBJ pixel sits above the not-yet-integrated room layers.
            if (objects[pixel].A != 0)
                output[pixel] = objects[pixel];
        }

    }
}
