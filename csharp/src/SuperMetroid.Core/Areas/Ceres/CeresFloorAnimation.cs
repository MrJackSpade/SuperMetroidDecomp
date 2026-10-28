using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Draws the seven fixed Ceres floor animation frames (<see cref="CeresFloorAnimationDefinitions"/>) as runs of tile
/// numbers on the rotating floor, and selects the capsule frame for a phase and the elevator platform's cells.
/// </summary>
public static class CeresFloorAnimation
{
    /// <summary>Each frame's rows expanded to their tile numbers, left to right.</summary>
    private static readonly IReadOnlyDictionary<CeresFloorAnimationFrame, byte[][]> Tiles =
        CeresFloorAnimationDefinitions.Frames.ToDictionary(frame => frame.Key,
            frame => frame.Value.Select(row => Expand(row.Tiles)).ToArray());

    /// <summary>
    /// The capsule frames in reflected phase order 0, 1, 2, 1; <paramref name="phase"/> must already be bounded to 0..3.
    /// </summary>
    internal static CeresFloorAnimationFrame BabyFrameForPhase(int phase)
    {
        if ((uint)phase >= CeresFloorAnimationDefinitions.BabyPhaseCount) throw new ArgumentOutOfRangeException(nameof(phase));
        int reflectedPhase = Math.Min(phase, CeresFloorAnimationDefinitions.BabyPhaseCount - phase);
        return CeresFloorAnimationDefinitions.BabyFrames[reflectedPhase];
    }

    /// <summary>Resolves a frame's runs without reading cartridge memory.</summary>
    public static FrameRuns Get(CeresFloorAnimationFrame frame) =>
        CeresFloorAnimationDefinitions.Frames.TryGetValue(frame, out IReadOnlyList<CeresFloorRunDefinition>? rows)
            ? new(frame, rows.Count)
            : throw new InvalidDataException($"Unknown Ceres floor animation frame {frame}.");

    /// <summary>
    /// Canonical two-phase four-cell platform composition: the light (0) or dark (1) strip's cap, interior, interior and
    /// cap characters. This does not select animation timing, floor placement, pixels or paint.
    /// </summary>
    internal static byte PlatformTile(int frame, int column)
    {
        if ((uint)frame >= CeresDoorVisualDefinitions.PlatformFrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)column >= CeresDoorVisualDefinitions.PlatformCellCount)
            throw new ArgumentOutOfRangeException(nameof(column));
        return Tiles[CeresFloorAnimationDefinitions.PlatformFrames[frame]][0][column];
    }

    /// <summary>The tile numbers of consecutive atlas runs, left to right.</summary>
    private static byte[] Expand(IReadOnlyList<CeresFloorAtlasRun> runs) =>
        [.. runs.SelectMany(run => Enumerable.Range(0, run.Length).Select(offset => checked((byte)(run.FirstTile + run.Step * offset))))];

    /// <summary>Allocation-free view of one Ceres floor animation frame's runs, in drawing order.</summary>
    /// <param name="frame">Supported frame identity, normally selected through <see cref="Get"/>.</param>
    /// <param name="count">Run count matching that identity: one for the elevator, two for the capsule, or six for Ridley's wing.</param>
    public readonly struct FrameRuns(CeresFloorAnimationFrame frame, int count) : IReadOnlyList<CeresFloorTileRun>
    {
        /// <summary>Number of runs in this frame, not total tile numbers or elapsed animation updates.</summary>
        public int Count => count;
        /// <summary>One run's floor cell and ordered tile numbers.</summary>
        /// <param name="index">Zero-based run ordinal in 0..<c>Count - 1</c>; capsule and wing ordinals also select their respective drawn rows.</param>
        /// <returns>A contiguous run with its placement and authored width.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The run ordinal is outside this view.</exception>
        public CeresFloorTileRun this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                CeresFloorRunDefinition row = CeresFloorAnimationDefinitions.Frames[frame][index];
                return new(row.Row * FloorMap.Side + row.Column, new(frame, index, Tiles[frame][index].Length));
            }
        }
        /// <summary>Enumerates runs in drawing order without drawing them or advancing animation.</summary>
        /// <returns>An enumerator yielding the same runs as indexed access.</returns>
        public IEnumerator<CeresFloorTileRun> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Adapts drawing-order runs for non-generic enumeration.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Allocation-free view of the authored character selectors in one contiguous Ceres floor run.</summary>
    /// <param name="frame">Supported elevator, capsule, or wing frame identity defining the displayed frame.</param>
    /// <param name="row">Run ordinal: zero for the elevator, 0..1 for the capsule, or 0..5 for the wing; not the absolute floor row.</param>
    /// <param name="count">Authored run width: four elevator cells, two capsule cells, or the selected wing region's width.</param>
    public readonly struct TileSequence(CeresFloorAnimationFrame frame, int row, int count) : IReadOnlyList<byte>
    {
        /// <summary>Number of eight-bit tile selectors written to consecutive floor cells by this run.</summary>
        public int Count => count;
        /// <summary>The selected frame's character number at a run-relative column, retaining transparent wing cells as their own tile.</summary>
        /// <param name="index">Zero-based column within this run, not the absolute floor column.</param>
        /// <returns>The eight-bit floor character selector.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The column is outside this view.</exception>
        public byte this[int index] => (uint)index < Count ? Tiles[frame][row][index] : throw new ArgumentOutOfRangeException(nameof(index));
        /// <summary>Enumerates character selectors from left to right in their drawing order.</summary>
        /// <returns>An enumerator yielding the same tile numbers as indexed access.</returns>
        public IEnumerator<byte> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Adapts left-to-right tile selectors for non-generic enumeration.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
