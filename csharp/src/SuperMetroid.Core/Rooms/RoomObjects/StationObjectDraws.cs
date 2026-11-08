using SuperMetroid.Core.Rooms.LevelData;
using System.Diagnostics.CodeAnalysis;
using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectStationDrawDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>How each station draw's cells are described.</summary>
internal static class StationObjectDraws
{
    /// <summary>
    /// Describes a draw handed over by the shared PLM drawing; draws of other families are not the stations'.
    /// </summary>
    internal static bool TryDescribe(RoomObjectDraw request, [MaybeNullWhen(false)] out Draw draw)
    {
        if (request is not RoomObjectFrameDraw<StationDraw>)
        {
            draw = null;
            return false;
        }
        var station = RoomObjectDraw.FrameOf<StationDraw>(request, "StationDraw");
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

    /// <summary>
    /// One station draw list's geometry. Station runs are horizontal and each starts at its own offset from the draw's
    /// origin; the list ends at its last run. Each layout is its own sealed record holding only what it needs; these
    /// rules cover only the twenty owned draw pointers.
    /// </summary>
    /// <param name="Id">The draw list.</param>
    internal abstract record Draw(StationDraw Id)
    {
        /// <summary>The number of runs the draw writes.</summary>
        internal abstract int RunCount { get; }

        /// <summary>The number of blocks in run <paramref name="run"/>.</summary>
        internal int BlockCount(int run)
        {
            CheckRun(run);
            return BlocksPerRun;
        }

        /// <summary>Where run <paramref name="run"/> starts, in blocks from the draw's origin; the first run starts at the origin.</summary>
        internal Offset16<RoomBlocks> Start(int run)
        {
            CheckRun(run);
            return new(run == 1 ? SecondRunX : 0, RunStartY(run));
        }

        /// <summary>The terrain at <paramref name="cell"/> of run <paramref name="run"/>.</summary>
        internal RoomTerrainBlock BlockAt(int run, int cell)
        {
            if ((uint)cell >= (uint)BlockCount(run)) throw new ArgumentOutOfRangeException(nameof(cell));
            return Block(run, cell);
        }

        /// <summary>The blocks each run writes.</summary>
        private protected virtual int BlocksPerRun => 1;

        /// <summary>Where the second run starts across from the origin; every other run starts in the origin's column.</summary>
        private protected virtual int SecondRunX => 0;

        /// <summary>The row <paramref name="run"/> starts on, from the origin's.</summary>
        private protected virtual int RunStartY(int run) => 0;

        /// <summary>The terrain at <paramref name="cell"/> of <paramref name="run"/>, both in range.</summary>
        private protected abstract RoomTerrainBlock Block(int run, int cell);

        /// <summary>Builds one block from its collision kind, mirroring and block number.</summary>
        private protected static RoomTerrainBlock Terrain(RoomCollisionType collision, BlockMirroring flips, int block) =>
            RoomTerrainBlock.Of(block, flips, collision);

        /// <summary>Rejects a run outside this layout's command count.</summary>
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new ArgumentOutOfRangeException(nameof(run));
        }
    }

    /// <summary>Two horizontal display cells written origin then left; the art advances 32 tiles per frame.</summary>
    /// <param name="Id">The draw list.</param>
    /// <param name="Frame">The display frame, zero through two.</param>
    internal sealed record MapDisplay(StationDraw Id, int Frame) : Draw(Id)
    {
        /// <inheritdoc/>
        internal override int RunCount => 2;

        /// <inheritdoc/>
        private protected override int SecondRunX => -1;

        /// <inheritdoc/>
        private protected override RoomTerrainBlock Block(int run, int cell) =>
            Terrain(RoomCollisionType.SolidBlock, BlockMirroring.None, MapFirstBlock + Frame * 32 - run);
    }

    /// <summary>
    /// A vertical refill or ammunition display written origin then above; the art advances one tile per frame and its upper
    /// cell, a slope, is 32 tiles earlier.
    /// </summary>
    /// <param name="Id">The draw list.</param>
    /// <param name="FirstTile">The display's tile at frame zero.</param>
    /// <param name="Frame">The display frame, zero through two.</param>
    internal sealed record ResourceDisplay(StationDraw Id, int FirstTile, int Frame) : Draw(Id)
    {


        /// <inheritdoc/>
        internal override int RunCount => 2;

        /// <inheritdoc/>
        private protected override int RunStartY(int run) => -run;

        /// <inheritdoc/>
        private protected override RoomTerrainBlock Block(int run, int cell) =>
            Terrain(run == 0 ? RoomCollisionType.SolidBlock : RoomCollisionType.Slope, BlockMirroring.None, FirstTile + Frame - run * 32);
    }

    /// <summary>
    /// Six two-cell save-pod rows from floor to cap. The art mirrors horizontally, the floor also vertically, and the
    /// active pose advances one tile. Floor and cap are solid except the idle floor-left trigger; the shaft is air.
    /// </summary>
    /// <param name="Id">The draw list.</param>
    internal sealed record SavePod(StationDraw Id) : Draw(Id)
    {
        /// <inheritdoc/>
        internal override int RunCount => 6;

        /// <inheritdoc/>
        private protected override int BlocksPerRun => 2;

        /// <inheritdoc/>
        private protected override int RunStartY(int run) => -run;

        /// <inheritdoc/>
        private protected override RoomTerrainBlock Block(int run, int cell)
        {
            bool shaft = run is > 0 and < 5;
            int tile = (shaft ? SaveShaftBlock : SavePodBlock) + (Id == StationDraw.SaveActive ? 1 : 0);
            BlockMirroring flips = (cell == 1 ? BlockMirroring.Horizontal : BlockMirroring.None) |
                (run == 0 ? BlockMirroring.Vertical : BlockMirroring.None);
            RoomCollisionType collision = shaft ? RoomCollisionType.Air
                : Id == StationDraw.SaveIdle && run == 0 && cell == 0 ? RoomCollisionType.SpecialBlock : RoomCollisionType.SolidBlock;
            return Terrain(collision, flips, tile);
        }
    }

    /// <summary>Map access cells, mirrored by side; the retracted pose writes a second cell beside the first.</summary>
    /// <param name="Id">The draw list.</param>
    /// <param name="Side">The side of its station the arm sits on.</param>
    /// <param name="Pose">Whether the arm is extended.</param>
    internal sealed record MapAccess(StationDraw Id, HorizontalDirection Side, StationAccessPose Pose) : Draw(Id)
    {
        /// <inheritdoc/>
        internal override int RunCount => Pose == StationAccessPose.Extended ? 1 : 2;

        /// <inheritdoc/>
        private protected override int SecondRunX => Pose == StationAccessPose.Extended ? 0
            : Side == HorizontalDirection.Left ? 3 : -3;

        /// <inheritdoc/>
        private protected override RoomTerrainBlock Block(int run, int cell) =>
            Terrain(RoomCollisionType.SolidBlock,
                (Side == HorizontalDirection.Left ^ run != 0) ? BlockMirroring.Horizontal : BlockMirroring.None,
                Pose == StationAccessPose.Extended ? MapAccessExtendedBlock : MapAccessRetractedBlock);
    }

    /// <summary>A resource access cell, mirrored by side, whose extension turns its trigger solid.</summary>
    /// <param name="Id">The draw list.</param>
    /// <param name="Side">The side of its station the arm sits on.</param>
    /// <param name="Pose">Whether the arm is extended.</param>
    internal sealed record ResourceAccess(StationDraw Id, HorizontalDirection Side, StationAccessPose Pose) : Draw(Id)
    {
        /// <inheritdoc/>
        internal override int RunCount => 1;

        /// <inheritdoc/>
        [SuppressMessage("Design", "SME6426",
            Justification = "Every other station layout reads the run and cell it is asked for; a resource access arm is one cell, so its block needs neither coordinate.")]
        private protected override RoomTerrainBlock Block(int run, int cell)
        {
            bool extended = Pose == StationAccessPose.Extended;
            return Terrain(extended ? RoomCollisionType.SolidBlock : RoomCollisionType.SpecialBlock,
                Side == HorizontalDirection.Left ? BlockMirroring.None : BlockMirroring.Horizontal,
                extended ? ResourceAccessExtendedBlock : ResourceAccessRetractedBlock);
        }
    }
}
