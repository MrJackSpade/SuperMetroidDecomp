using System.Buffers.Binary;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Fixed six-byte bank-$8F PLM placement records for every selected retail room
/// state. These are application-owned spawn definitions, not editable art or a
/// general cartridge-memory image. The sequential loader still owns allocation,
/// setup side effects and immediate slot reuse.
/// </summary>
internal static partial class RoomPlmPopulationDefinitions
{
    internal const int RetailPopulationCount = 284;
    internal const int RetailRecordCount = 941;

    // Sources lives in a generated partial declaration; defer Build until all
    // static fields in both declarations have completed initialization.
    private static readonly Lazy<IReadOnlyDictionary<ushort, ReadOnlyMemory<byte>>>
        Populations = new(Build);

    internal static ReadOnlyMemory<byte> Get(ushort pointer) =>
        Populations.Value.TryGetValue(pointer, out ReadOnlyMemory<byte> records)
            ? records
            : throw new InvalidDataException(
                $"Compiled room PLM populations lack retail source $8F:{pointer:X4}.");

    internal static IEnumerable<ushort> Pointers => Populations.Value.Keys;

    private static IReadOnlyDictionary<ushort, ReadOnlyMemory<byte>> Build()
    {
        var selected = new Dictionary<ushort, ReadOnlyMemory<byte>>(RetailPopulationCount);
        int records = 0;
        foreach ((ushort pointer, string hex) in Sources)
        {
            byte[] bytes = Convert.FromHexString(hex);
            if (pointer < 0x8000 || bytes.Length < 2 ||
                (bytes.Length - 2) % 6 != 0 ||
                bytes.Length > 256 * 6 + 2 ||
                BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(bytes.Length - 2)) != 0)
                throw new InvalidDataException(
                    $"Compiled room PLM population $8F:{pointer:X4} has invalid shape.");
            for (int offset = 0; offset < bytes.Length - 2; offset += 6)
            {
                if (BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset)) == 0)
                    throw new InvalidDataException(
                        $"Compiled room PLM population $8F:{pointer:X4} terminates early.");
                records++;
            }
            if (!selected.TryAdd(pointer, bytes))
                throw new InvalidDataException(
                    $"Duplicate compiled room PLM population $8F:{pointer:X4}.");
        }

        HashSet<ushort> expected = RoomStateDefinitions.All
            .Select(state => state.PlmPointer).ToHashSet();
        if (selected.Count != RetailPopulationCount ||
            records != RetailRecordCount ||
            !expected.SetEquals(selected.Keys))
            throw new InvalidDataException(
                "Compiled room PLM populations differ from the retail room-state inventory.");
        return selected;
    }
}
