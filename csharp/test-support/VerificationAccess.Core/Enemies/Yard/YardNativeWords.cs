using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// Verification's native view of the Yard identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class YardNativeWords
{
    /// <summary>
    /// The routine word naming each <see cref="YardMovement"/>: <see cref="YardMovement.Crawling"/> has one routine for
    /// each way a Yard crawls (<see cref="Crawls"/>, #2278).
    /// </summary>
    internal static readonly NativeWordIdentities<YardMovement> YardMovements = new(new Dictionary<ushort, YardMovement>
    {
        [0xcf5f] = YardMovement.HoldingStill,
        [0xcf60] = YardMovement.Hiding,
        [0xcfa6] = YardMovement.Crawling,
        [0xcfb7] = YardMovement.Crawling,
        [0xcfbd] = YardMovement.Crawling,
        [0xcfce] = YardMovement.Crawling,
        [0xcfd4] = YardMovement.Crawling,
        [0xcfe5] = YardMovement.Crawling,
        [0xcfeb] = YardMovement.Crawling,
        [0xcffc] = YardMovement.Crawling,
        [0xd1b3] = YardMovement.Airborne,
    });

    /// <summary>The crawl routine word naming each way a Yard crawls (#2278).</summary>
    internal static readonly NativeWordIdentities<YardDirection> Crawls = new(new Dictionary<YardDirection, ushort>
    {
        [YardDirection.OnFloorMovingLeft] = 0xcfa6,
        [YardDirection.OnRightWallMovingDown] = 0xcfb7,
        [YardDirection.OnCeilingMovingRight] = 0xcfbd,
        [YardDirection.OnLeftWallMovingUp] = 0xcfce,
        [YardDirection.OnFloorMovingRight] = 0xcfd4,
        [YardDirection.OnLeftWallMovingDown] = 0xcfe5,
        [YardDirection.OnCeilingMovingLeft] = 0xcfeb,
        [YardDirection.OnRightWallMovingUp] = 0xcffc,
    });
}
