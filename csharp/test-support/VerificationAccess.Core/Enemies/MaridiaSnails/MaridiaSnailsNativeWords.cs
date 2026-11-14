using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Verification's native view of the MaridiaSnails identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class MaridiaSnailsNativeWords
{
    /// <summary>The routine word naming each <see cref="MaridiaLargeSnailBouncePhase"/>.</summary>
    internal static readonly NativeWordIdentities<MaridiaLargeSnailBouncePhase> MaridiaLargeSnailBouncePhases = new(new Dictionary<MaridiaLargeSnailBouncePhase, ushort>
    {
        [MaridiaLargeSnailBouncePhase.Falling] = 0xcf66,
        [MaridiaLargeSnailBouncePhase.Rising] = 0xcfa9,
    });

    /// <summary>The routine word naming each <see cref="MaridiaLargeSnailAction"/>.</summary>
    internal static readonly NativeWordIdentities<MaridiaLargeSnailAction> MaridiaLargeSnailActions = new(new Dictionary<MaridiaLargeSnailAction, ushort>
    {
        [MaridiaLargeSnailAction.Idle] = 0xcde6,
        [MaridiaLargeSnailAction.Rolling] = 0xce2b,
        [MaridiaLargeSnailAction.Attacking] = 0xcf40,
    });

    /// <summary>
    /// The index naming each <see cref="MaridiaLargeSnailSegment"/> in the snail's eight-entry animation table: its low
    /// bit faces left, its next bit rolls and its third attacks, or, with the roll bit, rolls backwards.
    /// </summary>
    internal static readonly NativeWordIdentities<MaridiaLargeSnailSegment> MaridiaLargeSnailAnimations = new(new Dictionary<MaridiaLargeSnailSegment, ushort>
    {
        [MaridiaLargeSnailSegment.FacingRightIdle] = 0,
        [MaridiaLargeSnailSegment.FacingLeftIdle] = 1,
        [MaridiaLargeSnailSegment.FacingRightRollingForwards] = 2,
        [MaridiaLargeSnailSegment.FacingLeftRollingForwards] = 3,
        [MaridiaLargeSnailSegment.FacingRightAttacking] = 4,
        [MaridiaLargeSnailSegment.FacingLeftAttacking] = 5,
        [MaridiaLargeSnailSegment.FacingRightRollingBackwards] = 6,
        [MaridiaLargeSnailSegment.FacingLeftRollingBackwards] = 7,
    });
}
