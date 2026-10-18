using System.Text;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.TestSupport.CatalogInventories;

/// <summary>Reads the retail room-FX records and renders the generated Core catalog that holds them.</summary>
internal static class RoomFxRecordCatalogSource
{
    /// <summary>The checked-in generated catalog the renderer's output must equal.</summary>
    internal static readonly string GeneratedPath = Path.GetFullPath(Path.Combine(
        "csharp", "src", "SuperMetroid.Core", "Fx", "RoomFxRecordDefinitions.Generated.cs"));

    /// <summary>Imports all room-state FX lists plus Mother Brain's directly selected records.</summary>
    [CpuBusBoundary("codec: ROM bank-$83 room FX record lists → RoomFxRecordDefinition catalog source")]
    internal static SortedDictionary<ushort, RoomFxRecordDefinition> CaptureRetailRoomFxRecords(
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
                int source = RoomFxRomDataBanksTooling.RoomDefinitions | pointer;
                ushort door = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), source);
                RoomFxRecordDefinition record;
                if (door == RoomFxRomDataRecordTooling.TerminatorDoorPointer)
                {
                    record = new(pointer, null, true, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                }
                else
                {
                    byte[] native = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), source,
                        RoomFxRomData.Record.ByteCount);
                    static ushort Word(byte[] bytes, int offset) =>
                        (ushort)(SnesLittleEndian.Word(bytes[offset], bytes[offset + 1]));
                    record = new(pointer, RecordDoor(door), false,
                        Word(native, RoomFxRomDataRecordTooling.BaseYPositionOffset),
                        Word(native, RoomFxRomDataRecordTooling.TargetYPositionOffset),
                        Word(native, RoomFxRomDataRecordTooling.YVelocityOffset),
                        native[RoomFxRomDataRecordTooling.TimerOffset],
                        native[RoomFxRomDataRecordTooling.TypeOffset],
                        native[RoomFxRomDataRecordTooling.DefaultLayerBlendConfigurationOffset],
                        native[RoomFxRomDataRecordTooling.Layer3LayerBlendConfigurationOffset],
                        native[RoomFxRomDataRecordTooling.LiquidOptionsOffset],
                        native[RoomFxRomDataRecordTooling.PaletteFxBitsetOffset],
                        native[RoomFxRomDataRecordTooling.AnimatedTileBitsetOffset],
                        native[RoomFxRomDataRecordTooling.PaletteBlendOffset]);
                }
                if (records.TryGetValue(pointer, out RoomFxRecordDefinition? existing))
                {
                    if (!existing.Equals(record))
                        throw new InvalidDataException($"Shared room-FX record $83:{pointer:X4} reads differently from another room state.");
                }
                else
                    records.Add(pointer, record);
                if (door is 0 or RoomFxRomDataRecordTooling.TerminatorDoorPointer)
                {
                    terminated = true;
                    break;
                }
                pointer = unchecked((ushort)(pointer + RoomFxRomData.Record.ByteCount));
            }
            if (!terminated)
                throw new InvalidDataException($"Room state $8F:{state.Pointer:X4} FX list does not terminate.");
        }
        // Mother Brain selects these records by numeric FX index after room load.
        // The ordinary door-list walk stops at the default $A0A4 record and
        // therefore cannot discover them from room-state FX pointers alone.
        foreach (ushort pointer in MotherBrainFxRecordPointers.DirectRecords)
        {
            byte[] native = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                RoomFxRomDataBanksTooling.RoomDefinitions | pointer,
                RoomFxRomData.Record.ByteCount);
            static ushort Word(byte[] bytes, int offset) =>
                (ushort)(SnesLittleEndian.Word(bytes[offset], bytes[offset + 1]));
            ushort door = Word(native, RoomFxRomDataRecordTooling.DoorPointerOffset);
            if (door == RoomFxRomDataRecordTooling.TerminatorDoorPointer)
                throw new InvalidDataException($"Mother Brain's room-FX record $83:{pointer:X4} is a terminator.");
            records.Add(pointer, new RoomFxRecordDefinition(
                pointer,
                RecordDoor(door),
                false,
                Word(native, RoomFxRomDataRecordTooling.BaseYPositionOffset),
                Word(native, RoomFxRomDataRecordTooling.TargetYPositionOffset),
                Word(native, RoomFxRomDataRecordTooling.YVelocityOffset),
                native[RoomFxRomDataRecordTooling.TimerOffset],
                native[RoomFxRomDataRecordTooling.TypeOffset],
                native[RoomFxRomDataRecordTooling.DefaultLayerBlendConfigurationOffset],
                native[RoomFxRomDataRecordTooling.Layer3LayerBlendConfigurationOffset],
                native[RoomFxRomDataRecordTooling.LiquidOptionsOffset],
                native[RoomFxRomDataRecordTooling.PaletteFxBitsetOffset],
                native[RoomFxRomDataRecordTooling.AnimatedTileBitsetOffset],
                native[RoomFxRomDataRecordTooling.PaletteBlendOffset]));
        }
        return records;
    }

    /// <summary>The door a record's door word names: none for a default record, else the physical door.</summary>
    private static CartridgeDoorHeader? RecordDoor(ushort word) => word == 0 ? null : DoorHeaderSources.Door(word);

    /// <summary>Renders a record's door and terminator flag as the generated catalog spells them.</summary>
    private static string RenderDoor(RoomFxRecordDefinition record) =>
        record.Terminates ? "null, true" : record.Door is { } door ? $"DoorDefinitions.{door.Name}, false" : "null, false";

    /// <summary>Renders a pointer switch over captured records for the generated Core catalog.</summary>
    internal static string RenderRoomFxDefinitions(
        IEnumerable<RoomFxRecordDefinition> records)
    {
        var source = new StringBuilder();
        source.AppendLine("// <auto-generated>");
        source.AppendLine("//     Generated from the pinned cartridge by --generate-room-fx-records. Regenerate instead of editing.");
        source.AppendLine("// </auto-generated>");
        source.AppendLine("#nullable enable");
        source.AppendLine("using SuperMetroid.Core.Rooms.Doors;");
        source.AppendLine();
        source.AppendLine("namespace SuperMetroid.Core.Fx;");
        source.AppendLine();
        source.AppendLine("public static partial class RoomFxRecordDefinitions");
        source.AppendLine("{");
        source.AppendLine("    internal static RoomFxRecordDefinition? SelectRecord(ushort pointer) => pointer switch");
        source.AppendLine("    {");
        foreach (RoomFxRecordDefinition record in records)
            source.AppendLine($"        0x{record.Pointer:X4} => new(0x{record.Pointer:X4}, {RenderDoor(record)}, " +
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
