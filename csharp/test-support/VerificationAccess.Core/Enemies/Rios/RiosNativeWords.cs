using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// Verification's native view of the Rios identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class RiosNativeWords
{
    /// <summary>The routine word naming each <see cref="LowerNorfairRioEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<LowerNorfairRioEnemyFunction> LowerNorfairRioEnemyFunctions = new(new Dictionary<LowerNorfairRioEnemyFunction, ushort>
    {
        [LowerNorfairRioEnemyFunction.FollowParent] = 0xc72e,
        [LowerNorfairRioEnemyFunction.WaitForAttackOpportunity] = 0xc771,
        [LowerNorfairRioEnemyFunction.WaitForTakeoffAnimation] = 0xc7bb,
        [LowerNorfairRioEnemyFunction.Dive] = 0xc7d6,
        [LowerNorfairRioEnemyFunction.ReturnToPerch] = 0xc82d,
        [LowerNorfairRioEnemyFunction.WaitForLandingAnimation] = 0xc888,
    });

    /// <summary>The routine word naming each <see cref="NorfairRioEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<NorfairRioEnemyFunction> NorfairRioEnemyFunctions = new(new Dictionary<NorfairRioEnemyFunction, ushort>
    {
        [NorfairRioEnemyFunction.FollowParent] = 0xc281,
        [NorfairRioEnemyFunction.WaitForAttackOpportunity] = 0xc2e7,
        [NorfairRioEnemyFunction.WaitForTakeoffAnimation] = 0xc33f,
        [NorfairRioEnemyFunction.Dive] = 0xc361,
        [NorfairRioEnemyFunction.ReturnToPerch] = 0xc3b1,
        [NorfairRioEnemyFunction.FinishLanding] = 0xc406,
    });

    /// <summary>The routine word naming each <see cref="RioEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<RioEnemyFunction> RioEnemyFunctions = new(new Dictionary<RioEnemyFunction, ushort>
    {
        [RioEnemyFunction.WaitingForSamus] = 0xbbed,
        [RioEnemyFunction.WaitingForLandingAnimation] = 0xbc32,
        [RioEnemyFunction.Diving] = 0xbc48,
        [RioEnemyFunction.BouncingBackToPerch] = 0xbcb7,
        [RioEnemyFunction.HoveringTowardSamus] = 0xbcff,
    });
}
