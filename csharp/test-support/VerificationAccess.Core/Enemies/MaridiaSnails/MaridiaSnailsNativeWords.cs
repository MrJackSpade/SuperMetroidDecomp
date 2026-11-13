using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Verification's native view of the MaridiaSnails identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class MaridiaSnailsNativeWords
{
    /// <summary>The routine word naming each <see cref="MaridiaLargeSnailBounceFunction"/>.</summary>
    internal static readonly NativeWordIdentities<MaridiaLargeSnailBounceFunction> MaridiaLargeSnailBounceFunctions = new(new Dictionary<MaridiaLargeSnailBounceFunction, ushort>
    {
        [MaridiaLargeSnailBounceFunction.Falling] = 0xcf66,
        [MaridiaLargeSnailBounceFunction.Rising] = 0xcfa9,
    });

    /// <summary>The routine word naming each <see cref="MaridiaLargeSnailAction"/>.</summary>
    internal static readonly NativeWordIdentities<MaridiaLargeSnailAction> MaridiaLargeSnailActions = new(new Dictionary<MaridiaLargeSnailAction, ushort>
    {
        [MaridiaLargeSnailAction.Idle] = 0xcde6,
        [MaridiaLargeSnailAction.Rolling] = 0xce2b,
        [MaridiaLargeSnailAction.Attacking] = 0xcf40,
    });

    /// <summary>
    /// The index naming each <see cref="MaridiaLargeSnailAnimation"/> in the snail's eight-entry animation table: its low
    /// bit faces left, its next bit rolls and its third attacks, or, with the roll bit, rolls backwards.
    /// </summary>
    internal static readonly NativeWordIdentities<MaridiaLargeSnailAnimation> MaridiaLargeSnailAnimations = new(new Dictionary<MaridiaLargeSnailAnimation, ushort>
    {
        [MaridiaLargeSnailAnimation.FacingRightIdle] = 0,
        [MaridiaLargeSnailAnimation.FacingLeftIdle] = 1,
        [MaridiaLargeSnailAnimation.FacingRightRollingForwards] = 2,
        [MaridiaLargeSnailAnimation.FacingLeftRollingForwards] = 3,
        [MaridiaLargeSnailAnimation.FacingRightAttacking] = 4,
        [MaridiaLargeSnailAnimation.FacingLeftAttacking] = 5,
        [MaridiaLargeSnailAnimation.FacingRightRollingBackwards] = 6,
        [MaridiaLargeSnailAnimation.FacingLeftRollingBackwards] = 7,
    });
}
