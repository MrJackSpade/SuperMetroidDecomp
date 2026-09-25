using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Fixed bank-$8F scroll mutation programs referenced by the 284 compiled retail
/// PLM populations. These `(storage index, state)` pairs are room mechanics, not
/// editable presentation data. Constructed room populations retain their bus data.
/// </summary>
internal static partial class RoomPlmScrollProgramDefinitions
{
    internal const int RetailProgramCount = 173;
    internal const int RetailByteCount = 743;
    internal const int RetailPairCount = 285;

    // The generated partial owns Sources. Lazy construction prevents partial-type
    // field-initializer order from exposing an uninitialized array.
    private static readonly Lazy<IReadOnlyDictionary<ushort, ReadOnlyMemory<byte>>>
        Programs = new(Build);

    internal static ReadOnlyMemory<byte> Get(ushort pointer) =>
        Programs.Value.TryGetValue(pointer, out ReadOnlyMemory<byte> program)
            ? program
            : throw new InvalidDataException(
                $"Compiled room scroll programs lack retail source $8F:{pointer:X4}.");

    internal static IEnumerable<ushort> Pointers => Programs.Value.Keys;

    private static IReadOnlyDictionary<ushort, ReadOnlyMemory<byte>> Build()
    {
        var selected = new Dictionary<ushort, ReadOnlyMemory<byte>>(RetailProgramCount);
        int totalBytes = 0;
        int totalPairs = 0;
        foreach ((ushort pointer, string hex) in Sources)
        {
            byte[] bytes = Convert.FromHexString(hex);
            if (pointer < 0x8000 || bytes.Length is < 1 or > 2 * RoomScrollGrid.StorageByteCount + 1 ||
                bytes.Length % 2 == 0 || (bytes[^1] & 0x80) == 0)
                throw new InvalidDataException(
                    $"Compiled room scroll program $8F:{pointer:X4} has invalid shape.");
            for (int offset = 0; offset < bytes.Length - 1; offset += 2)
            {
                if (bytes[offset] >= RoomScrollGrid.StorageByteCount ||
                    bytes[offset + 1] > (byte)RoomScrollState.Green)
                    throw new InvalidDataException(
                        $"Compiled room scroll program $8F:{pointer:X4} has invalid pair {offset / 2}.");
                totalPairs++;
            }
            if (!selected.TryAdd(pointer, bytes))
                throw new InvalidDataException(
                    $"Duplicate compiled room scroll program $8F:{pointer:X4}.");
            totalBytes += bytes.Length;
        }

        var expected = new HashSet<ushort>();
        foreach (ushort populationPointer in RoomPlmPopulationDefinitions.Pointers)
        {
            ReadOnlySpan<byte> population =
                RoomPlmPopulationDefinitions.Get(populationPointer).Span;
            for (int offset = 0; offset < population.Length - 2; offset += 6)
            {
                ushort header = (ushort)(population[offset] |
                    population[offset + 1] << 8);
                if (header == RoomPlmHeaders.ScrollTrigger)
                    expected.Add((ushort)(population[offset + 4] |
                        population[offset + 5] << 8));
            }
        }
        if (selected.Count != RetailProgramCount || totalBytes != RetailByteCount ||
            totalPairs != RetailPairCount || !expected.SetEquals(selected.Keys))
            throw new InvalidDataException(
                "Compiled room scroll programs differ from the retail PLM population inventory.");
        return selected;
    }
}
