using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Visual-only level appearances for all four floor and four ceiling plant poses.
/// The compiled draw definitions retain their physical collision nibble and
/// three-run geometry even when a player replaces these visible block IDs.
/// </summary>
public sealed class InstalledRoomObjectSamusEaterVisuals
{
    /// <summary>Owned appearance overrides keyed by plant pose; null uses every stock appearance.</summary>
    private readonly RoomObjectVisualOverrides customBlocks;

    /// <summary>Holds the import-validated appearances of the poses that differ from the compiled poses.</summary>
    /// <param name="customBlocks">The changed poses' flattened appearances, keyed by draw.</param>
    internal InstalledRoomObjectSamusEaterVisuals(RoomObjectVisualOverrides customBlocks) =>
        this.customBlocks = Ensure.NotNull(customBlocks);

    /// <summary>Resolves one selected plant block, translating run-local indices into the flattened authored payload.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <param name="runIndex">Zero-based draw-run ordinal 0..2.</param>
    /// <param name="blockIndex">Zero-based block ordinal within the selected run.</param>
    /// <returns>The authored appearance, or the compiled terrain's appearance when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported pose.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The run or block ordinal is outside the compiled shape.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw, int runIndex, int blockIndex)
    {
        if (!SamusEaterObjectDraws.TryDescribe(draw, out var shape))
            throw new InvalidDataException(
                $"Samus Eater visuals lack frame {draw}.");
        if ((uint)runIndex >= 3 ||
            (uint)blockIndex >= (uint)SamusEaterObjectDraws.Draw.Count(runIndex))
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += SamusEaterObjectDraws.Draw.Count(run);
        return customBlocks.TryGet(draw, out var appearances)
            ? appearances[flatIndex] : shape.BlockAt(runIndex, blockIndex).Appearance;
    }
}
