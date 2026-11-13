using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>
/// Import/reference-only decoding of native PLM placements. The resulting typed
/// population can be consumed by Core without a cartridge-capable address space.
/// </summary>
internal static class RoomObjectPopulationImporter
{
    /// <summary>Imports bounded PLM placements, header words and scroll programs.</summary>
    /// <param name="source">The cartridge import source.</param>
    /// <param name="pointer">Where the population starts.</param>
    /// <param name="area">The area of the room the population is placed in, which a save station's argument needs; null outside a room.</param>
    public static RoomObjectPopulation Read(ISnesAddressSpace source, ushort pointer, SuperMetroid.Core.Progression.AreaId? area = null)
    {
        Ensure.NotNull(source);
        if (source is not IImportCartridgeSource cartridge)
            throw new ArgumentException("PLM import requires an explicit cartridge-import source.", nameof(source));
        var records = new List<RoomObjectPlacement>();
        ushort cursor = pointer;
        for (int index = 0; index < RoomObjectPopulationFormatConstants.MaximumParserIterations; index++)
        {
            ushort headerWord = Word(0x8f, cursor);
            if (headerWord == 0)
                return new RoomObjectPopulation($"$8F:{pointer:X4}", records);
            ushort argument = Word(0x8f, unchecked((ushort)(cursor + 4)));
            byte blockX = Byte(0x8f, unchecked((ushort)(cursor + 2)));
            byte blockY = Byte(0x8f, unchecked((ushort)(cursor + 3)));
            // RoomObjectKind's values are arbitrary (#1414): a word no member names cannot enter the population.
            if (!RoomObjectNativeWords.Headers.TryDecode(headerWord, out RoomObjectKind header))
                throw new InvalidDataException(
                    $"PLM population $8F:{pointer:X4} record {index} names unlabelled header $84:{headerWord:X4} " +
                    $"at block ({blockX},{blockY}) with argument ${argument:X4}.");
            RoomScrollWrite[]? scroll = header == RoomObjectKind.ScrollTrigger ? Scroll(argument) : null;
            RoomScrollChange? retailScroll = scroll is null ? null : RetailScrollProgram(argument, scroll);
            records.Add(new RoomObjectPlacement(header,
                new(blockX, blockY), RoomObjectRoomArgumentWords.Decode(header, argument, area), retailScroll is null ? scroll : null,
                DraygonCannonObjects.IsDraygonCannonKind(header) ? DraygonWallCannonAddresses.Decode(argument) : null)
            {
                CompiledScrollChange = retailScroll,
            });
            cursor = unchecked((ushort)(cursor + RoomObjectPopulationFormatConstants.RecordByteCount));
        }
        // A population whose records run round its whole bank has no terminator.
        throw new InvalidDataException($"Imported PLM population $8F:{pointer:X4} has no bounded zero terminator.");

        byte Byte(byte bank, ushort offset) => cartridge.ReadCartridgeByte(((int)new SnesAddress(bank, offset)));
        ushort Word(byte bank, ushort offset) =>
            LittleEndianBytes.Word(Byte(bank, offset), Byte(bank, unchecked((ushort)(offset + 1))));
        // A trigger whose argument is where a retail scroll program is stored, and which finds that program's writes there,
        // runs that retail program, as the compiled populations bind it; writes stored anywhere else are a constructed
        // program the trigger carries itself.
        static RoomScrollChange? RetailScrollProgram(ushort argument, RoomScrollWrite[] writes)
        {
            foreach ((RoomScrollChange program, ushort address) in RoomScrollProgramNative.Addresses)
            {
                if (address == argument && RoomObjectScrollChangeDefinitions.Changes[program].SequenceEqual(writes))
                    return program;
            }
            return null;
        }
        // The stored program is index/state byte pairs ended by a byte with its sign bit set; the import decodes that
        // terminator into the end of the write list.
        RoomScrollWrite[] Scroll(ushort argument)
        {
            var writes = new List<RoomScrollWrite>();
            int cursor = 0;
            for (int index = 0; index < RoomScrollGrid.KeptCellCount; index++)
            {
                byte cell = Byte(0x8f, unchecked((ushort)(argument + cursor++)));
                if ((cell & 0x80) != 0)
                    return [.. writes];
                byte state = Byte(0x8f, unchecked((ushort)(argument + cursor++)));
                writes.Add(new RoomScrollWrite(cell, RoomScrollAuthoredBytes.FromAuthored(state, $"scroll program $8F:{argument:X4}")));
            }
            throw new InvalidDataException($"Imported scroll program $8F:{argument:X4} has no bounded negative terminator.");
        }
    }
}
