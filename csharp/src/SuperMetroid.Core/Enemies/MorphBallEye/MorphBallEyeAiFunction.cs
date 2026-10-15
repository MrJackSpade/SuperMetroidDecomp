namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// Bank-$A8 function words stored in a morph-ball eye's native variable F. These values are
/// intentionally the cartridge addresses: debugger watches can therefore be compared with
/// WRAM traces without translating a host-only state number back into a ROM routine.
/// </summary>
public enum MorphBallEyeAiFunction : ushort
{
    /// <summary>Native <c>Function_Eye_WaitForSamusToGetNear</c> at <c>$A8:90F1</c>; waits until Samus is strictly within 128 room pixels on both axes before starting the opening animation.</summary>
    WaitForSamus = 0x90f1,
    /// <summary>Native <c>Function_Eye_Activating</c> at <c>$A8:912E</c>; decrements the 32-update opening countdown, then requests the reveal beam and begins tracking Samus.</summary>
    Activating = 0x912e,
    /// <summary>Native <c>Function_Eye_Active</c> at <c>$A8:9160</c>; aims toward Samus and selects angle-dependent eye artwork while within 176 horizontal and 128 vertical pixels, otherwise begins closing.</summary>
    Active = 0x9160,
    /// <summary>Native <c>Function_Eye_Deactivating</c> at <c>$A8:91CE</c>; waits for the 32-update closing countdown to reach zero or wrap negative, then resumes proximity waiting.</summary>
    Deactivating = 0x91ce,
    /// <summary>Native <c>RTL_A891DC</c> at <c>$A8:91DC</c> for the decorative mount selected by population parameter 2's sign bit; it does not track Samus or own the beam.</summary>
    MountNoOp = 0x91dc,
}
