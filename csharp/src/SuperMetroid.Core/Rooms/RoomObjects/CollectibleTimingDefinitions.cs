namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The authored timing of the in-world collectibles' pictures (#1554).</summary>
internal static class CollectibleTimingDefinitions
{
    /// <summary>The updates each of an item's two visible images shows.</summary>
    internal const int VisibleImageUpdates = 4;

    /// <summary>The updates each of a broken Chozo orb's empty, burst and empty images shows before the item appears.</summary>
    internal const int OrbBurstImageUpdates = 3;

    /// <summary>The updates each image of a shot block revealing, or concealing again, its item shows.</summary>
    internal const int RevealImageUpdates = 4;

    /// <summary>How many pairs of visible images a revealed item shows before its shot block conceals it again.</summary>
    internal const int RevealedImagePairs = 22;

    /// <summary>The updates a collected item's shot block stays empty before it respawns: twenty-two eight-update intervals.</summary>
    internal const int CollectedShotBlockEmptyUpdates = 8 * 22;

    /// <summary>The updates a collected shot block's three respawn images each show.</summary>
    internal const int RespawnImageUpdates = 4;

    /// <summary>The updates a picked-up item's empty block shows before the item leaves the room.</summary>
    internal const int PickedUpEmptyUpdates = 1;
}
