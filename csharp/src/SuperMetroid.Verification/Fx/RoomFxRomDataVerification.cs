using SuperMetroid.Core.Assets;
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
using SuperMetroid.Core.Rooms.Backgrounds;

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
        AssertEqual(23, ScrollingSkyBandDefinitions.SectionCount,
            "scrolling-sky section count");

        RepresentativeType[] representativeTypes =
        [
            new RepresentativeType(RoomFxType.Water, 0x0006),
            new RepresentativeType(RoomFxType.Lava, 0x0002),
            new RepresentativeType(RoomFxType.Acid, 0x0004),
            new RepresentativeType(RoomFxType.ScrollingSky, 0x0020),
            new RepresentativeType(RoomFxType.CeresHaze, 0x002c),
            new RepresentativeType(RoomFxType.CeresRidley, 0x0028),
            new RepresentativeType(RoomFxType.CeresElevator, 0x002a),
        ];
        foreach ((RoomFxType type, ushort nativeValue) in representativeTypes)
            AssertEqual(nativeValue, SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.Types.Word(type), $"{type} native room-FX value");

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
        // The identities' values are arbitrary (#1415); each member's dispatcher byte is the import's native word.
        AssertTrue(Enum.GetValues<RoomFxType>().Select(SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.Types.Word).SequenceEqual(original),
            "All named room FX identities name their original dispatcher bytes");
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
                AssertEqual((ushort)value, SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.Types.Word(SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.Types.Decode((byte)value, "record $9000 type")),
                    "Room FX record decoding preserves every accepted byte");
            else
            {
                var error = AssertThrows<InvalidDataException>(
                    () => SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.Types.Decode((byte)value, "record $9000 type"), "Undefined room FX rejected");
                AssertTrue(error.Message.Contains($"${value:X4}", StringComparison.Ordinal) &&
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
        // The identities' values are arbitrary (#1415); each member's dispatcher byte is the record codec's native word.
        AssertTrue(Enum.GetValues<LayerBlendingConfiguration>().All(x => original.Contains(RoomFxRecordCodec.LayerBlends.Word(x))),
            "All named blend identities name their original dispatcher bytes");
        AssertEqual((ushort)0x803e, ReadVerificationWord(rom, 0x88800e), "Native blending dispatcher operand");
        AssertEqual((ushort)0x8074, ReadVerificationWord(rom, 0x88803e), "First target marks original table end");
        foreach (ushort offset in original)
        {
            ushort target = ReadVerificationWord(rom, 0x88803e + offset);
            AssertTrue(target is >= 0x8074 and <= 0x8156, "Native slot points into its blending routines");
        }
        // Records decode at import (#1415): every record byte names its configuration or is rejected with its context.
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            if (original.Contains((ushort)value))
                AssertEqual((ushort)value, RoomFxRecordCodec.LayerBlends.Word(RoomFxRecordCodec.LayerBlends.Decode((byte)value, "record $9000 blend")),
                    "Cartridge blend preserves accepted identity");
            else
            {
                var error = AssertThrows<InvalidDataException>(
                    () => RoomFxRecordCodec.LayerBlends.Decode((byte)value, "record $9000 blend"), "Undefined cartridge blend rejected");
                AssertTrue(error.Message.Contains($"{value:X4}", StringComparison.Ordinal) &&
                    error.Message.Contains("$9000", StringComparison.Ordinal), "Cartridge blend failure retains value/context");
            }
        }
    }

    /// <summary>Checks the distinct BG3 routes and main/subscreen blend ownership for liquid, rain and fog.</summary>
    private static void VerifyRoomLayer3FxTypes()
    {
        var bus = new TestAddressSpace();
        var cgram = new GameplayColours(new SpriteColourSets());
        var presentation = RetailPresentationFixture();
        var stateSession = new RoomFxSession{
            PaletteBlendColors = presentation.RoomFxPaletteBlends,
            AnimatedTileArtwork = presentation.RoomFxAnimatedTiles,
        };
        var state = new RoomLayer3FxState(stateSession);
        const ushort record = 0x9400;
        SeedSyntheticRoomFxWord(bus, record, RoomFxRomDataRecordToolingConstants.DoorPointerOffset, 0);
        SeedSyntheticRoomFxByte(bus, record,
            RoomFxRomDataRecordTooling.Layer3LayerBlendConfigurationOffset,
            RoomFxRecordEncoding.LayerBlendByte(LayerBlendingConfiguration.NormalGameplay));

        RoomFxType[] nonLayer3Types =
        [
            RoomFxType.ScrollingSky,
            RoomFxType.CeresHaze,
            RoomFxType.CeresRidley,
            RoomFxType.CeresElevator,
        ];
        foreach (RoomFxType type in nonLayer3Types)
        {
            SeedSyntheticRoomFxByte(bus, record, RoomFxRomDataRecordTooling.TypeOffset, RoomFxRecordEncoding.TypeByte(type));
            state = LoadSyntheticRoomFx(stateSession, bus, cgram, record);
            AssertEqual(type, state.Type, $"{type} FX record type");
            AssertEqual(
                LayerBlendingConfiguration.NormalGameplay,
                state.LayerBlendConfiguration,
                $"{type} typed layer-blending configuration");
            AssertTrue(!state.IsRenderable, $"{type} does not impersonate rain/fog BG3");
        }

        Suite(nameof(VerifyRenderableLayer3Fx), () => VerifyRenderableLayer3Fx(bus, cgram, stateSession, record, RoomFxType.Lava));
        Suite(nameof(VerifyRenderableLayer3Fx), () => VerifyRenderableLayer3Fx(bus, cgram, stateSession, record, RoomFxType.Acid));
        Suite(nameof(VerifyRenderableLayer3Fx), () => VerifyRenderableLayer3Fx(bus, cgram, stateSession, record, RoomFxType.Water));
        Suite(nameof(VerifyRenderableLayer3Fx), () => VerifyRenderableLayer3Fx(bus, cgram, stateSession, record, RoomFxType.Rain));
        Suite(nameof(VerifyRenderableLayer3Fx), () => VerifyRenderableLayer3Fx(bus, cgram, stateSession, record, RoomFxType.Fog));
    }

    /// <summary>Captures a synthetic BG3 effect and checks its surface, wave tables, hidden-surface sentinel and type-specific blend routing.</summary>
    private static void VerifyRenderableLayer3Fx(
        TestAddressSpace bus,
        GameplayColours cgram,
        RoomFxSession session,
        ushort record,
        RoomFxType type)
    {
        SeedSyntheticRoomFxByte(bus, record, RoomFxRomDataRecordTooling.TypeOffset, RoomFxRecordEncoding.TypeByte(type));
        LayerBlendingConfiguration layerBlend = type switch
        {
            RoomFxType.Lava or RoomFxType.Acid =>
                LayerBlendingConfiguration.LavaAcidAdditive,
            RoomFxType.Water => LayerBlendingConfiguration.LiquidOrFogAdditive,
            RoomFxType.Rain => LayerBlendingConfiguration.Rain,
            RoomFxType.Fog => LayerBlendingConfiguration.FogAdditive,
            RoomFxType.None or
                RoomFxType.Spores or
                RoomFxType.ScrollingSky or
                RoomFxType.UnusedScrollingSky or
                RoomFxType.Fireflea or
                RoomFxType.TourianEntranceStatue or
                RoomFxType.CeresRidley or
                RoomFxType.CeresElevator or
                RoomFxType.CeresHaze => throw new InvalidOperationException($"Unexpected renderable room FX {type}."),
            _ => throw new InvalidOperationException($"Unexpected renderable room FX {type}."),
        };
        SeedSyntheticRoomFxByte(bus, record,
            RoomFxRomDataRecordTooling.Layer3LayerBlendConfigurationOffset, RoomFxRecordEncoding.LayerBlendByte(layerBlend));

        RoomLayer3FxState state = LoadSyntheticRoomFx(session, bus, cgram, record);
        Suite(nameof(VerifyRoomFxSharedRandomState), () => state = VerifyRoomFxSharedRandomState(bus, cgram, session, state, record, type));
        AssertEqual(Layer3PlaneConstants.ClearedCell.PackedWord,
            state.EffectPlane.WordsIn(SuperMetroid.AssetExtraction.Frontend.Map.InstalledAreaMapPresentation.RoomFxPagesOf(RetailPresentationFixture()))[Layer3PlaneConstants.HudRows * Layer3Plane.Columns],
            $"{type} room shows the cleared word in the first gameplay BG3 FX word");
        if (type is RoomFxType.Water or RoomFxType.Lava or RoomFxType.Acid)
        {
            const ushort surfaceY = 100;
            SeedSyntheticRoomFxByte(bus, record, RoomFxRomDataRecordTooling.LiquidOptionsOffset,
                type == RoomFxType.Water
                    ? (byte)3
                    : (byte)RoomFxNativeSources.VerticalBg2WaveOption);
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

            // Negative liquid positions are the cartridge's deliberate "no visible
            // surface" sentinel: the capture shows no BG3 liquid plane at all.
            AssertTrue(GameplayEffectCapture.CaptureRoomLayer3Fx(snapshot with
                {
                    Surface = new(0, ushort.MaxValue),
                    WaterSurfaceScreenY = -1,
                }, RoomBackgroundsFixtures.BlankBg3()) is null,
                $"{type} $FFFF surface suppresses the complete BG3 liquid plane");

            // The described BG2 wave (#1468), evaluated on each gameplay line.
            ushort[] bg2Scroll = ScrollShapeLines.GameplayScrolls(type == RoomFxType.Water
                ? GameplayEffectCapture.BuildWaterBackgroundHorizontalScrolls(
                    snapshot,
                    bg2Scroll: new Position16<BackgroundPixels>(0x0200, 0)) ?? throw new InvalidDataException(
                        "Wavy water did not publish a BG2 HDMA scroll table.")
                : GameplayEffectCapture.BuildLavaAcidBackgroundVerticalScrolls(
                    snapshot,
                    bg2VerticalScroll: 0x0200) ?? throw new InvalidDataException(
                        $"{type} did not publish its BG2 HDMA scroll table."));
            AssertEqual(DisplayScreen.Height - DisplayScreen.HudHeight,
                bg2Scroll.Length,
                $"{type} BG2 wave table covers gameplay scanlines");
            if (type == RoomFxType.Water)
            {
                AssertEqual((ushort)0x0200,
                    bg2Scroll[90 - DisplayScreen.HudHeight],
                    "water leaves BG2 undistorted above the surface");
                AssertTrue(
                    bg2Scroll.Skip(101 - DisplayScreen.HudHeight).Distinct().Count() > 1,
                    "water distorts BG2 below the surface");
            }
            else
            {
                AssertEqual(3, bg2Scroll.Distinct().Count(),
                    $"{type} uses the native three-value heat waveform");
                for (int frameNumber = 0;
                     frameNumber < LavaAcidWaveDefinitions.VerticalWavePhaseDuration - 1;
                     frameNumber++)
                {
                    state.Step(new ArtworkFrameSelections(), new RoomLayer3FxFrame { Camera = new Position16<RoomPixels>(0x0100, 0), TimeIsFrozen = false, MainGameLoopCarry = true });
                    AssertEqual(0, (state.CaptureForDisplay() ?? throw new InvalidOperationException("Verification requires state.CaptureForDisplay().")).LavaAcidBg2WavePhase,
                        $"{type} heat wave retains phase before frame four");
                }
                state.Step(new ArtworkFrameSelections(), new RoomLayer3FxFrame { Camera = new Position16<RoomPixels>(0x0100, 0), TimeIsFrozen = false, MainGameLoopCarry = true });
                AssertEqual(15, (state.CaptureForDisplay() ?? throw new InvalidOperationException("Verification requires state.CaptureForDisplay().")).LavaAcidBg2WavePhase,
                    $"{type} heat wave rotates backward on frame four");
            }
        }
        LayerBlendingConfiguration wrongBlend = type switch
        {
            RoomFxType.Lava or RoomFxType.Acid => LayerBlendingConfiguration.NormalGameplay,
            RoomFxType.Water => LayerBlendingConfiguration.NormalGameplay,
            RoomFxType.Rain => LayerBlendingConfiguration.FogAdditive,
            RoomFxType.None or
                RoomFxType.Spores or
                RoomFxType.Fog or
                RoomFxType.ScrollingSky or
                RoomFxType.UnusedScrollingSky or
                RoomFxType.Fireflea or
                RoomFxType.TourianEntranceStatue or
                RoomFxType.CeresRidley or
                RoomFxType.CeresElevator or
                RoomFxType.CeresHaze => LayerBlendingConfiguration.Rain,
            _ => LayerBlendingConfiguration.Rain,
        };
        AssertThrows<InvalidDataException>(
            () => GameplayEffectCapture.CaptureRoomLayer3Fx(
                snapshot with { LayerBlendConfiguration = wrongBlend }, RoomBackgroundsFixtures.BlankBg3()),
            $"{type} rejects another FX type's layer-blending route");
    }

    /// <summary>Decodes a literal synthetic sixteen-byte record independently of the compiled catalog before production FX initialization.</summary>
    internal static RoomLayer3FxState LoadSyntheticRoomFx(RoomFxSession session, TestAddressSpace bus,
        GameplayColours cgram, ushort pointer) =>
        RoomLayer3FxState.ForDefinition(session, cgram, ReadSyntheticRoomFxRecord(bus, pointer), randomNumber: 0);

    /// <summary>Reads all sixteen bank-$83 record bytes in native word/byte order for the independent fixture oracle.</summary>
    private static RoomFxRecordDefinition ReadSyntheticRoomFxRecord(TestAddressSpace bus, ushort pointer)
    {
        int address = 0x830000 | pointer;
        byte Byte(int offset) => bus.ReadByte(address + offset);
        ushort Word(int offset) => LittleEndianBytes.Word(Byte(offset), Byte(offset + 1));
        ushort door = Word(0);
        if (door == ushort.MaxValue)
            return RoomFxRecordCodec.Terminator;
        byte[] native = [.. Enumerable.Range(0, RoomFxToolingRomData.Record.ByteCount).Select(Byte)];
        // The record codec decodes the fixture's bytes as the catalog generator decodes the cartridge's (#1415).
        return RoomFxRecordCodec.Decode(door == 0 ? null : DoorDefinitions.Get(door), native, $"synthetic record ${pointer:X4}");
    }

    /// <summary>Writes one byte field of the synthetic bank-$83 room FX record at <paramref name="record"/>.</summary>
    private static void SeedSyntheticRoomFxByte(TestAddressSpace bus, ushort record, int fieldOffset, byte value) =>
        WriteTestByte(bus, RoomFxRomDataBanksTooling.RoomDefinitions | unchecked((ushort)(record + fieldOffset)), value);

    /// <summary>Writes one little-endian word field of the synthetic bank-$83 room FX record at <paramref name="record"/>.</summary>
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
