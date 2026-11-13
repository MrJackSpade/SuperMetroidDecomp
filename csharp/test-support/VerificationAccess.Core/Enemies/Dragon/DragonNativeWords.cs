using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// Verification's native view of the Dragon identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class DragonNativeWords
{
    /// <summary>The routine word naming each <see cref="DragonAction"/>.</summary>
    internal static readonly NativeWordIdentities<DragonAction> DragonActions = new(new Dictionary<DragonAction, ushort>
    {
        [DragonAction.WaitToRise] = 0xe654,
        [DragonAction.Rising] = 0xe6ad,
        [DragonAction.Attacking] = 0xe6f1,
        [DragonAction.WaitToSink] = 0xe734,
        [DragonAction.Sinking] = 0xe749,
        [DragonAction.WingNoOp] = 0xe781,
    });

    /// <summary>
    /// The cartridge's animation-table ordinal naming each <see cref="DragonAnimationSelector"/>: even ordinals face
    /// left, odd ordinals face right. The cartridge's all-ones installed ordinal is Core's
    /// <see cref="DragonEnemyState.ReinstallRequested"/>, not an animation.
    /// </summary>
    internal static readonly NativeWordIdentities<DragonAnimationSelector> AnimationSelectors = new(new Dictionary<DragonAnimationSelector, ushort>
    {
        [DragonAnimationSelector.IdleFacingLeft] = 0,
        [DragonAnimationSelector.IdleFacingRight] = 1,
        [DragonAnimationSelector.WingsFacingLeft] = 2,
        [DragonAnimationSelector.WingsFacingRight] = 3,
        [DragonAnimationSelector.AttackingFacingLeft] = 4,
        [DragonAnimationSelector.AttackingFacingRight] = 5,
    });
}
