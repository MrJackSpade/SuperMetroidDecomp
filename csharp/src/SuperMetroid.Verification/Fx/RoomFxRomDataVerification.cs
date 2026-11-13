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
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Progression;

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
        AssertEqual(11, RoomFxRomDataRecordTooling.EffectBlendOffset,
            "room FX effect blend field");
        AssertEqual(15, RoomFxRomDataRecordTooling.PaletteBlendOffset,
            "room FX palette-blend field");

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
        Suite(nameof(VerifyRoomFxBlendDecoding), () => VerifyRoomFxBlendDecoding(rom));
    }

    /// <summary>Matches accepted FX type bytes to native dispatch slots while rejecting unowned RTL aliases.</summary>
    private static void VerifyRoomFxTypeValidation(ISnesAddressSpace rom)
    {
        // Original pre-conversion enum membership, independently cross-checked with
        // the native function pointer table. Zero is deliberately named; the other
        // entries targeting the same RTL were never accepted by this port boundary, nor is
        // the alternate scrolling sky's, which no retail record selects (#1686).
        ushort[] original = [0, 2, 4, 6, 8, 10, 12, 32, 36, 38, 40, 42, 44];
        const int AlternateScrollingSkySlot = 34;
        // The identities' values are arbitrary (#1415); each member's dispatcher byte is the import's native word.
        AssertTrue(Enum.GetValues<RoomFxType>().Select(SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.Types.Word).SequenceEqual(original),
            "All named room FX identities name their original dispatcher bytes");
        AssertEqual((ushort)0xac18, ReadVerificationWord(rom, 0x89ac49), "Native room FX dispatcher table operand");
        for (int slot = 0; slot <= 44; slot += 2)
        {
            ushort pointer = ReadVerificationWord(rom, 0x83ac18 + slot);
            AssertEqual(original.Contains((ushort)slot), slot == 0 || (pointer != 0xb278 && slot != AlternateScrollingSkySlot),
                "Original named membership matches native routines excluding unnamed RTL slots and the unselected sky");
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

    /// <summary>
    /// Checks the import's reading of a record's two blend bytes (#1545): each byte a retail record holds is an entry of
    /// the game's blending dispatcher; the default byte reads as the visor's animation, and the effect byte as the blend
    /// its effect shows, every pairing an effect does not show and every other byte rejected with its context.
    /// </summary>
    private static void VerifyRoomFxBlendDecoding(ISnesAddressSpace rom)
    {
        // The dispatcher bytes the retail records hold, independently of the codec: the default bytes, and each effect's
        // effect bytes with the blend each shows.
        var visorBytes = new Dictionary<int, RoomVisorAnimation>
        {
            [0x00] = RoomVisorAnimation.Steady, [0x02] = RoomVisorAnimation.Steady, [0x08] = RoomVisorAnimation.Steady,
            [0x28] = RoomVisorAnimation.Animated, [0x2a] = RoomVisorAnimation.Animated,
        };
        var water = new Dictionary<int, RoomEffectBlend>
        {
            [0x14] = RoomEffectBlendDefinitions.OverlayDarkens,
            [0x16] = RoomEffectBlendDefinitions.WaterfallDarkensBehind,
            [0x18] = RoomEffectBlendDefinitions.OverlayBrightens,
        };
        Dictionary<int, RoomEffectBlend?> Shows(RoomFxType type) => type switch
        {
            RoomFxType.Lava or RoomFxType.Acid => new() { [0x1e] = RoomEffectBlendDefinitions.OverlayBrightens },
            RoomFxType.Water or RoomFxType.TourianEntranceStatue => water.ToDictionary(entry => entry.Key, entry => (RoomEffectBlend?)entry.Value),
            RoomFxType.Rain => new() { [0x0e] = RoomEffectBlendDefinitions.OverlayBrightens },
            RoomFxType.Fog => new() { [0x30] = RoomEffectBlendDefinitions.OverlayBrightens },
            RoomFxType.Spores => new() { [0x0a] = RoomEffectBlendDefinitions.SporesBrightenBehind },
            RoomFxType.None or RoomFxType.ScrollingSky or RoomFxType.Fireflea or
                RoomFxType.CeresRidley or RoomFxType.CeresElevator or RoomFxType.CeresHaze => new() { [0x00] = null, [0x02] = null },
            _ => throw new InvalidOperationException($"Unexpected room FX {type}."),
        };
        AssertEqual((ushort)0x803e, ReadVerificationWord(rom, 0x88800e), "Native blending dispatcher operand");
        AssertEqual((ushort)0x8074, ReadVerificationWord(rom, 0x88803e), "First target marks original table end");
        foreach (int offset in visorBytes.Keys.Concat(Enum.GetValues<RoomFxType>().SelectMany(type => Shows(type).Keys)).Distinct())
        {
            ushort target = ReadVerificationWord(rom, 0x88803e + offset);
            AssertTrue(target is >= 0x8074 and <= 0x8156, "Read blend byte is a slot of the native blending dispatcher");
        }
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            if (visorBytes.TryGetValue(value, out RoomVisorAnimation visor))
                AssertEqual(visor, RoomFxRecordCodec.VisorAnimation((byte)value, "record $9000"), "default blend byte reads as the visor's animation");
            else
                AssertBlendRejected(value, () => RoomFxRecordCodec.VisorAnimation((byte)value, "record $9000"));
            foreach (RoomFxType type in Enum.GetValues<RoomFxType>())
            {
                if (Shows(type).TryGetValue(value, out RoomEffectBlend? blend))
                    AssertEqual(blend, RoomFxRecordCodec.EffectBlend(type, (byte)value, "record $9000"), $"{type} effect blend byte reads as its blend");
                else
                    AssertBlendRejected(value, () => RoomFxRecordCodec.EffectBlend(type, (byte)value, "record $9000"));
            }
        }

        static void AssertBlendRejected(int value, Action read)
        {
            var error = AssertThrows<InvalidDataException>(read, "Unread blend byte rejected");
            AssertTrue(error.Message.Contains($"{value:X2}", StringComparison.Ordinal) &&
                error.Message.Contains("$9000", StringComparison.Ordinal), "Blend failure retains value/context");
        }
    }

    /// <summary>Checks the distinct BG3 routes and the blends the import reads for liquid, rain and fog.</summary>
    private static void VerifyRoomLayer3FxTypes()
    {
        var bus = new TestAddressSpace();
        var stateSession = new RoomFxSession();
        var state = new RoomFxOverlayState(stateSession);
        const ushort record = 0x9400;
        SeedSyntheticRoomFxWord(bus, record, RoomFxRomDataRecordToolingConstants.DoorPointerOffset, 0);
        SeedSyntheticRoomFxByte(bus, record,
            RoomFxRomDataRecordTooling.EffectBlendOffset, RoomFxRecordCodec.NormalBlendByte);

        RoomFxType[] nonFxOverlayTypes =
        [
            RoomFxType.ScrollingSky,
            RoomFxType.CeresHaze,
            RoomFxType.CeresRidley,
            RoomFxType.CeresElevator,
        ];
        foreach (RoomFxType type in nonFxOverlayTypes)
        {
            SeedSyntheticRoomFxByte(bus, record, RoomFxRomDataRecordTooling.TypeOffset, RoomFxRecordEncoding.TypeByte(type));
            state = LoadSyntheticRoomFx(stateSession, bus, record);
            AssertEqual(type, state.Type, $"{type} FX record type");
            AssertTrue(state.Blend is null, $"{type} describes no effect blend");
            AssertTrue(!state.IsRenderable, $"{type} does not impersonate rain/fog BG3");
        }

        Suite(nameof(VerifyRenderableLayer3Fx), () => VerifyRenderableLayer3Fx(bus, stateSession, record, RoomFxType.Lava));
        Suite(nameof(VerifyRenderableLayer3Fx), () => VerifyRenderableLayer3Fx(bus, stateSession, record, RoomFxType.Acid));
        Suite(nameof(VerifyRenderableLayer3Fx), () => VerifyRenderableLayer3Fx(bus, stateSession, record, RoomFxType.Water));
        Suite(nameof(VerifyRenderableLayer3Fx), () => VerifyRenderableLayer3Fx(bus, stateSession, record, RoomFxType.Rain));
        Suite(nameof(VerifyRenderableLayer3Fx), () => VerifyRenderableLayer3Fx(bus, stateSession, record, RoomFxType.Fog));
    }

    /// <summary>Captures a synthetic BG3 effect and checks its surface, wave tables, hidden-surface sentinel and type-specific blend routing.</summary>
    private static void VerifyRenderableLayer3Fx(
        TestAddressSpace bus,
        RoomFxSession session,
        ushort record,
        RoomFxType type)
    {
        SeedSyntheticRoomFxByte(bus, record, RoomFxRomDataRecordTooling.TypeOffset, RoomFxRecordEncoding.TypeByte(type));
        byte layerBlend = type switch
        {
            RoomFxType.Lava or RoomFxType.Acid => RoomFxRecordCodec.LavaAcidBlendByte,
            RoomFxType.Water => RoomFxRecordCodec.LiquidBrightensBlendByte,
            RoomFxType.Rain => RoomFxRecordCodec.RainBlendByte,
            RoomFxType.Fog => RoomFxRecordCodec.FogBlendByte,
            RoomFxType.None or
                RoomFxType.Spores or
                RoomFxType.ScrollingSky or
                RoomFxType.Fireflea or
                RoomFxType.TourianEntranceStatue or
                RoomFxType.CeresRidley or
                RoomFxType.CeresElevator or
                RoomFxType.CeresHaze => throw new InvalidOperationException($"Unexpected renderable room FX {type}."),
            _ => throw new InvalidOperationException($"Unexpected renderable room FX {type}."),
        };
        SeedSyntheticRoomFxByte(bus, record,
            RoomFxRomDataRecordTooling.EffectBlendOffset, layerBlend);

        RoomFxOverlayState state = LoadSyntheticRoomFx(session, bus, record);
        Suite(nameof(VerifyRoomFxSharedRandomState), () => state = VerifyRoomFxSharedRandomState(bus, session, state, record, type));
        AssertEqual(FxOverlayPlaneConstants.ClearedCell.PackedWord,
            state.EffectPlane.WordsIn(RetailImportPresentationFixture().RoomFxPages)[FxOverlayPlaneConstants.HudRows * FxOverlayPlaneConstants.Columns],
            $"{type} room shows the cleared word in the first gameplay BG3 FX word");
        if (type is RoomFxType.Water or RoomFxType.Lava or RoomFxType.Acid)
        {
            const ushort surfaceY = 100;
            SeedSyntheticRoomFxByte(bus, record, RoomFxRomDataRecordTooling.LiquidOptionsOffset,
                type == RoomFxType.Water
                    ? (byte)3
                    : (byte)RoomFxNativeSources.VerticalBg2WaveOption);
            SeedSyntheticRoomFxWord(bus, record, RoomFxRomDataRecordTooling.BaseYPositionOffset, surfaceY);
            state = LoadSyntheticRoomFx(session, bus, record);
            state.PrimeViewport(new Position16<RoomPixels>(0x0100, 0));
        }
        AssertEqual(type, state.Type, $"{type} layer-three type");
        AssertTrue(state.IsRenderable, $"{type} owns a translated layer-three plane");
        RoomFxOverlayRenderSnapshot snapshot = (state.CaptureForDisplay() ?? throw new InvalidOperationException("Verification requires state.CaptureForDisplay()."));
        AssertEqual(
            RoomEffectBlendDefinitions.OverlayBrightens,
            snapshot.Blend,
            $"{type} render snapshot retains the blend the import read");

        if (type is RoomFxType.Water or RoomFxType.Lava or RoomFxType.Acid)
        {
            AssertEqual(100, snapshot.WaterSurfaceScreenY,
                $"{type} surface retains room-relative scanline");
            // The seeded records select a drifting, BG2-waving water surface and lava/acid's vertical heat wave.
            RoomLiquid expectedLiquid = type == RoomFxType.Water
                ? new RoomWater(LiquidTide.None, SurfaceDrift: true, BackgroundWave: true, IgnoresSurfaceUntilEnabled: false)
                : new RoomLavaOrAcid(LiquidTide.None, HeatHazeDirection.Vertical);
            AssertEqual(expectedLiquid, snapshot.Liquid,
                $"{type} render snapshot retains its liquid's waves");
            AssertEqual<int?>(100, snapshot.LiquidSurfaceLine,
                $"{type} places its liquid on the screen from surface line 100");

            // A liquid with no surface, or one above the room's top, shows no BG3 liquid plane at all (#1546).
            AssertTrue(GameplayEffectCapture.CaptureRoomFxOverlay(snapshot with
                {
                    Surface = null,
                    WaterSurfaceScreenY = null,
                }, RoomBackgroundsFixtures.BlankFxOverlay()) is null,
                $"{type} without a surface shows no BG3 liquid plane");
            AssertTrue(GameplayEffectCapture.CaptureRoomFxOverlay(snapshot with { Surface = -1 },
                    RoomBackgroundsFixtures.BlankFxOverlay()) is null,
                $"{type} surface above the room's top shows no BG3 liquid plane");

            // The described BG2 wave (#1468), evaluated on each gameplay line.
            ushort[] backgroundScroll = ScrollShapeLines.GameplayScrolls(type == RoomFxType.Water
                ? GameplayEffectCapture.BuildWaterBackgroundHorizontalScrolls(
                    snapshot,
                    backgroundScroll: new Position16<BackgroundPixels>(0x0200, 0)) ?? throw new InvalidDataException(
                        "Wavy water did not publish a BG2 HDMA scroll table.")
                : GameplayEffectCapture.BuildLavaAcidBackgroundVerticalScrolls(
                    snapshot,
                    backgroundVerticalScroll: 0x0200) ?? throw new InvalidDataException(
                        $"{type} did not publish its BG2 HDMA scroll table."));
            AssertEqual(DisplayScreen.Height - DisplayScreen.HudHeight,
                backgroundScroll.Length,
                $"{type} BG2 wave table covers gameplay scanlines");
            if (type == RoomFxType.Water)
            {
                AssertEqual((ushort)0x0200,
                    backgroundScroll[90 - DisplayScreen.HudHeight],
                    "water leaves BG2 undistorted above the surface");
                AssertTrue(
                    backgroundScroll.Skip(101 - DisplayScreen.HudHeight).Distinct().Count() > 1,
                    "water distorts BG2 below the surface");
            }
            else
            {
                AssertEqual(3, backgroundScroll.Distinct().Count(),
                    $"{type} uses the native three-value heat waveform");
                for (int frameNumber = 0;
                     frameNumber < LavaAcidWaveDefinitions.VerticalWavePhaseDuration - 1;
                     frameNumber++)
                {
                    state.Step(new ArtworkFrameSelections(), new RoomFxOverlayFrame { Camera = new Position16<RoomPixels>(0x0100, 0), TimeIsFrozen = false, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame });
                    AssertEqual(0, (state.CaptureForDisplay() ?? throw new InvalidOperationException("Verification requires state.CaptureForDisplay().")).LavaAcidBackgroundWavePhase,
                        $"{type} heat wave retains phase before frame four");
                }
                state.Step(new ArtworkFrameSelections(), new RoomFxOverlayFrame { Camera = new Position16<RoomPixels>(0x0100, 0), TimeIsFrozen = false, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame });
                AssertEqual(15, (state.CaptureForDisplay() ?? throw new InvalidOperationException("Verification requires state.CaptureForDisplay().")).LavaAcidBackgroundWavePhase,
                    $"{type} heat wave rotates backward on frame four");
            }
        }
        byte wrongBlend = type switch
        {
            RoomFxType.Lava or RoomFxType.Acid => RoomFxRecordCodec.NormalBlendByte,
            RoomFxType.Water => RoomFxRecordCodec.NormalBlendByte,
            RoomFxType.Rain => RoomFxRecordCodec.FogBlendByte,
            RoomFxType.None or
                RoomFxType.Spores or
                RoomFxType.Fog or
                RoomFxType.ScrollingSky or
                RoomFxType.Fireflea or
                RoomFxType.TourianEntranceStatue or
                RoomFxType.CeresRidley or
                RoomFxType.CeresElevator or
                RoomFxType.CeresHaze => RoomFxRecordCodec.RainBlendByte,
            _ => RoomFxRecordCodec.RainBlendByte,
        };
        AssertThrows<InvalidDataException>(
            () => RoomFxRecordCodec.EffectBlend(type, wrongBlend, "synthetic record"),
            $"{type} rejects another FX type's blend at import");
    }

    /// <summary>Decodes a literal synthetic sixteen-byte record independently of the compiled catalog before production FX initialization.</summary>
    internal static RoomFxOverlayState LoadSyntheticRoomFx(RoomFxSession session, TestAddressSpace bus,
        ushort pointer) =>
        RoomFxOverlayState.ForDefinition(session, ReadSyntheticRoomFxRecord(bus, pointer), randomNumber: 0);

    /// <summary>Reads all sixteen bank-$83 record bytes in native word/byte order for the independent fixture oracle.</summary>
    private static RoomFxRecordDefinition ReadSyntheticRoomFxRecord(TestAddressSpace bus, ushort pointer)
    {
        int address = 0x830000 | pointer;
        byte Byte(int offset) => bus.ReadByte(address + offset);
        ushort Word(int offset) => LittleEndianBytes.Word(Byte(offset), Byte(offset + 1));
        // A terminator ends a header's list and is no record (#1546).
        if (Word(0) == ushort.MaxValue)
            throw new InvalidOperationException($"Synthetic record ${pointer:X4} is a terminator, which holds no record.");
        byte[] native = [.. Enumerable.Range(0, RoomFxToolingRomData.Record.ByteCount).Select(Byte)];
        // The record codec decodes the fixture's bytes as the catalog generator decodes the cartridge's (#1415). The fixtures
        // seed no effect bits and belong to no room, so the area their bits would select from is never consulted (#1526).
        if (native[RoomFxRomDataRecordTooling.PaletteFxBitsetOffset] != 0 || native[RoomFxRomDataRecordTooling.AnimatedTileBitsetOffset] != 0)
            throw new InvalidOperationException($"Synthetic record ${pointer:X4} seeds effect bits, which select from no room's area.");
        return RoomFxRecordCodec.Decode(AreaId.Crateria, native, $"synthetic record ${pointer:X4}");
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
