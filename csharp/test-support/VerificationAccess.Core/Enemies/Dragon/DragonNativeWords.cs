using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// Verification's native view of the Dragon identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class DragonNativeWords
{
    /// <summary>The routine word naming each <see cref="DragonEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<DragonEnemyFunction> DragonEnemyFunctions = new(new Dictionary<DragonEnemyFunction, ushort>
    {
        [DragonEnemyFunction.WaitToRise] = 0xe654,
        [DragonEnemyFunction.Rising] = 0xe6ad,
        [DragonEnemyFunction.Attacking] = 0xe6f1,
        [DragonEnemyFunction.WaitToSink] = 0xe734,
        [DragonEnemyFunction.Sinking] = 0xe749,
        [DragonEnemyFunction.WingNoOp] = 0xe781,
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
