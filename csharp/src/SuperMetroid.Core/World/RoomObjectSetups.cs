using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.World;

/// <summary>
/// The family setup each room-object kind a population places names (#2288): the coloured, grey and eye doors, the items,
/// the stations, the Draygon cannons and the scroll objects by their families' own tables of their kinds, and each other
/// kind its own. Every kind but <see cref="RoomObjectKind.None"/> names exactly one, checked once as the table is built, so
/// a placement of a kind no family sets up cannot reach the population loader.
/// </summary>
internal static class RoomObjectSetups
{
    /// <summary>The setup each placed kind names.</summary>
    private static readonly Dictionary<RoomObjectKind, RoomObjectSetup> ByKind = Build();

    /// <summary>The family setup a placement of <paramref name="kind"/> runs.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="kind"/> is <see cref="RoomObjectKind.None"/>, which no population places.</exception>
    internal static RoomObjectSetup Of(RoomObjectKind kind) =>
        ByKind.TryGetValue(kind, out RoomObjectSetup? setup)
            ? setup
            : throw new ArgumentOutOfRangeException(nameof(kind), kind, "No room-object population places this kind.");

    /// <summary>Gathers each family's kinds with its setup and checks every kind but <see cref="RoomObjectKind.None"/> names one.</summary>
    /// <exception cref="InvalidOperationException">A kind names no setup, or more than one.</exception>
    private static Dictionary<RoomObjectKind, RoomObjectSetup> Build()
    {
        var setups = new Dictionary<RoomObjectKind, RoomObjectSetup>();
        void Add(RoomObjectKind kind, RoomObjectSetup setup)
        {
            if (!setups.TryAdd(kind, setup))
                throw new InvalidOperationException($"Room-object kind {kind} names two family setups.");
        }

        foreach ((RoomObjectKind kind, ColoredDoorVariant door) in DoorObjectKindDefinitions.ColoredDoors)
            Add(kind, new RoomObjectSetup.ColoredDoor(door.Color, door.Orientation));
        foreach ((RoomObjectKind kind, ColoredDoorOrientation orientation) in DoorObjectKindDefinitions.GreyDoors)
            Add(kind, new RoomObjectSetup.GreyDoor(new GreyDoorVariant(orientation, BombTorizo: kind == RoomObjectKind.BombTorizoGreyDoor)));
        foreach ((RoomObjectKind kind, PermanentCollectibleObject item) in PermanentCollectibleObjectDefinitions.Items)
            Add(kind, new RoomObjectSetup.Collectible(item.Kind, item.Presentation));
        foreach (RoomObjectKind kind in EyeDoorPartDefinitions.Parts.Keys)
            Add(kind, new RoomObjectSetup.EyeDoor(kind));
        foreach ((RoomObjectKind kind, DraygonCannonSetup cannon) in DraygonCannonDefinitions.Setups)
            Add(kind, new RoomObjectSetup.DraygonCannon(cannon));
        foreach ((RoomObjectKind kind, StationKind station) in StationKindDefinitions.Kinds)
            Add(kind, new RoomObjectSetup.Station(station));
        foreach (RoomObjectKind kind in (RoomObjectKind[])[RoomObjectKind.ScrollTrigger, RoomObjectKind.RightwardsScrollExtension,
            RoomObjectKind.LeftwardsScrollExtension, RoomObjectKind.DownwardsScrollExtension, RoomObjectKind.UpwardsScrollExtension])
            Add(kind, new RoomObjectSetup.Scroll(kind));
        Add(RoomObjectKind.ElevatorPlatform, new RoomObjectSetup.ElevatorPlatform());
        Add(RoomObjectKind.MotherBrainGlass, new RoomObjectSetup.MotherBrainGlass());
        Add(RoomObjectKind.BombTorizoHand, new RoomObjectSetup.BombTorizoHand());
        Add(RoomObjectKind.MetroidRoomClearance, new RoomObjectSetup.MetroidRoomClearance());
        Add(RoomObjectKind.SpeedBoosterEscape, new RoomObjectSetup.SpeedBoosterEscape());
        Add(RoomObjectKind.WreckedShipAttic, new RoomObjectSetup.WreckedShipAttic());
        Add(RoomObjectKind.NoobTube, new RoomObjectSetup.NoobTube());
        Add(RoomObjectKind.MotherBrainEscapeRoomGate, new RoomObjectSetup.MotherBrainEscapeRoomGate());
        Add(RoomObjectKind.DownwardGate, new RoomObjectSetup.DownwardGate());
        Add(RoomObjectKind.DownwardGateShotBlock, new RoomObjectSetup.DownwardGateShotBlock());

        foreach (RoomObjectKind kind in Enum.GetValues<RoomObjectKind>())
        {
            if (kind != RoomObjectKind.None && !setups.ContainsKey(kind))
                throw new InvalidOperationException($"Room-object kind {kind} names no family setup.");
        }
        return setups;
    }
}
