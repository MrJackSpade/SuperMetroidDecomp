using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// Verification's native view of the Yard identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class YardNativeWords
{
    /// <summary>The routine word naming each <see cref="YardMovement"/>.</summary>
    internal static readonly NativeWordIdentities<YardMovement> YardMovements = new(new Dictionary<YardMovement, ushort>
    {
        [YardMovement.AwaitingAnimationCue] = 0xcf5f,
        [YardMovement.Hiding] = 0xcf60,
        [YardMovement.CrawlingUpsideUpMovingLeft] = 0xcfa6,
        [YardMovement.CrawlingUpsideLeftMovingDown] = 0xcfb7,
        [YardMovement.CrawlingUpsideDownMovingRight] = 0xcfbd,
        [YardMovement.CrawlingUpsideRightMovingUp] = 0xcfce,
        [YardMovement.CrawlingUpsideUpMovingRight] = 0xcfd4,
        [YardMovement.CrawlingUpsideRightMovingDown] = 0xcfe5,
        [YardMovement.CrawlingUpsideDownMovingLeft] = 0xcfeb,
        [YardMovement.CrawlingUpsideLeftMovingUp] = 0xcffc,
        [YardMovement.Airborne] = 0xd1b3,
    });
}
