using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>Editable tile/flip/palette reference for one collectible draw frame.</summary>
public sealed record RoomPlmCollectibleVisualEntry(string Id, ushort VisualWord);

/// <summary>
/// Complete collectible appearance; compiled draw words retain the physical
/// collision nibble and pickup effects independently of the visual override.
/// </summary>
public sealed class RoomPlmCollectibleVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmCollectibleVisualCatalog), content =>
    {
        Span<ushort> buffer = stackalloc ushort[1];
        foreach (var frame in RoomPlmCollectibleDrawDefinitions.All.OrderBy(frame => frame.Pointer))
        {
            content.Append("frame", frame.Pointer);
            content.Append("runs", 1);
            buffer[0] = GetWord(frame.Pointer);
            content.AppendWords("words", buffer);
        }
    });

    private readonly Dictionary<ushort, ushort>? customWords;

    private RoomPlmCollectibleVisualCatalog() { }

    public RoomPlmCollectibleVisualCatalog(
        IEnumerable<RoomPlmCollectibleVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmCollectibleVisualEntry entry in entries)
        {
            if (entry is null ||
                !RoomPlmCollectibleDrawDefinitions.TryGetById(entry.Id, out var frame) ||
                !RoomLevelWord.IsValidVisualWord(entry.VisualWord))
                throw new InvalidDataException(
                    "Collectible visuals changed a compiled frame identity or contain an invalid word.");
            if (!seen.Add(frame.Pointer))
                throw new InvalidDataException($"Collectible visuals repeat frame {entry.Id}.");
            if (entry.VisualWord != new RoomLevelWord(frame.LevelWord).VisualWord)
                selected.Add(frame.Pointer, entry.VisualWord);
        }
        if (seen.Count != RoomPlmCollectibleDrawDefinitions.All.Count())
            throw new InvalidDataException("Collectible visuals do not cover all compiled frames.");
        if (selected.Count != 0) customWords = selected;
    }

    /// <summary>Calculate stock appearances directly; retain only customized frames.</summary>
    public static RoomPlmCollectibleVisualCatalog Stock() => new();

    public ushort GetWord(ushort pointer)
    {
        if (!RoomPlmCollectibleDrawDefinitions.TryGetWord(pointer, out ushort physical))
            throw new InvalidDataException($"Collectible visuals lack frame ${pointer:X4}.");
        return customWords is not null && customWords.TryGetValue(pointer, out ushort word)
            ? word : new RoomLevelWord(physical).VisualWord;
    }
}