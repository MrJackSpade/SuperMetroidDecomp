using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Visual-only Mother Brain fake-death wall, room background, door, and tube
/// appearances. Native collision words, draw geometry, and PLM timing remain
/// immutable compiled data, independent of these replacement tile indexes.
/// </summary>
public sealed class RoomPlmMotherBrainFakeDeathVisualCatalog
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomPlmMotherBrainFakeDeathVisualCatalog), content =>
    {
        Span<ushort> words = stackalloc ushort[14];
        foreach (var draw in MotherBrainFakeDeathPlmDrawDefinitions.All.OrderBy(item => MotherBrainFakeDeathPlmDrawDefinitions.VisualId(item.Draw), StringComparer.Ordinal))
        {
            int index = 0;
            for (int run = 0; run < draw.Runs.Length; run++)
                for (int block = 0; block < draw.Runs.Span[run].LevelWords.Length; block++)
                    words[index++] = GetWord(draw.Draw, run, block);
            content.AppendName("frame", MotherBrainFakeDeathPlmDrawDefinitions.VisualId(draw.Draw));
            // Existing installations hash each draw as one flattened visual frame.
            content.Append("runs", 1);
            content.AppendWords("words", words[..index]);
        }
    });

    /// <summary>Only changed draw appearances, keyed by draw; unchanged draws use compiled visuals.</summary>
    private readonly PlmVisualOverrides customWords;

    /// <summary>Validates all twenty-two fake-death room layouts and copies visual differences while retaining collision words, signed draw offsets, and PLM timing.</summary>
    /// <param name="entries">One visual-only entry per compiled layout, with the exact flattened count of all its native run words.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    public RoomPlmMotherBrainFakeDeathVisualCatalog(
        IEnumerable<RoomPlmVisualEntry> entries)
    {
        customWords = new PlmVisualOverrides(entries, "Mother Brain fake-death", MotherBrainFakeDeathPlmDrawDefinitions.TryGetByVisualId,
            MotherBrainFakeDeathPlmDrawDefinitions.All.Count());
    }

    /// <summary>Resolves a fake-death room block's selected appearance, projecting native run-local indices into its flattened authored layout.</summary>
    /// <param name="draw">The draw whose visual word is resolved.</param>
    /// <param name="runIndex">Zero-based native run ordinal; background rows have one thirteen-word horizontal run, while other layouts retain their own run shapes.</param>
    /// <param name="blockIndex">Zero-based word ordinal within the run, not a flattened entry index or room coordinate.</param>
    /// <returns>The authored metatile/flip word, or visual bits from the compiled physical word when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported fake-death layout.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or word ordinal is outside the compiled draw shape.</exception>
    internal ushort GetWord(PlmDraw draw, int runIndex, int blockIndex)
    {
        if (MotherBrainFakeDeathPlmDrawDefinitions.TryDescribeBackground(draw, out var background))
        {
            if (runIndex != 0 || (uint)blockIndex >= 13)
                throw new ArgumentOutOfRangeException(nameof(blockIndex));
            return customWords.TryGet(draw, out var selectedBackground)
                ? selectedBackground[blockIndex] : new RoomLevelWord(background.WordAt(blockIndex)).VisualWord;
        }
        if (MotherBrainFakeDeathPlmDrawDefinitions.TryDescribeRegular(draw, out var regular))
        {
            if ((uint)runIndex >= regular.RunCount || (uint)blockIndex >= regular.Count(runIndex))
                throw new ArgumentOutOfRangeException(nameof(blockIndex));
            return customWords.TryGet(draw, out var selected)
                ? selected[(runIndex == 0 ? 0 : regular.Count(0)) + blockIndex]
                : new RoomLevelWord(regular.WordAt(runIndex, blockIndex)).VisualWord;
        }
        if (!MotherBrainFakeDeathPlmDrawDefinitions.TryDescribeBoundary(draw, out var shape))
            throw new InvalidDataException(
                $"Mother Brain fake-death visuals lack draw {draw}.");
        if ((uint)runIndex >= 2 ||
            (uint)blockIndex >= (uint)shape.Count(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += shape.Count(run);
        return customWords.TryGet(draw, out var words)
            ? words[flatIndex] : new RoomLevelWord(shape.WordAt(runIndex, blockIndex)).VisualWord;
    }
}
