using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// Verification's native view of the Rios identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class RiosNativeWords
{
    /// <summary>The routine word naming each <see cref="LowerNorfairRioAction"/>.</summary>
    internal static readonly NativeWordIdentities<LowerNorfairRioAction> LowerNorfairRioActions = new(new Dictionary<LowerNorfairRioAction, ushort>
    {
        [LowerNorfairRioAction.FollowParent] = 0xc72e,
        [LowerNorfairRioAction.WaitForAttackOpportunity] = 0xc771,
        [LowerNorfairRioAction.WaitForTakeoffAnimation] = 0xc7bb,
        [LowerNorfairRioAction.Dive] = 0xc7d6,
        [LowerNorfairRioAction.ReturnToPerch] = 0xc82d,
        [LowerNorfairRioAction.WaitForLandingAnimation] = 0xc888,
    });

    /// <summary>The routine word naming each <see cref="NorfairRioAction"/>.</summary>
    internal static readonly NativeWordIdentities<NorfairRioAction> NorfairRioActions = new(new Dictionary<NorfairRioAction, ushort>
    {
        [NorfairRioAction.FollowParent] = 0xc281,
        [NorfairRioAction.WaitForAttackOpportunity] = 0xc2e7,
        [NorfairRioAction.WaitForTakeoffAnimation] = 0xc33f,
        [NorfairRioAction.Dive] = 0xc361,
        [NorfairRioAction.ReturnToPerch] = 0xc3b1,
        [NorfairRioAction.FinishLanding] = 0xc406,
    });

    /// <summary>The routine word naming each <see cref="RioAction"/>.</summary>
    internal static readonly NativeWordIdentities<RioAction> RioActions = new(new Dictionary<RioAction, ushort>
    {
        [RioAction.WaitingForSamus] = 0xbbed,
        [RioAction.WaitingForLandingAnimation] = 0xbc32,
        [RioAction.Diving] = 0xbc48,
        [RioAction.BouncingBackToPerch] = 0xbcb7,
        [RioAction.HoveringTowardSamus] = 0xbcff,
    });
}
