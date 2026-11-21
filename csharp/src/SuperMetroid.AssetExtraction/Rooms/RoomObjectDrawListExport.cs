using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;
using static SuperMetroid.AssetExtraction.Rooms.RoomObjectDrawLists;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Builds the run-by-run draw lists the import lays room-object frames out from (#1557), from the shapes and collision
/// the game draws by. The game draws from the shapes directly and never builds these lists; what each cell shows is
/// pictured from the cartridge.
/// </summary>
internal static class RoomObjectDrawListExport
{
    /// <summary>
    /// Builds the draw list of a draw written as <paramref name="runs"/> (#1546), each run one description of where it
    /// starts, its direction and its cells, and each cell colliding as the game's <paramref name="shape"/> says; the
    /// cartridge's continuation after each run is derived by <see cref="RoomObjectRunContinuationCodec"/>.
    /// </summary>
    /// <param name="draw">The draw the list exports.</param>
    /// <param name="shape">The block shape the game draws the draw as, whose collision each cell takes.</param>
    /// <param name="runs">The draw's runs in the cartridge's write order; the first starts at the origin.</param>
    /// <exception cref="InvalidDataException">The draw has no run, or its first run does not start at its origin.</exception>
    internal static DrawList Runs(RoomObjectDraw draw, RoomBlockShape shape, IReadOnlyList<RoomObjectShapeRun> runs)
    {
        Ensure.NotNull(shape);
        if (Ensure.NotNull(runs).Count < 1 || runs[0].Start != Offset16<RoomBlocks>.Zero)
            throw new InvalidDataException($"Draw {draw} does not start its first run at its origin.");
        return new(draw, [.. runs.Select(run => new Run(run.Direction,
            [.. Enumerable.Range(0, run.Count).Select(cell => shape.CollisionAt(run.CellAt(cell)))], run.Start))]);
    }

    /// <summary>Builds the draw list of a draw that writes one run and ends there.</summary>
    /// <param name="draw">The draw the list exports.</param>
    /// <param name="direction">The direction the run writes its cells in.</param>
    /// <param name="collisions">How each of the run's cells collides.</param>
    internal static DrawList SingleRun(RoomObjectDraw draw, RoomObjectRunDirection direction, RoomCollisionType[] collisions) =>
        new(draw, new Run[] { new(direction, collisions, new(0, 0)) });
}
