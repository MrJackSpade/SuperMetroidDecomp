using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed room scroll overread file (#2356): for every explicit scroll layout, by its name, the cells its load writes
/// over the kept scroll cells past the room's screens, decoded from the player's cartridge.
/// </summary>
/// <param name="Version">The schema revision, <see cref="RoomScrollOverreadFormat.Version"/>.</param>
/// <param name="CellsPastScreens">Each explicit layout's cells past the room's screens, in order, by the layout's name.</param>
internal sealed record RoomScrollOverreadDocument(
    int Version,
    IReadOnlyDictionary<string, IReadOnlyList<RoomScrollState>> CellsPastScreens);
