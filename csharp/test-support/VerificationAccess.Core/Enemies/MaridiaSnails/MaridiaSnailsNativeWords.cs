using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Verification's native view of the MaridiaSnails identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class MaridiaSnailsNativeWords
{
    /// <summary>The routine word naming each <see cref="OumBouncePhase"/>.</summary>
    internal static readonly NativeWordIdentities<OumBouncePhase> OumBouncePhases = new(new Dictionary<OumBouncePhase, ushort>
    {
        [OumBouncePhase.Falling] = 0xcf66,
        [OumBouncePhase.Rising] = 0xcfa9,
    });

    /// <summary>The routine word naming each <see cref="OumAction"/>.</summary>
    internal static readonly NativeWordIdentities<OumAction> OumActions = new(new Dictionary<OumAction, ushort>
    {
        [OumAction.Idle] = 0xcde6,
        [OumAction.Rolling] = 0xce2b,
        [OumAction.Attacking] = 0xcf40,
    });

    /// <summary>
    /// The index naming each <see cref="OumSegment"/> in the snail's eight-entry animation table: its low
    /// bit faces left, its next bit rolls and its third attacks, or, with the roll bit, rolls backwards.
    /// </summary>
    internal static readonly NativeWordIdentities<OumSegment> OumAnimations = new(new Dictionary<OumSegment, ushort>
    {
        [OumSegment.FacingRightIdle] = 0,
        [OumSegment.FacingLeftIdle] = 1,
        [OumSegment.FacingRightRollingForwards] = 2,
        [OumSegment.FacingLeftRollingForwards] = 3,
        [OumSegment.FacingRightAttacking] = 4,
        [OumSegment.FacingLeftAttacking] = 5,
        [OumSegment.FacingRightRollingBackwards] = 6,
        [OumSegment.FacingLeftRollingBackwards] = 7,
    });
}
