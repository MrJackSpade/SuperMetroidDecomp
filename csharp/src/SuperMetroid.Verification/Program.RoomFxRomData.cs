using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    /// <summary>
    /// Verifies the shared FX-record schema, representative native type discriminants,
    /// typed blending state, and both renderable layer-three table lookups.
    /// </summary>
    static void VerifyRoomFxRomData()
    {
        AssertEqual(16, RoomFxRomData.Record.ByteCount, "room FX record width");
        AssertEqual(9, RoomFxRomData.Record.TypeOffset, "room FX type field");
        AssertEqual(11, RoomFxRomData.Record.Layer3LayerBlendConfigurationOffset,
            "room FX layer-three blend field");
        AssertEqual(15, RoomFxRomData.Record.PaletteBlendOffset,
            "room FX palette-blend field");
        AssertEqual(23, RoomFxRomData.ScrollingSky.Sections.Length,
            "scrolling-sky section count");
        AssertEqual(6, RoomFxRomData.ScrollingSky.LandChunkOffsets.Length,
            "scrolling-sky native land/ocean fall-through count");

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

        VerifyRoomFxRecordSelection();
        VerifyRoomLayer3FxTypes();
        NotSupportedException unknown = AssertThrows<NotSupportedException>(
            () => RoomFxTypes.FromCartridge(0x0e, "constructed FX record $9000"),
            "null room-FX dispatcher entry fails loudly");
        AssertTrue(unknown.Message.Contains("$0E", StringComparison.Ordinal) &&
            unknown.Message.Contains("$9000", StringComparison.Ordinal),
            "unknown room FX identifies value and record context");
        NotSupportedException unknownBlend = AssertThrows<NotSupportedException>(
            () => LayerBlendingConfigurations.FromCartridge(
                0x36, "constructed FX record $9000"),
            "past-table layer-blending configuration fails loudly");
        AssertTrue(unknownBlend.Message.Contains("0036", StringComparison.Ordinal) &&
            unknownBlend.Message.Contains("$9000", StringComparison.Ordinal),
            "unknown layer blend identifies value and record context");

        Console.WriteLine(
            "  Room FX: shared record/table catalog, typed blending, liquid, sky, haze, " +
            "Ceres, rain, and fog states agree.");
    }

    private static void VerifyRoomFxRecordSelection()
    {
        var bus = new TestAddressSpace();
        const ushort list = 0x9000;
        const ushort matchingDoor = 0x8123;
        WriteTestWord(
            bus,
            RoomFxRomData.Banks.RoomDefinitions | list,
            matchingDoor);
        WriteTestWord(
            bus,
            RoomFxRomData.Banks.RoomDefinitions | (list + RoomFxRomData.Record.ByteCount),
            0);

        AssertEqual(list, RoomFxRomData.SelectRecord(bus, list, matchingDoor),
            "door-specific FX record wins");
        AssertEqual(list + RoomFxRomData.Record.ByteCount,
            RoomFxRomData.SelectRecord(bus, list, 0x8456),
            "default FX record follows nonmatching door record");

        const ushort terminatedList = 0x9200;
        WriteTestWord(
            bus,
            RoomFxRomData.Banks.RoomDefinitions | terminatedList,
            RoomFxRomData.Record.TerminatorDoorPointer);
        AssertEqual(0, RoomFxRomData.SelectRecord(bus, terminatedList, matchingDoor),
            "FX terminator declines selection");
    }

    private static void VerifyRoomLayer3FxTypes()
    {
        var bus = new TestAddressSpace();
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        var state = new RoomLayer3FxState();
        const ushort record = 0x9400;
        int recordAddress = RoomFxRomData.Banks.RoomDefinitions | record;
        WriteTestWord(bus, recordAddress + RoomFxRomData.Record.DoorPointerOffset, 0);
        bus.WriteByte(
            recordAddress + RoomFxRomData.Record.Layer3LayerBlendConfigurationOffset,
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
            bus.WriteByte(recordAddress + RoomFxRomData.Record.TypeOffset, (byte)type);
            state.Load(bus, vram, cgram, record, doorPointer: 0, randomNumber: 0);
            AssertEqual(type, state.Type, $"{type} FX record type");
            AssertEqual(
                LayerBlendingConfiguration.NormalGameplay,
                state.LayerBlendConfiguration,
                $"{type} typed layer-blending configuration");
            AssertTrue(!state.IsRenderable, $"{type} does not impersonate rain/fog BG3");
        }

        VerifyRenderableLayer3Fx(bus, vram, cgram, state, record, RoomFxType.Lava);
        VerifyRenderableLayer3Fx(bus, vram, cgram, state, record, RoomFxType.Acid);
        VerifyRenderableLayer3Fx(bus, vram, cgram, state, record, RoomFxType.Water);
        VerifyRenderableLayer3Fx(bus, vram, cgram, state, record, RoomFxType.Rain);
        VerifyRenderableLayer3Fx(bus, vram, cgram, state, record, RoomFxType.Fog);
    }

    private static void VerifyRenderableLayer3Fx(
        TestAddressSpace bus,
        SnesVram vram,
        SnesCgram cgram,
        RoomLayer3FxState state,
        ushort record,
        RoomFxType type)
    {
        const ushort tilemapPointer = 0x9800;
        int typeIndex = ((byte)type) >> 1;
        WriteTestWord(
            bus,
            RoomFxRomData.Tables.Layer3TilemapPointers + typeIndex * sizeof(ushort),
            tilemapPointer);
        bus.WriteByte(
            RoomFxRomData.Banks.RoomDefinitions |
                unchecked((ushort)(record + RoomFxRomData.Record.TypeOffset)),
            (byte)type);
        LayerBlendingConfiguration layerBlend = type switch
        {
            RoomFxType.Lava or RoomFxType.Acid =>
                LayerBlendingConfiguration.LavaAcidAdditive,
            RoomFxType.Water => LayerBlendingConfiguration.LiquidOrFogAdditive,
            RoomFxType.Rain => LayerBlendingConfiguration.Rain,
            RoomFxType.Fog => LayerBlendingConfiguration.FogAdditive,
            _ => throw new InvalidOperationException($"Unexpected renderable room FX {type}."),
        };
        bus.WriteByte(
            RoomFxRomData.Banks.RoomDefinitions |
                unchecked((ushort)(record +
                    RoomFxRomData.Record.Layer3LayerBlendConfigurationOffset)),
            (byte)layerBlend);

        state.Load(bus, vram, cgram, record, doorPointer: 0, randomNumber: 0);
        if (type is RoomFxType.Water or RoomFxType.Lava or RoomFxType.Acid)
        {
            const ushort surfaceY = 100;
            bus.WriteByte(
                RoomFxRomData.Banks.RoomDefinitions |
                    unchecked((ushort)(record + RoomFxRomData.Record.LiquidOptionsOffset)),
                type == RoomFxType.Water
                    ? (byte)3
                    : (byte)RoomFxRomData.LavaAcid.VerticalBg2WaveOption);
            WriteTestWord(
                bus,
                RoomFxRomData.Banks.RoomDefinitions |
                    unchecked((ushort)(record + RoomFxRomData.Record.BaseYPositionOffset)),
                surfaceY);
            state.Load(bus, vram, cgram, record, doorPointer: 0, randomNumber: 0);
            state.PrimeViewport(cameraX: 0x0100, cameraY: 0);
        }
        AssertEqual(type, state.Type, $"{type} layer-three type");
        AssertTrue(state.IsRenderable, $"{type} owns a translated layer-three plane");
        RoomLayer3FxRenderSnapshot snapshot = state.CaptureForDisplay()!.Value;
        AssertEqual(
            layerBlend,
            snapshot.LayerBlendConfiguration,
            $"{type} render snapshot retains typed blending");

        if (type is RoomFxType.Water or RoomFxType.Lava or RoomFxType.Acid)
        {
            AssertEqual(100, snapshot.WaterSurfaceScreenY,
                $"{type} surface retains room-relative scanline");
            ushort expectedOptions = type == RoomFxType.Water
                ? (ushort)3
                : RoomFxRomData.LavaAcid.VerticalBg2WaveOption;
            AssertEqual(expectedOptions, snapshot.LiquidOptions,
                $"{type} render snapshot retains its wave options");

            // Give the liquid page one opaque two-bit tile and a visible blue color. The
            // exact surface assertion proves the compositor leaves the air row untouched
            // and applies the cartridge layer-three plane below it.
            vram.ExecuteWordTransfer(
                Enumerable.Repeat((ushort)1, 32 * 32).ToArray(),
                SnesPpuLayout.RoomFxTilemapWord,
                wordIncrement: 1);
            var character = new byte[16];
            for (int row = 0; row < 8; row++)
                character[row * 2] = 0xff;
            vram.LoadBytes((0x4000 + 8) * 2, character);
            cgram.SetColor(1, 0x7c00);
        }

        // The software compositor now consumes the same typed dispatcher identity as the
        // room loader. Rain and fog use different main/subscreen ownership in hardware even
        // though both resolve to additive BG3 pixels, so neither pairing may be substituted.
        var frame = new Rgba32[
            SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height];
        SnesGameplayFrameRenderer.ApplyRoomLayer3FxColorMath(
            frame, vram, cgram, snapshot);
        if (type is RoomFxType.Water or RoomFxType.Lava or RoomFxType.Acid)
        {
            int above = 90 * SnesGameplayFrameRenderer.Width + 20;
            int below = 110 * SnesGameplayFrameRenderer.Width + 20;
            AssertEqual(new Rgba32(0, 0, 0, 0), frame[above],
                $"{type} leaves pixels above its surface untouched");
            AssertTrue(frame[below].B != 0,
                $"{type} applies its visible BG3 color below the surface");

            ushort[] bg2Scroll = type == RoomFxType.Water
                ? SnesGameplayFrameRenderer.BuildWaterBg2HorizontalScrolls(
                    snapshot,
                    bg2HorizontalScroll: 0x0200,
                    bg2VerticalScroll: 0) ?? throw new InvalidDataException(
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
                    state.Step(bus, vram, cameraX: 0x0100, cameraY: 0, timeIsFrozen: false);
                    AssertEqual(0, state.CaptureForDisplay()!.Value.LavaAcidBg2WavePhase,
                        $"{type} heat wave retains phase before frame four");
                }
                state.Step(bus, vram, cameraX: 0x0100, cameraY: 0, timeIsFrozen: false);
                AssertEqual(15, state.CaptureForDisplay()!.Value.LavaAcidBg2WavePhase,
                    $"{type} heat wave rotates backward on frame four");
            }
        }
        LayerBlendingConfiguration wrongBlend = type switch
        {
            RoomFxType.Lava or RoomFxType.Acid => LayerBlendingConfiguration.NormalGameplay,
            RoomFxType.Water => LayerBlendingConfiguration.NormalGameplay,
            RoomFxType.Rain => LayerBlendingConfiguration.FogAdditive,
            _ => LayerBlendingConfiguration.Rain,
        };
        AssertThrows<InvalidDataException>(
            () => SnesGameplayFrameRenderer.ApplyRoomLayer3FxColorMath(
                frame,
                vram,
                cgram,
                snapshot with { LayerBlendConfiguration = wrongBlend }),
            $"{type} rejects another FX type's layer-blending route");
    }
}
