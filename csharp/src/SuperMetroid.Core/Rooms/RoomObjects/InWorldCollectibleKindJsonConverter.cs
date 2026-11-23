namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Reads and writes a <see cref="InWorldCollectibleKind"/> in an installed document by its installed key (#2520).</summary>
internal sealed partial class InWorldCollectibleKindJsonConverter() : InstalledKeyJsonConverter<InWorldCollectibleKind>(InWorldCollectibleKindKeys.Installed);
