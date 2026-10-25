using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// Verification's native view of the Yard identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class YardNativeWords
{
    /// <summary>The routine word naming each <see cref="YardMovementFunction"/>.</summary>
    internal static readonly NativeWordIdentities<YardMovementFunction> YardMovementFunctions = new(new Dictionary<YardMovementFunction, ushort>
    {
        [YardMovementFunction.InstructionPending] = 0xcf5f,
        [YardMovementFunction.Hiding] = 0xcf60,
        [YardMovementFunction.CrawlingUpsideUpMovingLeft] = 0xcfa6,
        [YardMovementFunction.CrawlingUpsideLeftMovingDown] = 0xcfb7,
        [YardMovementFunction.CrawlingUpsideDownMovingRight] = 0xcfbd,
        [YardMovementFunction.CrawlingUpsideRightMovingUp] = 0xcfce,
        [YardMovementFunction.CrawlingUpsideUpMovingRight] = 0xcfd4,
        [YardMovementFunction.CrawlingUpsideRightMovingDown] = 0xcfe5,
        [YardMovementFunction.CrawlingUpsideDownMovingLeft] = 0xcfeb,
        [YardMovementFunction.CrawlingUpsideLeftMovingUp] = 0xcffc,
        [YardMovementFunction.Airborne] = 0xd1b3,
    });
}
