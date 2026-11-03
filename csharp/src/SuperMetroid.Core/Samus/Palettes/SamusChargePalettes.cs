using static SuperMetroid.Core.Samus.Palettes.SamusChargePaletteDefinitions;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Which charge steps and Hyper-shot glow steps have a shade.</summary>
public static class SamusChargePalettes
{
    /// <summary>
    /// Whether the charged-beam lists and the pseudo-screw lists hold a palette for the step: each suit's
    /// list holds six.
    /// </summary>
    /// <param name="step">The charge cycle step.</param>
    public static bool IsCompiledChargeStep(int step) => (uint)step < ChargeSteps;

    /// <summary>
    /// Whether a Hyper-shot glow step shows a frame of the Hyper Beam's full-body glow: steps one through
    /// <see cref="SamusChargePaletteDefinitions.HyperShotSteps"/> do, and step zero restores the suit (#1512).
    /// </summary>
    /// <param name="step">The glow step the shot-glow timer holds, counting down to zero.</param>
    public static bool IsCompiledHyperShot(int step) => step is >= 1 and <= HyperShotSteps;

    /// <summary>
    /// The frame of the Hyper Beam's full-body glow a Hyper-shot glow step shows: the glow walks the cycle backward as the
    /// timer counts down, so its last step shows the cycle's last frame and step one its first.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The step shows no frame.</exception>
    public static int HyperShotFrame(int step) =>
        IsCompiledHyperShot(step) ? step - 1 : throw new ArgumentOutOfRangeException(nameof(step), step, "The glow step shows no Hyper Beam frame.");
}
