namespace SuperMetroid.Core.Game;

/// <summary>
/// Fixed scroll data used by <c>Update Crocomire BG2 scroll</c> at $A4:8B5B.
/// The cartridge searches the 17 pointers at $A4:8B79 in reverse order and,
/// on a match, adds that spritemap's fourth-component Y offset from pointer + $1C.
/// This is camera/presentation alignment, not editable boss artwork.
/// </summary>
internal static class CrocomireBg2ScrollDefinitions
{
    /// <summary>Native $A4:8B5B vertical-scroll origin.</summary>
    internal const ushort VerticalOrigin = 0x0043;

    /// <summary>$A4:BFC4 ExtendedSpritemap_Crocomire_ChargeForward_StepBack_0,
    /// first of twelve consecutive $32-byte extended frame records.</summary>
    private const ushort ChargeStepFrameStart = 0xbfc4;
    /// <summary>$A4:C47A ExtendedSpritemap_Crocomire_MovingClaws_0,
    /// first of five consecutive $32-byte extended frame records.</summary>
    private const ushort MovingClawsFrameStart = 0xc47a;

    /// <summary>$A4:BFF6 ExtendedSpritemap_Crocomire_ChargeForward_StepBack_1.</summary>
    private const ushort ChargeStep1 = ChargeStepFrameStart + 0x32;
    /// <summary>$A4:C028 ExtendedSpritemap_Crocomire_ChargeForward_StepBack_2.</summary>
    private const ushort ChargeStep2 = ChargeStepFrameStart + 2 * 0x32;
    /// <summary>$A4:C05A ExtendedSpritemap_Crocomire_ChargeForward_StepBack_3.</summary>
    private const ushort ChargeStep3 = ChargeStepFrameStart + 3 * 0x32;
    /// <summary>$A4:C08C ExtendedSpritemap_Crocomire_ChargeForward_StepBack_4.</summary>
    private const ushort ChargeStep4 = ChargeStepFrameStart + 4 * 0x32;
    /// <summary>$A4:C0BE ExtendedSpritemap_Crocomire_ChargeForward_StepBack_5.</summary>
    private const ushort ChargeStep5 = ChargeStepFrameStart + 5 * 0x32;
    /// <summary>$A4:C0F0 ExtendedSpritemap_Crocomire_ChargeForward_StepBack_6.</summary>
    private const ushort ChargeStep6 = ChargeStepFrameStart + 6 * 0x32;
    /// <summary>$A4:C122 ExtendedSpritemap_Crocomire_ChargeForward_StepBack_7.</summary>
    private const ushort ChargeStep7 = ChargeStepFrameStart + 7 * 0x32;
    /// <summary>$A4:C4AC ExtendedSpritemap_Crocomire_MovingClaws_1.</summary>
    private const ushort MovingClaws1 = MovingClawsFrameStart + 0x32;
    /// <summary>$A4:C4DE ExtendedSpritemap_Crocomire_MovingClaws_2.</summary>
    private const ushort MovingClaws2 = MovingClawsFrameStart + 2 * 0x32;
    /// <summary>$A4:C510 ExtendedSpritemap_Crocomire_MovingClaws_3.</summary>
    private const ushort MovingClaws3 = MovingClawsFrameStart + 3 * 0x32;
    /// <summary>$A4:C542 ExtendedSpritemap_Crocomire_MovingClaws_4.</summary>
    private const ushort MovingClaws4 = MovingClawsFrameStart + 4 * 0x32;

    /// <summary>Aligns BG2 to the selected body's extended-frame pose. These named
    /// pose cases reproduce the native pointer search and signed Y adjustment;
    /// all unlisted identities, including mapped zero-offset poses, add zero.</summary>
    private static int Correction(ushort frame) => frame switch
    {
        ChargeStep1 or ChargeStep2 or ChargeStep4 or ChargeStep5 or MovingClaws1 or MovingClaws3 => -2,
        ChargeStep3 or ChargeStep6 or ChargeStep7 or MovingClaws4 => -1,
        MovingClaws2 => -4,
        _ => 0,
    };

    /// <summary>Applies the signed correction with native sixteen-bit wrapping.</summary>
    internal static ushort VerticalScroll(ushort bodyY, ushort spritemapPointer) =>
        unchecked((ushort)(VerticalOrigin - bodyY + Correction(spritemapPointer)));
}