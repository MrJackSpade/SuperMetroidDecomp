using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Visual-only level appearances for all four floor and four ceiling plant poses.
/// The compiled draw definitions retain their physical collision nibble and
/// three-run geometry even when a player replaces these visible block IDs.
/// </summary>
public sealed class InstalledRoomPlmSamusEaterVisuals
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmSamusEaterVisuals), content =>
    {
        Span<RoomBlockAppearance> appearances = stackalloc RoomBlockAppearance[8];
        foreach (var draw in SamusEaterPlmDraws.All.OrderBy(item => SamusEaterPlmDraws.VisualId(item.Draw), StringComparer.Ordinal))
        {
            int index = 0;
            for (int run = 0; run < 3; run++)
                for (int block = 0; block < SamusEaterPlmDraws.Draw.Count(run); block++)
                    appearances[index++] = GetAppearance(draw.Draw, run, block);
            content.AppendName("frame", SamusEaterPlmDraws.VisualId(draw.Draw));
            // Preserve the original flattened-frame hash, not the three physical runs.
            content.Append("runs", 1);
            content.AppendAppearances("blocks", appearances);
        }
    });

    /// <summary>Owned appearance overrides keyed by plant pose; null uses every stock appearance.</summary>
    private readonly PlmVisualOverrides customBlocks;

    /// <summary>Validates all eight three-run plant poses and copies authored visual differences without changing collision, run geometry, chewing timing, or Samus interaction.</summary>
    /// <param name="entries">Exactly one eight-block visual-only entry for each compiled floor and ceiling pose.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves coverage incomplete.</exception>
    public InstalledRoomPlmSamusEaterVisuals(
        IEnumerable<RoomPlmVisualEntry> entries)
    {
        customBlocks = new PlmVisualOverrides(entries, "Samus Eater", SamusEaterPlmDraws.TryGetByVisualId,
            SamusEaterPlmDraws.All.Count());
    }

    /// <summary>Resolves one selected plant block, translating native run-local indices into the flattened authored payload.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based native draw-run ordinal 0..2.</param>
    /// <param name="blockIndex">Zero-based block ordinal within the selected run.</param>
    /// <returns>The authored appearance, or the compiled terrain's appearance when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported pose.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or block ordinal is outside the compiled shape.</exception>
    internal RoomBlockAppearance GetAppearance(PlmDraw draw, int runIndex, int blockIndex)
    {
        if (!SamusEaterPlmDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Samus Eater visuals lack frame {draw}.");
        if ((uint)runIndex >= 3 ||
            (uint)blockIndex >= (uint)SamusEaterPlmDraws.Draw.Count(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += SamusEaterPlmDraws.Draw.Count(run);
        return customBlocks.TryGet(draw, out var appearances)
            ? appearances[flatIndex] : shape.BlockAt(runIndex, blockIndex).Appearance;
    }
}
