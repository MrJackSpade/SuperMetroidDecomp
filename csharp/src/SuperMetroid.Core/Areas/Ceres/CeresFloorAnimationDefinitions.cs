using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The seven fixed frames the Ceres door/elevator and Ridley's getaway draw onto the rotating floor. Each frame is a
/// list of tile-number runs: a run replaces only the character each floor cell shows, never the characters' pixels.
/// </summary>
public static class CeresFloorAnimationDefinitions
{
    /// <summary>
    /// The capsule frames in reflected phase order 0, 1, 2, 1; <paramref name="phase"/> must already be bounded to 0..3.
    /// </summary>
    internal static CeresFloorAnimationFrame BabyFrameForPhase(int phase)
    {
        if ((uint)phase >= 4) throw new ArgumentOutOfRangeException(nameof(phase));
        int reflectedPhase = Math.Min(phase, 4 - phase);
        return BabyFrames[reflectedPhase];
    }

    /// <summary>The three capsule frames, indexed by reflected phase.</summary>
    private static readonly CeresFloorAnimationFrame[] BabyFrames =
        [CeresFloorAnimationFrame.BabyFrame0, CeresFloorAnimationFrame.BabyFrame1, CeresFloorAnimationFrame.BabyFrame2];

    /// <summary>The elevator platform's floor position (14, 12), an authored layout placement.</summary>
    private const int PlatformColumn = 14, PlatformRow = 12;
    /// <summary>The Baby capsule's floor position (4, 10), an authored layout placement.</summary>
    private const int BabyColumn = 4, BabyRow = 10;
    /// <summary>Starting floor column and authored run width for one Ridley wing row.</summary>
    private readonly record struct WingRegion(int Column, int Width);
    /// <summary>The six wing row extents trace the drawn wing silhouette and are authored drawing content.</summary>
    private static readonly WingRegion[] WingRegions =
    [ new(11,4), new(0,14), new(0,14), new(1,12), new(1,15), new(0,16) ];

    /// <summary>Resolves a frame's runs without reading cartridge memory or constructing a table.</summary>
    public static FrameRuns Get(CeresFloorAnimationFrame frame) => frame switch
    {
        CeresFloorAnimationFrame.ElevatorLight or CeresFloorAnimationFrame.ElevatorDark => new(frame, 1),
        CeresFloorAnimationFrame.BabyFrame0 or CeresFloorAnimationFrame.BabyFrame1 or CeresFloorAnimationFrame.BabyFrame2 => new(frame, 2),
        CeresFloorAnimationFrame.WingFrame0 or CeresFloorAnimationFrame.WingFrame1 => new(frame, WingRegions.Length),
        _ => throw new InvalidDataException($"Unknown Ceres floor animation frame {frame}."),
    };
    /// <summary>Allocation-free calculated view of one Ceres floor animation frame's runs, in drawing order.</summary>
    /// <param name="frame">Supported frame identity, normally selected through <see cref="Get"/>.</param>
    /// <param name="count">Run count matching that identity: one for the elevator, two for the capsule, or six for Ridley's wing.</param>
    public readonly struct FrameRuns(CeresFloorAnimationFrame frame, int count) : IReadOnlyList<CeresFloorTileRun>
    {
        /// <summary>Number of runs in this frame, not total tile numbers or elapsed animation updates.</summary>
        public int Count => count;
        /// <summary>Calculates one run's floor cell and ordered tile numbers.</summary>
        /// <param name="index">Zero-based run ordinal in 0..<c>Count - 1</c>; capsule and wing ordinals also select their respective drawn rows.</param>
        /// <returns>A contiguous run with its placement and authored width.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The run ordinal is outside this view.</exception>
        public CeresFloorTileRun this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                if (frame is CeresFloorAnimationFrame.ElevatorLight or CeresFloorAnimationFrame.ElevatorDark)
                    return new(PlatformRow * FloorMap.Side + PlatformColumn, new(frame, index, CeresDoorVisualDefinitions.PlatformCellCount));
                if (frame is CeresFloorAnimationFrame.BabyFrame0 or CeresFloorAnimationFrame.BabyFrame1 or CeresFloorAnimationFrame.BabyFrame2)
                    return new((BabyRow + index) * FloorMap.Side + BabyColumn, new(frame, index, 2));
                WingRegion region = WingRegions[index];
                return new(index * FloorMap.Side + region.Column, new(frame, index, region.Width));
            }
        }
        /// <summary>Enumerates calculated runs in drawing order without drawing them or advancing animation.</summary>
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
        /// <summary>Calculates the selected frame's character number at a run-relative column, retaining transparent wing cells as tile $FF.</summary>
        /// <param name="index">Zero-based column within this run, not the absolute floor column.</param>
        /// <returns>The eight-bit floor character selector.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The column is outside this view.</exception>
        public byte this[int index] => (uint)index < Count ? TileAt(frame, row, index) : throw new ArgumentOutOfRangeException(nameof(index));
        /// <summary>Enumerates character selectors from left to right in their drawing order.</summary>
        /// <returns>An enumerator yielding the same tile numbers as indexed access.</returns>
        public IEnumerator<byte> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Adapts left-to-right tile selectors for non-generic enumeration.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>The selected light and dark platform left caps in the floor's character atlas. Their adjacent cells are the corresponding interior strips.</summary>
    private const byte LightPlatformLeftCap = 0x68, DarkPlatformLeftCap = 0x8d;
    /// <summary>The floor's character sheet packs sixteen 8px tiles per source row; the full sheet reconstructs continuous multirow room artwork.</summary>
    private const int PlatformAtlasColumns = 16;
    /// <summary>The light right cap is directly below the light left cap in the atlas, followed by the adjacent dark-phase right cap.</summary>
    private const byte PlatformRightCapPair = LightPlatformLeftCap + PlatformAtlasColumns;

    /// <summary>Canonical two-phase four-cell platform composition. Selected light/dark left-cap identities, phase membership and the four-cell cap/interior/interior/cap topology are the exact authored strip imagery; generating different choices would change its drawn ends or width. Interior and paired right-cap atlas positions calculate. This does not select animation timing, floor placement, pixels or paint.</summary>
    internal static byte PlatformTile(int frame, int column)
    {
        if ((uint)frame >= CeresDoorVisualDefinitions.PlatformFrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)column >= CeresDoorVisualDefinitions.PlatformCellCount)
            throw new ArgumentOutOfRangeException(nameof(column));
        int left = frame == 0 ? LightPlatformLeftCap : DarkPlatformLeftCap;
        return checked((byte)(column == 0 ? left : column == CeresDoorVisualDefinitions.PlatformCellCount - 1
            ? PlatformRightCapPair + frame : left + 1));
    }

    /// <summary>Six baby row origins remain REQUIRED; adjacent right tiles calculate. Platform glyphs use their shared canonical owner.</summary>
    private static byte TileAt(CeresFloorAnimationFrame frame, int row, int column)
    {
        if (frame is CeresFloorAnimationFrame.ElevatorLight or CeresFloorAnimationFrame.ElevatorDark)
            return PlatformTile(frame == CeresFloorAnimationFrame.ElevatorDark ? 1 : 0, column);
        if (frame is CeresFloorAnimationFrame.BabyFrame0 or CeresFloorAnimationFrame.BabyFrame1 or CeresFloorAnimationFrame.BabyFrame2)
        {
            int origin = new CeresFloorAnimationRow(frame, row) switch
            {
                (CeresFloorAnimationFrame.BabyFrame0, 0) => 0x59, (CeresFloorAnimationFrame.BabyFrame0, 1) => 0x69,
                (CeresFloorAnimationFrame.BabyFrame1, 0) => 0x8a, (CeresFloorAnimationFrame.BabyFrame1, 1) => 0x8c,
                (CeresFloorAnimationFrame.BabyFrame2, 0) => 0x8e, (CeresFloorAnimationFrame.BabyFrame2, 1) => 0x9d,
                _ => throw new InvalidOperationException("Unknown baby tilemap row."),
            };
            return (byte)(origin + column);
        }
        return WingTile(frame == CeresFloorAnimationFrame.WingFrame1, row, column);
    }
    /// <summary>The wing cell that draws nothing.</summary>
    private const byte TransparentWingTile = 0xff;

    /// <summary>Contiguous atlas runs and shared cells derive. Every selected run origin/extent and isolated glyph below remains REQUIRED artwork placement; no artwork exemption is claimed.</summary>
    private static byte WingTile(bool secondFrame, int row, int column)
    {
        if (!secondFrame)
        {
            return row switch
            {
                0 => (byte)column,
                1 => column is 6 or 7 ? TransparentWingTile : (byte)(column < 6 ? 4 + column : 0x0a + column - 8),
                2 => (byte)(column == 13 ? 0xa8 : 0x10 + column),
                3 => (byte)(0x21 + column),
                4 => column is >= 2 and <= 4 ? (byte)(0x1d + column - 2)
                    : column is >= 5 and <= 9 ? (byte)(0x30 + column - 5) : TransparentWingTile,
                5 => column is 4 or 5 ? (byte)(0x2e + column - 4)
                    : column is >= 6 and <= 10 ? (byte)(0x40 + column - 6) : TransparentWingTile,
                _ => throw new InvalidOperationException("Unknown wing row."),
            };
        }
        if (row < 2) return TransparentWingTile;
        if (row == 2) return column switch { 6 => 0x20, 7 => WingTile(false,row,column), 8 => 0xaa, _ => TransparentWingTile };
        if (row == 3) return column is >= 5 and <= 7 ? WingTile(false,row,column) : TransparentWingTile;
        if (row == 4)
            return column < 5 ? (byte)(0x91 + column)
                : column < 8 ? WingTile(false,row,column)
                : column < 13 ? (byte)(0x96 + column - 8)
                : column == 13 ? (byte)0x98 : (byte)0x9c;
        if (row == 5)
            return column == 0 ? (byte)0x90
                : column < 6 ? (byte)(0x9f + column - 1)
                : column < 9 ? WingTile(false,row,column)
                : column < 13 ? (byte)(0xa4 + column - 9)
                : column == 13 ? (byte)0x7d : column == 14 ? (byte)0x83 : (byte)0x2d;
        throw new InvalidOperationException("Unknown wing row.");
    }
}
