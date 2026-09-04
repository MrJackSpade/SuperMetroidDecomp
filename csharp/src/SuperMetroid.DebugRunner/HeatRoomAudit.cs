using System.Globalization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Retail identities and fields used by the issue-#258 Norfair heat audit.</summary>
internal static class HeatRoomAuditDefinitions
{
    /// <summary>Business Center, the first area-$02 room, at <c>$8F:A75D</c>.</summary>
    public const ushort RoomHeader = 0xa75d;

    /// <summary>Business Center's default state at <c>$8F:A76A</c>.</summary>
    public const ushort RoomState = 0xa76a;

    /// <summary>Business Center's default FX record at <c>$83:8430</c>.</summary>
    public const ushort FxRecord = 0x8430;

    /// <summary>Cartridge-authored lava surface Y in the representative room.</summary>
    public const ushort SurfaceY = 0x00b8;

    /// <summary>All five Norfair palette objects selected by FX bitset <c>$1F</c>.</summary>
    public static ReadOnlySpan<ushort> PaletteFxDefinitions =>
        [0xf761, 0xf785, 0xf789, 0xf78d, 0xf791];

    /// <summary>Number of default Norfair room states whose FX type is lava or acid.</summary>
    public const int LavaAcidDefaultRoomCount = 63;

    /// <summary>Default Norfair lava/acid states selecting bank-$88's vertical BG2 wave.</summary>
    public const int VerticallyDistortedDefaultRoomCount = 49;
}

/// <summary>
/// Cartridge-data and rendered-pixel regression for issue #258. Heat is easy to mistake
/// for a standalone full-screen effect: bank $8D owns damage and palette animation, while
/// the visible one-pixel shimmer is the BG2VOFS program installed by lava/acid FX types.
/// </summary>
internal static class HeatRoomAudit
{
    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            HeatRoomAuditDefinitions.RoomHeader);
        AssertRepresentativeRecord(bus, room);
        AssertRetailHeatInventory(bus);

        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(
            HeatRoomAuditDefinitions.RoomHeader,
            cameraX: 0,
            cameraY: 0);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);

        RoomLayer3FxRenderSnapshot fx = runtime.DisplayedRoomLayer3Fx
            ?? throw new InvalidDataException("Business Center published no lava/acid FX snapshot.");
        if (fx.Type != RoomFxType.Lava ||
            fx.LayerBlendConfiguration != LayerBlendingConfiguration.LavaAcidAdditive ||
            fx.CurrentYPosition != HeatRoomAuditDefinitions.SurfaceY ||
            fx.WaterSurfaceScreenY != HeatRoomAuditDefinitions.SurfaceY ||
            fx.LiquidOptions != 0x000b)
        {
            throw new InvalidDataException(
                $"Business Center heat snapshot mismatch: type/blend={fx.Type}/" +
                $"${(ushort)fx.LayerBlendConfiguration:X2}, surface=" +
                $"${fx.CurrentYPosition:X4}/{fx.WaterSurfaceScreenY}, " +
                $"options=${fx.LiquidOptions:X2}.");
        }
        foreach (ushort definition in HeatRoomAuditDefinitions.PaletteFxDefinitions)
        {
            if (!runtime.RoomPaletteFx.IsDefinitionActive(definition))
                throw new InvalidDataException($"Norfair palette FX $8D:{definition:X4} is inactive.");
        }

        GameplayPpuRenderSnapshot ppu = runtime.DisplayedGameplayPpu;
        ushort[] verticalScrolls = SnesGameplayFrameRenderer.BuildLavaAcidBg2VerticalScrolls(
            fx,
            ppu.Bg2VerticalScroll) ?? throw new InvalidDataException(
                "Business Center did not publish its vertical heat-haze table.");
        Rgba32[] undistorted = SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(
            runtime.Vram,
            runtime.Cgram,
            runtime.DisplayedOam,
            ppu.Bg1HorizontalScroll,
            ppu.Bg1VerticalScroll,
            ppu.Bg2HorizontalScroll,
            ppu.Bg2VerticalScroll);
        Rgba32[] distorted = SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(
            runtime.Vram,
            runtime.Cgram,
            runtime.DisplayedOam,
            ppu.Bg1HorizontalScroll,
            ppu.Bg1VerticalScroll,
            ppu.Bg2HorizontalScroll,
            ppu.Bg2VerticalScroll,
            bg2VerticalScrollByLine: verticalScrolls);
        int changedHudPixels = CountChanges(
            undistorted,
            distorted,
            firstRow: 0,
            endRow: SnesGameplayFrameRenderer.HudHeight);
        int changedGameplayPixels = CountChanges(
            undistorted,
            distorted,
            firstRow: SnesGameplayFrameRenderer.HudHeight,
            endRow: SnesGameplayFrameRenderer.Height);
        if (changedHudPixels != 0 || changedGameplayPixels == 0 ||
            verticalScrolls.Distinct().Count() != 3)
        {
            throw new InvalidDataException(
                $"Heat distortion changed {changedHudPixels} HUD and {changedGameplayPixels} " +
                $"gameplay pixels using {verticalScrolls.Distinct().Count()} scroll values.");
        }

        AssertWaveTiming(bus, room);
        Console.WriteLine(
            "Heat FX audit passed: 49 of 63 default Norfair lava/acid states select the " +
            $"native vertical BG2 HDMA wave; Business Center changed {changedGameplayPixels} gameplay " +
            "pixels without touching the HUD, retained its $00B8 lava surface, and loaded all " +
            "five cartridge palette objects.");
        return 0;
    }

    private static void AssertRepresentativeRecord(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room)
    {
        ushort record = RoomFxRomData.SelectRecord(bus, room.State.FxPointer, doorPointer: 0);
        if (room.Identity != new RoomIdentity(AreaId.Norfair, 0x00) ||
            room.State.Pointer != HeatRoomAuditDefinitions.RoomState ||
            record != HeatRoomAuditDefinitions.FxRecord ||
            RoomFxRomData.ReadRecordWord(
                bus, record, RoomFxRomData.Record.BaseYPositionOffset) !=
                    HeatRoomAuditDefinitions.SurfaceY ||
            RoomFxRomData.ReadRecordByte(bus, record, RoomFxRomData.Record.TypeOffset) !=
                (byte)RoomFxType.Lava ||
            RoomFxRomData.ReadRecordByte(
                bus, record, RoomFxRomData.Record.Layer3LayerBlendConfigurationOffset) !=
                    (byte)LayerBlendingConfiguration.LavaAcidAdditive ||
            RoomFxRomData.ReadRecordByte(
                bus, record, RoomFxRomData.Record.LiquidOptionsOffset) != 0x0b ||
            RoomFxRomData.ReadRecordByte(
                bus, record, RoomFxRomData.Record.PaletteFxBitsetOffset) != 0x1f)
        {
            throw new InvalidDataException(
                $"Business Center selected state/FX $8F:{room.State.Pointer:X4}/$83:{record:X4} " +
                "instead of the audited Norfair heat record.");
        }
    }

    private static void AssertRetailHeatInventory(SuperMetroidAddressSpace bus)
    {
        string symbolPath = Path.GetFullPath(Path.Combine("upstream-sm", "assets", "names.txt"));
        int lavaAcidRooms = 0;
        int verticallyDistortedRooms = 0;
        var optionCounts = new Dictionary<byte, int>();
        foreach (ushort pointer in File.ReadLines(symbolPath)
                     .Select(ParseRoomPointer)
                     .Where(pointer => pointer.HasValue)
                     .Select(pointer => pointer!.Value)
                     .Distinct())
        {
            CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, pointer);
            if (room.AreaIndex != AreaId.Norfair || room.State.FxPointer == 0)
                continue;
            ushort record = RoomFxRomData.SelectRecord(bus, room.State.FxPointer, doorPointer: 0);
            if (record == 0)
                continue;
            RoomFxType type = RoomFxTypes.FromCartridge(
                RoomFxRomData.ReadRecordByte(bus, record, RoomFxRomData.Record.TypeOffset),
                $"Norfair room {room.Identity} FX record $83:{record:X4}");
            if (type is not (RoomFxType.Lava or RoomFxType.Acid))
                continue;
            lavaAcidRooms++;
            byte options = RoomFxRomData.ReadRecordByte(
                bus,
                record,
                RoomFxRomData.Record.LiquidOptionsOffset);
            optionCounts[options] = optionCounts.GetValueOrDefault(options) + 1;
            if ((options & RoomFxRomData.LavaAcid.VerticalBg2WaveOption) != 0)
                verticallyDistortedRooms++;
        }
        Console.WriteLine(
            "  Norfair lava/acid liquid-option distribution: " +
            string.Join(", ", optionCounts.OrderBy(pair => pair.Key)
                .Select(pair => $"${pair.Key:X2}={pair.Value}")));
        if (lavaAcidRooms != HeatRoomAuditDefinitions.LavaAcidDefaultRoomCount ||
            verticallyDistortedRooms !=
                HeatRoomAuditDefinitions.VerticallyDistortedDefaultRoomCount)
        {
            throw new InvalidDataException(
                $"Found {lavaAcidRooms} default Norfair lava/acid rooms and " +
                $"{verticallyDistortedRooms} vertical-wave rooms; expected " +
                $"{HeatRoomAuditDefinitions.LavaAcidDefaultRoomCount} and " +
                $"{HeatRoomAuditDefinitions.VerticallyDistortedDefaultRoomCount}.");
        }
    }

    private static void AssertWaveTiming(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room)
    {
        var state = new RoomLayer3FxState();
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        state.Load(bus, vram, cgram, room.State.FxPointer, doorPointer: 0, randomNumber: 0);
        state.PrimeViewport(cameraX: 0, cameraY: 0);
        if (state.CaptureForDisplay()!.Value.LavaAcidBg2WavePhase != 0)
            throw new InvalidDataException("Lava/acid wave did not start at native phase zero.");
        for (int frame = 0; frame < RoomFxRomData.LavaAcid.VerticalWavePhaseDuration - 1; frame++)
        {
            state.Step(bus, vram, cameraX: 0, cameraY: 0, timeIsFrozen: false);
            if (state.CaptureForDisplay()!.Value.LavaAcidBg2WavePhase != 0)
                throw new InvalidDataException($"Lava/acid wave advanced early on frame {frame + 1}.");
        }
        state.Step(bus, vram, cameraX: 0, cameraY: 0, timeIsFrozen: false);
        if (state.CaptureForDisplay()!.Value.LavaAcidBg2WavePhase != 15)
            throw new InvalidDataException("Lava/acid wave did not rotate backward after four frames.");
    }

    private static int CountChanges(Rgba32[] first, Rgba32[] second, int firstRow, int endRow)
    {
        int count = 0;
        for (int y = firstRow; y < endRow; y++)
        {
            int row = y * SnesGameplayFrameRenderer.Width;
            for (int x = 0; x < SnesGameplayFrameRenderer.Width; x++)
                count += first[row + x] != second[row + x] ? 1 : 0;
        }
        return count;
    }

    private static ushort? ParseRoomPointer(string line)
    {
        string[] fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (fields.Length != 2 || !fields[0].StartsWith("0x8f", StringComparison.Ordinal) ||
            !fields[1].StartsWith("kRoom_", StringComparison.Ordinal) ||
            fields[1].Contains("_DoorOuts", StringComparison.Ordinal))
        {
            return null;
        }
        return ushort.TryParse(
            fields[1]["kRoom_".Length..],
            NumberStyles.AllowHexSpecifier,
            CultureInfo.InvariantCulture,
            out ushort pointer) && pointer != 0xe82c
                ? pointer
                : null;
    }
}
