using SuperMetroid.Core.Assets;
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
