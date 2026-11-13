namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>The program each of a Zoa's animations plays.</summary>
internal static class ZoaAnimationDefinitions
{
    /// <summary>The program of each rising and shooting animation, facing left and right.</summary>
    internal static readonly Dictionary<ZoaAnimationSelector, ZoaSegment> Animations = new()
    {
        [ZoaAnimationSelector.ShootingFacingLeft] = ZoaSegment.FacingLeftShooting,
        [ZoaAnimationSelector.RisingFacingLeft] = ZoaSegment.FacingLeftRising,
        [ZoaAnimationSelector.ShootingFacingRight] = ZoaSegment.FacingRightShooting,
        [ZoaAnimationSelector.RisingFacingRight] = ZoaSegment.FacingRightRising,
    };
}
