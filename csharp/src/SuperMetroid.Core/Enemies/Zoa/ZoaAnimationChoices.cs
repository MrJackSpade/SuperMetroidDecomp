namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>Chooses a Zoa's animation by its facing and movement (#1624, #1810).</summary>
internal static class ZoaAnimationChoices
{
    /// <summary>The rising animation facing <paramref name="facing"/>.</summary>
    internal static ZoaSegment Rising(HorizontalDirection facing) =>
        facing == HorizontalDirection.Right ? ZoaSegment.FacingRightRising : ZoaSegment.FacingLeftRising;

    /// <summary>The shooting animation facing <paramref name="facing"/>.</summary>
    internal static ZoaSegment Shooting(HorizontalDirection facing) =>
        facing == HorizontalDirection.Right ? ZoaSegment.FacingRightShooting : ZoaSegment.FacingLeftShooting;
}
