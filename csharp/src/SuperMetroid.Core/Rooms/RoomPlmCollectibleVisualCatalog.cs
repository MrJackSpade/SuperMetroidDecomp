using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>Editable metatile/flip reference for one collectible draw frame.</summary>
/// <param name="Id">Compiled identity of an empty, orb, tank, dynamic-slot, or shot-reveal frame.</param>
/// <param name="VisualWord">Twelve-bit metatile reference and parent flips, excluding the physical collision nibble.</param>
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

    /// <summary>Validates all twenty-four compiled one-cell frames and retains authored visual differences without changing pickup, collision, or persistence rules.</summary>
    /// <param name="entries">Exactly one visual-only word for each compiled collectible frame identity.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, or leaves compiled frame coverage incomplete.</exception>
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

    /// <summary>Resolves the selected appearance of one collectible draw list while leaving its physical level word compiled.</summary>
    /// <param name="pointer">Bank-$84 identity of a supported six-byte, one-cell draw list, not a collectible PLM header or dynamic graphics address.</param>
    /// <returns>The authored metatile/flip word, or those visual bits from the compiled stock frame when unchanged.</returns>
    /// <exception cref="InvalidDataException">The pointer does not identify a supported collectible frame.</exception>
    public ushort GetWord(ushort pointer)
    {
        if (!RoomPlmCollectibleDrawDefinitions.TryGetWord(pointer, out ushort physical))
            throw new InvalidDataException($"Collectible visuals lack frame ${pointer:X4}.");
        return customWords is not null && customWords.TryGetValue(pointer, out ushort word)
            ? word : new RoomLevelWord(physical).VisualWord;
    }
}