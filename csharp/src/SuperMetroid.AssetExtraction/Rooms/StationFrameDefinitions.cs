using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Each station frame the import pictures (#1557, #1862): the stations whose kind draws it, and its block shape written as
/// rows, the object's block first.
/// </summary>
internal static class StationFrameDefinitions
{
    /// <summary>The object's own block, which every frame writes first.</summary>
    private static readonly RoomObjectShapeRun Own = RoomObjectShapeRun.Row(new(0, 0), 1);

    /// <summary>The object's own block with a second block above it, as a refill station's display writes.</summary>
    private static readonly RoomObjectShapeRun[] OwnAndAbove = [Own, RoomObjectShapeRun.Row(new(0, -1), 1)];

    /// <summary>A map display: its second block beside the first, to its left.</summary>
    private static readonly StationFrameDefinition MapDisplay = new([RoomObjectKind.MapStation], [Own, RoomObjectShapeRun.Row(new(-1, 0), 1)]);

    /// <summary>An energy display: its second block above the first.</summary>
    private static readonly StationFrameDefinition EnergyDisplay = new([RoomObjectKind.EnergyStation], OwnAndAbove);

    /// <summary>A missile display: its second block above the first.</summary>
    private static readonly StationFrameDefinition MissileDisplay = new([RoomObjectKind.MissileStation], OwnAndAbove);

    /// <summary>The save pod: six two-block rows upward.</summary>
    private static readonly StationFrameDefinition SavePod = new([RoomObjectKind.SaveStation],
    [
        RoomObjectShapeRun.Row(new(0, 0), 2), RoomObjectShapeRun.Row(new(0, -1), 2), RoomObjectShapeRun.Row(new(0, -2), 2),
        RoomObjectShapeRun.Row(new(0, -3), 2), RoomObjectShapeRun.Row(new(0, -4), 2), RoomObjectShapeRun.Row(new(0, -5), 2),
    ]);

    /// <summary>An extended map access arm: its own block alone.</summary>
    private static readonly StationFrameDefinition MapArmExtended = new([RoomObjectKind.MapStation], [Own]);

    /// <summary>A refill station's access arm, which the energy and the missile stations both draw: its own block alone.</summary>
    private static readonly StationFrameDefinition ResourceArm = new([RoomObjectKind.EnergyStation, RoomObjectKind.MissileStation], [Own]);

    /// <summary>Each station frame's definition.</summary>
    internal static readonly IReadOnlyDictionary<StationDraw, StationFrameDefinition> Frames = new Dictionary<StationDraw, StationFrameDefinition>
    {
        [StationDraw.MapDim] = MapDisplay,
        [StationDraw.MapLit] = MapDisplay,
        [StationDraw.MapBright] = MapDisplay,
        [StationDraw.MapRightRetracted] = new([RoomObjectKind.MapStation], [Own, RoomObjectShapeRun.Row(new(-3, 0), 1)]),
        [StationDraw.MapLeftRetracted] = new([RoomObjectKind.MapStation], [Own, RoomObjectShapeRun.Row(new(3, 0), 1)]),
        [StationDraw.MapRightExtended] = MapArmExtended,
        [StationDraw.MapLeftExtended] = MapArmExtended,
        [StationDraw.EnergyDim] = EnergyDisplay,
        [StationDraw.EnergyLit] = EnergyDisplay,
        [StationDraw.EnergyBright] = EnergyDisplay,
        [StationDraw.MissileDim] = MissileDisplay,
        [StationDraw.MissileLit] = MissileDisplay,
        [StationDraw.MissileBright] = MissileDisplay,
        [StationDraw.SaveIdle] = SavePod,
        [StationDraw.SaveLit] = SavePod,
        [StationDraw.SaveDark] = SavePod,
        [StationDraw.ResourceRightRetracted] = ResourceArm,
        [StationDraw.ResourceLeftRetracted] = ResourceArm,
        [StationDraw.ResourceRightExtended] = ResourceArm,
        [StationDraw.ResourceLeftExtended] = ResourceArm,
    };
}
