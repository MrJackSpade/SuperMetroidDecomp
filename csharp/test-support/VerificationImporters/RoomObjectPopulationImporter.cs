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
    /// <param name="screens">
    /// The size in screens of the room the population is placed in, at whose width a constructed scroll program's stored cell
    /// numbers decode into screens; null when no constructed program writes a cell.
    /// </param>
    /// <exception cref="InvalidDataException">A constructed scroll program writes a cell and <paramref name="screens"/> is null.</exception>
    public static RoomObjectPopulation Read(ISnesAddressSpace source, ushort pointer, SuperMetroid.Core.Progression.AreaId? area = null,
        Extent16<RoomScreens>? screens = null)
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
            byte[]? storedScroll = header == RoomObjectKind.ScrollTrigger ? StoredScroll(argument) : null;
            RoomScrollChange? retailScroll = storedScroll is null ? null : RetailScrollProgram(argument, storedScroll);
            records.Add(new RoomObjectPlacement(header,
                new(blockX, blockY), RoomObjectRoomArgumentWords.Decode(header, argument, area),
                storedScroll is null || retailScroll is not null ? null : ConstructedScroll(argument, storedScroll),
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
        // A trigger whose argument is where a retail scroll program is stored, and which finds that program's native
        // encoding there, runs that retail program, as the compiled populations bind it; a program stored anywhere else is
        // a constructed program the trigger carries itself.
        static RoomScrollChange? RetailScrollProgram(ushort argument, byte[] stored)
        {
            foreach ((RoomScrollChange program, ushort address) in RoomScrollProgramNative.Addresses)
            {
                if (address == argument && RoomObjectScrollChangeDefinitions.Get(address).Span.SequenceEqual(stored))
                    return program;
            }
            return null;
        }
        // The stored program is cell/state byte pairs ended by a byte with its sign bit set, read through the terminator.
        byte[] StoredScroll(ushort argument)
        {
            var stored = new List<byte>();
            int cursor = 0;
            for (int index = 0; index < RoomScrollGrid.KeptCellCount; index++)
            {
                byte cell = Byte(0x8f, unchecked((ushort)(argument + cursor++)));
                stored.Add(cell);
                if ((cell & 0x80) != 0)
                    return [.. stored];
                stored.Add(Byte(0x8f, unchecked((ushort)(argument + cursor++))));
            }
            throw new InvalidDataException($"Imported scroll program $8F:{argument:X4} has no bounded negative terminator.");
        }
        // A constructed program's writes: each stored row-major cell number decodes once, at the room's width, into its
        // screen's column and row (#1741), a cell past the room's own screens into a row past its rows; the terminator ends
        // the write list.
        ScreenScrollChange[] ConstructedScroll(ushort argument, byte[] stored)
        {
            var writes = new ScreenScrollChange[(stored.Length - 1) / 2];
            for (int pair = 0; pair < writes.Length; pair++)
            {
                int width = (screens ?? throw new InvalidDataException(
                    $"Constructed scroll program $8F:{argument:X4} writes a cell, which decodes only at its room's width.")).X;
                byte cell = stored[2 * pair];
                writes[pair] = new ScreenScrollChange(new(cell % width, cell / width),
                    RoomScrollAuthoredBytes.FromAuthored(stored[2 * pair + 1], $"scroll program $8F:{argument:X4}"));
            }
            return writes;
        }
    }
}
