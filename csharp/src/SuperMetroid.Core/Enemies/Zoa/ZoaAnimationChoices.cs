namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>Chooses a Zoa's animation by its facing and movement (#1624, #1810).</summary>
internal static class ZoaAnimationChoices
{
    /// <summary>The rising animation facing right or left.</summary>
    internal static ZoaSegment Rising(bool facingRight) =>
        facingRight ? ZoaSegment.FacingRightRising : ZoaSegment.FacingLeftRising;

    /// <summary>The shooting animation with <paramref name="animation"/>'s facing.</summary>
    internal static ZoaSegment ShootingWithSameFacing(ZoaSegment animation) => animation switch
    {
        ZoaSegment.FacingLeftRising or ZoaSegment.FacingLeftShooting => ZoaSegment.FacingLeftShooting,
        ZoaSegment.FacingRightRising or ZoaSegment.FacingRightShooting => ZoaSegment.FacingRightShooting,
        _ => throw new ArgumentOutOfRangeException(nameof(animation), animation, $"Undefined {nameof(ZoaSegment)}."),
    };
}
