using SuperMetroid.Core.Rooms.LevelData;
using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// How each station draw's cells are laid out and collide. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class StationObjectDraws
{
    /// <summary>
    /// Describes a draw handed over by the shared room-object drawing; draws of other families are not the stations'.
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
            StationDraw.MapFrame0 or StationDraw.MapFrame1 or StationDraw.MapFrame2 => new MapDisplay(station),
            StationDraw.EnergyFrame0 or StationDraw.EnergyFrame1 or StationDraw.EnergyFrame2 or
                StationDraw.MissileFrame0 or StationDraw.MissileFrame1 or StationDraw.MissileFrame2 => new ResourceDisplay(station),
            StationDraw.SaveIdle or StationDraw.SaveActive or StationDraw.SaveAlternate => new SavePod(station),
            StationDraw.MapRightRetracted => new MapAccess(station, HorizontalDirection.Right, StationAccessPose.Retracted),
            StationDraw.MapRightExtended => new MapAccess(station, HorizontalDirection.Right, StationAccessPose.Extended),
            StationDraw.MapLeftRetracted => new MapAccess(station, HorizontalDirection.Left, StationAccessPose.Retracted),
            StationDraw.MapLeftExtended => new MapAccess(station, HorizontalDirection.Left, StationAccessPose.Extended),
            StationDraw.ResourceRightRetracted or StationDraw.ResourceLeftRetracted => new ResourceAccess(station, StationAccessPose.Retracted),
            StationDraw.ResourceRightExtended or StationDraw.ResourceLeftExtended => new ResourceAccess(station, StationAccessPose.Extended),
            _ => throw new InvalidOperationException($"Undefined station draw {request}."),
        };
        return true;
    }

    /// <summary>
    /// One station draw list's geometry. Station runs are horizontal and each starts at its own offset from the draw's
    /// origin; the list ends at its last run. Each layout is its own sealed record holding only what it needs; these
    /// rules cover only the twenty owned draws.
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

        /// <summary>How the cell at <paramref name="cell"/> of run <paramref name="run"/> collides.</summary>
        internal RoomCollisionType CollisionAt(int run, int cell)
        {
            if ((uint)cell >= (uint)BlockCount(run)) throw new ArgumentOutOfRangeException(nameof(cell));
            return Collision(run, cell);
        }

        /// <summary>The blocks each run writes.</summary>
        private protected virtual int BlocksPerRun => 1;

        /// <summary>Where the second run starts across from the origin; every other run starts in the origin's column.</summary>
        private protected virtual int SecondRunX => 0;

        /// <summary>The row <paramref name="run"/> starts on, from the origin's.</summary>
        private protected virtual int RunStartY(int run) => 0;

        /// <summary>How the cell at <paramref name="cell"/> of <paramref name="run"/>, both in range, collides.</summary>
        private protected abstract RoomCollisionType Collision(int run, int cell);

        /// <summary>Rejects a run outside this layout's run count.</summary>
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new ArgumentOutOfRangeException(nameof(run));
        }
    }

    /// <summary>Two horizontal solid display cells written origin then left.</summary>
    /// <param name="Id">The draw list.</param>
    internal sealed record MapDisplay(StationDraw Id) : Draw(Id)
    {
        /// <inheritdoc/>
        internal override int RunCount => 2;

        /// <inheritdoc/>
        private protected override int SecondRunX => -1;

        /// <inheritdoc/>
        [SuppressMessage("Design", "SME6426",
            Justification = "Every cell of a map display is solid, so its collision needs neither coordinate; other station layouts read them.")]
        private protected override RoomCollisionType Collision(int run, int cell) => RoomCollisionType.SolidBlock;
    }

    /// <summary>A vertical refill or ammunition display written origin then above: a solid cell under a slope.</summary>
    /// <param name="Id">The draw list.</param>
    internal sealed record ResourceDisplay(StationDraw Id) : Draw(Id)
    {
        /// <inheritdoc/>
        internal override int RunCount => 2;

        /// <inheritdoc/>
        private protected override int RunStartY(int run) => -run;

        /// <inheritdoc/>
        private protected override RoomCollisionType Collision(int run, int cell) =>
            run == 0 ? RoomCollisionType.SolidBlock : RoomCollisionType.Slope;
    }

    /// <summary>
    /// Six two-cell save-pod rows from floor to cap. Floor and cap are solid except the idle floor-left trigger; the shaft
    /// is air.
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
        private protected override RoomCollisionType Collision(int run, int cell) =>
            run is > 0 and < 5 ? RoomCollisionType.Air
                : Id == StationDraw.SaveIdle && run == 0 && cell == 0 ? RoomCollisionType.SpecialBlock : RoomCollisionType.SolidBlock;
    }

    /// <summary>Solid map access cells; the retracted pose writes a second cell three blocks beside the first.</summary>
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
        [SuppressMessage("Design", "SME6426",
            Justification = "Every cell of a map access arm is solid, so its collision needs neither coordinate; other station layouts read them.")]
        private protected override RoomCollisionType Collision(int run, int cell) => RoomCollisionType.SolidBlock;
    }

    /// <summary>A resource access cell whose extension turns its trigger solid.</summary>
    /// <param name="Id">The draw list.</param>
    /// <param name="Pose">Whether the arm is extended.</param>
    internal sealed record ResourceAccess(StationDraw Id, StationAccessPose Pose) : Draw(Id)
    {
        /// <inheritdoc/>
        internal override int RunCount => 1;

        /// <inheritdoc/>
        [SuppressMessage("Design", "SME6426",
            Justification = "Every other station layout reads the run and cell it is asked for; a resource access arm is one cell, so its collision needs neither coordinate.")]
        private protected override RoomCollisionType Collision(int run, int cell) =>
            Pose == StationAccessPose.Extended ? RoomCollisionType.SolidBlock : RoomCollisionType.SpecialBlock;
    }
}
