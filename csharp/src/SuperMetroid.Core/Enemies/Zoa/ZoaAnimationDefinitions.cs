namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>The program each of a Zoa's animations plays.</summary>
internal static class ZoaAnimationDefinitions
{
    /// <summary>The program of each rising and shooting animation, facing left and right.</summary>
    internal static readonly Dictionary<ZoaAnimationSelector, ZoaProgram> Programs = new()
    {
        [ZoaAnimationSelector.ShootingFacingLeft] = ZoaProgram.FacingLeftShooting,
        [ZoaAnimationSelector.RisingFacingLeft] = ZoaProgram.FacingLeftRising,
        [ZoaAnimationSelector.ShootingFacingRight] = ZoaProgram.FacingRightShooting,
        [ZoaAnimationSelector.RisingFacingRight] = ZoaProgram.FacingRightRising,
    };
}
