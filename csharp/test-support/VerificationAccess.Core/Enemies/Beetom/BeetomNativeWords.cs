using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// Verification's native view of the Beetom identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class BeetomNativeWords
{
    /// <summary>The routine word naming each <see cref="BeetomEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<BeetomEnemyFunction> BeetomEnemyFunctions = new(new Dictionary<BeetomEnemyFunction, ushort>
    {
        [BeetomEnemyFunction.DecideAction] = 0xb814,
        [BeetomEnemyFunction.DecideActionSamusNotInProximity] = 0xb82f,
        [BeetomEnemyFunction.StartIdling] = 0xb84f,
        [BeetomEnemyFunction.StartCrawlingLeft] = 0xb85f,
        [BeetomEnemyFunction.StartCrawlingRight] = 0xb873,
        [BeetomEnemyFunction.StartShortHopLeft] = 0xb887,
        [BeetomEnemyFunction.StartShortHopRight] = 0xb8a9,
        [BeetomEnemyFunction.StartLongHopLeft] = 0xb8cb,
        [BeetomEnemyFunction.StartLongHopRight] = 0xb8ed,
        [BeetomEnemyFunction.DecideActionSamusInProximity] = 0xb90f,
        [BeetomEnemyFunction.StartDrainingLeft] = 0xb952,
        [BeetomEnemyFunction.StartDrainingRight] = 0xb966,
        [BeetomEnemyFunction.StartDropping] = 0xb97a,
        [BeetomEnemyFunction.StartBeingFlung] = 0xb9a2,
        [BeetomEnemyFunction.Idling] = 0xb9b2,
        [BeetomEnemyFunction.CrawlingLeft] = 0xb9c1,
        [BeetomEnemyFunction.CrawlingRight] = 0xba24,
        [BeetomEnemyFunction.ShortHopLeft] = 0xba84,
        [BeetomEnemyFunction.ShortHopRight] = 0xbab7,
        [BeetomEnemyFunction.LongHopLeft] = 0xbb55,
        [BeetomEnemyFunction.LongHopRight] = 0xbb88,
        [BeetomEnemyFunction.LungeLeft] = 0xbc26,
        [BeetomEnemyFunction.LungeRight] = 0xbc5a,
        [BeetomEnemyFunction.DrainingLeft] = 0xbcf8,
        [BeetomEnemyFunction.DrainingRight] = 0xbd42,
        [BeetomEnemyFunction.Dropping] = 0xbd9d,
        [BeetomEnemyFunction.BeingFlung] = 0xbdc5,
    });
}
