namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Reads and writes a <see cref="RoomScrollState"/> in an installed document by its installed key (#2520).</summary>
internal sealed class RoomScrollStateJsonConverter() : InstalledKeyJsonConverter<RoomScrollState>(RoomScrollStateKeys.Installed);
