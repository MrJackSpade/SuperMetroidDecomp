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
    /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
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

    /// <summary>Holds the import-validated appearances of the poses that differ from the compiled poses.</summary>
    /// <param name="customBlocks">The changed poses' flattened appearances, keyed by draw.</param>
    internal InstalledRoomPlmSamusEaterVisuals(PlmVisualOverrides customBlocks) =>
        this.customBlocks = Ensure.NotNull(customBlocks);

    /// <summary>Resolves one selected plant block, translating run-local indices into the flattened authored payload.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based draw-run ordinal 0..2.</param>
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
