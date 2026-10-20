using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using System.Buffers;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge values and helpers of <see cref="SnesGameplayFrameRenderer"/> that only verification reads.</summary>
[PpuLayoutBoundary("exact hardware test: the legacy software gameplay compositors kept for focused presentation checks")]
internal static class SnesGameplayFrameRendererConstants
{
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
    public static Rgba32[] RenderHudMode7AndObjs(SnesVram vram, SnesCgram cgram, SpriteFrameBuffer sprites, Mode7RenderRegisters registers, Rgba32[]? outputBuffer = null)
    {
        Rgba32[] output = SnesGameplayFrameRenderer.CreateBackdrop(cgram, outputBuffer);
        SnesMode7Renderer.CompositeViewport(output, vram, cgram, new Mode7RenderRegisters { MatrixA = registers.MatrixA, MatrixB = registers.MatrixB, MatrixC = registers.MatrixC, MatrixD = registers.MatrixD, Center = registers.Center, Scroll = registers.Scroll });

        // IRQ command four gives the upper 32 physical scanlines exclusively to BG3.
        // Mode 7 is projected at physical screen coordinates across the destination first;
        // DrawHudAndObjects replaces its upper band with BG3 rather than cropping a separate
        // 192-line transform whose Y coordinate would incorrectly restart at zero.

        DrawHudAndObjects(output, vram, cgram, sprites);
        return output;
    }
    /// <summary>
    /// Composes Ceres Ridley's getaway with the two bank-$88 HDMA layer splits that the
    /// generic Mode-7 elevator path does not have: BG3 owns the HUD, Mode 7 owns the arena,
    /// and the final sixteen scanlines switch back to Mode 1 with only BG2 and OBJ enabled.
    /// </summary>
    public static Rgba32[] RenderHudCeresRidleyGetawayAndObjs(SnesVram vram, SnesCgram cgram, SpriteFrameBuffer sprites, Mode7RenderRegisters registers, CeresGetawayFloor floor, byte obsel = SnesGameplayFrameRenderer.DefaultObjectSelection, Rgba32[]? outputBuffer = null)
    {
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        Ensure.NotNull(sprites);

        const int floorHeight = 16;
        const int floorFirstScanline = SnesGameplayFrameRenderer.Height - floorHeight;
        Rgba32[] output = SnesGameplayFrameRenderer.CreateBackdrop(cgram, outputBuffer);
        using var scratch = new RenderScratch();
        Rgba32[] mode7 = scratch.Colors(SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height);
        SnesMode7Renderer.CompositeViewport(mode7, vram, cgram, new Mode7RenderRegisters { MatrixA = registers.MatrixA, MatrixB = registers.MatrixB, MatrixC = registers.MatrixC, MatrixD = registers.MatrixD, Center = registers.Center, Scroll = registers.Scroll });
        ResolvedObjFrame objects = scratch.ResolveObjects(sprites, cgram);

        // IndirectHDMATable_CeresRidleyMode_BGTileSize and its matching TM table keep
        // scanlines 32..207 in Mode 7 with BG1/BG2/OBJ selected. The existing Mode-7
        // compositor treats every OBJ priority alike here because no ordinary BG priority
        // planes compete with it.
        for (int screenY = SnesGameplayFrameRenderer.HudHeight; screenY < floorFirstScanline; screenY++)
        {
            int row = screenY * SnesGameplayFrameRenderer.Width;
            for (int screenX = 0; screenX < SnesGameplayFrameRenderer.Width; screenX++)
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
        SnesGameplayFrameRenderer.ExpandCgram(cgram, palette);
        var floorGeometry = new BgTilemapGeometry(GameplayVramLayoutConstants.RoomTilemapSize);
        for (int screenY = floorFirstScanline; screenY < SnesGameplayFrameRenderer.Height; screenY++)
        {
            int scrolledY = floorGeometry.WrapY(floor.Bg2Scroll.Y + screenY);
            int tileY = BgCharacterGrid.CharacterOf(scrolledY);
            int pixelY = BgCharacterGrid.PixelWithinCharacter(scrolledY);
            int previousTileX = -1;
            SnesBgTilemapWord bg2Entry = default;

            for (int screenX = 0; screenX < SnesGameplayFrameRenderer.Width; screenX++)
            {
                int destination = screenY * SnesGameplayFrameRenderer.Width + screenX;
                Rgba32 winner = output[destination];
                int winnerRank = 0;
                int scrolledX = floorGeometry.WrapX(floor.Bg2Scroll.X + screenX);
                int tileX = BgCharacterGrid.CharacterOf(scrolledX);
                if (tileX != previousTileX)
                {
                    bg2Entry = SnesVramAddressingConstants.ReadWord(vramBytes, floorGeometry.EntryWordAddress(
                        GameplayVramLayout.Bg2TilemapWord, new Position16<TilemapCells>((ushort)tileX, (ushort)tileY)));
                    previousTileX = tileX;
                }

                if (SnesGameplayFrameRenderer.TryDecodeOrdinaryGameplayBgPixel(
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

        SnesGameplayFrameRenderer.DrawHud(output, vram, cgram, Layer3PlaneFixtures.FromVram(vram), GameplayVramLayout.HudCharacterWord);
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
        Ensure.LengthEqual(frame, SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height);

        for (int screenY = SnesGameplayFrameRenderer.HudHeight; screenY < SnesGameplayFrameRenderer.Height; screenY++)
        {
            Bgr555 haze = CeresHazeRenderDefinitions.ResolveComponents(
                screenY, intensity, ridleyIsDead, colors);
            byte addRed = SnesGameplayFrameRenderer.ExpandFiveBit(haze.Red);
            byte addGreen = SnesGameplayFrameRenderer.ExpandFiveBit(haze.Green);
            byte addBlue = SnesGameplayFrameRenderer.ExpandFiveBit(haze.Blue);
            int row = screenY * SnesGameplayFrameRenderer.Width;
            for (int screenX = 0; screenX < SnesGameplayFrameRenderer.Width; screenX++)
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
        PowerBombRenderSnapshot explosion,
        Position16<RoomPixels> layer1)
    {
        Ensure.LengthEqual(frame, SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height);
        if (!explosion.IsActive)
            return;

        Offset16<ScreenPixels> center = explosion.Position
            .RelativeToCamera(layer1);
        int centerX = center.X;
        int centerY = center.Y;
        byte addRed = SnesGameplayFrameRenderer.ExpandFiveBit(explosion.FixedColorRed);
        byte addGreen = SnesGameplayFrameRenderer.ExpandFiveBit(explosion.FixedColorGreen);
        byte addBlue = SnesGameplayFrameRenderer.ExpandFiveBit(explosion.FixedColorBlue);

        for (int screenY = SnesGameplayFrameRenderer.HudHeight; screenY < SnesGameplayFrameRenderer.Height; screenY++)
        {
            int halfWidth = SnesGameplayFrameRenderer.ReadPowerBombHalfWidth(explosion, screenY - centerY);
            if (halfWidth < 0)
                continue;

            int left = Math.Max(0, centerX - halfWidth);
            int right = Math.Min(SnesGameplayFrameRenderer.Width - 1, centerX + halfWidth);
            if (left > right)
                continue;

            int row = screenY * SnesGameplayFrameRenderer.Width;
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
        Ensure.LengthEqual(frame, SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height);
        LayerBlendingConfiguration expectedConfiguration = fx.Type switch
        {
            RoomFxType.Lava or RoomFxType.Acid => LayerBlendingConfiguration.LavaAcidAdditive,
            RoomFxType.Water or RoomFxType.TourianEntranceStatue => fx.LayerBlendConfiguration,
            RoomFxType.Rain => LayerBlendingConfiguration.Rain,
            RoomFxType.Fog => LayerBlendingConfiguration.FogAdditive,
            RoomFxType.None or
                RoomFxType.Spores or
                RoomFxType.ScrollingSky or
                RoomFxType.UnusedScrollingSky or
                RoomFxType.Fireflea or
                RoomFxType.CeresRidley or
                RoomFxType.CeresElevator or
                RoomFxType.CeresHaze => throw new NotSupportedException(
                $"Room FX type {(ushort)fx.Type:X2} has no BG3 compositor."),
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
            unchecked((short)fx.Surface.Y) < 0)
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
            ? RoomFxRomDataLayer3Constants.FullScreenAtmosphereTilemapBaseWord
            : RoomFxRomDataLayer3Constants.LiquidTilemapBaseWord;
        var geometry = new BgTilemapGeometry(fullScreenAtmosphere
            ? RoomFxRomDataLayer3Constants.FullScreenAtmosphereTilemapSize
            : RoomFxRomDataLayer3Constants.LiquidTilemapSize);
        BgCharacterFormat format = BgCharacterFormat.TwoBitsPerPixel;
        // The image a capture shows: bound background characters over VRAM (#1350).
        SnesVram image = BgArtworkFixtures.ComposedVram(vram);
        ReadOnlySpan<byte> fxVram = image.Bytes;
        int firstLiquidScrollLine = fx.WaterSurfaceScreenY -
            (SnesPpuLayout.BackgroundTileSizePixels - 1);
        for (int screenY = SnesGameplayFrameRenderer.HudHeight; screenY < SnesGameplayFrameRenderer.Height; screenY++)
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
            for (int screenX = 0; screenX < SnesGameplayFrameRenderer.Width; screenX++)
            {
                int scrolledX = geometry.WrapX(fx.Scroll.X + waveDisplacement + screenX);
                SnesBgTilemapWord entry = image.ReadWord(geometry.EntryWordAddress(tilemapBaseWord,
                    new Position16<TilemapCells>((ushort)BgCharacterGrid.CharacterOf(scrolledX), (ushort)tileY)));
                int row = format.RowByteOffset(GameplayVramLayout.HudCharacterWord, entry.CharacterIndex,
                    BgCharacterGrid.SourcePixel(pixelY, entry.FlipVertically));
                int color = format.ColorIndexAt(fxVram, row,
                    BgCharacterGrid.SourcePixel(BgCharacterGrid.PixelWithinCharacter(scrolledX), entry.FlipHorizontally));
                if (color == BgCharacterFormat.TransparentColorIndex)
                    continue;

                Rgba32 overlay = cgram.GetRgba(format.CgramColor(entry.PaletteIndex, color));
                int destination = screenY * SnesGameplayFrameRenderer.Width + screenX;
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
    /// Applies a frame-coherent Morph Ball eye snapshot captured at the OAM-upload NMI.
    /// </summary>
    public static void ApplyMorphBallEyeBeamColorMath(
        Span<Rgba32> frame,
        MorphBallEyeBeamRenderSnapshot beam,
        Position16<RoomPixels> layer1)
    {
        Ensure.LengthEqual(frame, SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height);
        if (beam.Phase is MorphBallEyeBeamPhase.Inactive or
            MorphBallEyeBeamPhase.PendingInitialization)
        {
            return;
        }

        // Both display-packet and immediate rendering consume the same cartridge
        // endpoint builder. Backend equality is useful coverage, but the separate
        // executed-ROM fixture is the independent oracle for these endpoints.
        ScanlineColorAddRenderLayer layer = SnesGameplayFrameRenderer.CaptureMorphBallEyeBeam(beam, layer1) ??
            throw new InvalidOperationException("An active morph-ball eye beam always captures its windows.");
        for (int screenY = SnesGameplayFrameRenderer.HudHeight; screenY < SnesGameplayFrameRenderer.Height; screenY++)
        {
            ColorAddWindow window = layer.Windows[screenY];
            int row = screenY * SnesGameplayFrameRenderer.Width;
            for (int screenX = window.Left; screenX <= window.Right; screenX++)
            {
                Rgba32 source = frame[row + screenX];
                frame[row + screenX] = new Rgba32(
                    SaturatingAdd(source.R, checked((byte)window.Red)),
                    SaturatingAdd(source.G, checked((byte)window.Green)),
                    SaturatingAdd(source.B, checked((byte)window.Blue)),
                    source.A);
            }
        }
    }
    /// <summary>Adds two expanded color channels with a white ceiling.</summary>
    internal static byte SaturatingAdd(byte left, byte right) =>
        (byte)Math.Min(byte.MaxValue, left + right);
    /// <summary>Subtracts one expanded color channel without wrapping below black.</summary>
    internal static byte SaturatingSubtract(byte left, byte right) =>
        (byte)Math.Max(byte.MinValue, left - right);
    /// <summary>Applies the BG3-only HUD split and places all resolved OBJ pixels over the Mode-7 gameplay region.</summary>
    internal static void DrawHudAndObjects(
        Rgba32[] output,
        SnesVram vram,
        SnesCgram cgram,
        SpriteFrameBuffer sprites)
    {
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        Ensure.NotNull(sprites);

        // IRQ command 4 writes BG3SC=$5A and TM=$04 for scanlines 0-31. That exposes the
        // four HUD rows at tilemap word $5800 while disabling OBJ in the HUD band.
        SnesGameplayFrameRenderer.DrawHud(output, vram, cgram, Layer3PlaneFixtures.FromVram(vram), GameplayVramLayout.HudCharacterWord);

        int pixelCount = SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height;
        Rgba32[] rentedObjectPixels = ArrayPool<Rgba32>.Shared.Rent(pixelCount);
        byte[] rentedObjectPriorities = ArrayPool<byte>.Shared.Rent(pixelCount);
        try
        {
            Span<Rgba32> objectPixels = rentedObjectPixels.AsSpan(0, pixelCount);
            Span<byte> objectPriorities = rentedObjectPriorities.AsSpan(0, pixelCount);
            SnesObjRenderer.RenderResolved(sprites.Draws, cgram, new ObjRasterTargets(objectPixels, objectPriorities));
            for (int pixel = SnesGameplayFrameRenderer.HudHeight * SnesGameplayFrameRenderer.Width; pixel < output.Length; pixel++)
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
}
