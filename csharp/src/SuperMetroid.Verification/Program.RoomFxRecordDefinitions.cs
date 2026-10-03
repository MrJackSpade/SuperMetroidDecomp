using System.Text;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static readonly string RoomFxGeneratedPath = Path.GetFullPath(Path.Combine(
        "csharp", "src", "SuperMetroid.Core", "Game", "RoomFxRecordDefinitions.Generated.cs"));

    /// <summary>One-time code generator for typed FX setup data from the pinned retail ROM.</summary>
    private static void GenerateRoomFxRecordDefinitions(string romPath)
    {
        if (File.Exists(RoomFxGeneratedPath))
            throw new IOException($"Refusing to overwrite existing compiled definitions: {RoomFxGeneratedPath}");
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(romPath);
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bus.Rom)), "FX oracle revision");
        SortedDictionary<ushort, RoomFxRecordDefinition> records = CaptureRetailRoomFxRecords(bus);
        File.WriteAllText(RoomFxGeneratedPath, RenderRoomFxDefinitions(records.Values));
        Console.WriteLine($"Generated {records.Count} typed room-FX records at {RoomFxGeneratedPath}.");
    }

    /// <summary>Checks the complete generated catalog and every source field against the ROM.</summary>
    private static void VerifyRoomFxRecordDefinitions(string romPath)
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(romPath);
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bus.Rom)), "FX oracle revision");
        SortedDictionary<ushort, RoomFxRecordDefinition> records = CaptureRetailRoomFxRecords(bus);
        VerifyRoomFxFields(bus, records);
        VerifyRoomFxListSelection(bus);
        string generated = RenderRoomFxDefinitions(records.Values).Replace("\r\n", "\n");
        string checkedIn = File.ReadAllText(RoomFxGeneratedPath).Replace("\r\n", "\n");
        AssertEqual(generated, checkedIn,
            "checked-in room-FX catalog is deterministic from pinned cartridge and all retail states");
        VerifyCompiledCeresRoomFxConsumers(bus);
        Console.WriteLine($"Compiled room FX: {records.Count} typed records across {RoomStateDefinitions.All.Count()} room states match every native field.");
    }

    private static void VerifyCompiledCeresRoomFxConsumers(SuperMetroidAddressSpace bus)
    {
        LoadStationEntry station = LoadStationDefinitions.Get(AreaId.Ceres, 0);
        CartridgeRoomHeader room = SuperMetroid.AssetExtraction.CartridgeRoomHeaderImporter.Load(bus, station.RoomPointer);
        var guarded = new RoomFxRecordReadGuard(bus);
        var nativeVram = new SnesVram();
        var compiledVram = new SnesVram();
        var nativeCgram = new SnesCgram();
        var compiledCgram = new SnesCgram();
        RoomFxPaletteBlendCatalog colors = RoomFxPaletteBlendCatalog.Load(
            new MemoryStream(RoomFxPaletteBlendExtractor.Extract(bus)));
        RoomFxLayer3TilemapCatalog tilemaps = RoomFxLayer3TilemapCatalog.Load(
            new MemoryStream(RoomFxLayer3TilemapExtractor.Extract(bus)));
        var nativeFx = new RoomLayer3FxState { PaletteBlendColors = colors, Layer3Tilemaps = tilemaps };
        var compiledFx = new RoomLayer3FxState { PaletteBlendColors = colors, Layer3Tilemaps = tilemaps };
        nativeFx.Load(bus, nativeVram, nativeCgram, room.State.FxPointer,
            station.DoorPointer, randomNumber: 0, room.Pointer);
        compiledFx.Load(guarded, compiledVram, compiledCgram, room.State.FxPointer,
            station.DoorPointer, randomNumber: 0, room.Pointer);
        AssertEqual(nativeFx.Type, compiledFx.Type, "Ceres compiled FX type");
        AssertEqual(nativeFx.BaseYPosition, compiledFx.BaseYPosition, "Ceres compiled FX base Y");
        AssertEqual(nativeFx.TargetYPosition, compiledFx.TargetYPosition, "Ceres compiled FX target Y");
        AssertEqual(nativeFx.PackedYVelocity, compiledFx.PackedYVelocity,
            "Ceres compiled FX velocity");
        AssertEqual(nativeFx.Timer, compiledFx.Timer, "Ceres compiled FX timer");
        AssertEqual(nativeFx.LiquidOptions, compiledFx.LiquidOptions,
            "Ceres compiled FX liquid options");
        AssertTrue(nativeVram.Bytes.SequenceEqual(compiledVram.Bytes),
            "Ceres compiled FX retains VRAM output");
        AssertTrue(nativeCgram.Colors.SequenceEqual(compiledCgram.Colors),
            "Ceres compiled FX retains CGRAM output");

        var nativePalette = new RoomPaletteFxSystem();
        var compiledPalette = new RoomPaletteFxSystem();
        nativePalette.LoadRoom(bus, room.State.FxPointer, station.DoorPointer,
            room.AreaIndex, equippedItems: 0, areaMiniBossDefeated: false);
        compiledPalette.LoadRoom(guarded, room.State.FxPointer, station.DoorPointer,
            room.AreaIndex, equippedItems: 0, areaMiniBossDefeated: false);
        AssertEqual(nativePalette.ActiveCount, compiledPalette.ActiveCount,
            "Ceres compiled palette-FX object count");

        var nativeSand = new RoomSandAnimatedTilesState();
        var compiledSand = new RoomSandAnimatedTilesState();
        nativeSand.LoadRoom(bus, room.State.FxPointer, station.DoorPointer, room.AreaIndex);
        compiledSand.LoadRoom(guarded, room.State.FxPointer, station.DoorPointer,
            room.AreaIndex);
        AssertEqual(nativeSand.Count, compiledSand.Count,
            "Ceres compiled sand animation count");

        var nativeTreadmills = new RoomTreadmillAnimatedTilesState();
        var compiledTreadmills = new RoomTreadmillAnimatedTilesState();
        nativeTreadmills.LoadRoom(bus, room.State.FxPointer, station.DoorPointer, room.AreaIndex);
        compiledTreadmills.LoadRoom(guarded, room.State.FxPointer, station.DoorPointer,
            room.AreaIndex);
        AssertEqual(nativeTreadmills.Count, compiledTreadmills.Count,
            "Ceres compiled treadmill animation count");
    }

    private sealed class RoomFxRecordReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        private static readonly HashSet<int> Forbidden = BuildForbidden();

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (Forbidden.Contains(address))
                throw new InvalidOperationException(
                    $"Compiled room-FX consumer reread source record byte ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        private static HashSet<int> BuildForbidden()
        {
            var result = new HashSet<int>();
            foreach (RoomFxRecordDefinition record in RoomFxRecordDefinitions.All)
            {
                int length = record.DoorPointer == RoomFxRomData.Record.TerminatorDoorPointer
                    ? sizeof(ushort) : RoomFxRomData.Record.ByteCount;
                for (int offset = 0; offset < length; offset++)
                    result.Add(RoomFxRomData.Banks.RoomDefinitions | (record.Pointer + offset));
            }
            return result;
        }
    }

    private static SortedDictionary<ushort, RoomFxRecordDefinition> CaptureRetailRoomFxRecords(
        ISnesAddressSpace bus)
    {
        var records = new SortedDictionary<ushort, RoomFxRecordDefinition>();
        foreach (CartridgeRoomState state in RoomStateDefinitions.All)
        {
            ushort pointer = state.FxPointer;
            if (pointer == 0) continue;
            bool terminated = false;
            for (int guard = 0; guard < 256; guard++)
            {
                if (pointer < 0x8000)
                    throw new InvalidDataException($"Room state $8F:{state.Pointer:X4} FX list crossed below the LoROM window.");
                int source = RoomFxRomData.Banks.RoomDefinitions | pointer;
                ushort door = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), source);
                RoomFxRecordDefinition record;
                if (door == RoomFxRomData.Record.TerminatorDoorPointer)
                {
                    record = new(pointer, door, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                }
                else
                {
                    byte[] native = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), source,
                        RoomFxRomData.Record.ByteCount);
                    static ushort Word(byte[] bytes, int offset) =>
                        (ushort)(bytes[offset] | bytes[offset + 1] << 8);
                    record = new(pointer, door,
                        Word(native, RoomFxRomData.Record.BaseYPositionOffset),
                        Word(native, RoomFxRomData.Record.TargetYPositionOffset),
                        Word(native, RoomFxRomData.Record.YVelocityOffset),
                        native[RoomFxRomData.Record.TimerOffset],
                        native[RoomFxRomData.Record.TypeOffset],
                        native[RoomFxRomData.Record.DefaultLayerBlendConfigurationOffset],
                        native[RoomFxRomData.Record.Layer3LayerBlendConfigurationOffset],
                        native[RoomFxRomData.Record.LiquidOptionsOffset],
                        native[RoomFxRomData.Record.PaletteFxBitsetOffset],
                        native[RoomFxRomData.Record.AnimatedTileBitsetOffset],
                        native[RoomFxRomData.Record.PaletteBlendOffset]);
                }
                if (records.TryGetValue(pointer, out RoomFxRecordDefinition? existing))
                    AssertEqual(existing, record, $"shared room-FX record $83:{pointer:X4}");
                else
                    records.Add(pointer, record);
                if (door is 0 or RoomFxRomData.Record.TerminatorDoorPointer)
                {
                    terminated = true;
                    break;
                }
                pointer = unchecked((ushort)(pointer + RoomFxRomData.Record.ByteCount));
            }
            AssertTrue(terminated, $"room state $8F:{state.Pointer:X4} FX list terminates");
        }
        // Mother Brain selects these records by numeric FX index after room load.
        // The ordinary door-list walk stops at the default $A0A4 record and
        // therefore cannot discover them from room-state FX pointers alone.
        foreach (ushort pointer in MotherBrainFxRecordPointers.DirectRecords)
        {
            byte[] native = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                RoomFxRomData.Banks.RoomDefinitions | pointer,
                RoomFxRomData.Record.ByteCount);
            static ushort Word(byte[] bytes, int offset) =>
                (ushort)(bytes[offset] | bytes[offset + 1] << 8);
            records.Add(pointer, new RoomFxRecordDefinition(
                pointer,
                Word(native, RoomFxRomData.Record.DoorPointerOffset),
                Word(native, RoomFxRomData.Record.BaseYPositionOffset),
                Word(native, RoomFxRomData.Record.TargetYPositionOffset),
                Word(native, RoomFxRomData.Record.YVelocityOffset),
                native[RoomFxRomData.Record.TimerOffset],
                native[RoomFxRomData.Record.TypeOffset],
                native[RoomFxRomData.Record.DefaultLayerBlendConfigurationOffset],
                native[RoomFxRomData.Record.Layer3LayerBlendConfigurationOffset],
                native[RoomFxRomData.Record.LiquidOptionsOffset],
                native[RoomFxRomData.Record.PaletteFxBitsetOffset],
                native[RoomFxRomData.Record.AnimatedTileBitsetOffset],
                native[RoomFxRomData.Record.PaletteBlendOffset]));
        }
        return records;
    }

    private static string RenderRoomFxDefinitions(
        IEnumerable<RoomFxRecordDefinition> records)
    {
        var source = new StringBuilder();
        source.AppendLine("// Generated from the pinned Super Metroid cartridge by --generate-room-fx-records.");
        source.AppendLine("#nullable enable");
        source.AppendLine("namespace SuperMetroid.Core.Game;");
        source.AppendLine();
        source.AppendLine("public static partial class RoomFxRecordDefinitions");
        source.AppendLine("{");
        source.AppendLine("    private static RoomFxRecordDefinition? SelectRecord(ushort pointer) => pointer switch");
        source.AppendLine("    {");
        foreach (RoomFxRecordDefinition record in records)
            source.AppendLine($"        0x{record.Pointer:X4} => new(0x{record.Pointer:X4}, 0x{record.DoorPointer:X4}, " +
                $"0x{record.BaseYPosition:X4}, 0x{record.TargetYPosition:X4}, " +
                $"0x{record.PackedYVelocity:X4}, 0x{record.Timer:X2}, 0x{record.Type:X2}, " +
                $"0x{record.DefaultLayerBlend:X2}, 0x{record.Layer3LayerBlend:X2}, " +
                $"0x{record.LiquidOptions:X2}, 0x{record.PaletteFxBitset:X2}, " +
                $"0x{record.AnimatedTileBitset:X2}, 0x{record.PaletteBlend:X2}),");
        source.AppendLine("        _ => null,");
        source.AppendLine("    };");
        source.AppendLine("}");
        return source.ToString();
    }
}
