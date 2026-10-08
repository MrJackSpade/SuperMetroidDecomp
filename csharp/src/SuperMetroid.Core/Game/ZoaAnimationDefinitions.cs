namespace SuperMetroid.Core.Game;

/// <summary>
/// Composable Zoa animation selector stored in variables C/D. Native AI uses bit zero
/// for rising versus shooting and bit one for right-facing versus left-facing.
/// </summary>
[Flags]
public enum ZoaAnimationSelector : ushort
{
    None = 0,
    Rising = 1,
    FacingRight = 2,
}

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
