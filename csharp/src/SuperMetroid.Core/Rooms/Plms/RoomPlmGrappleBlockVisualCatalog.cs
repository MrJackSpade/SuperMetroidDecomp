using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

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
        foreach (var draw in RoomPlmGrappleBlockDrawDefinitions.All.OrderBy(draw => RoomPlmGrappleBlockDrawDefinitions.VisualId(draw.Frame), StringComparer.Ordinal))
        {
            content.AppendName("frame", RoomPlmGrappleBlockDrawDefinitions.VisualId(draw.Frame));
            content.Append("runs", 1);
            word[0] = GetWord(PlmDraw.Of(draw.Frame));
            content.AppendWords("words", word);
        }
    });

    /// <summary>Only Grapple-block frame appearances that differ from compiled visual bits.</summary>
    private readonly Dictionary<GrappleBlockDraw, int>? customWords;

    /// <summary>Validates all five compiled one-block frames and retains authored visual differences without changing Grapple collision, breakup timing, or draw shape.</summary>
    /// <param name="entries">Exactly one visual-only entry for each compiled initial, breakup, and blank draw identity.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry changes an identity/collision bits, repeats a frame, or leaves coverage incomplete.</exception>
    public RoomPlmGrappleBlockVisualCatalog(IEnumerable<RoomPlmVisualWordEntry> entries)
    {
        Ensure.NotNull(entries);
        var selected = new Dictionary<GrappleBlockDraw, int>();
        var seen = new HashSet<GrappleBlockDraw>();
        foreach (RoomPlmVisualWordEntry entry in entries)
        {
            if (entry is null || !RoomPlmGrappleBlockDrawDefinitions.TryGetByVisualId(entry.Id, out var draw) ||
                !RoomLevelWord.IsValidVisualWord(entry.VisualWord))
                throw new InvalidDataException(
                    "Grapple-block visuals changed a compiled draw identity or collision bits.");
            if (!seen.Add(draw.Frame))
                throw new InvalidDataException($"Grapple-block visuals repeat draw {entry.Id}.");
            if (entry.VisualWord != new RoomLevelWord(draw.LevelWord).VisualWord)
                selected.Add(draw.Frame, entry.VisualWord);
        }

        if (seen.Count != RoomPlmGrappleBlockDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Grapple-block visuals do not cover all compiled draw lists.");
        if (selected.Count != 0) customWords = selected;
    }

    /// <summary>Resolves the selected appearance for one breakable-Grapple frame while preserving its compiled Grapple or air collision word.</summary>
    /// <param name="draw">The draw whose visual word is resolved.</param>
    /// <returns>The authored metatile/flip word, or the compiled stock word's visual bits when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported Grapple-block frame.</exception>
    internal ushort GetWord(PlmDraw draw)
    {
        if (!RoomPlmGrappleBlockDrawDefinitions.TryGet(draw, out var definition))
            throw new InvalidDataException($"Grapple-block visuals lack draw {draw}.");
        return customWords is not null && customWords.TryGetValue(definition.Frame, out int word)
            ? checked((ushort)word) : new RoomLevelWord(definition.LevelWord).VisualWord;
    }
}
