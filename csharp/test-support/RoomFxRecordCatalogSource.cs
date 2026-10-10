using System.Text;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

/// <summary>Reads the retail room-FX records and renders the generated Core catalog that holds them.</summary>
internal static class RoomFxRecordCatalogSource
{
    /// <summary>The checked-in generated catalog the renderer's output must equal.</summary>
    internal static readonly string GeneratedPath = Path.GetFullPath(Path.Combine(
        "csharp", "src", "SuperMetroid.Core", "Game", "RoomFxRecordDefinitions.Generated.cs"));

    /// <summary>
    /// Reads room-state FX lists and Mother Brain's directly selected records from the retail
    /// address space, rejecting unterminated lists or conflicting records that share a pointer.
    /// </summary>
    /// <param name="bus">Cartridge address space used to read fixed-bank room-FX data.</param>
    /// <returns>Room-FX definitions keyed by record pointer in ascending order.</returns>
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
                if (door is 0 or RoomFxRomData.Record.TerminatorDoorPointer)
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
                (ushort)(bytes[offset] | bytes[offset + 1] << 8);
            records.Add(pointer, new RoomFxRecordDefinition(
                pointer,
                Word(native, RoomFxRomDataRecordTooling.DoorPointerOffset),
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

    /// <summary>
    /// Renders the captured room-FX records as the generated Core catalog's pointer-selection
    /// switch, preserving each record's fields in source form.
    /// </summary>
    /// <param name="records">Definitions to emit, in the order used to generate the switch arms.</param>
    /// <returns>Complete C# source for the generated room-FX definition catalog.</returns>
    internal static string RenderRoomFxDefinitions(
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
