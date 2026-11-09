using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>One Tourian access-floor room object and the terrain change it makes.</summary>
/// <param name="Header">The room object.</param>
/// <param name="Change">The change it makes to the access floor.</param>
internal readonly record struct TourianAccessObjectDefinition(
    RoomObjectHeaderId Header,
    TerrainChange Change);
