using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

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
        foreach (var frame in RoomPlmCollectibleDrawDefinitions.All.OrderBy(frame => frame.Id, StringComparer.Ordinal))
        {
            content.AppendName("frame", frame.Id);
            content.Append("runs", 1);
            buffer[0] = GetWord(PlmDraw.Of(frame.Draw));
            content.AppendWords("words", buffer);
        }
    });

    /// <summary>Only collectible frame appearances that differ from compiled stock visual bits.</summary>
    private readonly Dictionary<RoomPlmCollectibleDrawDefinitions.Draw, int>? customWords;

    /// <summary>Validates all twenty-four compiled one-cell frames and retains authored visual differences without changing pickup, collision, or persistence rules.</summary>
    /// <param name="entries">Exactly one visual-only word for each compiled collectible frame identity.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, or leaves compiled frame coverage incomplete.</exception>
    public RoomPlmCollectibleVisualCatalog(
        IEnumerable<RoomPlmVisualWordEntry> entries)
    {
        Ensure.NotNull(entries);
        var selected = new Dictionary<RoomPlmCollectibleDrawDefinitions.Draw, int>();
        var seen = new HashSet<RoomPlmCollectibleDrawDefinitions.Draw>();
        foreach (RoomPlmVisualWordEntry entry in entries)
        {
            if (entry is null ||
                !RoomPlmCollectibleDrawDefinitions.TryGetById(entry.Id, out var frame) ||
                !RoomLevelWord.IsValidVisualWord(entry.VisualWord))
                throw new InvalidDataException(
                    "Collectible visuals changed a compiled frame identity or contain an invalid word.");
            if (!seen.Add(frame.Draw))
                throw new InvalidDataException($"Collectible visuals repeat frame {entry.Id}.");
            if (entry.VisualWord != frame.LevelWord.VisualWord)
                selected.Add(frame.Draw, entry.VisualWord);
        }
        if (seen.Count != RoomPlmCollectibleDrawDefinitions.All.Count())
            throw new InvalidDataException("Collectible visuals do not cover all compiled frames.");
        if (selected.Count != 0) customWords = selected;
    }

    /// <summary>Resolves the selected appearance of one collectible draw list while leaving its physical level word compiled.</summary>
    /// <param name="draw">The collectible draw whose visual word is resolved.</param>
    /// <returns>The authored metatile/flip word, or those visual bits from the compiled stock frame when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported collectible frame.</exception>
    internal ushort GetWord(PlmDraw draw)
    {
        if (!RoomPlmCollectibleDrawDefinitions.TryGetWord(draw, out int physical))
            throw new InvalidDataException($"Collectible visuals lack frame {draw}.");
        return customWords is not null &&
            customWords.TryGetValue(PlmDraw.FrameOf<RoomPlmCollectibleDrawDefinitions.Draw>(draw, "collectible"), out int word)
            ? checked((ushort)word) : new RoomLevelWord(checked((ushort)physical)).VisualWord;
    }
}
