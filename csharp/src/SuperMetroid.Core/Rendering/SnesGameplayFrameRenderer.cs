using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Rendering;

/// <summary>Composes the currently modeled gameplay PPU layers into one desktop frame.</summary>
[PpuLayoutBoundary("backend: software PPU gameplay frame compositor")]
public static class SnesGameplayFrameRenderer
{
    /// <summary>Visible SNES viewport width in pixels.</summary>
    public const int Width = SnesPpuLayout.ScreenWidthPixels;
    /// <summary>Visible SNES viewport height in physical scanlines.</summary>
    public const int Height = SnesPpuLayout.ScreenHeightPixels;
    /// <summary>Height in pixels of the fixed gameplay HUD above the room viewport.</summary>
    public const int HudHeight = SnesPpuLayout.GameplayHudHeightPixels;

    /// <summary>
    /// Composes the ordinary gameplay PPU layout used by Ceres and most interior rooms,
    /// including the priority bit of every BG tile and all four OBJ priority groups.
    /// </summary>
    public static Rgba32[] RenderHudOrdinaryBackgroundsAndObjs(
        SnesVram vram,
        SnesCgram cgram,
        OamBuffer oam,
        OrdinaryGameplayRenderLayer layer,
        byte obsel = 0x03,
        Rgba32[]? outputBuffer = null)
    {
        Ensure.NotNull(layer);
        Rgba32[] output = CreateBackdrop(cgram, outputBuffer);
        OrdinaryGameplayRegisters registers = layer.Registers;
        Span<SnesMainScreenLayers> windowMasks = stackalloc SnesMainScreenLayers[Width];
        if (registers.MainScreenWindowMask == SnesMainScreenLayers.None)
            windowMasks.Clear();
        else
            for (int x = 0; x < Width; x++)
                windowMasks[x] = registers.Windows.MaskedLayers((byte)x, registers.MainScreenWindowMask);
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
            SnesObjRenderer.RenderResolved(oam, vram, cgram, obsel, new ObjRasterTargets(objectPixels, objectPriorities));
            CompositeOrdinaryGameplayViewport(output, vram, cgram, objectPixels, objectPriorities, layer, windowMasks);
        }
        finally
        {
            // Every opaque object pixel is overwritten before use and transparency is
            // governed by the parallel priority sentinel, so clearing these private render
            // buffers during return would spend frame time without changing observability.
            ArrayPool<Rgba32>.Shared.Return(rentedObjectPixels, clearArray: false);
            ArrayPool<byte>.Shared.Return(rentedObjectPriorities, clearArray: false);
        }
        DrawHud(output, vram, cgram, registers.HudCharacterWord);
        if ((registers.MainScreenWindowMask & SnesMainScreenLayers.Bg3) != 0)
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

    /// <summary>Resolves the Mode-1 BG and OBJ priority ladder in one viewport scan, with tilemap and palette values cached for this frame.</summary>
    /// <remarks>This pure PPU projection runs roughly 50,000 times per frame; optimized Debug JIT code keeps it inside the video-frame budget.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void CompositeOrdinaryGameplayViewport(
        Span<Rgba32> output,
        SnesVram vram,
        SnesCgram cgram,
        ReadOnlySpan<Rgba32> objectPixels,
        ReadOnlySpan<byte> objectPriorities,
        OrdinaryGameplayRenderLayer layer,
        ReadOnlySpan<SnesMainScreenLayers> windowMasks)
    {
        if (objectPixels.Length != output.Length ||
            objectPriorities.Length != output.Length)
        {
            throw new ArgumentException(
                "A resolved gameplay OBJ raster must contain exactly 256x224 pixels.",
                nameof(objectPixels));
        }

        // The layer validated its BG2 map size and every per-line table at construction.
        OrdinaryGameplayRegisters registers = layer.Registers;
        Position16<RoomPixels> bg1Scroll = registers.Bg1Scroll;
        Position16<Bg2Pixels> bg2Scroll = registers.Bg2Scroll;
        ReadOnlySpan<ushort> bg2HorizontalScrollByLine = layer.Bg2LineScrolls.Horizontal;
        ReadOnlySpan<ushort> bg2VerticalScrollByLine = layer.Bg2LineScrolls.Vertical;
        Extent16<TilemapCells> bg2TilemapSize = registers.Bg2MapSize;
        ushort bg1CharacterBaseWord = registers.Bg1CharacterWord;
        ushort bg2CharacterBaseWord = registers.Bg2CharacterWord;
        ushort bg2TilemapBaseWord = registers.Bg2TilemapWord;
        SnesMainScreenLayers mainScreenLayers = registers.MainScreenLayers;
        int bg2FirstScanline = registers.Bg2FirstScanline;
        int bg2EndScanline = registers.Bg2EndScanline;
        BackgroundMosaicSampling bg2Mosaic = registers.Bg2Mosaic;
        ReadOnlySpan<ushort> mainScreenLayersByLine = layer.MainScreenLayersByLine;

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

        var bg1Geometry = new BgTilemapGeometry(GameplayVramLayout.RoomTilemapSize);
        var bg2Geometry = new BgTilemapGeometry(bg2TilemapSize);
        for (int screenY = HudHeight; screenY < Height; screenY++)
        {
            SnesMainScreenLayers activeLayers = mainScreenLayersByLine.IsEmpty ? mainScreenLayers
                : (SnesMainScreenLayers)mainScreenLayersByLine[screenY - HudHeight];
            bool bg1Enabled = (activeLayers & SnesMainScreenLayers.Bg1) != 0;
            bool bg2LayerEnabled = (activeLayers & SnesMainScreenLayers.Bg2) != 0;
            bool objEnabled = (activeLayers & SnesMainScreenLayers.Obj) != 0;
            bool bg2Enabled = bg2LayerEnabled && screenY >= bg2FirstScanline && screenY < bg2EndScanline;
            // Output rows are zero-based, while Mode-1 BG sampling starts on
            // physical scanline one. OBJ's preceding-line evaluation is separate.
            int physicalBackgroundY = screenY + SnesPpuLayout.FirstVisibleBackgroundScanline;
            int bg1ScrolledY = bg1Geometry.WrapY(bg1Scroll.Y + physicalBackgroundY);
            ushort activeBg2HorizontalScroll = bg2HorizontalScrollByLine.IsEmpty
                ? bg2Scroll.X
                : bg2HorizontalScrollByLine[screenY - HudHeight];
            ushort activeBg2VerticalScroll = bg2VerticalScrollByLine.IsEmpty
                ? bg2Scroll.Y
                : bg2VerticalScrollByLine[screenY - HudHeight];
            int bg2ScrolledY = bg2Geometry.WrapY(bg2Mosaic.SourceY(screenY, activeBg2VerticalScroll));
            int bg1TileY = BgCharacterGrid.CharacterOf(bg1ScrolledY);
            int bg2TileY = BgCharacterGrid.CharacterOf(bg2ScrolledY);
            int bg1PixelY = BgCharacterGrid.PixelWithinCharacter(bg1ScrolledY);
            int bg2PixelY = BgCharacterGrid.PixelWithinCharacter(bg2ScrolledY);
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
                int bg2ScrolledX = bg2Geometry.WrapX(bg2Mosaic.SourceX(screenX, activeBg2HorizontalScroll));
                int bg2TileX = BgCharacterGrid.CharacterOf(bg2ScrolledX);
                if (bg2Enabled && bg2TileX != previousBg2TileX)
                {
                    // BGSC's two size bits select either two horizontal 32x32 screens
                    // (ordinary rooms) or two vertical screens (Landing Site's sky).
                    // Both contain 2,048 words, but putting the screen index on the wrong
                    // axis makes the sky go black and then decode unrelated VRAM as the
                    // camera crosses the first 256-pixel boundary.
                    bg2Entry = SnesVramAddressing.ReadWord(vramBytes, bg2Geometry.EntryWordAddress(
                        bg2TilemapBaseWord, new Position16<TilemapCells>((ushort)bg2TileX, (ushort)bg2TileY)));
                    previousBg2TileX = bg2TileX;
                }

                if (bg2Enabled && (windowMasks[screenX] & SnesMainScreenLayers.Bg2) == 0 &&
                    TryDecodeOrdinaryGameplayBgPixel(
                        vramBytes,
                        palette,
                        bg2Entry,
                        bg2CharacterBaseWord,
                        new Position16<CharacterPixels>((ushort)BgCharacterGrid.PixelWithinCharacter(bg2ScrolledX), (ushort)bg2PixelY),
                        out Rgba32 bg2Color,
                        out bool bg2High))
                {
                    winner = bg2Color;
                    winnerRank = bg2High ? 6 : 3;
                }

                int bg1ScrolledX = bg1Geometry.WrapX(bg1Scroll.X + screenX);
                int bg1TileX = BgCharacterGrid.CharacterOf(bg1ScrolledX);
                if (bg1Enabled && bg1TileX != previousBg1TileX)
                {
                    bg1Entry = SnesVramAddressing.ReadWord(vramBytes, bg1Geometry.EntryWordAddress(
                        GameplayVramLayout.Bg1TilemapWord, new Position16<TilemapCells>((ushort)bg1TileX, (ushort)bg1TileY)));
                    previousBg1TileX = bg1TileX;
                }

                if (bg1Enabled && (windowMasks[screenX] & SnesMainScreenLayers.Bg1) == 0 &&
                    TryDecodeOrdinaryGameplayBgPixel(
                        vramBytes,
                        palette,
                        bg1Entry,
                        bg1CharacterBaseWord,
                        new Position16<CharacterPixels>((ushort)BgCharacterGrid.PixelWithinCharacter(bg1ScrolledX), (ushort)bg1PixelY),
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
                    {
                        winner = objectPixels[destination];
                        winnerRank = objRank;
                    }
                }


                output[destination] = winner;
            }
        }
    }

    /// <summary>Decodes a four-bit BG tile pixel, reporting transparency and the tile's priority bit alongside its CGRAM color.</summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static bool TryDecodeOrdinaryGameplayBgPixel(
        ReadOnlySpan<byte> vram,
        ReadOnlySpan<Rgba32> palette,
        SnesBgTilemapWord entry,
        ushort characterBaseWord,
        Position16<CharacterPixels> pixel,
        out Rgba32 color,
        out bool highPriority)
    {
        highPriority = entry.HasPriority;

        BgCharacterFormat format = BgCharacterFormat.FourBitsPerPixel;
        int row = format.RowByteOffset(characterBaseWord, entry.CharacterIndex,
            BgCharacterGrid.SourcePixel(pixel.Y, entry.FlipVertically));
        int colorIndex = format.ColorIndexAt(vram, row, BgCharacterGrid.SourcePixel(pixel.X, entry.FlipHorizontally));
        if (colorIndex == BgCharacterFormat.TransparentColorIndex)
        {
            color = default;
            return false;
        }

        color = palette[format.CgramColor(entry.PaletteIndex, colorIndex)];
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

        ReadOnlySpan<Bgr555> source = cgram.Colors;
        for (int color = 0; color < destination.Length; color++)
            destination[color] = source[color].ToRgba32();
    }

    /// <summary>Renders the four BG3 HUD rows from the native HUD tilemap and selected character base.</summary>
    private static void DrawHud(
        Span<Rgba32> output,
        SnesVram vram,
        SnesCgram cgram,
        ushort characterBaseWord = GameplayVramLayout.HudCharacterWord)
    {
        SnesBgTilemapRenderer.Render2Bpp(output[..(HudHeight * Width)], vram, cgram, new BgVramAddresses { TilemapBaseWord = GameplayVramLayout.HudTilemapWord, CharacterBaseWord = characterBaseWord }, rowCount: 4);
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
    public static Rgba32[] RenderHudMode7AndObjs(SnesVram vram, SnesCgram cgram, OamBuffer oam, Mode7RenderRegisters registers, byte obsel = 0x03, Rgba32[]? outputBuffer = null)
    {
        Rgba32[] output = CreateBackdrop(cgram, outputBuffer);
        SnesMode7Renderer.CompositeViewport(output, vram, cgram, new Mode7RenderRegisters { MatrixA = registers.MatrixA, MatrixB = registers.MatrixB, MatrixC = registers.MatrixC, MatrixD = registers.MatrixD, Center = registers.Center, Scroll = registers.Scroll });

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
    public static Rgba32[] RenderHudCeresRidleyGetawayAndObjs(SnesVram vram, SnesCgram cgram, OamBuffer oam, Mode7RenderRegisters registers, CeresGetawayFloor floor, byte obsel = 0x03, Rgba32[]? outputBuffer = null)
    {
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        Ensure.NotNull(oam);

        const int floorHeight = 16;
        const int floorFirstScanline = Height - floorHeight;
        Rgba32[] output = CreateBackdrop(cgram, outputBuffer);
        using var scratch = new RenderScratch();
        Rgba32[] mode7 = scratch.Colors(Width * Height);
        SnesMode7Renderer.CompositeViewport(mode7, vram, cgram, new Mode7RenderRegisters { MatrixA = registers.MatrixA, MatrixB = registers.MatrixB, MatrixC = registers.MatrixC, MatrixD = registers.MatrixD, Center = registers.Center, Scroll = registers.Scroll });
        ResolvedObjFrame objects = scratch.ResolveObjects(oam, vram, cgram, obsel);

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
        var floorGeometry = new BgTilemapGeometry(GameplayVramLayout.RoomTilemapSize);
        for (int screenY = floorFirstScanline; screenY < Height; screenY++)
        {
            int scrolledY = floorGeometry.WrapY(floor.Bg2Scroll.Y + screenY);
            int tileY = BgCharacterGrid.CharacterOf(scrolledY);
            int pixelY = BgCharacterGrid.PixelWithinCharacter(scrolledY);
            int previousTileX = -1;
            SnesBgTilemapWord bg2Entry = default;

            for (int screenX = 0; screenX < Width; screenX++)
            {
                int destination = screenY * Width + screenX;
                Rgba32 winner = output[destination];
                int winnerRank = 0;
                int scrolledX = floorGeometry.WrapX(floor.Bg2Scroll.X + screenX);
                int tileX = BgCharacterGrid.CharacterOf(scrolledX);
                if (tileX != previousTileX)
                {
                    bg2Entry = SnesVramAddressing.ReadWord(vramBytes, floorGeometry.EntryWordAddress(
                        GameplayVramLayout.Bg2TilemapWord, new Position16<TilemapCells>((ushort)tileX, (ushort)tileY)));
                    previousTileX = tileX;
                }

                if (TryDecodeOrdinaryGameplayBgPixel(
                        vramBytes,
                        palette,
                        bg2Entry,
                        floor.Bg2CharacterBaseWord,
                        new Position16<CharacterPixels>((ushort)BgCharacterGrid.PixelWithinCharacter(scrolledX), (ushort)pixelY),
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
    public static void ApplyCeresHaze(Span<Rgba32> frame, bool ridleyIsDead,
        int intensity = CeresHazeRenderDefinitions.MaximumComponent,
        RoomFxPaletteBlendCatalog? colors = null)
    {
        Ensure.LengthEqual(frame, Width * Height);

        for (int screenY = HudHeight; screenY < Height; screenY++)
        {
            (byte red, byte green, byte blue) = CeresHazeRenderDefinitions.ResolveComponents(
                screenY, intensity, ridleyIsDead, colors);
            byte addRed = ExpandFiveBit(red);
            byte addGreen = ExpandFiveBit(green);
            byte addBlue = ExpandFiveBit(blue);
            int row = screenY * Width;
            for (int screenX = 0; screenX < Width; screenX++)
            {
                int pixel = row + screenX;
                Rgba32 source = frame[pixel];
                frame[pixel] = new Rgba32(SaturatingAdd(source.R, addRed),
                    SaturatingAdd(source.G, addGreen), SaturatingAdd(source.B, addBlue), source.A);
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
    /// against the compiled native horizontal and vertical curve bytes; this
    /// deliberately preserves the SNES routine's 8x8 multiply truncation and its
    /// one-scanline overlap between adjacent curve bands.
    /// </remarks>
    public static void ApplyPowerBombColorMath(
        Span<Rgba32> frame,
        ISnesAddressSpace bus,
        SamusPowerBombExplosionState explosion,
        Position16<RoomPixels> layer1)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(explosion);
        Ensure.LengthEqual(frame, Width * Height);
        if (!explosion.IsActive)
            return;

        Offset16<ScreenPixels> center = explosion.Position
            .RelativeToCamera(layer1);
        int centerX = center.X;
        int centerY = center.Y;
        byte addRed = ExpandFiveBit(explosion.FixedColorRed);
        byte addGreen = ExpandFiveBit(explosion.FixedColorGreen);
        byte addBlue = ExpandFiveBit(explosion.FixedColorBlue);

        for (int screenY = HudHeight; screenY < Height; screenY++)
        {
            int halfWidth = ReadPowerBombHalfWidth(explosion, screenY - centerY);
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
    /// Adds the room-FX BG3 plane to the already resolved BG1/BG2/OBJ gameplay raster.
    /// </summary>
    public static void ApplyRoomLayer3FxColorMath(
        Span<Rgba32> frame,
        SnesVram vram,
        SnesCgram cgram,
        RoomLayer3FxRenderSnapshot fx)
    {
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        Ensure.LengthEqual(frame, Width * Height);
        LayerBlendingConfiguration expectedConfiguration = fx.Type switch
        {
            RoomFxType.Lava or RoomFxType.Acid => LayerBlendingConfiguration.LavaAcidAdditive,
            RoomFxType.Water or RoomFxType.TourianEntranceStatue => fx.LayerBlendConfiguration,
            RoomFxType.Rain => LayerBlendingConfiguration.Rain,
            RoomFxType.Fog => LayerBlendingConfiguration.FogAdditive,
            _ => throw new NotSupportedException(
                $"Room FX type {(ushort)fx.Type:X2} has no BG3 compositor."),
        };
        if (RoomFxTypes.UsesWater(fx.Type) && fx.LayerBlendConfiguration is not (
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
        if (fx.Type is RoomFxType.Water or RoomFxType.TourianEntranceStatue or RoomFxType.Lava or RoomFxType.Acid &&
            unchecked((short)fx.CurrentYPosition) < 0)
        {
            return;
        }

        // Rain ($0E) keeps BG1/BG2/OBJ on main and BG3 on sub; fog ($30) reverses those
        // screens. Water selects either the additive route ($18) or one of the two
        // subtractive routes ($14/$16). Compositing the already-resolved scene with the
        // nontransparent BG3 pixel reproduces that final PPU equation without flattening
        // the cartridge's animated surface into a host-authored rectangle.
        bool liquid = fx.Type is RoomFxType.Water or RoomFxType.TourianEntranceStatue or RoomFxType.Lava or RoomFxType.Acid;
        bool fullScreenAtmosphere = fx.Type is RoomFxType.Rain or RoomFxType.Fog;
        ushort tilemapBaseWord = fullScreenAtmosphere
            ? RoomFxRomData.Layer3.FullScreenAtmosphereTilemapBaseWord
            : RoomFxRomData.Layer3.LiquidTilemapBaseWord;
        var geometry = new BgTilemapGeometry(fullScreenAtmosphere
            ? RoomFxRomData.Layer3.FullScreenAtmosphereTilemapSize
            : RoomFxRomData.Layer3.LiquidTilemapSize);
        BgCharacterFormat format = BgCharacterFormat.TwoBitsPerPixel;
        ReadOnlySpan<byte> fxVram = vram.Bytes;
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
                : fx.Scroll.Y;
            int scrolledY = geometry.WrapY(verticalScroll + screenY);
            int tileY = BgCharacterGrid.CharacterOf(scrolledY);
            int pixelY = BgCharacterGrid.PixelWithinCharacter(scrolledY);
            int waveDisplacement = RoomFxTypes.UsesWater(fx.Type) &&
                screenY > fx.WaterSurfaceScreenY
                ? RoomFxRomData.Water.WaveDisplacement(
                    (screenY - fx.WaterSurfaceScreenY - 1 - fx.WaterBg3WavePhase +
                        RoomFxRomData.Water.WaveDisplacementCount) %
                    RoomFxRomData.Water.WaveDisplacementCount)
                : 0;
            for (int screenX = 0; screenX < Width; screenX++)
            {
                int scrolledX = geometry.WrapX(fx.Scroll.X + waveDisplacement + screenX);
                SnesBgTilemapWord entry = vram.ReadWord(geometry.EntryWordAddress(tilemapBaseWord,
                    new Position16<TilemapCells>((ushort)BgCharacterGrid.CharacterOf(scrolledX), (ushort)tileY)));
                int row = format.RowByteOffset(GameplayVramLayout.HudCharacterWord, entry.CharacterIndex,
                    BgCharacterGrid.SourcePixel(pixelY, entry.FlipVertically));
                int color = format.ColorIndexAt(fxVram, row,
                    BgCharacterGrid.SourcePixel(BgCharacterGrid.PixelWithinCharacter(scrolledX), entry.FlipHorizontally));
                if (color == BgCharacterFormat.TransparentColorIndex)
                    continue;

                Rgba32 overlay = cgram.GetRgba(format.CgramColor(entry.PaletteIndex, color));
                int destination = screenY * Width + screenX;
                Rgba32 source = frame[destination];
                bool subtract = RoomFxTypes.UsesWater(fx.Type) &&
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
        Position16<Bg2Pixels> bg2Scroll)
    {
        if (fx.Type != RoomFxType.Water || (fx.LiquidOptions & 2) == 0)
            return null;

        var result = Enumerable.Repeat(bg2Scroll.X, Height - HudHeight).ToArray();
        int verticalPhase = bg2Scroll.Y & 0x000f;
        for (int screenY = Math.Max(HudHeight, fx.WaterSurfaceScreenY + 1);
             screenY < Height;
             screenY++)
        {
            int index = (fx.WaterBg2WavePhase + verticalPhase +
                screenY - fx.WaterSurfaceScreenY - 1) &
                (RoomFxRomData.Water.WaveDisplacementCount - 1);
            result[screenY - HudHeight] = unchecked((ushort)(
                bg2Scroll.X + RoomFxRomData.Water.WaveDisplacement(index)));
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
        Position16<Bg2Pixels> bg2Scroll)
    {
        bool isLavaAcid = fx.Type is RoomFxType.Lava or RoomFxType.Acid;
        bool usesVertical =
            (fx.LiquidOptions & RoomFxRomData.LavaAcid.VerticalBg2WaveOption) != 0;
        bool usesHorizontal =
            (fx.LiquidOptions & RoomFxRomData.LavaAcid.HorizontalBg2WaveOption) != 0;
        if (!isLavaAcid || usesVertical || !usesHorizontal)
            return null;

        return BuildLavaAcidBg2Wave(
            bg2Scroll.X,
            bg2Scroll.Y,
            fx.LavaAcidBg2WavePhase,
            verticalWave: false);
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
            verticalWave: true);
    }

    /// <summary>Expands the rotating sixteen-entry heat-haze wave into one BG2 scroll word per gameplay scanline.</summary>
    private static ushort[] BuildLavaAcidBg2Wave(
        ushort baseScroll,
        ushort bg2VerticalScroll,
        int wavePhase,
        bool verticalWave)
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
            short displacement = verticalWave
                ? RoomFxRomData.LavaAcid.VerticalWaveDisplacement(waveIndex)
                : RoomFxRomData.LavaAcid.HorizontalWaveDisplacement(waveIndex);
            result[line] = unchecked((ushort)(baseScroll + displacement));
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
        Position16<RoomPixels> layer1)
    {
        Ensure.NotNull(bus);
        Ensure.LengthEqual(frame, Width * Height);
        if (beam.Phase is MorphBallEyeBeamPhase.Inactive or
            MorphBallEyeBeamPhase.PendingInitialization)
        {
            return;
        }

        // Both display-packet and immediate rendering consume the same cartridge
        // endpoint builder. Backend equality is useful coverage, but the separate
        // executed-ROM fixture is the independent oracle for these endpoints.
        ScanlineColorAddRenderLayer layer = CaptureMorphBallEyeBeam(bus, beam, layer1) ??
            throw new InvalidOperationException("An active morph-ball eye beam always captures its windows.");
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

    /// <summary>Samples the active power-bomb window's half-width at a signed distance from its center, or minus one outside the flash.</summary>
    internal static int ReadPowerBombHalfWidth(
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

        ushort shapePointer = explosion.RenderedShapeDefinitionPointer;
        if ((renderedPhase is PowerBombExplosionPhase.PreExplosionYellow or
             PowerBombExplosionPhase.ExplosionWhite or PowerBombExplosionPhase.Afterglow) && shapePointer != 0)
        {
            // $88:8B98 only fades COLDATA: the final white window remains installed.
            // It can still leave visible corners outside the oval near screen edges.
            // Each 192-byte record is a half-profile, not a 192-line top-to-bottom
            // bitmap. Byte zero is the widest center line; increasing indices travel
            // away from the center until the first zero terminates the native copier.
            int shapeLine = Math.Abs(yFromCenter);
            if ((uint)shapeLine >= 192)
                return -1;
            byte halfWidth = PowerBombShapeDefinitions.ReadPreScaledHalfWidth(
                shapePointer, shapeLine);
            return halfWidth == 0 ? -1 : halfWidth;
        }

        int horizontalRadius = renderedPhase == PowerBombExplosionPhase.PreExplosionWhite
            ? SnesLittleEndian.HighByte(explosion.RenderedPreExplosionRadius)
            : SnesLittleEndian.HighByte(explosion.RenderedExplosionRadius);
        int scanlineDistance = Math.Abs(yFromCenter);
        if (horizontalRadius == 0 || scanlineDistance >= 192)
            return -1;

        // `$88:8CC6/8D04/8D46` all build the same width profile and differ only in how
        // they clip left/right endpoints for an off-screen origin. The desktop renderer
        // performs that clipping after this method, so only the common profile builder
        // is needed here. Widths increase while vertical boundaries decrease.
        int currentOuterScanline =
            SnesEightEight.Multiply(horizontalRadius, PowerBombShapeDefinitions.TopOffset(0));
        if (scanlineDistance > currentOuterScanline)
            return -1;

        int selectedHalfWidth = -1;
        int finalHalfWidth = 0;
        for (int curveIndex = 0; curveIndex < 32; curveIndex++)
        {
            // The 65816 routine uses the high byte of an unsigned 8x8 product. An
            // ordinary integer multiply followed by `>> 8` is exactly that operation.
            int innerScanline = SnesEightEight.Multiply(horizontalRadius, PowerBombShapeDefinitions.TopOffset(curveIndex));
            int halfWidth = SnesEightEight.Multiply(horizontalRadius, PowerBombShapeDefinitions.Width(curveIndex));
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

    /// <summary>Replicates low color bits to expand one SNES five-bit channel to host eight-bit color.</summary>
    private static byte ExpandFiveBit(byte value) =>
        (byte)(((value & 0x1f) << 3) | ((value & 0x1f) >> 2));

    /// <summary>Adds two expanded color channels with a white ceiling.</summary>
    private static byte SaturatingAdd(byte left, byte right) =>
        (byte)Math.Min(byte.MaxValue, left + right);

    /// <summary>Subtracts one expanded color channel without wrapping below black.</summary>
    private static byte SaturatingSubtract(byte left, byte right) =>
        (byte)Math.Max(byte.MinValue, left - right);

    /// <summary>One signed 8.8 screen-space direction vector reconstructed from `$91:C9D4`.</summary>
    internal static Offset16<ScreenPixels> ReadXrayDirection(int angle)
    {
        int wrappedAngle = SnesAngle.NormalizeTableIndex(angle).TableIndex;

        // The four cardinals are mathematically exact. At horizontal entries the ROM table
        // contains `$3C00` as a screen-sized infinity substitute, but using (±1,0) here is
        // the identical limiting ray and also preserves the dedicated zero-width line.
        if (wrappedAngle == SnesAngle.Zero.TableIndex)
            return Direction(0, -SamusXrayRomData.Window.UnitVector);
        if (wrappedAngle == SnesAngle.QuarterTurn.TableIndex)
            return Direction(SamusXrayRomData.Window.UnitVector, 0);
        if (wrappedAngle == SnesAngle.HalfTurn.TableIndex)
            return Direction(0, SamusXrayRomData.Window.UnitVector);
        if (wrappedAngle == SnesAngle.ThreeQuarterTurn.TableIndex)
            return Direction(-SamusXrayRomData.Window.UnitVector, 0);

        int tangentIndex = wrappedAngle & (SnesAngle.HalfTurn.TableIndex - 1);
        int tangent = AbsoluteTangentDefinitions.Sample(tangentIndex);
        if (wrappedAngle < SnesAngle.QuarterTurn.TableIndex)
            return Direction(tangent, -SamusXrayRomData.Window.UnitVector);
        if (wrappedAngle < SnesAngle.HalfTurn.TableIndex)
            return Direction(tangent, SamusXrayRomData.Window.UnitVector);
        if (wrappedAngle < SnesAngle.ThreeQuarterTurn.TableIndex)
            return Direction(-tangent, SamusXrayRomData.Window.UnitVector);
        return Direction(-tangent, -SamusXrayRomData.Window.UnitVector);

        // One screen-space ray direction; the table's largest tangent ($3C00) fits a signed word.
        static Offset16<ScreenPixels> Direction(int x, int y) => new(checked((short)x), checked((short)y));
    }

    /// <summary>Fills a correctly sized gameplay frame with CGRAM color zero before any visible PPU layer is composed.</summary>
    private static Rgba32[] CreateBackdrop(SnesCgram cgram, Rgba32[]? outputBuffer = null)
    {
        Ensure.NotNull(cgram);

        // CGRAM color zero is the PPU backdrop. Starting with it (rather than transparent
        // host pixels) matches every screen location no translated layer has covered yet.
        var output = outputBuffer ?? new Rgba32[Width * Height];
        if (output.Length != Width * Height)
            throw new ArgumentException("Gameplay output must have native frame dimensions.", nameof(outputBuffer));
        Array.Fill(output, cgram.GetRgba(0));
        return output;
    }

    /// <summary>Applies the BG3-only HUD split and places all resolved OBJ pixels over the Mode-7 gameplay region.</summary>
    internal static void DrawHudAndObjects(
        Rgba32[] output,
        SnesVram vram,
        SnesCgram cgram,
        OamBuffer oam,
        byte obsel)
    {
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        Ensure.NotNull(oam);

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
            SnesObjRenderer.RenderResolved(oam, vram, cgram, obsel, new ObjRasterTargets(objectPixels, objectPriorities));
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

    /// <summary>Resolves the published eye cone and ROM tangent table into inclusive color windows.</summary>
    public static ScanlineColorAddRenderLayer? CaptureMorphBallEyeBeam(ISnesAddressSpace bus,
        MorphBallEyeBeamRenderSnapshot beam, Position16<RoomPixels> layer1)
    {
        Ensure.NotNull(bus);
        if (beam.Phase is MorphBallEyeBeamPhase.Inactive or MorphBallEyeBeamPhase.PendingInitialization)
            return null;
        Offset16<ScreenPixels> origin = beam.WorldPosition.RelativeToCamera(layer1);
        int angle = beam.Angle.TableIndex;
        int angularWidth = unchecked((byte)beam.AngularWidth);
        ushort[] nativeWindows = EyeBeamWindowBuilder.Build(origin, angle, angularWidth);
        var windows = new ColorAddWindow[Height];
        Array.Fill(windows, ColorAddWindow.Empty);
        byte red = ExpandFiveBit(beam.Red), green = ExpandFiveBit(beam.Green), blue = ExpandFiveBit(beam.Blue);
        for (int y = HudHeight; y < Height; y++)
        {
            int left = SnesLittleEndian.LowByte(nativeWindows[y]), right = SnesLittleEndian.HighByte(nativeWindows[y]);
            if (left <= right) windows[y] = new((byte)left, (byte)right, red, green, blue);
        }
        return new(windows);
    }

    /// <summary>
    /// Captures bank-$88 layer configuration $24's fixed-color addition through window 1,
    /// rasterizing the published beam geometry. This shared layer is consumed by both the
    /// software and Windows Direct3D backends.
    /// </summary>
    public static ScanlineColorAddRenderLayer? CaptureMotherBrainRainbowBeam(MotherBrainRainbowBeamRenderSnapshot beam)
    {
        if (!beam.Active) return null;
        ushort[] nativeWindows = RainbowBeamWindowBuilder.Build(beam.Geometry);
        var windows = new ColorAddWindow[Height];
        Array.Fill(windows, ColorAddWindow.Empty);
        byte red = ExpandFiveBit((byte)beam.Color.Red);
        byte green = ExpandFiveBit((byte)beam.Color.Green);
        byte blue = ExpandFiveBit((byte)beam.Color.Blue);
        for (int y = HudHeight; y < Height; y++)
        {
            ushort word = nativeWindows[y];
            byte left = (byte)word, right = SnesLittleEndian.HighByte(word);
            if (left <= right) windows[y] = new(left, right, red, green, blue);
        }
        return new(windows);
    }

    /// <summary>Composes native configuration $16 with both BG2 and BG3 on the subscreen.</summary>
    public static GameplayColorMathRenderLayer CaptureWaterfall(OrdinaryGameplayRenderLayer gameplay,
        RoomLayer3FxRenderSnapshot fx)
    {
        if (fx.LayerBlendConfiguration != LayerBlendingConfiguration.WaterfallSubtractive)
            throw new ArgumentException("Waterfall capture requires configuration $16.", nameof(fx));
        var windows = new XrayWindowLine[Height];
        Array.Fill(windows, new XrayWindowLine(255, 0));
        return new(gameplay, windows, false, new ColorMathRegisters { ColorMath = WaterfallRoomDisplayRules.ColorMath, AddSubscreen = true, FixedRed = 0, FixedGreen = 0, FixedBlue = 0 }, CaptureRoomLayer3Fx(fx), subscreenUsesBg2: true);
    }

    /// <summary>Resolves FX type, liquid visibility and wave phase into a backend-neutral BG equation.</summary>
    public static Bg2BppColorMathRenderLayer? CaptureRoomLayer3Fx(RoomLayer3FxRenderSnapshot fx)
    {
        LayerBlendingConfiguration required = fx.Type switch
        {
            RoomFxType.Lava or RoomFxType.Acid => LayerBlendingConfiguration.LavaAcidAdditive,
            RoomFxType.Water or RoomFxType.TourianEntranceStatue => fx.LayerBlendConfiguration,
            RoomFxType.Rain => LayerBlendingConfiguration.Rain,
            RoomFxType.Fog => LayerBlendingConfiguration.FogAdditive,
            RoomFxType.Spores => LayerBlendingConfiguration.Spores,
            _ => throw new NotSupportedException($"Room FX type {fx.Type} has no captured BG3 compositor."),
        };
        if (RoomFxTypes.UsesWater(fx.Type) && fx.LayerBlendConfiguration is not
            (LayerBlendingConfiguration.WaterSubtractive or LayerBlendingConfiguration.WaterfallSubtractive or
            LayerBlendingConfiguration.LiquidOrFogAdditive))
            throw new InvalidDataException("Water has an incompatible layer-blending configuration.");
        if (fx.LayerBlendConfiguration != required) throw new InvalidDataException("Room FX has an incompatible layer-blending configuration.");
        bool liquid = fx.Type is RoomFxType.Water or RoomFxType.TourianEntranceStatue or RoomFxType.Lava or RoomFxType.Acid;
        if (liquid && unchecked((short)fx.CurrentYPosition) < 0) return null;
        bool atmosphere = fx.Type is RoomFxType.Rain or RoomFxType.Fog or RoomFxType.Spores;
        var scrolls = new Position16<BgPlanePixels>[Height];
        int firstSurfaceLine = fx.WaterSurfaceScreenY - (SnesPpuLayout.BackgroundTileSizePixels - 1);
        for (int y = HudHeight; y < Height; y++)
        {
            ushort vertical = liquid && y < firstSurfaceLine ? (ushort)0 : fx.Scroll.Y;
            int wave = RoomFxTypes.UsesWater(fx.Type) && y > fx.WaterSurfaceScreenY
                ? RoomFxRomData.Water.WaveDisplacement((y - fx.WaterSurfaceScreenY - 1 - fx.WaterBg3WavePhase +
                    RoomFxRomData.Water.WaveDisplacementCount) % RoomFxRomData.Water.WaveDisplacementCount) : 0;
            scrolls[y] = new(unchecked((ushort)(fx.Scroll.X + wave)), vertical);
        }
        bool subtract = RoomFxTypes.UsesWater(fx.Type) && fx.LayerBlendConfiguration is
            LayerBlendingConfiguration.WaterSubtractive or LayerBlendingConfiguration.WaterfallSubtractive;
        return new(atmosphere ? RoomFxRomData.Layer3.FullScreenAtmosphereTilemapBaseWord : RoomFxRomData.Layer3.LiquidTilemapBaseWord,
            GameplayVramLayout.HudCharacterWord,
            (atmosphere ? RoomFxRomData.Layer3.FullScreenAtmosphereTilemapSize : RoomFxRomData.Layer3.LiquidTilemapSize).Y,
            HudHeight, subtract ? ExpandedColorMathOperation.Subtract : ExpandedColorMathOperation.Add, scrolls);
    }

    /// <summary>
    /// FX $08 retains the winning main-screen source until color math. A post-frame
    /// overlay would incorrectly brighten foreground terrain (and OBJ palettes 0-3).
    /// Use the same literal Mode-1 composition as the other source-aware effects.
    /// </summary>
    public static GameplayColorMathRenderLayer CaptureSpores(OrdinaryGameplayRenderLayer gameplay,
        RoomLayer3FxRenderSnapshot fx)
    {
        if (fx.Type != RoomFxType.Spores)
            throw new ArgumentException("Spore blending requires a spores FX snapshot.", nameof(fx));
        var emptyWindows = new XrayWindowLine[Height];
        Array.Fill(emptyWindows, new XrayWindowLine(255, 0));
        return new(gameplay, emptyWindows, false, new ColorMathRegisters { ColorMath = (SnesColorMathControl)RoomFxRomData.Spores.ColorMathSources, AddSubscreen = true, FixedRed = 0, FixedGreen = 0, FixedBlue = 0 }, CaptureRoomLayer3Fx(fx));
    }

    /// <summary>Resolves the selected Ceres gradient into RGB window rows, with no boss-state reference.</summary>
    public static ScanlineColorAddRenderLayer CaptureCeresHaze(bool ridleyIsDead,
        int intensity = CeresHazeRenderDefinitions.MaximumComponent,
        RoomFxPaletteBlendCatalog? colors = null)
    {
        var windows = new ColorAddWindow[Height];
        Array.Fill(windows, ColorAddWindow.Empty);
        for (int y = HudHeight; y < Height; y++)
        {
            (byte red, byte green, byte blue) = CeresHazeRenderDefinitions.ResolveComponents(
                y, intensity, ridleyIsDead, colors);
            windows[y] = new(0, Width - 1, ExpandFiveBit(red),
                ExpandFiveBit(green), ExpandFiveBit(blue));
        }
        return new(windows);
    }

    /// <summary>
    /// Resolves ROM curve data and the rendered explosion phase on the producer side.
    /// The renderer receives only clipped endpoints/colors, not a bus or projectile owner.
    /// </summary>
    public static ScanlineColorAddRenderLayer? CapturePowerBombColorMath(ISnesAddressSpace bus,
        SamusPowerBombExplosionState explosion, Position16<RoomPixels> layer1,
        int firstVisibleScanline = HudHeight)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(explosion);
        if ((uint)firstVisibleScanline > Height)
            throw new ArgumentOutOfRangeException(nameof(firstVisibleScanline));
        if (!explosion.IsActive) return null;
        var windows = new ColorAddWindow[Height];
        Array.Fill(windows, ColorAddWindow.Empty);
        Offset16<ScreenPixels> center = explosion.Position
            .RelativeToCamera(layer1);
        int centerX = center.X;
        int centerY = center.Y;
        byte red = ExpandFiveBit(explosion.FixedColorRed);
        byte green = ExpandFiveBit(explosion.FixedColorGreen);
        byte blue = ExpandFiveBit(explosion.FixedColorBlue);
        // Gameplay protects its HUD; full-screen cinematics use the same native
        // window profile starting at scanline zero instead.
        for (int y = firstVisibleScanline; y < Height; y++)
        {
            int halfWidth = ReadPowerBombHalfWidth(explosion, y - centerY);
            if (halfWidth < 0) continue;
            int left = Math.Max(0, centerX - halfWidth);
            int right = Math.Min(Width - 1, centerX + halfWidth);
            if (left <= right) windows[y] = new((byte)left, (byte)right, red, green, blue);
        }
        return new(windows);
    }

    /// <summary>Resolves the same tangent inequalities as the reference X-ray raster into immutable line intervals.</summary>
    public static XrayWindowLine[] CaptureXrayWindowLines(ISnesAddressSpace bus, SamusState samus,
        Position16<RoomPixels> layer1)
    {
        bool leftFacing = samus.PoseMetadata.IsFacingLeft(bus);
        Offset16<ScreenPixels> samusOnScreen = samus.Position.Whole.RelativeToCamera(layer1);
        int originX = samusOnScreen.X +
            (leftFacing ? -XrayWindowRenderDefinitions.EyeHorizontalOffset : XrayWindowRenderDefinitions.EyeHorizontalOffset);
        int originY = samusOnScreen.Y -
            (samus.PoseMetadata.ReadMovementType(bus) == SamusMovementType.Crouching
                ? XrayWindowRenderDefinitions.CrouchingEyeHeight : XrayWindowRenderDefinitions.StandingEyeHeight);
        int angle = samus.Xray.Angle.TableIndex;
        int width = SnesLittleEndian.LowByte(samus.Xray.AngularWidth);
        Offset16<ScreenPixels> a = ReadXrayDirection(angle - width), b = ReadXrayDirection(angle + width);
        bool horizontal = width == 0 && (angle == SnesAngle.QuarterTurn.TableIndex || angle == SnesAngle.ThreeQuarterTurn.TableIndex);
        var lines = new XrayWindowLine[Height];
        Array.Fill(lines, new XrayWindowLine(1, 0));
        for (int y = HudHeight; y < Height; y++)
        {
            long left = 0, right = Width - 1;
            if (horizontal)
            {
                if (y != originY) continue;
                if (angle == SnesAngle.QuarterTurn.TableIndex) left = Math.Max(left, originX);
                else right = Math.Min(right, originX);
            }
            else
            {
                long dy = y - originY;
                if (!IntegerWindowIntersection.IntersectGreaterEqual(-a.Y, (long)a.X * dy + (long)a.Y * originX,
                        -XrayWindowRenderDefinitions.SubpixelTolerance, ref left, ref right)) continue;
                if (!IntegerWindowIntersection.IntersectGreaterEqual(b.Y, -(long)b.X * dy - (long)b.Y * originX,
                        -XrayWindowRenderDefinitions.SubpixelTolerance, ref left, ref right)) continue;
            }
            if (left <= right) lines[y] = new((byte)left, (byte)right);
        }
        return lines;
    }
}
