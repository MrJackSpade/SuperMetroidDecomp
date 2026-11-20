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
    /// <exception cref="InvalidDataException">
    /// A scroll trigger's argument is not where a retail scroll program is stored, or does not hold that program (#2355).
    /// </exception>
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
                return global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPopulation>([typeof(string), typeof(global::System.Collections.Generic.IEnumerable<global::SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPlacement>)], new object?[] { (string)($"$8F:{pointer:X4}"), (global::System.Collections.Generic.IEnumerable<global::SuperMetroid.Core.Rooms.RoomObjects.RoomObjectPlacement>)(records) });
            ushort argument = Word(0x8f, unchecked((ushort)(cursor + 4)));
            byte blockX = Byte(0x8f, unchecked((ushort)(cursor + 2)));
            byte blockY = Byte(0x8f, unchecked((ushort)(cursor + 3)));
            // RoomObjectKind's values are arbitrary (#1414): a word no member names cannot enter the population.
            if (!RoomObjectNativeWords.Headers.TryDecode(headerWord, out RoomObjectKind header))
                throw new InvalidDataException(
                    $"PLM population $8F:{pointer:X4} record {index} names unlabelled header $84:{headerWord:X4} " +
                    $"at block ({blockX},{blockY}) with argument ${argument:X4}.");
            // A Metroids-cleared record whose argument selects a no-op entry places an escape room's held place (#2426).
            RoomObjectKind placed = RoomObjectRoomArgumentWords.PlacedKind(header, argument);
            records.Add(RoomObjectPlacementAccess.Placed(placed,
                new(blockX, blockY), RoomObjectRoomArgumentWords.Decode(placed, argument, area),
                DraygonCannonObjectsConstants.IsDraygonCannonKind(header) ? DraygonWallCannonAddresses.Decode(argument) : null,
                header == RoomObjectKind.ScrollTrigger ? RetailScrollProgram(argument, StoredScroll(argument)) : null));
            cursor = unchecked((ushort)(cursor + RoomObjectPopulationFormatConstants.RecordByteCount));
        }
        // A population whose records run round its whole bank has no terminator.
        throw new InvalidDataException($"Imported PLM population $8F:{pointer:X4} has no bounded zero terminator.");

        byte Byte(byte bank, ushort offset) => cartridge.ReadCartridgeByte(((int)new SnesAddress(bank, offset)));
        ushort Word(byte bank, ushort offset) =>
            LittleEndianBytes.Word(Byte(bank, offset), Byte(bank, unchecked((ushort)(offset + 1))));
        // A trigger's argument is where a retail scroll program is stored, and the trigger finds that program's native
        // encoding there: it runs that retail program, as the compiled populations bind it. A trigger runs no other program.
        static RoomScrollChange RetailScrollProgram(ushort argument, byte[] stored)
        {
            foreach ((RoomScrollChange program, ushort address) in RoomScrollProgramNative.Addresses)
            {
                if (address == argument && RoomObjectScrollChangeDefinitions.Get(address).Span.SequenceEqual(stored))
                    return program;
            }
            throw new InvalidDataException($"Scroll trigger argument $8F:{argument:X4} holds no retail scroll program.");
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
    }
}
