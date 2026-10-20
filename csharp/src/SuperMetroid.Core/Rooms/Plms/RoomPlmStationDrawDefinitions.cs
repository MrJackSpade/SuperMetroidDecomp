using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Complete bank-$84 draw lists selected by station animation programs. The native
/// level words are physical room mutations; replaceable artwork remains separate.
/// </summary>
internal static class RoomPlmStationDrawDefinitions
{
    /// <summary>Byte stride between a station's consecutive frame draw lists.</summary>
    internal const int FrameStride = 12;

    /// <summary>Published native enumeration order: frames by station, then save and access states.</summary>
    private static readonly StationDraw[] NativeOrder =
    [
        StationDraw.MapFrame0, StationDraw.EnergyFrame0, StationDraw.MissileFrame0,
        StationDraw.MapFrame1, StationDraw.EnergyFrame1, StationDraw.MissileFrame1,
        StationDraw.MapFrame2, StationDraw.EnergyFrame2, StationDraw.MissileFrame2,
        StationDraw.SaveIdle, StationDraw.SaveActive, StationDraw.SaveAlternate,
        StationDraw.MapRightRetracted, StationDraw.MapRightExtended,
        StationDraw.MapLeftRetracted, StationDraw.MapLeftExtended,
        StationDraw.ResourceRightRetracted, StationDraw.ResourceRightExtended,
        StationDraw.ResourceLeftRetracted, StationDraw.ResourceLeftExtended,
    ];

    /// <summary>
    /// One native station draw list's geometry. Station runs are horizontal and every continuation is an absolute
    /// origin-relative offset, not accumulated. Each layout is its own sealed record holding only what it needs; these
    /// rules cover only the twenty native owned draw pointers.
    /// </summary>
    /// <param name="Pointer">The native draw list.</param>
    internal abstract record Draw(StationDraw Pointer) : IPlmRunDrawShape
    {
        /// <inheritdoc/>
        public abstract int RunCount { get; }

        /// <inheritdoc/>
        public int WordCount(int run)
        {
            CheckRun(run);
            return WordsPerRun;
        }

        /// <inheritdoc/>
        public sbyte NextX(int run)
        {
            CheckRun(run);
            return run == 0 ? FirstRunX : (sbyte)0;
        }

        /// <inheritdoc/>
        public sbyte NextY(int run)
        {
            CheckRun(run);
            return RunY(run);
        }

        /// <inheritdoc/>
        public ushort WordAt(int run, int cell)
        {
            if ((uint)cell >= (uint)WordCount(run)) throw new ArgumentOutOfRangeException(nameof(cell));
            return Word(run, cell);
        }

        /// <summary>The level words each run writes.</summary>
        private protected virtual int WordsPerRun => 1;

        /// <summary>The horizontal offset after the first run; every later run adds none.</summary>
        private protected virtual sbyte FirstRunX => 0;

        /// <summary>The vertical offset after <paramref name="run"/>.</summary>
        private protected virtual sbyte RunY(int run) => 0;

        /// <summary>The level word at <paramref name="cell"/> of <paramref name="run"/>, both in range.</summary>
        private protected abstract ushort Word(int run, int cell);

        /// <summary>Builds one native level word from its collision type, flip bits and tile.</summary>
        private protected static ushort LevelWord(int collision, int flip, int tile) => (ushort)(collision << 12 | flip | tile);

        /// <summary>Rejects a run outside this layout's native command count.</summary>
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new ArgumentOutOfRangeException(nameof(run));
        }
    }

    /// <summary>Two horizontal display cells written origin then left; the art advances 32 tiles per frame.</summary>
    /// <param name="Pointer">The native draw list.</param>
    /// <param name="Frame">The display frame, zero through two.</param>
    internal sealed record MapDisplay(StationDraw Pointer, int Frame) : Draw(Pointer)
    {
        /// <inheritdoc/>
        public override int RunCount => 2;

        /// <inheritdoc/>
        private protected override sbyte FirstRunX => -1;

        /// <inheritdoc/>
        private protected override ushort Word(int run, int cell) => LevelWord(8, 0, 0x10c + Frame * 32 - run);
    }

    /// <summary>
    /// A vertical refill or ammunition display written origin then above; the art advances one tile per frame and its upper
    /// cell, a slope, is 32 tiles earlier.
    /// </summary>
    /// <param name="Pointer">The native draw list.</param>
    /// <param name="FirstTile">The display's tile at frame zero.</param>
    /// <param name="Frame">The display frame, zero through two.</param>
    internal sealed record ResourceDisplay(StationDraw Pointer, int FirstTile, int Frame) : Draw(Pointer)
    {
        /// <summary>The energy display's tile at frame zero.</summary>
        internal const int EnergyFirstTile = 0xc4;

        /// <summary>The missile display's tile at frame zero.</summary>
        internal const int MissileFirstTile = 0xc7;

        /// <inheritdoc/>
        public override int RunCount => 2;

        /// <inheritdoc/>
        private protected override sbyte RunY(int run) => run == 0 ? (sbyte)-1 : (sbyte)0;

        /// <inheritdoc/>
        private protected override ushort Word(int run, int cell) => LevelWord(run == 0 ? 8 : 1, 0, FirstTile + Frame - run * 32);
    }

    /// <summary>
    /// Six two-cell save-pod rows from floor to cap. The art mirrors horizontally, the floor also vertically, and the
    /// active pose advances one tile. Floor and cap are solid except the idle floor-left trigger; the shaft is air.
    /// </summary>
    /// <param name="Pointer">The native draw list.</param>
    internal sealed record SavePod(StationDraw Pointer) : Draw(Pointer)
    {
        /// <inheritdoc/>
        public override int RunCount => 6;

        /// <inheritdoc/>
        private protected override int WordsPerRun => 2;

        /// <inheritdoc/>
        private protected override sbyte RunY(int run) => run < 5 ? (sbyte)(-run - 1) : (sbyte)0;

        /// <inheritdoc/>
        private protected override ushort Word(int run, int cell)
        {
            bool shaft = run is > 0 and < 5;
            int tile = (shaft ? 0x5b : 0x59) + (Pointer == StationDraw.SaveActive ? 1 : 0);
            int flip = cell * 0x400 | (run == 0 ? 0x800 : 0);
            int collision = shaft ? 0 : Pointer == StationDraw.SaveIdle && run == 0 && cell == 0 ? 11 : 8;
            return LevelWord(collision, flip, tile);
        }
    }

    /// <summary>Map access cells, mirrored by side; the retracted pose writes a second cell beside the first.</summary>
    /// <param name="Pointer">The native draw list.</param>
    /// <param name="Side">The side of its station the arm sits on.</param>
    /// <param name="Pose">Whether the arm is extended.</param>
    internal sealed record MapAccess(StationDraw Pointer, HorizontalDirection Side, StationAccessPose Pose) : Draw(Pointer)
    {
        /// <inheritdoc/>
        public override int RunCount => Pose == StationAccessPose.Extended ? 1 : 2;

        /// <inheritdoc/>
        private protected override sbyte FirstRunX => Pose == StationAccessPose.Extended ? (sbyte)0
            : Side == HorizontalDirection.Left ? (sbyte)3 : (sbyte)-3;

        /// <inheritdoc/>
        private protected override ushort Word(int run, int cell) =>
            LevelWord(8, (Side == HorizontalDirection.Left ^ run != 0) ? 0x400 : 0, Pose == StationAccessPose.Extended ? 0x129 : 0x128);
    }

    /// <summary>A resource access cell, mirrored by side, whose extension turns its trigger solid.</summary>
    /// <param name="Pointer">The native draw list.</param>
    /// <param name="Side">The side of its station the arm sits on.</param>
    /// <param name="Pose">Whether the arm is extended.</param>
    internal sealed record ResourceAccess(StationDraw Pointer, HorizontalDirection Side, StationAccessPose Pose) : Draw(Pointer)
    {
        /// <inheritdoc/>
        public override int RunCount => 1;

        /// <inheritdoc/>
        [SuppressMessage("Design", "SME6426",
            Justification = "Every other station layout reads the run and cell it is asked for; a resource access arm is one cell, so its word needs neither coordinate.")]
        private protected override ushort Word(int run, int cell)
        {
            bool extended = Pose == StationAccessPose.Extended;
            return LevelWord(extended ? 8 : 11, Side == HorizontalDirection.Left ? 0 : 0x400, extended ? 0xc1 : 0xc3);
        }
    }

    /// <summary>The layout of <paramref name="pointer"/>, which must be one of the twenty station draw lists.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="pointer"/> is not a station draw list.</exception>
    internal static Draw Describe(ushort pointer) =>
        TryDescribe(pointer, out Draw? draw) ? draw : throw new InvalidOperationException($"${pointer:X4} is not a station draw list.");

    /// <summary>
    /// Describes a draw pointer handed over by the shared PLM draw interpreter; pointers
    /// outside the twenty station lists belong to other families.
    /// </summary>
    internal static bool TryDescribe(ushort pointer, [MaybeNullWhen(false)] out Draw draw)
    {
        if (!Enum.IsDefined((StationDraw)pointer))
        {
            draw = null;
            return false;
        }
        var station = (StationDraw)pointer;
        draw = station switch
        {
            StationDraw.MapFrame0 => new MapDisplay(station, 0),
            StationDraw.MapFrame1 => new MapDisplay(station, 1),
            StationDraw.MapFrame2 => new MapDisplay(station, 2),
            StationDraw.EnergyFrame0 => new ResourceDisplay(station, ResourceDisplay.EnergyFirstTile, 0),
            StationDraw.EnergyFrame1 => new ResourceDisplay(station, ResourceDisplay.EnergyFirstTile, 1),
            StationDraw.EnergyFrame2 => new ResourceDisplay(station, ResourceDisplay.EnergyFirstTile, 2),
            StationDraw.MissileFrame0 => new ResourceDisplay(station, ResourceDisplay.MissileFirstTile, 0),
            StationDraw.MissileFrame1 => new ResourceDisplay(station, ResourceDisplay.MissileFirstTile, 1),
            StationDraw.MissileFrame2 => new ResourceDisplay(station, ResourceDisplay.MissileFirstTile, 2),
            StationDraw.SaveIdle or StationDraw.SaveActive or StationDraw.SaveAlternate => new SavePod(station),
            StationDraw.MapRightRetracted => new MapAccess(station, HorizontalDirection.Right, StationAccessPose.Retracted),
            StationDraw.MapRightExtended => new MapAccess(station, HorizontalDirection.Right, StationAccessPose.Extended),
            StationDraw.MapLeftRetracted => new MapAccess(station, HorizontalDirection.Left, StationAccessPose.Retracted),
            StationDraw.MapLeftExtended => new MapAccess(station, HorizontalDirection.Left, StationAccessPose.Extended),
            StationDraw.ResourceRightRetracted => new ResourceAccess(station, HorizontalDirection.Right, StationAccessPose.Retracted),
            StationDraw.ResourceRightExtended => new ResourceAccess(station, HorizontalDirection.Right, StationAccessPose.Extended),
            StationDraw.ResourceLeftRetracted => new ResourceAccess(station, HorizontalDirection.Left, StationAccessPose.Retracted),
            StationDraw.ResourceLeftExtended => new ResourceAccess(station, HorizontalDirection.Left, StationAccessPose.Extended),
            _ => throw new InvalidOperationException($"Undefined station draw ${pointer:X4}."),
        };
        return true;
    }
    /// <summary>Calculates the twenty complete native station lists in published enumeration order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (StationDraw pointer in NativeOrder)
            {
                TryGet((ushort)pointer, out var list);
                yield return list;
            }
        }
    }
    /// <summary>Constructs a temporary artwork draw-list DTO for a station pointer; runtime drawing calculates cells directly.</summary>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmShotBlockDrawDefinitions.TryCompile<Draw>(pointer, TryDescribe, out list);

    /// <summary>
    /// Published ordinal artwork IDs: three numbered frames per map/resource
    /// station, and named save/access states. These are identity selections,
    /// independent of the physical words. Only complete native lists have IDs.
    /// </summary>
    internal static string VisualId(ushort pointer) =>
        ClosedNativeWords.Decode<StationDraw>(pointer, "station draw list") switch
        {
            StationDraw.MapFrame0 => "map-frame-0",
            StationDraw.MapFrame1 => "map-frame-1",
            StationDraw.MapFrame2 => "map-frame-2",
            StationDraw.EnergyFrame0 => "energy-frame-0",
            StationDraw.EnergyFrame1 => "energy-frame-1",
            StationDraw.EnergyFrame2 => "energy-frame-2",
            StationDraw.MissileFrame0 => "missile-frame-0",
            StationDraw.MissileFrame1 => "missile-frame-1",
            StationDraw.MissileFrame2 => "missile-frame-2",
            StationDraw.SaveIdle => "save-idle",
            StationDraw.SaveActive => "save-active-a",
            StationDraw.SaveAlternate => "save-active-b",
            StationDraw.MapRightRetracted => "map-right-retracted",
            StationDraw.MapRightExtended => "map-right-extended",
            StationDraw.MapLeftRetracted => "map-left-retracted",
            StationDraw.MapLeftExtended => "map-left-extended",
            StationDraw.ResourceRightRetracted => "resource-right-retracted",
            StationDraw.ResourceRightExtended => "resource-right-extended",
            StationDraw.ResourceLeftRetracted => "resource-left-retracted",
            StationDraw.ResourceLeftExtended => "resource-left-extended",
            _ => throw new InvalidOperationException($"Undefined station draw list ${pointer:X4}."),
        };

    /// <summary>Finds a station draw list by its stable artwork identity rather than native pointer.</summary>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (var candidate in All)
        {
            if (string.Equals(id, VisualId(candidate.Pointer), StringComparison.Ordinal))
            {
                list = candidate;
                return true;
            }
        }
        list = default;
        return false;
    }

}
