namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Where the cartridge keeps room-state records and the fields extraction reads from them.</summary>
internal static class RoomStateSourceDefinitions
{
    /// <summary>Bank $8F, which holds the room-state records.</summary>
    internal const int Bank = 0x8f0000;

    /// <summary>Offset of a room-state record's enemy-set pointer word.</summary>
    internal const int EnemySetOffset = 10;
}
