using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Composes the currently modeled gameplay PPU layers into one desktop frame.</summary>
public static class SnesGameplayFrameRenderer
{
    // `$91:C9D4` is the 129-word absolute-tangent table shared by the on-screen and
    // off-screen X-ray window builders. Angles are measured clockwise in 1/256 turns,
    // with zero pointing up; every table word is |dx/dy| in 8.8 fixed point.
    private const int XrayAbsoluteTangentTableAddress = 0x91c9d4;

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
        ushort bg1CharacterBaseWord = 0,
        ushort bg2CharacterBaseWord = 0,
        byte obsel = 0x03)
    {
        Rgba32[] output = CreateBackdrop(cgram);
        // BGMODE=$09 is Mode 1 with the BG3-priority flag. Below the HUD, BG3 is disabled
        // by TM and the relevant back-to-front ladder is OBJ0, OBJ1, BG2-low, BG1-low,
        // OBJ2, BG2-high, BG1-high, OBJ3. Samus's body entries use OBJ2, so cartridge-
        // authored high-priority door frames, railings, and laboratory consoles cover her.
        // Resolve OAM ownership once. Priority planes are a BG-compositor concern only;
        // decoding all 128 sprite records independently for each of the four planes was
        // both slower and easier to get subtly wrong at cross-priority overlaps.
        ResolvedObjFrame objects = SnesObjRenderer.RenderResolved(
            oam,
            vram,
            cgram,
            obsel,
            Width,
            Height);
        CompositeOrdinaryGameplayViewport(
            output,
            vram,
            cgram,
            objects,
            bg1CharacterBaseWord,
            bg2CharacterBaseWord,
            bg1HorizontalScroll,
            bg1VerticalScroll,
            bg2HorizontalScroll,
            bg2VerticalScroll,
            bg2HorizontalScrollByLine,
            bg2VerticalScrollByLine,
            bg2TilemapWidthInTiles,
            bg2TilemapHeightInTiles);
        DrawHud(output, vram, cgram);
        return output;
    }

    private static void CompositeOrdinaryGameplayViewport(
        Span<Rgba32> output,
        SnesVram vram,
        SnesCgram cgram,
        ResolvedObjFrame objects,
        ushort bg1CharacterBaseWord,
        ushort bg2CharacterBaseWord,
        ushort bg1HorizontalScroll,
        ushort bg1VerticalScroll,
        ushort bg2HorizontalScroll,
        ushort bg2VerticalScroll,
        IReadOnlyList<ushort>? bg2HorizontalScrollByLine,
        IReadOnlyList<ushort>? bg2VerticalScrollByLine,
        int bg2TilemapWidthInTiles,
        int bg2TilemapHeightInTiles)
    {
        if (objects.Width != Width || objects.Height != Height ||
            objects.Pixels.Length != output.Length ||
            objects.Priorities.Length != output.Length)
        {
            throw new ArgumentException(
                "A resolved gameplay OBJ raster must contain exactly 256x224 pixels.",
                nameof(objects));
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
            bg2TilemapWidthInTiles * bg2TilemapHeightInTiles != 2048)
        {
            throw new ArgumentException(
                "BG2 gameplay tilemaps must be either 64x32 or 32x64 tiles.",
                nameof(bg2TilemapWidthInTiles));
        }

        // Resolve the complete Mode-1 ladder in a single destination scan. The previous
        // implementation walked the 256x192 viewport eight times (four BG insertions and
        // four OBJ insertions), repeating tilemap address arithmetic and moving temporary
        // RGBA planes through memory. Each candidate below receives its literal back-to-
        // front rank from BGMODE=$09; the largest opaque rank owns the final pixel.
        int bg2XMask = bg2TilemapWidthInTiles * 8 - 1;
        int bg2YMask = bg2TilemapHeightInTiles * 8 - 1;
        int bg2ScreensPerRow = bg2TilemapWidthInTiles >> 5;
        for (int screenY = HudHeight; screenY < Height; screenY++)
        {
            int bg1ScrolledY = unchecked(bg1VerticalScroll + screenY) & 0xff;
            ushort activeBg2HorizontalScroll = bg2HorizontalScrollByLine is null
                ? bg2HorizontalScroll
                : bg2HorizontalScrollByLine[screenY - HudHeight];
            ushort activeBg2VerticalScroll = bg2VerticalScrollByLine is null
                ? bg2VerticalScroll
                : bg2VerticalScrollByLine[screenY - HudHeight];
            int bg2ScrolledY = unchecked(activeBg2VerticalScroll + screenY) & bg2YMask;
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
                if (bg2TileX != previousBg2TileX)
                {
                    // BGSC's two size bits select either two horizontal 32x32 screens
                    // (ordinary rooms) or two vertical screens (Landing Site's sky).
                    // Both contain 2,048 words, but putting the screen index on the wrong
                    // axis makes the sky go black and then decode unrelated VRAM as the
                    // camera crosses the first 256-pixel boundary.
                    int bg2ScreenColumn = bg2TileX >> 5;
                    int bg2ScreenRow = bg2TileY >> 5;
                    int bg2MapWord = (
                        0x4800 +
                        (bg2ScreenRow * bg2ScreensPerRow + bg2ScreenColumn) * 0x0400 +
                        (bg2TileY & 31) * 32 +
                        (bg2TileX & 31)) & 0x7fff;
                    bg2Entry = vram.ReadWord(bg2MapWord);
                    previousBg2TileX = bg2TileX;
                }

                if (TryDecodeOrdinaryGameplayBgPixel(
                        vram,
                        cgram,
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
                if (bg1TileX != previousBg1TileX)
                {
                    int bg1MapWord = (
                        0x5000 +
                        (bg1TileX >> 5) * 0x0400 +
                        bg1TileY * 32 +
                        (bg1TileX & 31)) & 0x7fff;
                    bg1Entry = vram.ReadWord(bg1MapWord);
                    previousBg1TileX = bg1TileX;
                }

                if (TryDecodeOrdinaryGameplayBgPixel(
                        vram,
                        cgram,
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
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static bool TryDecodeOrdinaryGameplayBgPixel(
        SnesVram vram,
        SnesCgram cgram,
        SnesBgTilemapWord entry,
        ushort characterBaseWord,
        int pixelX,
        int pixelY,
        out Rgba32 color,
        out bool highPriority)
    {
        highPriority = entry.HasPriority;

        int sourceX = entry.FlipHorizontally ? 7 - pixelX : pixelX;
        int sourceY = entry.FlipVertically ? 7 - pixelY : pixelY;
        int characterByteAddress =
            ((characterBaseWord + entry.CharacterIndex * 16) & 0x7fff) * 2;
        int mask = 1 << (7 - sourceX);
        int rowAddress = characterByteAddress + sourceY * 2;
        int colorIndex = ((vram.ReadByte(rowAddress) & mask) != 0 ? 1 : 0)
                       | ((vram.ReadByte(rowAddress + 1) & mask) != 0 ? 2 : 0)
                       | ((vram.ReadByte(rowAddress + 16) & mask) != 0 ? 4 : 0)
                       | ((vram.ReadByte(rowAddress + 17) & mask) != 0 ? 8 : 0);
        if (colorIndex == 0)
        {
            color = default;
            return false;
        }

        color = cgram.GetRgba(entry.PaletteIndex * 16 + colorIndex);
        return true;
    }

    private static void DrawHud(Span<Rgba32> output, SnesVram vram, SnesCgram cgram)
    {
        Rgba32[] hud = SnesBgTilemapRenderer.Render2Bpp(
            vram,
            cgram,
            tilemapBaseWord: 0x5800,
            characterBaseWord: 0x4000,
            rowCount: 4);
        hud.CopyTo(output);
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

        // IRQ command four gives the upper 32 physical scanlines exclusively to BG3.
        // Mode 7 continues to use physical screen coordinates below the split; cropping a
        // separately rendered 192-line image would incorrectly restart its Y coordinate.
        for (int pixel = HudHeight * Width; pixel < output.Length; pixel++)
        {
            if (mode7[pixel].A != 0)
                output[pixel] = mode7[pixel];
        }

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
                    bg2Entry = vram.ReadWord(mapWord);
                    previousTileX = tileX;
                }

                if (TryDecodeOrdinaryGameplayBgPixel(
                        vram,
                        cgram,
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
    public static void ApplyCeresHaze(Span<Rgba32> frame, bool ridleyIsDead)
    {
        if (frame.Length != Width * Height)
            throw new ArgumentException("Ceres haze requires one complete 256x224 frame.", nameof(frame));

        for (int screenY = HudHeight; screenY < Height; screenY++)
        {
            int component = screenY < 64
                ? 1
                : Math.Min(16, 2 + (screenY - 64) / 8);
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
        byte movementType = samus.ReadMovementType(bus);

        // `$88:88B8-$88F3` anchors the ray three pixels in front of Samus. Standing and
        // turning bodies place it sixteen pixels above the center; only stable movement
        // type five uses the crouching twelve-pixel offset.
        int originX = unchecked((short)(samus.XPosition - layer1X)) + (facingLeft ? -3 : 3);
        int originY = unchecked((short)(samus.YPosition - layer1Y)) - (movementType == 5 ? 12 : 16);
        int centerAngle = xray.Angle & 0x00ff;
        int angularWidth = xray.AngularWidth & 0x00ff;
        int leftEdgeAngle = (centerAngle - angularWidth) & 0x00ff;
        int rightEdgeAngle = (centerAngle + angularWidth) & 0x00ff;

        // State zero calculates a genuinely zero-width horizontal table before changing
        // state to one. `$91:C901` special-cases exactly right/left so it produces a single
        // horizontal scanline instead of using the finite `$3C00` tangent-table sentinel.
        bool horizontalLine = angularWidth == 0 && centerAngle is 0x40 or 0xc0;
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
                        (centerAngle == 0x40 ? fromOriginX >= 0 : fromOriginX <= 0);
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
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(beam);
        ArgumentNullException.ThrowIfNull(eyeBody);
        ArgumentNullException.ThrowIfNull(eyeState);
        if (frame.Length != Width * Height)
            throw new ArgumentException("Morph Ball eye compositor requires one complete 256x224 frame.", nameof(frame));
        if (beam.Phase is MorphBallEyeBeamPhase.Inactive or
            MorphBallEyeBeamPhase.PendingInitialization)
        {
            return;
        }

        int originX = unchecked((short)(eyeBody.XPosition - layer1X));
        int originY = unchecked((short)(eyeBody.YPosition - layer1Y));
        int centerAngle = eyeState.Angle & 0x00ff;
        int angularWidth = beam.AngularWidth & 0x00ff;
        XrayDirection leftEdge = ReadXrayDirection(bus, centerAngle - angularWidth);
        XrayDirection rightEdge = ReadXrayDirection(bus, centerAngle + angularWidth);
        bool horizontalLine = angularWidth == 0 && centerAngle is 0x40 or 0xc0;

        byte addRed = ExpandFiveBit((byte)(beam.Red & 0x1f));
        byte addGreen = ExpandFiveBit((byte)(beam.Green & 0x1f));
        byte addBlue = ExpandFiveBit((byte)(beam.Blue & 0x1f));
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
                        (centerAngle == 0x40 ? fromOriginX >= 0 : fromOriginX <= 0);
                }
                else
                {
                    long leftCross = (long)leftEdge.X * fromOriginY -
                        (long)leftEdge.Y * fromOriginX;
                    long rightCross = (long)rightEdge.X * fromOriginY -
                        (long)rightEdge.Y * fromOriginX;
                    inside = leftCross >= -0x00ff && rightCross <= 0x00ff;
                }

                if (!inside)
                    continue;
                Rgba32 source = frame[row + screenX];
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

    private static XrayDirection ReadXrayDirection(ISnesAddressSpace bus, int angle)
    {
        int wrappedAngle = angle & 0x00ff;

        // The four cardinals are mathematically exact. At horizontal entries the ROM table
        // contains `$3C00` as a screen-sized infinity substitute, but using (±1,0) here is
        // the identical limiting ray and also preserves the dedicated zero-width line.
        if (wrappedAngle == 0x00)
            return new XrayDirection(0, -0x0100);
        if (wrappedAngle == 0x40)
            return new XrayDirection(0x0100, 0);
        if (wrappedAngle == 0x80)
            return new XrayDirection(0, 0x0100);
        if (wrappedAngle == 0xc0)
            return new XrayDirection(-0x0100, 0);

        int tangentIndex = wrappedAngle & 0x007f;
        int tableAddress = XrayAbsoluteTangentTableAddress + tangentIndex * 2;
        int tangent = bus.ReadByte(tableAddress) | (bus.ReadByte(tableAddress + 1) << 8);
        return wrappedAngle switch
        {
            < 0x40 => new XrayDirection(tangent, -0x0100),
            < 0x80 => new XrayDirection(tangent, 0x0100),
            < 0xc0 => new XrayDirection(-tangent, 0x0100),
            _ => new XrayDirection(-tangent, -0x0100),
        };
    }

    private static Rgba32 ApplyXrayOutsideHalfColor(Rgba32 source)
    {
        // `$88:8709-$8716` loads COLDATA component seven when the room has revealable
        // blocks, and `$88:817B/$81A4` selects addition followed by SNES half-color math.
        // All compositor colors originated as expanded BGR555, so reducing with `>> 3`,
        // saturating in five-bit space, halving, and expanding again is lossless here.
        const int FixedComponent = 7;
        return new Rgba32(
            ExpandFiveBit((byte)(Math.Min(31, (source.R >> 3) + FixedComponent) >> 1)),
            ExpandFiveBit((byte)(Math.Min(31, (source.G >> 3) + FixedComponent) >> 1)),
            ExpandFiveBit((byte)(Math.Min(31, (source.B >> 3) + FixedComponent) >> 1)),
            source.A);
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

    /// <summary>One signed 8.8 direction vector reconstructed from `$91:C9D4`.</summary>
    private readonly record struct XrayDirection(int X, int Y);
}
