using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="RoomObjectPopulationLoader"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
[ArchitectureReview("context-bag", "The room-object population loader's constructor dependencies (#621): each member is a room-object family it sets up as placements load (#2288), read only by that one constructor, not a context handed on.")]
internal sealed partial record RoomObjectPopulationLoaderDependencies
{
    /// <summary>The Bomb Torizo statue's hand, which sets up by whether the Torizo is beaten.</summary>
    public required BombTorizoHandObjects BombTorizoHand { get; init; }

    /// <summary>The room's items, which set up their pictures as they are placed.</summary>
    public required CollectibleObjects CollectibleObjects { get; init; }

    /// <summary>The eye doors, which set up once they have taken their places.</summary>
    public required EyeDoorObjects EyeDoors { get; init; }

    /// <summary>The grey door caps, which set up their locks as they are placed (#2486).</summary>
    public required GreyDoorObjects GreyDoors { get; init; }

    /// <summary>The room's stations, which hold the map stations they place.</summary>
    public required StationObjects Stations { get; init; }

    /// <summary>The gates, which set up their blocks as they are placed.</summary>
    public required GateObjects Gates { get; init; }

    /// <summary>Mother Brain's glass, which sets up as it is placed.</summary>
    public required MotherBrainGlassObjects MotherBrainGlass { get; init; }

    /// <summary>The room's room-object places.</summary>
    public required RoomObjectPlaceList Places { get; init; }

    /// <summary>The Speed Booster's escape, which sets up when its room places it.</summary>
    public required SpeedBoosterEscapeObjects SpeedBoosterEscape { get; init; }

    /// <summary>Draygon's wall cannons, whose objects each placement sets up with the cannons' firing they stop (#2400).</summary>
    public required DraygonCannonObjects DraygonCannons { get; init; }

    /// <summary>Maridia's glass tube, which the room's population sets up as it is placed.</summary>
    public required NoobTubeObjects NoobTube { get; init; }
}
