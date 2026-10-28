using SuperMetroid.Core.Rooms.LevelData;
using System.Diagnostics.CodeAnalysis;
using static SuperMetroid.Core.Rooms.Plms.RoomPlmStationDrawDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>How each station draw's cells are described, found and named.</summary>
internal static class StationPlmDraws
{
    /// <summary>The layout of <paramref name="request"/>, which must be one of the twenty station draw lists.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="request"/> is not a station draw list.</exception>
    internal static Draw Describe(PlmDraw request) =>
        TryDescribe(request, out Draw? draw) ? draw : throw new InvalidOperationException($"{request} is not a station draw list.");

    /// <summary>
    /// Describes a draw handed over by the shared PLM drawing; draws of other families are not the stations'.
    /// </summary>
    internal static bool TryDescribe(PlmDraw request, [MaybeNullWhen(false)] out Draw draw)
    {
        if (request is not PlmFrameDraw<StationDraw>)
        {
            draw = null;
            return false;
        }
        var station = PlmDraw.FrameOf<StationDraw>(request, "StationDraw");
        draw = station switch
        {
            StationDraw.MapFrame0 => new MapDisplay(station, 0),
            StationDraw.MapFrame1 => new MapDisplay(station, 1),
            StationDraw.MapFrame2 => new MapDisplay(station, 2),
            StationDraw.EnergyFrame0 => new ResourceDisplay(station, EnergyFirstBlock, 0),
            StationDraw.EnergyFrame1 => new ResourceDisplay(station, EnergyFirstBlock, 1),
            StationDraw.EnergyFrame2 => new ResourceDisplay(station, EnergyFirstBlock, 2),
            StationDraw.MissileFrame0 => new ResourceDisplay(station, MissileFirstBlock, 0),
            StationDraw.MissileFrame1 => new ResourceDisplay(station, MissileFirstBlock, 1),
            StationDraw.MissileFrame2 => new ResourceDisplay(station, MissileFirstBlock, 2),
            StationDraw.SaveIdle or StationDraw.SaveActive or StationDraw.SaveAlternate => new SavePod(station),
            StationDraw.MapRightRetracted => new MapAccess(station, HorizontalDirection.Right, StationAccessPose.Retracted),
            StationDraw.MapRightExtended => new MapAccess(station, HorizontalDirection.Right, StationAccessPose.Extended),
            StationDraw.MapLeftRetracted => new MapAccess(station, HorizontalDirection.Left, StationAccessPose.Retracted),
            StationDraw.MapLeftExtended => new MapAccess(station, HorizontalDirection.Left, StationAccessPose.Extended),
            StationDraw.ResourceRightRetracted => new ResourceAccess(station, HorizontalDirection.Right, StationAccessPose.Retracted),
            StationDraw.ResourceRightExtended => new ResourceAccess(station, HorizontalDirection.Right, StationAccessPose.Extended),
            StationDraw.ResourceLeftRetracted => new ResourceAccess(station, HorizontalDirection.Left, StationAccessPose.Retracted),
            StationDraw.ResourceLeftExtended => new ResourceAccess(station, HorizontalDirection.Left, StationAccessPose.Extended),
            _ => throw new InvalidOperationException($"Undefined station draw {request}."),
        };
        return true;
    }

    /// <summary>Constructs a temporary artwork draw-list DTO for a station pointer; runtime drawing calculates cells directly.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        ShotBlockPlmDraws.TryCompile<Draw>(request, TryDescribe, out list);

    /// <summary>
    /// Published ordinal artwork IDs: three numbered frames per map/resource
    /// station, and named save/access states. These are identity selections,
    /// independent of the physical words. Only complete native lists have IDs.
    /// </summary>
    internal static string VisualId(PlmDraw request) =>
        PlmDraw.FrameOf<StationDraw>(request, "station draw list") switch
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
            _ => throw new InvalidOperationException($"Undefined station draw list {request}."),
        };

    /// <summary>Finds a station draw list by its stable artwork identity rather than native pointer.</summary>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (var candidate in All)
        {
            if (string.Equals(id, VisualId(candidate.Draw), StringComparison.Ordinal))
            {
                list = candidate;
                return true;
            }
        }
        list = default;
        return false;
    }

    /// <summary>Calculates the twenty complete native station lists in published enumeration order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (StationDraw frame in PublishedOrder)
            {
                TryGet(PlmDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }

    /// <summary>
    /// One native station draw list's geometry. Station runs are horizontal and every continuation is an absolute
    /// origin-relative offset, not accumulated. Each layout is its own sealed record holding only what it needs; these
    /// rules cover only the twenty native owned draw pointers.
    /// </summary>
    /// <param name="Id">The native draw list.</param>
    internal abstract record Draw(StationDraw Id) : IPlmRunDrawShape
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
            return run == 0 ? checked((sbyte)FirstRunX) : (sbyte)0;
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
        private protected virtual int FirstRunX => 0;

        /// <summary>The vertical offset after <paramref name="run"/>.</summary>
        private protected virtual sbyte RunY(int run) => 0;

        /// <summary>The level word at <paramref name="cell"/> of <paramref name="run"/>, both in range.</summary>
        private protected abstract ushort Word(int run, int cell);

        /// <summary>Builds one native level word from its collision type, flip bits and tile.</summary>
        private protected static ushort LevelWord(RoomCollisionType collision, LevelBlockFlipFlags flips, int block) =>
            RoomLevelWord.Create(block, flips, collision).Raw;

        /// <summary>Rejects a run outside this layout's native command count.</summary>
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new ArgumentOutOfRangeException(nameof(run));
        }
    }

    /// <summary>Two horizontal display cells written origin then left; the art advances 32 tiles per frame.</summary>
    /// <param name="Id">The native draw list.</param>
    /// <param name="Frame">The display frame, zero through two.</param>
    internal sealed record MapDisplay(StationDraw Id, int Frame) : Draw(Id)
    {
        /// <inheritdoc/>
        public override int RunCount => 2;

        /// <inheritdoc/>
        private protected override int FirstRunX => -1;

        /// <inheritdoc/>
        private protected override ushort Word(int run, int cell) =>
            LevelWord(RoomCollisionType.SolidBlock, LevelBlockFlipFlags.None, MapFirstBlock + Frame * 32 - run);
    }

    /// <summary>
    /// A vertical refill or ammunition display written origin then above; the art advances one tile per frame and its upper
    /// cell, a slope, is 32 tiles earlier.
    /// </summary>
    /// <param name="Id">The native draw list.</param>
    /// <param name="FirstTile">The display's tile at frame zero.</param>
    /// <param name="Frame">The display frame, zero through two.</param>
    internal sealed record ResourceDisplay(StationDraw Id, int FirstTile, int Frame) : Draw(Id)
    {


        /// <inheritdoc/>
        public override int RunCount => 2;

        /// <inheritdoc/>
        private protected override sbyte RunY(int run) => run == 0 ? (sbyte)-1 : (sbyte)0;

        /// <inheritdoc/>
        private protected override ushort Word(int run, int cell) =>
            LevelWord(run == 0 ? RoomCollisionType.SolidBlock : RoomCollisionType.Slope, LevelBlockFlipFlags.None, FirstTile + Frame - run * 32);
    }

    /// <summary>
    /// Six two-cell save-pod rows from floor to cap. The art mirrors horizontally, the floor also vertically, and the
    /// active pose advances one tile. Floor and cap are solid except the idle floor-left trigger; the shaft is air.
    /// </summary>
    /// <param name="Id">The native draw list.</param>
    internal sealed record SavePod(StationDraw Id) : Draw(Id)
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
            int tile = (shaft ? SaveShaftBlock : SavePodBlock) + (Id == StationDraw.SaveActive ? 1 : 0);
            LevelBlockFlipFlags flips = (cell == 1 ? LevelBlockFlipFlags.Horizontal : LevelBlockFlipFlags.None) |
                (run == 0 ? LevelBlockFlipFlags.Vertical : LevelBlockFlipFlags.None);
            RoomCollisionType collision = shaft ? RoomCollisionType.Air
                : Id == StationDraw.SaveIdle && run == 0 && cell == 0 ? RoomCollisionType.SpecialBlock : RoomCollisionType.SolidBlock;
            return LevelWord(collision, flips, tile);
        }
    }

    /// <summary>Map access cells, mirrored by side; the retracted pose writes a second cell beside the first.</summary>
    /// <param name="Id">The native draw list.</param>
    /// <param name="Side">The side of its station the arm sits on.</param>
    /// <param name="Pose">Whether the arm is extended.</param>
    internal sealed record MapAccess(StationDraw Id, HorizontalDirection Side, StationAccessPose Pose) : Draw(Id)
    {
        /// <inheritdoc/>
        public override int RunCount => Pose == StationAccessPose.Extended ? 1 : 2;

        /// <inheritdoc/>
        private protected override int FirstRunX => Pose == StationAccessPose.Extended ? 0
            : Side == HorizontalDirection.Left ? 3 : -3;

        /// <inheritdoc/>
        private protected override ushort Word(int run, int cell) =>
            LevelWord(RoomCollisionType.SolidBlock,
                (Side == HorizontalDirection.Left ^ run != 0) ? LevelBlockFlipFlags.Horizontal : LevelBlockFlipFlags.None,
                Pose == StationAccessPose.Extended ? MapAccessExtendedBlock : MapAccessRetractedBlock);
    }

    /// <summary>A resource access cell, mirrored by side, whose extension turns its trigger solid.</summary>
    /// <param name="Id">The native draw list.</param>
    /// <param name="Side">The side of its station the arm sits on.</param>
    /// <param name="Pose">Whether the arm is extended.</param>
    internal sealed record ResourceAccess(StationDraw Id, HorizontalDirection Side, StationAccessPose Pose) : Draw(Id)
    {
        /// <inheritdoc/>
        public override int RunCount => 1;

        /// <inheritdoc/>
        [SuppressMessage("Design", "SME6426",
            Justification = "Every other station layout reads the run and cell it is asked for; a resource access arm is one cell, so its word needs neither coordinate.")]
        private protected override ushort Word(int run, int cell)
        {
            bool extended = Pose == StationAccessPose.Extended;
            return LevelWord(extended ? RoomCollisionType.SolidBlock : RoomCollisionType.SpecialBlock,
                Side == HorizontalDirection.Left ? LevelBlockFlipFlags.None : LevelBlockFlipFlags.Horizontal,
                extended ? ResourceAccessExtendedBlock : ResourceAccessRetractedBlock);
        }
    }
}
