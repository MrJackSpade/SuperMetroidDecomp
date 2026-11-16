namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>Chooses a Zoa's animation by its facing and movement (#1624, #1810).</summary>
internal static class ZoaAnimationChoices
{
    /// <summary>The rising animation facing right or left.</summary>
    internal static ZoaSegment Rising(bool facingRight) =>
        facingRight ? ZoaSegment.FacingRightRising : ZoaSegment.FacingLeftRising;

    /// <summary>The shooting animation facing right or left.</summary>
    internal static ZoaSegment Shooting(bool facingRight) =>
        facingRight ? ZoaSegment.FacingRightShooting : ZoaSegment.FacingLeftShooting;
}
