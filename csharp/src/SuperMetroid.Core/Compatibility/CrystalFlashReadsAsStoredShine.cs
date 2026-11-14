namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Samus holds a stored shine, as the game's shine tests see it, while Crystal Flash's palette runs (#1546): from the
/// flash's activation until the palette update after its finish restores the beams' colours, her footsteps make no
/// sound, invincibility flicker never hides her body, and a jump into the neutral or upward jump poses starts a
/// shinespark windup, exactly as a stored shine does. Otherwise she holds one only while her stored-shine countdown runs.
/// </summary>
/// <remarks>
/// Consulted by the footstep sound during Samus's animation, by her body's draw, and by the jump pose transition and the
/// shinespark windup it starts. Evidence: Crystal Flash's colour step keeps its bubble countdown, and at its finish
/// the request to restore the beams' colours, in the timer the stored shine counts down, and the countdown is reloaded
/// on reaching zero, so the timer never reads zero while the handler is installed; the footstep, draw and jump tests
/// read that timer without testing which handler owns it.
/// </remarks>
[CompatibilityService("Samus reads as holding a stored shine while Crystal Flash's palette runs")]
public static class CrystalFlashReadsAsStoredShine
{
    /// <summary>Whether Samus holds a stored shine as the game's shine tests see it.</summary>
    /// <param name="crystalFlashPaletteRunning">Whether Crystal Flash's palette runs, through its beam-colour restoration.</param>
    /// <param name="storedShineFrames">The updates left on Samus's stored-shine countdown.</param>
    public static bool HoldsShine(bool crystalFlashPaletteRunning, int storedShineFrames) =>
        crystalFlashPaletteRunning || storedShineFrames != 0;
}
