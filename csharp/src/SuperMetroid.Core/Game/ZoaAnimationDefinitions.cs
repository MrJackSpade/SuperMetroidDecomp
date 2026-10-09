namespace SuperMetroid.Core.Game;

/// <summary>
/// Composable Zoa animation selector stored in variables C/D. Native AI uses bit zero
/// for rising versus shooting and bit one for right-facing versus left-facing.
/// </summary>
[Flags]
public enum ZoaAnimationSelector : ushort
{
    /// <summary>Neither bit set: left-facing shooting art, entry zero of $A3:B40D InstListPointers_Zoa.</summary>
    None = 0,
    /// <summary>Bit 0 selects rising art at $A3:B40F/$B413 rather than shooting; AI clears it on reaching Samus's height while retaining facing.</summary>
    Rising = 1,
    /// <summary>Bit 1 selects right-facing shooting art, or right-facing rising when combined with Rising; absence of this bit selects left-facing art.</summary>
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
