using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// What a placement of each door, item, map or refill station and scroll-extension kind carries in its setup (#2481,
/// #2483), read from Core's table of the kind each setup places (#2557): verification builds a synthetic placement of a
/// kind from them. Each kind the retail rooms place is listed by the one table of its family; the save station, whose
/// setup carries only its placement's load station, is listed by none.
/// </summary>
internal static class RoomObjectKindVariants
{
    /// <summary>The colour and facing of the cap each coloured-door kind places.</summary>
    internal static IReadOnlyDictionary<RoomObjectKind, ColoredDoorVariant> ColoredDoors { get; } = ByKind(RoomObjectKindDefinitions.ColoredDoors);

    /// <summary>
    /// The grey cap each grey-door kind places: its facing, and whether it is Bomb Torizo's door, which faces right like the
    /// generic right-facing one and whose closing waits for Bombs.
    /// </summary>
    internal static IReadOnlyDictionary<RoomObjectKind, GreyDoorVariant> GreyDoors { get; } = ByKind(RoomObjectKindDefinitions.GreyDoors);

    /// <summary>
    /// The item and presentation each permanent-item kind places: one for each collectible some room places in each of the
    /// exposed, Chozo-orb and concealed shot-block presentations (#2354).
    /// </summary>
    internal static IReadOnlyDictionary<RoomObjectKind, PermanentCollectibleObject> Items { get; } = ByKind(RoomObjectKindDefinitions.Items);

    /// <summary>The way the door of each eye-door eye kind faces.</summary>
    internal static IReadOnlyDictionary<RoomObjectKind, EyeDoorOrientation> EyeDoorEyes { get; } = ByKind(RoomObjectKindDefinitions.EyeDoorEyes);

    /// <summary>The body part each eye-door body or bottom kind places, and the way its door faces.</summary>
    internal static IReadOnlyDictionary<RoomObjectKind, EyeDoorPart> EyeDoorParts { get; } = ByKind(RoomObjectKindDefinitions.EyeDoorParts);

    /// <summary>The access rule of the station each map, energy and missile station kind places.</summary>
    internal static IReadOnlyDictionary<RoomObjectKind, StationAccessRule> AccessStations { get; } = ByKind(RoomObjectKindDefinitions.AccessStations);

    /// <summary>What each scroll-extension kind writes to extend the trigger beside it.</summary>
    internal static IReadOnlyDictionary<RoomObjectKind, ScrollExtension> ScrollExtensions { get; } = ByKind(RoomObjectKindDefinitions.ScrollExtensions);

    /// <summary>Each kind of a family's table with what its setup carries, read from the table of kinds by setup.</summary>
    private static Dictionary<RoomObjectKind, TVariant> ByKind<TVariant>(IReadOnlyDictionary<TVariant, RoomObjectKind> kinds)
        where TVariant : notnull => kinds.ToDictionary(pair => pair.Value, pair => pair.Key);
}
