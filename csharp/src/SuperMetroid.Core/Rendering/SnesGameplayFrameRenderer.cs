using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Rendering;

/// <summary>Composes the currently modeled gameplay PPU layers into one desktop frame.</summary>
public static partial class SnesGameplayFrameRenderer
{
    public const int Width = SnesPpuLayout.ScreenWidthPixels;
    public const int Height = SnesPpuLayout.ScreenHeightPixels;
    public const int HudHeight = SnesPpuLayout.GameplayHudHeightPixels;

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
            tilemapBaseWord: SnesPpuLayout.GameplayBg1TilemapWord,
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
            tilemapBaseWord: SnesPpuLayout.GameplayBg2TilemapWord,
            characterBaseWord: 0,
            horizontalScroll: 0,
            // IRQ command 4 reserves physical scanlines 0-31 for BG3. The PPU does not
            // restart BG scroll coordinates when command 6 restores gameplay layers at
            // scanline 32, so the first output row samples BG2VOFS + 32.
            verticalScroll: unchecked((ushort)(bg2VerticalScroll + HudHeight)),
            width: Width,
            height: Height - HudHeight,
            tilemapWidthInTiles: 32,
            // Scrolling-sky setup selects BG2SC=$4A. Its low bits are %10: one 32x32
            // circular screen at $4800 above another at $4C00.
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
    /// Composes the ordinary gameplay PPU layout used by Ceres and most interior rooms,
    /// including the priority bit of every BG tile and all four OBJ priority groups.
    /// </summary>
    public static Rgba32[] RenderHudOrdinaryBackgroundsAndObjs(
        SnesVram vram,
        SnesCgram cgram,
        OamBuffer oam,
        ushort bg1HorizontalScroll,
        ushort bg1VerticalScroll,
        ushort bg2HorizontalScroll,
        ushort bg2VerticalScroll,
        IReadOnlyList<ushort>? bg2HorizontalScrollByLine = null,
        IReadOnlyList<ushort>? bg2VerticalScrollByLine = null,
        int bg2TilemapWidthInTiles = 64,
        int bg2TilemapHeightInTiles = 32,
        ushort bg2TilemapBaseWord = SnesPpuLayout.GameplayBg2TilemapWord,
        ushort bg1CharacterBaseWord = 0,
        ushort bg2CharacterBaseWord = 0,
        ushort bg3CharacterBaseWord = SnesPpuLayout.GameplayHudCharacterBaseWord,
        byte obsel = 0x03,
        SnesMainScreenLayers mainScreenLayers =
            SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj,
        Rgba32[]? outputBuffer = null,
        int bg2FirstScanline = 32, int bg2EndScanline = 224,
        SnesWindowRegisters windowRegisters = default,
        SnesMainScreenLayers mainScreenWindowMask = SnesMainScreenLayers.None)
    {
        Rgba32[] output = CreateBackdrop(cgram, outputBuffer);
        Span<SnesMainScreenLayers> windowMasks = stackalloc SnesMainScreenLayers[Width];
        if (mainScreenWindowMask == SnesMainScreenLayers.None)
            windowMasks.Clear();
        else
            for (int x = 0; x < Width; x++)
                windowMasks[x] = windowRegisters.MaskedLayers((byte)x, mainScreenWindowMask);
        // BGMODE=$09 is Mode 1 with the BG3-priority flag. Below the HUD, BG3 is disabled
        // by TM and the relevant back-to-front ladder is OBJ0, OBJ1, BG2-low, BG1-low,
        // OBJ2, BG2-high, BG1-high, OBJ3. Samus's body entries use OBJ2, so cartridge-
        // authored high-priority door frames, railings, and laboratory consoles cover her.
        // Resolve OAM ownership once. Priority planes are a BG-compositor concern only;
        // decoding all 128 sprite records independently for each of the four planes was
        // both slower and easier to get subtly wrong at cross-priority overlaps.
        int pixelCount = Width * Height;
        Rgba32[] rentedObjectPixels = ArrayPool<Rgba32>.Shared.Rent(pixelCount);
        byte[] rentedObjectPriorities = ArrayPool<byte>.Shared.Rent(pixelCount);
        try
        {
            Span<Rgba32> objectPixels = rentedObjectPixels.AsSpan(0, pixelCount);
            Span<byte> objectPriorities = rentedObjectPriorities.AsSpan(0, pixelCount);
            SnesObjRenderer.RenderResolved(
                oam,
                vram,
                cgram,
                obsel,
                objectPixels,
                objectPriorities,
                Width,
                Height);
            CompositeOrdinaryGameplayViewport(
                output,
                vram,
                cgram,
                objectPixels,
                objectPriorities,
                bg1CharacterBaseWord,
                bg2CharacterBaseWord,
                bg1HorizontalScroll,
                bg1VerticalScroll,
                bg2HorizontalScroll,
                bg2VerticalScroll,
                bg2HorizontalScrollByLine,
                bg2VerticalScrollByLine,
                bg2TilemapWidthInTiles,
                bg2TilemapHeightInTiles,
                bg2TilemapBaseWord,
                mainScreenLayers, bg2FirstScanline, bg2EndScanline, windowMasks);
        }
        finally
        {
            // Every opaque object pixel is overwritten before use and transparency is
            // governed by the parallel priority sentinel, so clearing these private render
            // buffers during return would spend frame time without changing observability.
            ArrayPool<Rgba32>.Shared.Return(rentedObjectPixels, clearArray: false);
            ArrayPool<byte>.Shared.Return(rentedObjectPriorities, clearArray: false);
        }
        DrawHud(output, vram, cgram, bg3CharacterBaseWord);
        if ((mainScreenWindowMask & SnesMainScreenLayers.Bg3) != 0)
        {
            // The modeled HUD IRQ admits only BG3. A masked HUD pixel therefore
            // exposes backdrop, not the gameplay planes disabled on these scanlines.
            Rgba32 backdrop = cgram.GetRgba(0);
            for (int x = 0; x < Width; x++)
                if ((windowMasks[x] & SnesMainScreenLayers.Bg3) != 0)
                    for (int y = 0; y < HudHeight; y++)
                        output[y * Width + x] = backdrop;
        }
        return output;
    }

    // This pixel loop is deliberately optimized even in a Debug host. It is pure PPU
    // projection rather than translated game logic, runs roughly 50,000 times per frame,
    // and is not a useful breakpoint surface for ordinary gameplay debugging. Without the
    // attribute, Debug JIT call/range-check overhead alone consumes most of the 16.67 ms
    // video-frame budget.
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void CompositeOrdinaryGameplayViewport(
        Span<Rgba32> output,
        SnesVram vram,
        SnesCgram cgram,
        ReadOnlySpan<Rgba32> objectPixels,
        ReadOnlySpan<byte> objectPriorities,
        ushort bg1CharacterBaseWord,
        ushort bg2CharacterBaseWord,
        ushort bg1HorizontalScroll,
        ushort bg1VerticalScroll,
        ushort bg2HorizontalScroll,
        ushort bg2VerticalScroll,
        IReadOnlyList<ushort>? bg2HorizontalScrollByLine,
        IReadOnlyList<ushort>? bg2VerticalScrollByLine,
        int bg2TilemapWidthInTiles,
        int bg2TilemapHeightInTiles,
        ushort bg2TilemapBaseWord,
        SnesMainScreenLayers mainScreenLayers,
        int bg2FirstScanline, int bg2EndScanline,
        ReadOnlySpan<SnesMainScreenLayers> windowMasks)
    {
        if (objectPixels.Length != output.Length ||
            objectPriorities.Length != output.Length)
        {
            throw new ArgumentException(
                "A resolved gameplay OBJ raster must contain exactly 256x224 pixels.",
                nameof(objectPixels));
        }
        if (bg2HorizontalScrollByLine is not null &&
            bg2HorizontalScrollByLine.Count < Height - HudHeight)
        {
            throw new ArgumentException(
                "BG2 horizontal HDMA scrolls must cover all 192 gameplay lines.",
                nameof(bg2HorizontalScrollByLine));
        }
        if (bg2VerticalScrollByLine is not null &&
            bg2VerticalScrollByLine.Count < Height - HudHeight)
        {
            throw new ArgumentException(
                "BG2 vertical HDMA scrolls must cover all 192 gameplay lines.",
                nameof(bg2VerticalScrollByLine));
        }
        if (bg2TilemapWidthInTiles is not (32 or 64) ||
            bg2TilemapHeightInTiles is not (32 or 64) ||
            bg2TilemapWidthInTiles * bg2TilemapHeightInTiles is not (1024 or 2048 or 4096))
        {
            throw new ArgumentException(
                "BG2 gameplay tilemaps must use 32 or 64 tiles on each axis.",
                nameof(bg2TilemapWidthInTiles));
        }

        // Resolve the complete Mode-1 ladder in a single destination scan. The previous
        // implementation walked the 256x192 viewport eight times (four BG insertions and
        // four OBJ insertions), repeating tilemap address arithmetic and moving temporary
        // RGBA planes through memory. Each candidate below receives its literal back-to-
        // front rank from BGMODE=$09; the largest opaque rank owns the final pixel.
        // Snapshot the hardware arrays once and expand the tiny 256-color CGRAM table once
        // per frame. Going through bounds-checking object methods and decoding BGR555 for
        // both background candidates at every pixel dominated Debug-host frame time while
        // producing the same immutable values throughout this render pass.
        ReadOnlySpan<byte> vramBytes = vram.Bytes;
        Span<Rgba32> palette = stackalloc Rgba32[SnesCgram.ColorCount];
        ExpandCgram(cgram, palette);
        bool bg1Enabled = (mainScreenLayers & SnesMainScreenLayers.Bg1) != 0;
        bool bg2LayerEnabled = (mainScreenLayers & SnesMainScreenLayers.Bg2) != 0;
        bool objEnabled = (mainScreenLayers & SnesMainScreenLayers.Obj) != 0;

        int bg2XMask = bg2TilemapWidthInTiles * 8 - 1;
        int bg2YMask = bg2TilemapHeightInTiles * 8 - 1;
        int bg2ScreensPerRow = bg2TilemapWidthInTiles >> 5;
        for (int screenY = HudHeight; screenY < Height; screenY++)
        {
            bool bg2Enabled = bg2LayerEnabled && screenY >= bg2FirstScanline && screenY < bg2EndScanline;
            // Output rows are zero-based, while Mode-1 BG sampling starts on
            // physical scanline one. OBJ's preceding-line evaluation is separate.
            int physicalBackgroundY = screenY + SnesPpuLayout.FirstVisibleBackgroundScanline;
            int bg1ScrolledY = unchecked(bg1VerticalScroll + physicalBackgroundY) & 0xff;
            ushort activeBg2HorizontalScroll = bg2HorizontalScrollByLine is null
                ? bg2HorizontalScroll
                : bg2HorizontalScrollByLine[screenY - HudHeight];
            ushort activeBg2VerticalScroll = bg2VerticalScrollByLine is null
                ? bg2VerticalScroll
                : bg2VerticalScrollByLine[screenY - HudHeight];
            int bg2ScrolledY = unchecked(activeBg2VerticalScroll + physicalBackgroundY) & bg2YMask;
            int bg1TileY = bg1ScrolledY >> 3;
            int bg2TileY = bg2ScrolledY >> 3;
            int bg1PixelY = bg1ScrolledY & 7;
            int bg2PixelY = bg2ScrolledY & 7;
            int previousBg1TileX = -1;
            int previousBg2TileX = -1;
            SnesBgTilemapWord bg1Entry = default;
            SnesBgTilemapWord bg2Entry = default;

            for (int screenX = 0; screenX < Width; screenX++)
            {
                int destination = screenY * Width + screenX;
                Rgba32 winner = output[destination];
                int winnerRank = 0;

                // Horizontal fine scrolling changes the active tile only once every eight
                // output pixels. Cache each BGSC word across that run; rereading the same
                // two VRAM bytes for every pixel was pure interpreter overhead, especially
                // in Debug builds where these tiny accessors are not reliably inlined.
                int bg2ScrolledX = unchecked(activeBg2HorizontalScroll + screenX) & bg2XMask;
                int bg2TileX = bg2ScrolledX >> 3;
                if (bg2Enabled && bg2TileX != previousBg2TileX)
                {
                    // BGSC's two size bits select either two horizontal 32x32 screens
                    // (ordinary rooms) or two vertical screens (Landing Site's sky).
                    // Both contain 2,048 words, but putting the screen index on the wrong
                    // axis makes the sky go black and then decode unrelated VRAM as the
                    // camera crosses the first 256-pixel boundary.
                    int bg2ScreenColumn = bg2TileX >> 5;
                    int bg2ScreenRow = bg2TileY >> 5;
                    int bg2MapWord = (
                        bg2TilemapBaseWord +
                        (bg2ScreenRow * bg2ScreensPerRow + bg2ScreenColumn) * SnesPpuLayout.TilemapPageWordCount +
                        (bg2TileY & 31) * 32 +
                        (bg2TileX & 31)) & 0x7fff;
                    int bg2MapByte = bg2MapWord * 2;
                    bg2Entry = (ushort)(
                        vramBytes[bg2MapByte] |
                        (vramBytes[bg2MapByte + 1] << 8));
                    previousBg2TileX = bg2TileX;
                }

                if (bg2Enabled && (windowMasks[screenX] & SnesMainScreenLayers.Bg2) == 0 &&
                    TryDecodeOrdinaryGameplayBgPixel(
                        vramBytes,
                        palette,
                        bg2Entry,
                        bg2CharacterBaseWord,
                        bg2ScrolledX & 7,
                        bg2PixelY,
                        out Rgba32 bg2Color,
                        out bool bg2High))
                {
                    winner = bg2Color;
                    winnerRank = bg2High ? 6 : 3;
                }

                int bg1ScrolledX = unchecked(bg1HorizontalScroll + screenX) & 0x01ff;
                int bg1TileX = bg1ScrolledX >> 3;
                if (bg1Enabled && bg1TileX != previousBg1TileX)
                {
                    int bg1MapWord = (
                        SnesPpuLayout.GameplayBg1TilemapWord +
                        (bg1TileX >> 5) * SnesPpuLayout.TilemapPageWordCount +
                        bg1TileY * 32 +
                        (bg1TileX & 31)) & 0x7fff;
                    int bg1MapByte = bg1MapWord * 2;
                    bg1Entry = (ushort)(
                        vramBytes[bg1MapByte] |
                        (vramBytes[bg1MapByte + 1] << 8));
                    previousBg1TileX = bg1TileX;
                }

                if (bg1Enabled && (windowMasks[screenX] & SnesMainScreenLayers.Bg1) == 0 &&
                    TryDecodeOrdinaryGameplayBgPixel(
                        vramBytes,
                        palette,
                        bg1Entry,
                        bg1CharacterBaseWord,
                        bg1ScrolledX & 7,
                        bg1PixelY,
                        out Rgba32 bg1Color,
                        out bool bg1High))
                {
                    int bg1Rank = bg1High ? 7 : 4;
                    if (bg1Rank > winnerRank)
                    {
                        winner = bg1Color;
                        winnerRank = bg1Rank;
                    }
                }

                byte objPriority = objectPriorities[destination];
                if (objEnabled && (windowMasks[screenX] & SnesMainScreenLayers.Obj) == 0 &&
                    objPriority != SnesObjRenderer.TransparentPriority)
                {
                    int objRank = objPriority switch
                    {
                        0 => 1,
                        1 => 2,
                        2 => 5,
                        3 => 8,
                        _ => throw new InvalidDataException(
                            $"Resolved OBJ priority {objPriority} is outside zero through three."),
                    };
                    if (objRank > winnerRank)
                        winner = objectPixels[destination];
                }

                output[destination] = winner;
            }
        }
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static bool TryDecodeOrdinaryGameplayBgPixel(
        ReadOnlySpan<byte> vram,
        ReadOnlySpan<Rgba32> palette,
        SnesBgTilemapWord entry,
        ushort characterBaseWord,
        int pixelX,
        int pixelY,
        out Rgba32 color,
        out bool highPriority)
    {
        highPriority = entry.HasPriority;

        int sourceX = entry.FlipHorizontally
            ? 7 - pixelX
            : pixelX;
        int sourceY = entry.FlipVertically
            ? 7 - pixelY
            : pixelY;
        int characterByteAddress =
            ((characterBaseWord + entry.CharacterIndex * 16) & 0x7fff) * 2;
        int mask = 1 << (7 - sourceX);
        int rowAddress = characterByteAddress + sourceY * 2;
        int colorIndex = ((vram[rowAddress] & mask) != 0 ? 1 : 0)
                       | ((vram[rowAddress + 1] & mask) != 0 ? 2 : 0)
                       | ((vram[rowAddress + 16] & mask) != 0 ? 4 : 0)
                       | ((vram[rowAddress + 17] & mask) != 0 ? 8 : 0);
        if (colorIndex == 0)
        {
            color = default;
            return false;
        }

        color = palette[entry.PaletteIndex * 16 + colorIndex];
        return true;
    }

    /// <summary>
    /// Expands one frame-stable CGRAM snapshot into directly indexable host colors. The
    /// 256-entry conversion is substantially cheaper than decoding the same palette word
    /// independently for tens of thousands of background pixels.
    /// </summary>
    private static void ExpandCgram(SnesCgram cgram, Span<Rgba32> destination)
    {
        if (destination.Length != SnesCgram.ColorCount)
            throw new ArgumentException("Expanded CGRAM requires exactly 256 colors.", nameof(destination));

        ReadOnlySpan<ushort> source = cgram.Colors;
        for (int color = 0; color < destination.Length; color++)
            destination[color] = SnesGraphics.DecodeBgr555Color(source[color]);
    }

    private static void DrawHud(
        Span<Rgba32> output,
        SnesVram vram,
        SnesCgram cgram,
        ushort characterBaseWord = SnesPpuLayout.GameplayHudCharacterBaseWord)
    {
        SnesBgTilemapRenderer.Render2Bpp(
            output[..(HudHeight * Width)],
            vram,
            cgram,
            tilemapBaseWord: SnesPpuLayout.GameplayHudTilemapWord,
            characterBaseWord,
            rowCount: 4);
    }

    /// <summary>
    /// Composes the IRQ-split gameplay display used by the opening Ceres elevator: Mode 7
    /// below scanline 32, the ordinary BG3 HUD above it, then gameplay-region objects.
    /// </summary>
    /// <remarks>
    /// Door setup $8F:E4E0 changes BGMODE from the gameplay default $09 to $07 and installs
    /// the identity matrix A=D=$0100, B=C=0 with center ($0080,$03F0). Rendering the same
    /// interleaved VRAM as Mode-1 4-bpp tiles produces regular one-pixel color stripes—the
    /// conspicuous failure this dedicated path prevents.
    /// </remarks>
    public static Rgba32[] RenderHudMode7AndObjs(
        SnesVram vram,
        SnesCgram cgram,
        OamBuffer oam,
        short matrixA,
        short matrixB,
        short matrixC,
        short matrixD,
        short centerX,
        short centerY,
        short horizontalOffset,
        short verticalOffset,
        byte obsel = 0x03)
    {
        Rgba32[] output = CreateBackdrop(cgram);
        SnesMode7Renderer.CompositeViewport(
            output,
            vram,
            cgram,
            matrixA,
            matrixB,
            matrixC,
            matrixD,
            centerX,
            centerY,
            horizontalOffset,
            verticalOffset,
            Width,
            Height);

        // IRQ command four gives the upper 32 physical scanlines exclusively to BG3.
        // Mode 7 is projected at physical screen coordinates across the destination first;
        // DrawHudAndObjects replaces its upper band with BG3 rather than cropping a separate
        // 192-line transform whose Y coordinate would incorrectly restart at zero.

        DrawHudAndObjects(output, vram, cgram, oam, obsel);
        return output;
    }

    /// <summary>
    /// Composes Ceres Ridley's getaway with the two bank-$88 HDMA layer splits that the
    /// generic Mode-7 elevator path does not have: BG3 owns the HUD, Mode 7 owns the arena,
    /// and the final sixteen scanlines switch back to Mode 1 with only BG2 and OBJ enabled.
    /// </summary>
    public static Rgba32[] RenderHudCeresRidleyGetawayAndObjs(
        SnesVram vram,
        SnesCgram cgram,
        OamBuffer oam,
        short matrixA,
        short matrixB,
        short matrixC,
        short matrixD,
        short centerX,
        short centerY,
        short horizontalOffset,
        short verticalOffset,
        ushort bg2HorizontalScroll,
        ushort bg2VerticalScroll,
        ushort bg2CharacterBaseWord,
        byte obsel = 0x03)
    {
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(cgram);
        ArgumentNullException.ThrowIfNull(oam);

        const int floorHeight = 16;
        const int floorFirstScanline = Height - floorHeight;
        Rgba32[] output = CreateBackdrop(cgram);
        Rgba32[] mode7 = SnesMode7Renderer.RenderViewport(
            vram,
            cgram,
            matrixA,
            matrixB,
            matrixC,
            matrixD,
            centerX,
            centerY,
            horizontalOffset,
            verticalOffset,
            Width,
            Height);
        ResolvedObjFrame objects = SnesObjRenderer.RenderResolved(
            oam,
            vram,
            cgram,
            obsel,
            Width,
            Height);

        // IndirectHDMATable_CeresRidleyMode_BGTileSize and its matching TM table keep
        // scanlines 32..207 in Mode 7 with BG1/BG2/OBJ selected. The existing Mode-7
        // compositor treats every OBJ priority alike here because no ordinary BG priority
        // planes compete with it.
        for (int screenY = HudHeight; screenY < floorFirstScanline; screenY++)
        {
            int row = screenY * Width;
            for (int screenX = 0; screenX < Width; screenX++)
            {
                int destination = row + screenX;
                if (mode7[destination].A != 0)
                    output[destination] = mode7[destination];
                if (objects.Priorities[destination] != SnesObjRenderer.TransparentPriority)
                    output[destination] = objects.Pixels[destination];
            }
        }

        // Instruction_VideoMode_for_HUD_and_Floor_1 installs BGMODE=$09 for the last
        // sixteen lines and TM=$12: BG2 plus sprites, with BG1 deliberately absent. Reuse
        // the ordinary Mode-1 pixel decoder but resolve only the surviving two layer types.
        // Their native back-to-front ranks are OBJ0, OBJ1, BG2-low, OBJ2, BG2-high, OBJ3.
        ReadOnlySpan<byte> vramBytes = vram.Bytes;
        Span<Rgba32> palette = stackalloc Rgba32[SnesCgram.ColorCount];
        ExpandCgram(cgram, palette);
        for (int screenY = floorFirstScanline; screenY < Height; screenY++)
        {
            int scrolledY = unchecked(bg2VerticalScroll + screenY) & 0xff;
            int tileY = scrolledY >> 3;
            int pixelY = scrolledY & 7;
            int previousTileX = -1;
            SnesBgTilemapWord bg2Entry = default;

            for (int screenX = 0; screenX < Width; screenX++)
            {
                int destination = screenY * Width + screenX;
                Rgba32 winner = output[destination];
                int winnerRank = 0;
                int scrolledX = unchecked(bg2HorizontalScroll + screenX) & 0x01ff;
                int tileX = scrolledX >> 3;
                if (tileX != previousTileX)
                {
                    int mapWord = (
                        0x4800 +
                        (tileX >> 5) * 0x0400 +
                        tileY * 32 +
                        (tileX & 31)) & 0x7fff;
                    int mapByte = mapWord * 2;
                    bg2Entry = (ushort)(vramBytes[mapByte] | (vramBytes[mapByte + 1] << 8));
                    previousTileX = tileX;
                }

                if (TryDecodeOrdinaryGameplayBgPixel(
                        vramBytes,
                        palette,
                        bg2Entry,
                        bg2CharacterBaseWord,
                        scrolledX & 7,
                        pixelY,
                        out Rgba32 bg2Color,
                        out bool bg2High))
                {
                    winner = bg2Color;
                    winnerRank = bg2High ? 6 : 3;
                }

                byte objPriority = objects.Priorities[destination];
                if (objPriority != SnesObjRenderer.TransparentPriority)
                {
                    int objRank = objPriority switch
                    {
                        0 => 1,
                        1 => 2,
                        2 => 5,
                        3 => 8,
                        _ => throw new InvalidDataException(
                            $"Resolved OBJ priority {objPriority} is outside zero through three."),
                    };
                    if (objRank > winnerRank)
                        winner = objects.Pixels[destination];
                }

                output[destination] = winner;
            }
        }

        DrawHud(output, vram, cgram);
        return output;
    }

    /// <summary>
    /// Replays the fully faded-in scanline result of FX type $2C (Ceres haze).
    /// </summary>
    /// <remarks>
    /// The HDMA table at $88:DF03 holds blue fixed colour one for its first 64 scanlines,
    /// then advances through blue two..sixteen in eight-line bands. Layer-blending config
    /// $2C adds that fixed backdrop colour to BG1, BG2, OBJ, and the backdrop while excluding
    /// BG3; consequently the IRQ-owned 32-line HUD must remain untouched. The Ridley-dead
    /// route uses the same table with a red selector and is retained as an explicit argument
    /// rather than hiding boss-state policy in this renderer.
    /// </remarks>
    public static void ApplyCeresHaze(Span<Rgba32> frame, bool ridleyIsDead, int intensity = CeresHazeRenderDefinitions.MaximumComponent)
    {
        if (frame.Length != Width * Height)
            throw new ArgumentException("Ceres haze requires one complete 256x224 frame.", nameof(frame));

        for (int screenY = HudHeight; screenY < Height; screenY++)
        {
            int component = screenY < CeresHazeRenderDefinitions.RampFirstLine
                ? CeresHazeRenderDefinitions.InitialComponent
                : Math.Min(CeresHazeRenderDefinitions.MaximumComponent,
                    CeresHazeRenderDefinitions.RampFirstComponent +
                    (screenY - CeresHazeRenderDefinitions.RampFirstLine) / CeresHazeRenderDefinitions.BandHeight);
            component = Math.Max(0, component + intensity - CeresHazeRenderDefinitions.MaximumComponent);
            byte addition = ExpandFiveBit((byte)component);
            int row = screenY * Width;
            for (int screenX = 0; screenX < Width; screenX++)
            {
                int pixel = row + screenX;
                Rgba32 source = frame[pixel];
                frame[pixel] = ridleyIsDead
                    ? new Rgba32(SaturatingAdd(source.R, addition), source.G, source.B, source.A)
                    : new Rgba32(source.R, source.G, SaturatingAdd(source.B, addition), source.A);
            }
        }
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

    /// <summary>
    /// Applies X-ray's moving window and outside-window half-color-math operation.
    /// </summary>
    /// <remarks>
    /// The cartridge writes two window endpoints per scanline through `$88:8896` and the
    /// bank-$91 `$C54B/$BD1A` geometry routines. Replaying the same angular boundaries at
    /// desktop composition time is equivalent to those WH2/WH3 writes: the two edge rays
    /// use the ROM's own 8.8 absolute-tangent table and pixels between them remain bright.
    /// `$88:817B/$81A4` then enable halved color math outside that window. The separate
    /// X-ray BG2 tilemap producer—which replaces hidden block tiles inside the window—is
    /// intentionally not claimed here; this method only owns the already-independent PPU
    /// window/color-math part of the effect.
    /// </remarks>
    public static void ApplyXrayWindowColorMath(
        Span<Rgba32> frame,
        ISnesAddressSpace bus,
        SamusXrayState xray,
        SamusState samus,
        ushort layer1X,
        ushort layer1Y)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(xray);
        ArgumentNullException.ThrowIfNull(samus);
        if (frame.Length != Width * Height)
            throw new ArgumentException("X-ray compositor requires one complete 256x224 frame.", nameof(frame));

        // Setup stages have not installed `$88:86EF` yet. States three through five have
        // already replaced the active table with `$00FF` endpoints while the two backed-up
        // BG2 screens are restored, so neither interval has a visible beam to composite.
        if (!xray.IsActive || xray.SetupStage != 0 ||
            xray.BeamPhase is not (XrayBeamPhase.Widening or XrayBeamPhase.Full))
        {
            return;
        }

        bool facingLeft = samus.ReadPoseXDirection(bus) == 4;
        SamusMovementType movementType = samus.ReadMovementType(bus);

        // `$88:88B8-$88F3` anchors the ray three pixels in front of Samus. Standing and
        // turning bodies place it sixteen pixels above the center; only stable movement
        // type five uses the crouching twelve-pixel offset.
        int originX = unchecked((short)(samus.XPosition - layer1X)) + (facingLeft ? -3 : 3);
        int originY = unchecked((short)(samus.YPosition - layer1Y)) -
            (movementType == SamusMovementType.Crouching ? 12 : 16);
        int centerAngle = xray.Angle.TableIndex;
        int angularWidth = xray.AngularWidth & 0x00ff;
        int leftEdgeAngle = (centerAngle - angularWidth) & 0x00ff;
        int rightEdgeAngle = (centerAngle + angularWidth) & 0x00ff;

        // State zero calculates a genuinely zero-width horizontal table before changing
        // state to one. `$91:C901` special-cases exactly right/left so it produces a single
        // horizontal scanline instead of using the finite `$3C00` tangent-table sentinel.
        bool horizontalLine = angularWidth == 0 &&
            (centerAngle == SnesAngle.QuarterTurn.TableIndex ||
             centerAngle == SnesAngle.ThreeQuarterTurn.TableIndex);
        XrayDirection leftEdge = ReadXrayDirection(bus, leftEdgeAngle);
        XrayDirection rightEdge = ReadXrayDirection(bus, rightEdgeAngle);

        for (int screenY = HudHeight; screenY < Height; screenY++)
        {
            int fromOriginY = screenY - originY;
            int row = screenY * Width;
            for (int screenX = 0; screenX < Width; screenX++)
            {
                int fromOriginX = screenX - originX;
                bool inside;
                if (horizontalLine)
                {
                    inside = fromOriginY == 0 &&
                        (centerAngle == SnesAngle.QuarterTurn.TableIndex
                            ? fromOriginX >= 0
                            : fromOriginX <= 0);
                }
                else
                {
                    // Super Metroid angles increase clockwise. A point is inside this
                    // always-narrower-than-180-degree cone when it is clockwise from the
                    // left edge and counter-clockwise from the right edge. Cross products
                    // retain the exact ROM tangent ratios without floating-point atan/trig.
                    long leftCross = (long)leftEdge.X * fromOriginY -
                        (long)leftEdge.Y * fromOriginX;
                    long rightCross = (long)rightEdge.X * fromOriginY -
                        (long)rightEdge.Y * fromOriginX;
                    // The assembly stores the high byte after each 8.8 accumulation, so
                    // a fractional boundary is rounded outward to its containing pixel.
                    // A ±$FF cross-product tolerance is precisely that subpixel remainder.
                    inside = leftCross >= -0x00ff && rightCross <= 0x00ff;
                }

                if (!inside)
                    frame[row + screenX] = ApplyXrayOutsideHalfColor(frame[row + screenX]);
            }
        }
    }

    /// <summary>
    /// Applies the Morph Ball security eye's bank-$88 window-two fixed-color beam.
    /// </summary>
    /// <remarks>
    /// <c>$88:E987</c> feeds the eye body's screen position, tracked angle, and HDMA
    /// object's widening word through the same <c>CalculateXrayHdmaTableInner</c> geometry
    /// used by X-ray. Layer-blending selector <c>$10</c> then windows BG3 off the subscreen,
    /// exposing COLDATA as additive color inside the cone. Replaying that final scanout
    /// result here keeps the translated actor and renderer connected without inventing a
    /// second room-specific animation.
    /// </remarks>
    public static void ApplyMorphBallEyeBeamColorMath(
        Span<Rgba32> frame,
        ISnesAddressSpace bus,
        MorphBallEyeBeamState beam,
        RoomEnemySlot eyeBody,
        MorphBallEyeEnemyState eyeState,
        ushort layer1X,
        ushort layer1Y)
    {
        ArgumentNullException.ThrowIfNull(beam);
        ArgumentNullException.ThrowIfNull(eyeBody);
        ArgumentNullException.ThrowIfNull(eyeState);
        ApplyMorphBallEyeBeamColorMath(
            frame,
            bus,
            new MorphBallEyeBeamRenderSnapshot(
                beam.Phase,
                eyeBody.XPosition,
                eyeBody.YPosition,
                eyeState.Angle,
                beam.AngularWidth,
                beam.Red,
                beam.Green,
                beam.Blue),
            layer1X,
            layer1Y);
    }

    /// <summary>
    /// Adds the room-FX BG3 plane to the already resolved BG1/BG2/OBJ gameplay raster.
    /// </summary>
    public static void ApplyRoomLayer3FxColorMath(
        Span<Rgba32> frame,
        SnesVram vram,
        SnesCgram cgram,
        RoomLayer3FxRenderSnapshot fx)
    {
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(cgram);
        if (frame.Length != Width * Height)
            throw new ArgumentException("Room FX compositor requires one complete 256x224 frame.", nameof(frame));
        LayerBlendingConfiguration expectedConfiguration = fx.Type switch
        {
            RoomFxType.Lava or RoomFxType.Acid => LayerBlendingConfiguration.LavaAcidAdditive,
            RoomFxType.Water => fx.LayerBlendConfiguration,
            RoomFxType.Rain => LayerBlendingConfiguration.Rain,
            RoomFxType.Fog => LayerBlendingConfiguration.FogAdditive,
            _ => throw new NotSupportedException(
                $"Room FX type {(ushort)fx.Type:X2} has no BG3 compositor."),
        };
        if (fx.Type == RoomFxType.Water && fx.LayerBlendConfiguration is not (
                LayerBlendingConfiguration.WaterSubtractive or
                LayerBlendingConfiguration.WaterfallSubtractive or
                LayerBlendingConfiguration.LiquidOrFogAdditive))
        {
            throw new InvalidDataException(
                $"Water FX requires a cartridge water layer-blending configuration, not " +
                $"${(ushort)fx.LayerBlendConfiguration:X2}.");
        }
        if (fx.LayerBlendConfiguration != expectedConfiguration)
        {
            throw new InvalidDataException(
                $"Room FX type {fx.Type} requires layer-blending configuration " +
                $"${(ushort)expectedConfiguration:X2}, not " +
                $"${(ushort)fx.LayerBlendConfiguration:X2}.");
        }

        // $88:B3B0 sends a negative lava/acid position to the all-blank HDMA table.
        // Many heated Norfair rooms deliberately retain type $02, its palette objects,
        // and BG2 shimmer while using $FFFF to mean that no liquid surface exists. A
        // signed -1 must therefore suppress BG3 rather than compare as a line above zero.
        if (fx.Type is RoomFxType.Water or RoomFxType.Lava or RoomFxType.Acid &&
            unchecked((short)fx.CurrentYPosition) < 0)
        {
            return;
        }

        // Rain ($0E) keeps BG1/BG2/OBJ on main and BG3 on sub; fog ($30) reverses those
        // screens. Water selects either the additive route ($18) or one of the two
        // subtractive routes ($14/$16). Compositing the already-resolved scene with the
        // nontransparent BG3 pixel reproduces that final PPU equation without flattening
        // the cartridge's animated surface into a host-authored rectangle.
        bool liquid = fx.Type is RoomFxType.Water or RoomFxType.Lava or RoomFxType.Acid;
        bool fullScreenAtmosphere = fx.Type is RoomFxType.Rain or RoomFxType.Fog;
        ushort tilemapBaseWord = fullScreenAtmosphere
            ? RoomFxRomData.Layer3.FullScreenAtmosphereTilemapBaseWord
            : RoomFxRomData.Layer3.LiquidTilemapBaseWord;
        int verticalCoordinateMask = fullScreenAtmosphere
            ? RoomFxRomData.Layer3.FullScreenAtmosphereVerticalCoordinateMask
            : RoomFxRomData.Layer3.LiquidVerticalCoordinateMask;
        int firstLiquidScrollLine = fx.WaterSurfaceScreenY -
            (SnesPpuLayout.BackgroundTileSizePixels - 1);
        for (int screenY = HudHeight; screenY < Height; screenY++)
        {
            // `$88:B3B0` leaves BG3VOFS zero above the effect and switches to the
            // surface-relative value eight scanlines before the liquid coordinate. Lava's
            // animated surface occupies that preceding tile row at VRAM $5BE0; clipping at
            // the liquid Y coordinate erased it and exposed only dotted body tile $53.
            ushort verticalScroll = liquid && screenY < firstLiquidScrollLine
                ? (ushort)0
                : fx.VerticalScroll;
            int scrolledY = unchecked(verticalScroll + screenY) & verticalCoordinateMask;
            int tileY = scrolledY >> 3;
            int pixelY = scrolledY & 7;
            int waveDisplacement = fx.Type == RoomFxType.Water &&
                screenY > fx.WaterSurfaceScreenY
                ? RoomFxRomData.Water.WaveDisplacements[
                    (screenY - fx.WaterSurfaceScreenY - 1 - fx.WaterBg3WavePhase +
                        RoomFxRomData.Water.WaveDisplacementCount) %
                    RoomFxRomData.Water.WaveDisplacementCount]
                : 0;
            for (int screenX = 0; screenX < Width; screenX++)
            {
                int scrolledX = unchecked(
                    fx.HorizontalScroll + waveDisplacement + screenX) & 0xff;
                int tileX = scrolledX >> 3;
                int pixelX = scrolledX & 7;
                SnesBgTilemapWord entry = vram.ReadWord(
                    tilemapBaseWord +
                    tileY * SnesPpuLayout.TilemapPageWidthInTiles + tileX);
                int sourceX = entry.FlipHorizontally ? 7 - pixelX : pixelX;
                int sourceY = entry.FlipVertically ? 7 - pixelY : pixelY;
                int characterByte = ((0x4000 + entry.CharacterIndex * 8) & 0x7fff) * 2;
                int planes = characterByte + sourceY * 2;
                int mask = 1 << (7 - sourceX);
                int color = ((vram.ReadByte(planes) & mask) != 0 ? 1 : 0) |
                    ((vram.ReadByte(planes + 1) & mask) != 0 ? 2 : 0);
                if (color == 0)
                    continue;

                Rgba32 overlay = cgram.GetRgba(entry.PaletteIndex * 4 + color);
                int destination = screenY * Width + screenX;
                Rgba32 source = frame[destination];
                bool subtract = fx.Type == RoomFxType.Water &&
                    fx.LayerBlendConfiguration is
                        LayerBlendingConfiguration.WaterSubtractive or
                        LayerBlendingConfiguration.WaterfallSubtractive;
                frame[destination] = subtract
                    ? new Rgba32(
                        SaturatingSubtract(source.R, overlay.R),
                        SaturatingSubtract(source.G, overlay.G),
                        SaturatingSubtract(source.B, overlay.B),
                        source.A)
                    : new Rgba32(
                        SaturatingAdd(source.R, overlay.R),
                        SaturatingAdd(source.G, overlay.G),
                        SaturatingAdd(source.B, overlay.B),
                        source.A);
            }
        }
    }

    /// <summary>
    /// Resolves water's optional per-scanline BG2 distortion from <c>$88:C5E4</c>. The
    /// ordinary compositor already accepts the resulting 192-line HDMA projection.
    /// </summary>
    public static ushort[]? BuildWaterBg2HorizontalScrolls(
        RoomLayer3FxRenderSnapshot fx,
        ushort bg2HorizontalScroll,
        ushort bg2VerticalScroll)
    {
        if (fx.Type != RoomFxType.Water || (fx.LiquidOptions & 2) == 0)
            return null;

        var result = Enumerable.Repeat(bg2HorizontalScroll, Height - HudHeight).ToArray();
        int verticalPhase = bg2VerticalScroll & 0x000f;
        ReadOnlySpan<short> wave = RoomFxRomData.Water.WaveDisplacements;
        for (int screenY = Math.Max(HudHeight, fx.WaterSurfaceScreenY + 1);
             screenY < Height;
             screenY++)
        {
            int index = (fx.WaterBg2WavePhase + verticalPhase +
                screenY - fx.WaterSurfaceScreenY - 1) &
                (RoomFxRomData.Water.WaveDisplacementCount - 1);
            result[screenY - HudHeight] = unchecked((ushort)(
                bg2HorizontalScroll + wave[index]));
        }
        return result;
    }

    /// <summary>
    /// Resolves the optional lava/acid BG2HOFS waveform from <c>$88:B53B</c>. Vertical
    /// distortion has cartridge priority when both option bits are set, matching the
    /// branch order in <c>$88:B4D5</c>.
    /// </summary>
    public static ushort[]? BuildLavaAcidBg2HorizontalScrolls(
        RoomLayer3FxRenderSnapshot fx,
        ushort bg2HorizontalScroll,
        ushort bg2VerticalScroll)
    {
        bool isLavaAcid = fx.Type is RoomFxType.Lava or RoomFxType.Acid;
        bool usesVertical =
            (fx.LiquidOptions & RoomFxRomData.LavaAcid.VerticalBg2WaveOption) != 0;
        bool usesHorizontal =
            (fx.LiquidOptions & RoomFxRomData.LavaAcid.HorizontalBg2WaveOption) != 0;
        if (!isLavaAcid || usesVertical || !usesHorizontal)
            return null;

        return BuildLavaAcidBg2Wave(
            bg2HorizontalScroll,
            bg2VerticalScroll,
            fx.LavaAcidBg2WavePhase,
            RoomFxRomData.LavaAcid.HorizontalWaveDisplacements);
    }

    /// <summary>Resolves the Norfair heat-haze BG2VOFS waveform from <c>$88:B5A9</c>.</summary>
    public static ushort[]? BuildLavaAcidBg2VerticalScrolls(
        RoomLayer3FxRenderSnapshot fx,
        ushort bg2VerticalScroll)
    {
        if (fx.Type is not (RoomFxType.Lava or RoomFxType.Acid) ||
            (fx.LiquidOptions & RoomFxRomData.LavaAcid.VerticalBg2WaveOption) == 0)
        {
            return null;
        }

        return BuildLavaAcidBg2Wave(
            bg2VerticalScroll,
            bg2VerticalScroll,
            fx.LavaAcidBg2WavePhase,
            RoomFxRomData.LavaAcid.VerticalWaveDisplacements);
    }

    private static ushort[] BuildLavaAcidBg2Wave(
        ushort baseScroll,
        ushort bg2VerticalScroll,
        int wavePhase,
        ReadOnlySpan<short> wave)
    {
        var result = new ushort[Height - HudHeight];
        int verticalPhase = bg2VerticalScroll & 0x000f;
        for (int line = 0; line < result.Length; line++)
        {
            // `$88:C0B1` is 16 one-scanline indirect entries repeated through the frame.
            // Its pointer starts at BG2VOFS's low nibble, while `$88:B53B/$B5A9` rotates
            // the sixteen resident words backward through A. HUD height is 32, exactly two
            // waveform periods, so gameplay-local and physical scanline indices coincide.
            int waveIndex = (verticalPhase + line + wavePhase) &
                (RoomFxRomData.LavaAcid.WaveDisplacementCount - 1);
            result[line] = unchecked((ushort)(baseScroll + wave[waveIndex]));
        }
        return result;
    }

    /// <summary>
    /// Applies a frame-coherent Morph Ball eye snapshot captured at the OAM-upload NMI.
    /// </summary>
    public static void ApplyMorphBallEyeBeamColorMath(
        Span<Rgba32> frame,
        ISnesAddressSpace bus,
        MorphBallEyeBeamRenderSnapshot beam,
        ushort layer1X,
        ushort layer1Y)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (frame.Length != Width * Height)
            throw new ArgumentException("Morph Ball eye compositor requires one complete 256x224 frame.", nameof(frame));
        if (beam.Phase is MorphBallEyeBeamPhase.Inactive or
            MorphBallEyeBeamPhase.PendingInitialization)
        {
            return;
        }

        // Both display-packet and immediate rendering consume the same cartridge
        // endpoint builder. Backend equality is useful coverage, but the separate
        // executed-ROM fixture is the independent oracle for these endpoints.
        ScanlineColorAddRenderLayer layer = CaptureMorphBallEyeBeam(bus, beam, layer1X, layer1Y)!;
        for (int screenY = HudHeight; screenY < Height; screenY++)
        {
            ColorAddWindow window = layer.Windows[screenY];
            int row = screenY * Width;
            for (int screenX = window.Left; screenX <= window.Right; screenX++)
            {
                Rgba32 source = frame[row + screenX];
                frame[row + screenX] = new Rgba32(
                    SaturatingAdd(source.R, window.Red),
                    SaturatingAdd(source.G, window.Green),
                    SaturatingAdd(source.B, window.Blue),
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
            byte halfWidth = bus.ReadByte(
                (int)new SnesAddress(0x88, unchecked((ushort)(shapePointer + shapeLine))));
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

    private static byte SaturatingSubtract(byte left, byte right) =>
        (byte)Math.Max(byte.MinValue, left - right);

    private static XrayDirection ReadXrayDirection(ISnesAddressSpace bus, int angle)
    {
        int wrappedAngle = SnesAngle.NormalizeTableIndex(angle).TableIndex;

        // The four cardinals are mathematically exact. At horizontal entries the ROM table
        // contains `$3C00` as a screen-sized infinity substitute, but using (±1,0) here is
        // the identical limiting ray and also preserves the dedicated zero-width line.
        if (wrappedAngle == SnesAngle.Zero.TableIndex)
            return new XrayDirection(0, -SamusXrayRomData.Window.UnitVector);
        if (wrappedAngle == SnesAngle.QuarterTurn.TableIndex)
            return new XrayDirection(SamusXrayRomData.Window.UnitVector, 0);
        if (wrappedAngle == SnesAngle.HalfTurn.TableIndex)
            return new XrayDirection(0, SamusXrayRomData.Window.UnitVector);
        if (wrappedAngle == SnesAngle.ThreeQuarterTurn.TableIndex)
            return new XrayDirection(-SamusXrayRomData.Window.UnitVector, 0);

        int tangentIndex = wrappedAngle & (SnesAngle.HalfTurn.TableIndex - 1);
        int tableAddress = SamusXrayRomData.Window.AbsoluteTangentTable + tangentIndex * 2;
        int tangent = bus.ReadByte(tableAddress) | (bus.ReadByte(tableAddress + 1) << 8);
        if (wrappedAngle < SnesAngle.QuarterTurn.TableIndex)
            return new XrayDirection(tangent, -SamusXrayRomData.Window.UnitVector);
        if (wrappedAngle < SnesAngle.HalfTurn.TableIndex)
            return new XrayDirection(tangent, SamusXrayRomData.Window.UnitVector);
        if (wrappedAngle < SnesAngle.ThreeQuarterTurn.TableIndex)
            return new XrayDirection(-tangent, SamusXrayRomData.Window.UnitVector);
        return new XrayDirection(-tangent, -SamusXrayRomData.Window.UnitVector);
    }

    internal static Rgba32 ApplyXrayOutsideHalfColor(Rgba32 source)
    {
        // This fixed-color operation applies only where half-color math is enabled;
        // window/subscreen selection is the compositor's responsibility.
        // All compositor colors originated as expanded BGR555, so reducing with `>> 3`,
        // halving the full sum BEFORE saturation, and expanding again preserves the carry.
        const int FixedComponent = XrayWindowRenderDefinitions.FixedColorComponent;
        return new Rgba32(
            ExpandFiveBit((byte)Math.Min(31, ((source.R >> 3) + FixedComponent) >> 1)),
            ExpandFiveBit((byte)Math.Min(31, ((source.G >> 3) + FixedComponent) >> 1)),
            ExpandFiveBit((byte)Math.Min(31, ((source.B >> 3) + FixedComponent) >> 1)),
            source.A);
    }

    private static Rgba32[] CreateBackdrop(SnesCgram cgram, Rgba32[]? outputBuffer = null)
    {
        ArgumentNullException.ThrowIfNull(cgram);

        // CGRAM color zero is the PPU backdrop. Starting with it (rather than transparent
        // host pixels) matches every screen location no translated layer has covered yet.
        var output = outputBuffer ?? new Rgba32[Width * Height];
        if (output.Length != Width * Height)
            throw new ArgumentException("Gameplay output must have native frame dimensions.", nameof(outputBuffer));
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
        DrawHud(output, vram, cgram);

        int pixelCount = Width * Height;
        Rgba32[] rentedObjectPixels = ArrayPool<Rgba32>.Shared.Rent(pixelCount);
        byte[] rentedObjectPriorities = ArrayPool<byte>.Shared.Rent(pixelCount);
        try
        {
            Span<Rgba32> objectPixels = rentedObjectPixels.AsSpan(0, pixelCount);
            Span<byte> objectPriorities = rentedObjectPriorities.AsSpan(0, pixelCount);
            SnesObjRenderer.RenderResolved(
                oam,
                vram,
                cgram,
                obsel,
                objectPixels,
                objectPriorities,
                Width,
                Height);
            for (int pixel = HudHeight * Width; pixel < output.Length; pixel++)
            {
                // Every resolved OBJ priority is above Mode 7 in this PPU configuration.
                if (objectPriorities[pixel] != SnesObjRenderer.TransparentPriority)
                    output[pixel] = objectPixels[pixel];
            }
        }
        finally
        {
            ArrayPool<Rgba32>.Shared.Return(rentedObjectPixels, clearArray: false);
            ArrayPool<byte>.Shared.Return(rentedObjectPriorities, clearArray: false);
        }
    }

    /// <summary>One signed 8.8 direction vector reconstructed from `$91:C9D4`.</summary>
    private readonly record struct XrayDirection(int X, int Y);
}
