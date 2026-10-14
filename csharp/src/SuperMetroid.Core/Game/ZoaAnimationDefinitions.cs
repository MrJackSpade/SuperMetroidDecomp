namespace SuperMetroid.Core.Game;

/// <summary>Compiled fixed animation-program selectors for Zoa.</summary>
internal static class ZoaAnimationDefinitions
{

    /// <summary>Selects the native animation program by facing and movement flags.
    /// Only the four documented combinations are valid; unsupported bits are rejected.</summary>
    internal static ushort InstructionList(ZoaAnimationSelector selector) => selector switch
    {
        ZoaAnimationSelector.None => ZoaInstructionProgramDefinitions.FacingLeftShooting,
        ZoaAnimationSelector.Rising => ZoaInstructionProgramDefinitions.FacingLeftRising,
        ZoaAnimationSelector.FacingRight => ZoaInstructionProgramDefinitions.FacingRightShooting,
        ZoaAnimationSelector.FacingRight | ZoaAnimationSelector.Rising => ZoaInstructionProgramDefinitions.FacingRightRising,
        _ => throw new InvalidDataException(
            $"Zoa animation selector ${(int)selector:X4} exceeds its four-entry table."),
    };
}
