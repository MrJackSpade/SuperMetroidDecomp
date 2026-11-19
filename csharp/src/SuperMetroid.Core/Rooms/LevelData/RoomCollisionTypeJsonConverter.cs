namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Reads and writes a <see cref="RoomCollisionType"/> in an installed document by its installed key (#2520).</summary>
internal sealed class RoomCollisionTypeJsonConverter() : InstalledKeyJsonConverter<RoomCollisionType>(RoomCollisionTypeKeys.Installed);
