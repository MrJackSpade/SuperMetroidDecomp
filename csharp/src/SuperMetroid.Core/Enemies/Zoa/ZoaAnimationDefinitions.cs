namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>Compiled fixed animation-program selectors for Zoa.</summary>
internal static class ZoaAnimationDefinitions
{

    /// <summary>Selects the animation by facing and movement flags, as $A3:B40D InstListPointers_Zoa does.
    /// Only the four documented combinations are valid; unsupported bits are rejected.</summary>
    internal static ZoaProgram Program(ZoaAnimationSelector selector) => selector switch
    {
        ZoaAnimationSelector.None => ZoaProgram.FacingLeftShooting,
        ZoaAnimationSelector.Rising => ZoaProgram.FacingLeftRising,
        ZoaAnimationSelector.FacingRight => ZoaProgram.FacingRightShooting,
        ZoaAnimationSelector.FacingRight | ZoaAnimationSelector.Rising => ZoaProgram.FacingRightRising,
        _ => throw new InvalidDataException(
            $"Zoa animation selector ${(int)selector:X4} exceeds its four-entry table."),
    };
}
