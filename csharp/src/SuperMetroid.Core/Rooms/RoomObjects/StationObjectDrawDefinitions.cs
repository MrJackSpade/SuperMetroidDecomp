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

    /// <summary>The save pod's shaft: two blocks wide and four tall above its floor, all air.</summary>
    private static readonly RoomBlockShape SavePodShaft = RoomBlockShapes.Rectangle(new(0, -4), new(2, 4), RoomCollisionType.Air);

    /// <summary>The save pod's cap: the two solid blocks above its shaft.</summary>
    private static readonly RoomBlockShape SavePodCap = RoomBlockShapes.Rectangle(new(0, -5), new(2, 1), RoomCollisionType.SolidBlock);

    /// <summary>The idle save pod: its floor's left block its trigger and its right block solid, under the shaft and cap.</summary>
    private static readonly RoomBlockShape SavePodIdle = RoomBlockShapes.Joined(
        RoomBlockShapes.Line(down: false, [RoomCollisionType.SpecialBlock, RoomCollisionType.SolidBlock]), SavePodShaft, SavePodCap);

    /// <summary>The active save pod: its floor solid, under the shaft and cap.</summary>
    private static readonly RoomBlockShape SavePodActive = RoomBlockShapes.Joined(
        RoomBlockShapes.Row(2, RoomCollisionType.SolidBlock), SavePodShaft, SavePodCap);

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
        [StationDraw.MapDim] = MapDisplay,
        [StationDraw.MapLit] = MapDisplay,
        [StationDraw.MapBright] = MapDisplay,
        [StationDraw.MapRightRetracted] = MapRightRetracted,
        [StationDraw.MapRightExtended] = MapExtended,
        [StationDraw.MapLeftRetracted] = MapLeftRetracted,
        [StationDraw.MapLeftExtended] = MapExtended,
        [StationDraw.EnergyDim] = ResourceDisplay,
        [StationDraw.EnergyLit] = ResourceDisplay,
        [StationDraw.EnergyBright] = ResourceDisplay,
        [StationDraw.MissileDim] = ResourceDisplay,
        [StationDraw.MissileLit] = ResourceDisplay,
        [StationDraw.MissileBright] = ResourceDisplay,
        [StationDraw.ResourceRightRetracted] = ResourceRetracted,
        [StationDraw.ResourceRightExtended] = ResourceExtended,
        [StationDraw.ResourceLeftRetracted] = ResourceRetracted,
        [StationDraw.ResourceLeftExtended] = ResourceExtended,
        [StationDraw.SaveIdle] = SavePodIdle,
        [StationDraw.SaveLit] = SavePodActive,
        [StationDraw.SaveDark] = SavePodActive,
    };
}
