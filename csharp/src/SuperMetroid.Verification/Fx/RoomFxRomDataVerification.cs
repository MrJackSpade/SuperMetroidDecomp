using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Fx.AreaAnimatedTileObjectDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Fx.RoomFxAnimatedTileArtworkVerification;
using static SuperMetroid.Verification.Fx.RoomFxAnimatedTileMechanicsDefinitionsVerification;
using static SuperMetroid.Verification.Fx.RoomFxLayer3TilemapsVerification;
using static SuperMetroid.Verification.Fx.RoomFxPaletteBlendsVerification;
using static SuperMetroid.Verification.Fx.RoomFxRetailInventoryVerification;
using static SuperMetroid.Verification.Fx.RoomFxSharedRandomVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Fx;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;

/// <summary>Checks typed room-FX records, blend dispatch and renderable BG3 effects against native definitions.</summary>
internal static class RoomFxRomDataVerification
{
    /// <summary>
    /// Verifies the shared FX-record schema, representative native type discriminants,
    /// typed blending state, and both renderable layer-three table lookups.
    /// </summary>
    internal static void VerifyRoomFxRomData()
    {
        AssertEqual(16, RoomFxToolingRomData.Record.ByteCount, "room FX record width");
        AssertEqual(9, RoomFxRomDataRecordTooling.TypeOffset, "room FX type field");
        AssertEqual(11, RoomFxRomDataRecordTooling.Layer3LayerBlendConfigurationOffset,
            "room FX layer-three blend field");
        AssertEqual(15, RoomFxRomDataRecordTooling.PaletteBlendOffset,
            "room FX palette-blend field");
        AssertEqual(23, RoomFxRomData.ScrollingSky.SectionCount,
            "scrolling-sky section count");

        (RoomFxType Type, ushort NativeValue)[] representativeTypes =
        [
            (RoomFxType.Water, 0x0006),
            (RoomFxType.Lava, 0x0002),
            (RoomFxType.Acid, 0x0004),
            (RoomFxType.ScrollingSky, 0x0020),
            (RoomFxType.CeresHaze, 0x002c),
            (RoomFxType.CeresRidley, 0x0028),
            (RoomFxType.CeresElevator, 0x002a),
        ];
        foreach ((RoomFxType type, ushort nativeValue) in representativeTypes)
            AssertEqual(nativeValue, (ushort)type, $"{type} native room-FX value");

        Suite(nameof(VerifyRoomLayer3FxTypes), () => VerifyRoomLayer3FxTypes());
        Suite(nameof(VerifyAreaAnimatedTileObjectDefinitions), () => VerifyAreaAnimatedTileObjectDefinitions());
        Suite(nameof(VerifyRoomFxAnimatedTileMechanicsDefinitions), () => VerifyRoomFxAnimatedTileMechanicsDefinitions());
        Suite(nameof(VerifyRoomFxAnimatedTileArtwork), () => VerifyRoomFxAnimatedTileArtwork());
        Suite(nameof(VerifyRoomFxLayer3Tilemaps), () => VerifyRoomFxLayer3Tilemaps());
        Suite(nameof(VerifyRoomFxPaletteBlends), () => VerifyRoomFxPaletteBlends());
        Suite(nameof(VerifyRetailRoomFxInventory), () => VerifyRetailRoomFxInventory());
        Suite(nameof(VerifyFxValidationBoundaries), () => VerifyFxValidationBoundaries());

        Console.WriteLine(
            "  Room FX: shared record/table catalog, typed blending, liquid rise " +
            "sound/shake, sky, haze, Ceres, rain, and fog states agree.");
    }

    /// <summary>Checks all FX type and blend selector values, including unsupported native function pointers.</summary>
    internal static void VerifyFxValidationBoundaries()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "FX validation oracle revision");
        Suite(nameof(VerifyRoomFxTypeValidation), () => VerifyRoomFxTypeValidation(rom));
        Suite(nameof(VerifyLayerBlendValidation), () => VerifyLayerBlendValidation(rom));
    }

    /// <summary>Matches accepted FX type bytes to native dispatch slots while rejecting unowned RTL aliases.</summary>
    private static void VerifyRoomFxTypeValidation(ISnesAddressSpace rom)
    {
        // Original pre-conversion enum membership, independently cross-checked with
        // the native function pointer table. Zero is deliberately named; the other
        // entries targeting the same RTL were never accepted by this port boundary.
        ushort[] original = [0, 2, 4, 6, 8, 10, 12, 32, 34, 36, 38, 40, 42, 44];
        AssertTrue(Enum.GetValues<RoomFxType>().Select(x => (ushort)x).SequenceEqual(original),
            "All named room FX identities retain their original values");
        AssertEqual((ushort)0xac18, ReadVerificationWord(rom, 0x89ac49), "Native room FX dispatcher table operand");
        for (int slot = 0; slot <= 44; slot += 2)
        {
            ushort pointer = ReadVerificationWord(rom, 0x83ac18 + slot);
            AssertEqual(original.Contains((ushort)slot), slot == 0 || pointer != 0xb278,
                "Original named membership matches native routines excluding unnamed RTL slots");
        }
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            if (original.Contains((ushort)value))
                AssertEqual((ushort)value, (ushort)RoomFxTypes.FromCartridge((byte)value, "record $9000"),
                    "Room FX identity conversion preserves every accepted byte");
            else
            {
                var error = AssertThrows<NotSupportedException>(
                    () => RoomFxTypes.FromCartridge((byte)value, "record $9000"), "Undefined room FX rejected");
                AssertTrue(error.Message.Contains($"${value:X2}", StringComparison.Ordinal) &&
                    error.Message.Contains("$9000", StringComparison.Ordinal), "Room FX failure retains value/context");
            }
        }
    }

    /// <summary>Checks typed layer-blend identities against their native room-record byte domain.</summary>
    private static void VerifyLayerBlendValidation(ISnesAddressSpace rom)
    {
        // Original pre-conversion enum membership, not generated by the new validator.
        ushort[] original = [0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26,
            28, 30, 32, 34, 36, 38, 40, 42, 44, 46, 48, 50, 52];
        AssertTrue(Enum.GetValues<LayerBlendingConfiguration>().All(x => original.Contains((ushort)x)),
            "All named blend identities retain their original values");
        AssertEqual((ushort)0x803e, ReadVerificationWord(rom, 0x88800e), "Native blending dispatcher operand");
        AssertEqual((ushort)0x8074, ReadVerificationWord(rom, 0x88803e), "First target marks original table end");
        foreach (ushort offset in original)
        {
            ushort target = ReadVerificationWord(rom, 0x88803e + offset);
            AssertTrue(target is >= 0x8074 and <= 0x8156, "Native slot points into its blending routines");
        }
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            var configuration = (LayerBlendingConfiguration)value;
            bool accepted = original.Contains((ushort)value);
            if (accepted) LayerBlendingConfigurations.Validate(configuration, "blend");
            else
            {
                var error = AssertThrows<ArgumentOutOfRangeException>(
                    () => LayerBlendingConfigurations.Validate(configuration, "blend"), "Undefined host blend rejected");
                AssertEqual("blend", error.ParamName, "Host blend failure retains parameter name");
                AssertEqual(configuration, (LayerBlendingConfiguration)(error.ActualValue ?? throw new InvalidOperationException("Verification requires error.ActualValue.")), "Host blend failure retains value");
            }
            if (value > byte.MaxValue) continue;
            if (accepted)
                AssertEqual(configuration, LayerBlendingConfigurations.FromCartridge((byte)value, "record $9000"),
                    "Cartridge blend preserves accepted identity");
            else
            {
                var error = AssertThrows<NotSupportedException>(
                    () => LayerBlendingConfigurations.FromCartridge((byte)value, "record $9000"), "Undefined cartridge blend rejected");
                AssertTrue(error.Message.Contains($"{value:X4}", StringComparison.Ordinal) &&
                    error.Message.Contains("$9000", StringComparison.Ordinal), "Cartridge blend failure retains value/context");
            }
        }
    }

    /// <summary>Checks the distinct BG3 routes and main/subscreen blend ownership for liquid, rain and fog.</summary>
    private static void VerifyRoomLayer3FxTypes()
    {
        var bus = new TestAddressSpace();
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        var presentation = RetailPresentationFixture();
        var stateSession = new RoomFxSession{
            Layer3Tilemaps = presentation.RoomFxLayer3Tilemaps,
            PaletteBlendColors = presentation.RoomFxPaletteBlends,
            AnimatedTileArtwork = presentation.RoomFxAnimatedTiles,
        };
        var state = new RoomLayer3FxState(stateSession);
        const ushort record = 0x9400;
        SeedSyntheticRoomFxWord(bus, record, RoomFxRomDataRecordToolingConstants.DoorPointerOffset, 0);
        SeedSyntheticRoomFxByte(bus, record,
            RoomFxRomDataRecordTooling.Layer3LayerBlendConfigurationOffset,
            (byte)LayerBlendingConfiguration.NormalGameplay);

        RoomFxType[] nonLayer3Types =
        [
            RoomFxType.ScrollingSky,
            RoomFxType.CeresHaze,
            RoomFxType.CeresRidley,
            RoomFxType.CeresElevator,
        ];
        foreach (RoomFxType type in nonLayer3Types)
        {
            SeedSyntheticRoomFxByte(bus, record, RoomFxRomDataRecordTooling.TypeOffset, (byte)type);
            state = LoadSyntheticRoomFx(stateSession, bus, cgram, record);
            AssertEqual(type, state.Type, $"{type} FX record type");
            AssertEqual(
                LayerBlendingConfiguration.NormalGameplay,
                state.LayerBlendConfiguration,
                $"{type} typed layer-blending configuration");
            AssertTrue(!state.IsRenderable, $"{type} does not impersonate rain/fog BG3");
        }

        Suite(nameof(VerifyRenderableLayer3Fx), () => VerifyRenderableLayer3Fx(bus, vram, cgram, stateSession, record, RoomFxType.Lava));
        Suite(nameof(VerifyRenderableLayer3Fx), () => VerifyRenderableLayer3Fx(bus, vram, cgram, stateSession, record, RoomFxType.Acid));
        Suite(nameof(VerifyRenderableLayer3Fx), () => VerifyRenderableLayer3Fx(bus, vram, cgram, stateSession, record, RoomFxType.Water));
        Suite(nameof(VerifyRenderableLayer3Fx), () => VerifyRenderableLayer3Fx(bus, vram, cgram, stateSession, record, RoomFxType.Rain));
        Suite(nameof(VerifyRenderableLayer3Fx), () => VerifyRenderableLayer3Fx(bus, vram, cgram, stateSession, record, RoomFxType.Fog));
    }

    /// <summary>Renders a synthetic BG3 surface through the production compositor and checks source page, clipping and type-specific blend routing.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    private static void VerifyRenderableLayer3Fx(
        TestAddressSpace bus,
        SnesVram vram,
        SnesCgram cgram,
        RoomFxSession session,
        ushort record,
        RoomFxType type)
    {
        SeedSyntheticRoomFxByte(bus, record, RoomFxRomDataRecordTooling.TypeOffset, (byte)type);
        LayerBlendingConfiguration layerBlend = type switch
        {
            RoomFxType.Lava or RoomFxType.Acid =>
                LayerBlendingConfiguration.LavaAcidAdditive,
            RoomFxType.Water => LayerBlendingConfiguration.LiquidOrFogAdditive,
            RoomFxType.Rain => LayerBlendingConfiguration.Rain,
            RoomFxType.Fog => LayerBlendingConfiguration.FogAdditive,
            _ => throw new InvalidOperationException($"Unexpected renderable room FX {type}."),
        };
        SeedSyntheticRoomFxByte(bus, record,
            RoomFxRomDataRecordTooling.Layer3LayerBlendConfigurationOffset, (byte)layerBlend);

        RoomLayer3FxState state = LoadSyntheticRoomFx(session, bus, cgram, record);
        Suite(nameof(VerifyRoomFxSharedRandomState), () => state = VerifyRoomFxSharedRandomState(bus, cgram, session, state, record, type));
        AssertEqual(RoomFxRomData.Layer3.ClearTilemapWord,
            Layer3Plane.Compose(new ushort[Layer3Plane.HudRows * Layer3Plane.Columns], state.EffectPageWords())
                .WordAt(Layer3Plane.HudRows, 0).Raw,
            $"{type} room shows the cleared word in the first gameplay BG3 FX word");
        if (type is RoomFxType.Water or RoomFxType.Lava or RoomFxType.Acid)
        {
            const ushort surfaceY = 100;
            SeedSyntheticRoomFxByte(bus, record, RoomFxRomDataRecordTooling.LiquidOptionsOffset,
                type == RoomFxType.Water
                    ? (byte)3
                    : (byte)RoomFxRomDataLavaAcidConstants.VerticalBg2WaveOption);
            SeedSyntheticRoomFxWord(bus, record, RoomFxRomDataRecordTooling.BaseYPositionOffset, surfaceY);
            state = LoadSyntheticRoomFx(session, bus, cgram, record);
            state.PrimeViewport(new Position16<RoomPixels>(0x0100, 0));
        }
        AssertEqual(type, state.Type, $"{type} layer-three type");
        AssertTrue(state.IsRenderable, $"{type} owns a translated layer-three plane");
        RoomLayer3FxRenderSnapshot snapshot = (state.CaptureForDisplay() ?? throw new InvalidOperationException("Verification requires state.CaptureForDisplay()."));
        AssertEqual(
            layerBlend,
            snapshot.LayerBlendConfiguration,
            $"{type} render snapshot retains typed blending");

        if (type is RoomFxType.Water or RoomFxType.Lava or RoomFxType.Acid)
        {
            AssertEqual(100, snapshot.WaterSurfaceScreenY,
                $"{type} surface retains room-relative scanline");
            // The seeded records select a drifting, BG2-waving water surface and lava/acid's vertical heat wave.
            RoomLiquidOptions expectedOptions = type == RoomFxType.Water
                ? RoomLiquidOptions.SurfaceDrift | RoomLiquidOptions.Bg2Wave
                : RoomLiquidOptions.Bg2Wave;
            AssertEqual(expectedOptions, snapshot.LiquidOptions,
                $"{type} render snapshot retains its wave options");
            AssertEqual((ushort)0x009b, snapshot.Scroll.Y,
                $"{type} anchors BG3 row zero immediately below surface Y 100");

            // Give the liquid page one opaque two-bit tile and a visible blue color. The
            // exact surface assertion proves the compositor leaves the air row untouched
            // and applies the cartridge layer-three plane below it.
            vram.ExecuteWordTransfer(
                Enumerable.Repeat((ushort)1, 33 * 32).ToArray(),
                RoomFxRomDataLayer3Constants.TilemapDestinationWord,
                wordIncrement: 1);
            var character = new byte[16];
            for (int row = 0; row < 8; row++)
                character[row * 2] = 0xff;
            vram.LoadBytes((0x4000 + 8) * 2, character);
            cgram.SetColor(1, Bgr555.FromWord(0x7c00));
        }
        else
        {
            // Rain/fog write BG3SC=$5C and therefore address the second page as a
            // standalone 32x32 tilemap. Give the ordinary $5800 page a red decoy and the
            // real $5C00 page a blue source. This reproduces the former HUD-clone failure:
            // indexing from $5800 sampled the first page and moved it with the effect.
            vram.ExecuteWordTransfer(
                Enumerable.Repeat((ushort)1, SnesPpuLayout.TilemapPageWordCount).ToArray(),
                RoomFxRomDataLayer3Constants.LiquidTilemapBaseWord,
                wordIncrement: 1);
            vram.ExecuteWordTransfer(
                Enumerable.Repeat((ushort)2, SnesPpuLayout.TilemapPageWordCount).ToArray(),
                RoomFxRomDataLayer3Constants.FullScreenAtmosphereTilemapBaseWord,
                wordIncrement: 1);
            var redDecoyCharacter = new byte[16];
            var blueEffectCharacter = new byte[16];
            for (int row = 0; row < 8; row++)
            {
                redDecoyCharacter[row * 2] = 0xff;
                blueEffectCharacter[row * 2 + 1] = 0xff;
            }
            vram.LoadBytes((0x4000 + 1 * 8) * 2, redDecoyCharacter);
            vram.LoadBytes((0x4000 + 2 * 8) * 2, blueEffectCharacter);
            cgram.SetColor(1, Bgr555.FromWord(0x001f));
            cgram.SetColor(2, Bgr555.FromWord(0x7c00));
            snapshot = snapshot with { Scroll = Position16<Bg3Pixels>.Zero };
        }

        // The software compositor now consumes the same typed dispatcher identity as the
        // room loader. Rain and fog use different main/subscreen ownership in hardware even
        // though both resolve to additive BG3 pixels, so neither pairing may be substituted.
        var frame = new Rgba32[
            SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height];
        SnesGameplayFrameRendererConstants.ApplyRoomLayer3FxColorMath(
            frame, vram, cgram, snapshot);
        if (type is RoomFxType.Water or RoomFxType.Lava or RoomFxType.Acid)
        {
            int above = 90 * SnesGameplayFrameRenderer.Width + 20;
            int below = 110 * SnesGameplayFrameRenderer.Width + 20;
            AssertEqual(new Rgba32(0, 0, 0, 0), frame[above],
                $"{type} leaves pixels above its surface untouched");
            AssertTrue(frame[below].B != 0,
                $"{type} applies its visible BG3 color below the surface");

            // Negative liquid positions are the cartridge's deliberate "no visible
            // surface" sentinel. Reuse the same known-opaque tile and palette so this
            // assertion would fail if the signed boundary check regressed; an empty
            // fixture could otherwise make a broken compositor look correct by accident.
            var hiddenSurfaceFrame = new Rgba32[frame.Length];
            SnesGameplayFrameRendererConstants.ApplyRoomLayer3FxColorMath(
                hiddenSurfaceFrame,
                vram,
                cgram,
                snapshot with
                {
                    Surface = new(0, ushort.MaxValue),
                    WaterSurfaceScreenY = -1,
                });
            AssertTrue(hiddenSurfaceFrame.All(pixel => pixel == default),
                $"{type} $FFFF surface suppresses the complete BG3 liquid plane");

            ushort[] bg2Scroll = type == RoomFxType.Water
                ? SnesGameplayFrameRenderer.BuildWaterBg2HorizontalScrolls(
                    snapshot,
                    bg2Scroll: new Position16<Bg2Pixels>(0x0200, 0)) ?? throw new InvalidDataException(
                        "Wavy water did not publish a BG2 HDMA scroll table.")
                : SnesGameplayFrameRenderer.BuildLavaAcidBg2VerticalScrolls(
                    snapshot,
                    bg2VerticalScroll: 0x0200) ?? throw new InvalidDataException(
                        $"{type} did not publish its BG2 HDMA scroll table.");
            AssertEqual(SnesGameplayFrameRenderer.Height - SnesGameplayFrameRenderer.HudHeight,
                bg2Scroll.Length,
                $"{type} BG2 wave table covers gameplay scanlines");
            if (type == RoomFxType.Water)
            {
                AssertEqual((ushort)0x0200,
                    bg2Scroll[90 - SnesGameplayFrameRenderer.HudHeight],
                    "water leaves BG2 undistorted above the surface");
                AssertTrue(
                    bg2Scroll.Skip(101 - SnesGameplayFrameRenderer.HudHeight).Distinct().Count() > 1,
                    "water distorts BG2 below the surface");
            }
            else
            {
                AssertEqual(3, bg2Scroll.Distinct().Count(),
                    $"{type} uses the native three-value heat waveform");
                for (int frameNumber = 0;
                     frameNumber < RoomFxRomData.LavaAcid.VerticalWavePhaseDuration - 1;
                     frameNumber++)
                {
                    state.Step(vram, new RoomLayer3FxFrame { Camera = new Position16<RoomPixels>(0x0100, 0), TimeIsFrozen = false, MainGameLoopCarry = true });
                    AssertEqual(0, (state.CaptureForDisplay() ?? throw new InvalidOperationException("Verification requires state.CaptureForDisplay().")).LavaAcidBg2WavePhase,
                        $"{type} heat wave retains phase before frame four");
                }
                state.Step(vram, new RoomLayer3FxFrame { Camera = new Position16<RoomPixels>(0x0100, 0), TimeIsFrozen = false, MainGameLoopCarry = true });
                AssertEqual(15, (state.CaptureForDisplay() ?? throw new InvalidOperationException("Verification requires state.CaptureForDisplay().")).LavaAcidBg2WavePhase,
                    $"{type} heat wave rotates backward on frame four");
            }
        }
        else
        {
            Rgba32 firstEffectPixel = frame[
                SnesGameplayFrameRenderer.HudHeight * SnesGameplayFrameRenderer.Width];
            AssertTrue(firstEffectPixel.B != 0 && firstEffectPixel.R == 0,
                $"{type} samples the $5C00 effect page instead of the $5800 HUD page");

            var wrappedFrame = new Rgba32[frame.Length];
            SnesGameplayFrameRendererConstants.ApplyRoomLayer3FxColorMath(
                wrappedFrame,
                vram,
                cgram,
                snapshot with { Scroll = snapshot.Scroll.WithY(0x0100) });
            Rgba32 wrappedEffectPixel = wrappedFrame[
                SnesGameplayFrameRenderer.HudHeight * SnesGameplayFrameRenderer.Width];
            AssertTrue(wrappedEffectPixel.B != 0 && wrappedEffectPixel.R == 0,
                $"{type} wraps vertical scroll within its 32-row $5C00 page");
        }
        LayerBlendingConfiguration wrongBlend = type switch
        {
            RoomFxType.Lava or RoomFxType.Acid => LayerBlendingConfiguration.NormalGameplay,
            RoomFxType.Water => LayerBlendingConfiguration.NormalGameplay,
            RoomFxType.Rain => LayerBlendingConfiguration.FogAdditive,
            _ => LayerBlendingConfiguration.Rain,
        };
        AssertThrows<InvalidDataException>(
            () => SnesGameplayFrameRendererConstants.ApplyRoomLayer3FxColorMath(
                frame,
                vram,
                cgram,
                snapshot with { LayerBlendConfiguration = wrongBlend }),
            $"{type} rejects another FX type's layer-blending route");
    }

    /// <summary>Decodes a literal synthetic sixteen-byte record independently of the compiled catalog before production FX initialization.</summary>
    internal static RoomLayer3FxState LoadSyntheticRoomFx(RoomFxSession session, TestAddressSpace bus,
        SnesCgram cgram, ushort pointer) =>
        RoomLayer3FxState.ForDefinition(session, cgram, ReadSyntheticRoomFxRecord(bus, pointer), randomNumber: 0);

    /// <summary>Reads all sixteen bank-$83 record bytes in native word/byte order for the independent fixture oracle.</summary>
    [CpuBusBoundary("exact bus test: decodes the synthetic sixteen-byte bank-$83 room FX record")]
    private static RoomFxRecordDefinition ReadSyntheticRoomFxRecord(TestAddressSpace bus, ushort pointer)
    {
        int address = 0x830000 | pointer;
        byte Byte(int offset) => bus.ReadByte(address + offset);
        ushort Word(int offset) => (ushort)(SnesLittleEndian.Word(Byte(offset), Byte(offset + 1)));
        ushort door = Word(0);
        return new RoomFxRecordDefinition(
            door is 0 or ushort.MaxValue ? null : DoorDefinitions.Get(door), door == ushort.MaxValue, Word(2), Word(4), Word(6), Byte(8), Byte(9), Byte(10), Byte(11),
            Byte(12), Byte(13), Byte(14), Byte(15));
    }

    /// <summary>Writes one byte field of the synthetic bank-$83 room FX record at <paramref name="record"/>.</summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for one bank-$83 room FX record byte field")]
    private static void SeedSyntheticRoomFxByte(TestAddressSpace bus, ushort record, int fieldOffset, byte value) =>
        WriteTestByte(bus, RoomFxRomDataBanksTooling.RoomDefinitions | unchecked((ushort)(record + fieldOffset)), value);

    /// <summary>Writes one little-endian word field of the synthetic bank-$83 room FX record at <paramref name="record"/>.</summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for one bank-$83 room FX record word field")]
    private static void SeedSyntheticRoomFxWord(TestAddressSpace bus, ushort record, int fieldOffset, ushort value) =>
        WriteTestWord(bus, RoomFxRomDataBanksTooling.RoomDefinitions | unchecked((ushort)(record + fieldOffset)), value);

    /// <summary>Runs FX type and blend-selector domain validation.</summary>
    [VerificationCommand("--lookup-fx-validation")]
    internal static int LookupFxValidationCommand()
    {
        Suite(nameof(VerifyFxValidationBoundaries), () => VerifyFxValidationBoundaries());
        Console.WriteLine("FX validation: native dispatcher slots, every cartridge byte and every host blend ushort pass.");
        return 0;
    }

    /// <summary>Runs the typed room-FX record and BG3 rendering fixture.</summary>
    [VerificationCommand("--room-fx-fixture")]
    internal static int RoomFxFixtureCommand()
    {
        Suite(nameof(VerifyRoomFxRomData), () => VerifyRoomFxRomData());
        return 0;
    }
}
