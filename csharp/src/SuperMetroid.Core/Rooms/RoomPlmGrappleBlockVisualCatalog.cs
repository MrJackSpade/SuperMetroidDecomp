using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>One editable visual reference for a compiled breakable-Grapple draw list.</summary>
/// <param name="DrawPointer">Bank-$84 identity of the initial, one of three breakup, or blank one-block draw lists.</param>
/// <param name="VisualWord">Twelve-bit metatile reference and parent flips, excluding Grapple/air collision bits.</param>
public sealed record RoomPlmGrappleBlockVisualEntry(ushort DrawPointer, ushort VisualWord);

/// <summary>
/// Complete visual selection for the five Grapple-block frames. Collision, timing,
/// draw shape, and the original full level word remain in compiled definitions.
/// </summary>
public sealed class RoomPlmGrappleBlockVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmGrappleBlockVisualCatalog), content =>
    {
        Span<ushort> word = stackalloc ushort[1];
        foreach (var draw in RoomPlmGrappleBlockDrawDefinitions.All.OrderBy(draw => draw.Pointer))
        {
            content.Append("frame", draw.Pointer);
            content.Append("runs", 1);
            word[0] = GetWord(draw.Pointer);
            content.AppendWords("words", word);
        }
    });

    /// <summary>Authored visual-word overrides keyed by draw-list pointer; null when every frame uses its compiled appearance.</summary>
    private readonly Dictionary<ushort, ushort>? customWords;

    /// <summary>Validates all five compiled one-block frames and retains authored visual differences without changing Grapple collision, breakup timing, or draw shape.</summary>
    /// <param name="entries">Exactly one visual-only entry for each compiled initial, breakup, and blank draw identity.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry changes an identity/collision bits, repeats a frame, or leaves coverage incomplete.</exception>
    public RoomPlmGrappleBlockVisualCatalog(IEnumerable<RoomPlmGrappleBlockVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort>();
        var seen = new HashSet<ushort>();
        foreach (RoomPlmGrappleBlockVisualEntry entry in entries)
        {
            if (entry is null ||
                !RoomPlmGrappleBlockDrawDefinitions.TryGet(entry.DrawPointer, out var draw) ||
                !RoomLevelWord.IsValidVisualWord(entry.VisualWord))
                throw new InvalidDataException(
                    "Grapple-block visuals changed a compiled draw identity or collision bits.");
            if (!seen.Add(entry.DrawPointer))
                throw new InvalidDataException(
                    $"Grapple-block visuals repeat draw list ${entry.DrawPointer:X4}.");
            if (entry.VisualWord != new RoomLevelWord(draw.LevelWord).VisualWord)
                selected.Add(entry.DrawPointer, entry.VisualWord);
        }

        if (seen.Count != RoomPlmGrappleBlockDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Grapple-block visuals do not cover all compiled draw lists.");
        if (selected.Count != 0) customWords = selected;
    }

    /// <summary>Resolves the selected appearance for one breakable-Grapple frame while preserving its compiled Grapple or air collision word.</summary>
    /// <param name="drawPointer">Bank-$84 identity of a supported six-byte, one-cell draw list.</param>
    /// <returns>The authored metatile/flip word, or the compiled stock word's visual bits when unchanged.</returns>
    /// <exception cref="InvalidDataException">The pointer does not identify a supported Grapple-block frame.</exception>
    public ushort GetWord(ushort drawPointer)
    {
        if (!RoomPlmGrappleBlockDrawDefinitions.TryGet(drawPointer, out var draw))
            throw new InvalidDataException($"Grapple-block visuals lack draw list ${drawPointer:X4}.");
        return customWords is not null && customWords.TryGetValue(drawPointer, out ushort word)
            ? word : new RoomLevelWord(draw.LevelWord).VisualWord;
    }
}
