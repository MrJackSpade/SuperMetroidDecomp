namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>Compiled fixed animation-program selectors for Zoa.</summary>
internal static class ZoaAnimationDefinitions
{
    /// <summary>The animation for each of the four documented facing and movement flag combinations, as the Zoa's animation table selects.</summary>
    internal static readonly Dictionary<ZoaAnimationSelector, ZoaProgram> Programs = new()
    {
        [ZoaAnimationSelector.None] = ZoaProgram.FacingLeftShooting,
        [ZoaAnimationSelector.Rising] = ZoaProgram.FacingLeftRising,
        [ZoaAnimationSelector.FacingRight] = ZoaProgram.FacingRightShooting,
        [ZoaAnimationSelector.FacingRight | ZoaAnimationSelector.Rising] = ZoaProgram.FacingRightRising,
    };
}
