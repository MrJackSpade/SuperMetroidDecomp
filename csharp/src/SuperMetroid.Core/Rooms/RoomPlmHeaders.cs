namespace SuperMetroid.Core.Rooms;

/// <summary>Grouped selections over bank-$84 <see cref="PlmHeaderId"/> headers.</summary>
internal static class RoomPlmHeaders
{
    /// <summary>Number of concrete headers in each permanent-collectible presentation.</summary>
    public const int PermanentCollectibleKindCount = 21;

    /// <summary>Selects the named contact-crumble header for bank-$94 special-block
    /// BTS $00..07 at $94:9139. Sizes 1x1, 2x1, 1x2, 2x2 appear first as
    /// respawning blocks, then as permanent blocks.</summary>
    public static PlmHeaderId ContactCrumbleByReactionIndex(int index) => index switch
    {
        0 => PlmHeaderId.ContactCrumble1x1Respawning,
        1 => PlmHeaderId.ContactCrumble2x1Respawning,
        2 => PlmHeaderId.ContactCrumble1x2Respawning,
        3 => PlmHeaderId.ContactCrumble2x2Respawning,
        4 => PlmHeaderId.ContactCrumble1x1Permanent,
        5 => PlmHeaderId.ContactCrumble2x1Permanent,
        6 => PlmHeaderId.ContactCrumble1x2Permanent,
        7 => PlmHeaderId.ContactCrumble2x2Permanent,
        _ => throw new IndexOutOfRangeException(),
    };
}
