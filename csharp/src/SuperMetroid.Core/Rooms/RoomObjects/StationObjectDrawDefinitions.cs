using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The block shape each of the twenty station draws draws (#1862). What each frame shows is its installed picture (#1557).
/// </summary>
internal static class StationObjectDrawDefinitions
{
    /// <summary>A map station's display: its block and the one left of it, both solid.</summary>
    private static readonly RoomBlockShape MapDisplay = new([new(new(0, 0), RoomCollisionType.SolidBlock), new(new(-1, 0), RoomCollisionType.SolidBlock)]);

    /// <summary>A refill or ammunition station's display: a solid block under a slope.</summary>
    private static readonly RoomBlockShape ResourceDisplay = new([new(new(0, 0), RoomCollisionType.SolidBlock), new(new(0, -1), RoomCollisionType.Slope)]);

    /// <summary>The idle save pod: six rows two blocks wide up from its floor, the floor's left block its trigger, the shaft air and the cap solid.</summary>
    private static readonly RoomBlockShape SavePodIdle = new(
    [
        new(new(0, 0), RoomCollisionType.SpecialBlock), new(new(1, 0), RoomCollisionType.SolidBlock),
        new(new(0, -1), RoomCollisionType.Air), new(new(1, -1), RoomCollisionType.Air),
        new(new(0, -2), RoomCollisionType.Air), new(new(1, -2), RoomCollisionType.Air),
        new(new(0, -3), RoomCollisionType.Air), new(new(1, -3), RoomCollisionType.Air),
        new(new(0, -4), RoomCollisionType.Air), new(new(1, -4), RoomCollisionType.Air),
        new(new(0, -5), RoomCollisionType.SolidBlock), new(new(1, -5), RoomCollisionType.SolidBlock),
    ]);

    /// <summary>The active save pod: its floor and cap solid around the air shaft.</summary>
    private static readonly RoomBlockShape SavePodActive = new(
    [
        new(new(0, 0), RoomCollisionType.SolidBlock), new(new(1, 0), RoomCollisionType.SolidBlock),
        new(new(0, -1), RoomCollisionType.Air), new(new(1, -1), RoomCollisionType.Air),
        new(new(0, -2), RoomCollisionType.Air), new(new(1, -2), RoomCollisionType.Air),
        new(new(0, -3), RoomCollisionType.Air), new(new(1, -3), RoomCollisionType.Air),
        new(new(0, -4), RoomCollisionType.Air), new(new(1, -4), RoomCollisionType.Air),
        new(new(0, -5), RoomCollisionType.SolidBlock), new(new(1, -5), RoomCollisionType.SolidBlock),
    ]);

    /// <summary>A retracted map access arm on its station's right: its block and the one three left of it, both solid.</summary>
    private static readonly RoomBlockShape MapRightRetracted = new([new(new(0, 0), RoomCollisionType.SolidBlock), new(new(-3, 0), RoomCollisionType.SolidBlock)]);

    /// <summary>A retracted map access arm on its station's left: its block and the one three right of it, both solid.</summary>
    private static readonly RoomBlockShape MapLeftRetracted = new([new(new(0, 0), RoomCollisionType.SolidBlock), new(new(3, 0), RoomCollisionType.SolidBlock)]);

    /// <summary>An extended map access arm: its solid block.</summary>
    private static readonly RoomBlockShape MapExtended = RoomBlockShapes.Block(RoomCollisionType.SolidBlock);

    /// <summary>A retracted resource access arm: its trigger block.</summary>
    private static readonly RoomBlockShape ResourceRetracted = RoomBlockShapes.Block(RoomCollisionType.SpecialBlock);

    /// <summary>An extended resource access arm: its block, turned solid.</summary>
    private static readonly RoomBlockShape ResourceExtended = RoomBlockShapes.Block(RoomCollisionType.SolidBlock);

    /// <summary>The block shape each station draw draws.</summary>
    internal static readonly IReadOnlyDictionary<StationDraw, RoomBlockShape> Shapes = new Dictionary<StationDraw, RoomBlockShape>
    {
        [StationDraw.MapFrame0] = MapDisplay,
        [StationDraw.MapFrame1] = MapDisplay,
        [StationDraw.MapFrame2] = MapDisplay,
        [StationDraw.MapRightRetracted] = MapRightRetracted,
        [StationDraw.MapRightExtended] = MapExtended,
        [StationDraw.MapLeftRetracted] = MapLeftRetracted,
        [StationDraw.MapLeftExtended] = MapExtended,
        [StationDraw.EnergyFrame0] = ResourceDisplay,
        [StationDraw.EnergyFrame1] = ResourceDisplay,
        [StationDraw.EnergyFrame2] = ResourceDisplay,
        [StationDraw.MissileFrame0] = ResourceDisplay,
        [StationDraw.MissileFrame1] = ResourceDisplay,
        [StationDraw.MissileFrame2] = ResourceDisplay,
        [StationDraw.ResourceRightRetracted] = ResourceRetracted,
        [StationDraw.ResourceRightExtended] = ResourceExtended,
        [StationDraw.ResourceLeftRetracted] = ResourceRetracted,
        [StationDraw.ResourceLeftExtended] = ResourceExtended,
        [StationDraw.SaveIdle] = SavePodIdle,
        [StationDraw.SaveActive] = SavePodActive,
        [StationDraw.SaveAlternate] = SavePodActive,
    };
}
