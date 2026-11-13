using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// Verification's native view of the Beetom identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class BeetomNativeWords
{
    /// <summary>The routine word naming each <see cref="BeetomAction"/>.</summary>
    internal static readonly NativeWordIdentities<BeetomAction> BeetomActions = new(new Dictionary<BeetomAction, ushort>
    {
        [BeetomAction.DecideAction] = 0xb814,
        [BeetomAction.DecideActionSamusNotInProximity] = 0xb82f,
        [BeetomAction.StartIdling] = 0xb84f,
        [BeetomAction.StartCrawlingLeft] = 0xb85f,
        [BeetomAction.StartCrawlingRight] = 0xb873,
        [BeetomAction.StartShortHopLeft] = 0xb887,
        [BeetomAction.StartShortHopRight] = 0xb8a9,
        [BeetomAction.StartLongHopLeft] = 0xb8cb,
        [BeetomAction.StartLongHopRight] = 0xb8ed,
        [BeetomAction.DecideActionSamusInProximity] = 0xb90f,
        [BeetomAction.StartDrainingLeft] = 0xb952,
        [BeetomAction.StartDrainingRight] = 0xb966,
        [BeetomAction.StartDropping] = 0xb97a,
        [BeetomAction.StartBeingFlung] = 0xb9a2,
        [BeetomAction.Idling] = 0xb9b2,
        [BeetomAction.CrawlingLeft] = 0xb9c1,
        [BeetomAction.CrawlingRight] = 0xba24,
        [BeetomAction.ShortHopLeft] = 0xba84,
        [BeetomAction.ShortHopRight] = 0xbab7,
        [BeetomAction.LongHopLeft] = 0xbb55,
        [BeetomAction.LongHopRight] = 0xbb88,
        [BeetomAction.LungeLeft] = 0xbc26,
        [BeetomAction.LungeRight] = 0xbc5a,
        [BeetomAction.DrainingLeft] = 0xbcf8,
        [BeetomAction.DrainingRight] = 0xbd42,
        [BeetomAction.Dropping] = 0xbd9d,
        [BeetomAction.BeingFlung] = 0xbdc5,
    });
}
