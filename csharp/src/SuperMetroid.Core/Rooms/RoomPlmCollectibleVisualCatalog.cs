namespace SuperMetroid.Core.Rooms;

/// <summary>Editable tile/flip/palette reference for one collectible draw frame.</summary>
public sealed record RoomPlmCollectibleVisualEntry(string Id, ushort VisualWord);

/// <summary>
/// Complete collectible appearance; compiled draw words retain the physical
/// collision nibble and pickup effects independently of the visual override.
/// </summary>
public sealed class RoomPlmCollectibleVisualCatalog
{
    private readonly IReadOnlyDictionary<ushort, ushort> words;

    public RoomPlmCollectibleVisualCatalog(
        IEnumerable<RoomPlmCollectibleVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort>();
        foreach (RoomPlmCollectibleVisualEntry entry in entries)
        {
            if (entry is null ||
                !RoomPlmCollectibleDrawDefinitions.TryGetById(entry.Id, out var frame) ||
                !RoomLevelWord.IsValidVisualWord(entry.VisualWord))
                throw new InvalidDataException(
                    "Collectible visuals changed a compiled frame identity or contain an invalid word.");
            if (!selected.TryAdd(frame.Pointer, entry.VisualWord))
                throw new InvalidDataException($"Collectible visuals repeat frame {entry.Id}.");
        }
        if (selected.Count != RoomPlmCollectibleDrawDefinitions.All.Length)
            throw new InvalidDataException("Collectible visuals do not cover all compiled frames.");
        words = selected;
    }

    public static RoomPlmCollectibleVisualCatalog Stock() => new(
        RoomPlmCollectibleDrawDefinitions.All.ToArray().Select(frame =>
            new RoomPlmCollectibleVisualEntry(frame.Id,
                new RoomLevelWord(frame.LevelWord).VisualWord)));

    public ushort GetWord(ushort pointer) => words.TryGetValue(pointer, out ushort word)
        ? word
        : throw new InvalidDataException(
            $"Collectible visuals lack frame ${pointer:X4}.");
}
