using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The kind, Power Bomb cost and spread of each charged beam combo.</summary>
internal static class SamusComboMechanics
{
    /// <summary>
    /// The special beam attack a combination fires, or null when it fires none: only
    /// Wave, Ice, Spazer or Plasma equipped alone has one.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination pairs Spazer with Plasma, which cannot be equipped.</exception>
    internal static SamusComboKind? ComboKind(SamusBeamCombination beam) => beam switch
    {
        SamusBeamCombination.Wave => SamusComboKind.Wave,
        SamusBeamCombination.Ice => SamusComboKind.Ice,
        SamusBeamCombination.Spazer => SamusComboKind.Spazer,
        SamusBeamCombination.Plasma => SamusComboKind.Plasma,
        SamusBeamCombination.Power or SamusBeamCombination.IceWave or SamusBeamCombination.SpazerWave or
            SamusBeamCombination.SpazerIce or SamusBeamCombination.SpazerIceWave or SamusBeamCombination.PlasmaWave or
            SamusBeamCombination.PlasmaIce or SamusBeamCombination.PlasmaIceWave => null,
        SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaWave or
            SamusBeamCombination.SpazerPlasmaIce or SamusBeamCombination.SpazerPlasmaIceWave =>
            throw new ArgumentOutOfRangeException(nameof(beam), beam, "Spazer and Plasma cannot be equipped together."),
        _ => throw new ArgumentOutOfRangeException(nameof(beam), beam, "Undefined beam combination."),
    };

    /// <summary>
    /// The special beam attacks' cost in Power Bombs: one Power Bomb for Wave, Ice,
    /// Spazer, and Plasma alone; zero for every other beam combination.
    /// </summary>
    internal static int GetPowerBombCost(SamusBeamCombination beam) =>
        ComboKind(beam) is null ? 0 : 1;

    /// <summary>
    /// Scales the sine and cosine of an angle by an amplitude, as the combos place their
    /// particles. The game negates the truncated positive magnitude, so this deliberately
    /// truncates toward zero in all quadrants.
    /// </summary>
    /// <remarks>The positive sample's share of the amplitude, in whole pixels rounded down, gives the magnitude; the sign
    /// is restored afterward, including at the sine's exact peak.
    /// The Ice, Plasma and Spazer attacks give angles in 256ths of a turn; Y turns back a quarter turn
    /// with the same wrap. The amplitude is 0 through 255. It reads the game's signed sine and keeps no
    /// values of its own.</remarks>
    internal static Offset16<RoomPixels> GetSineOffset(TrigonometryAngle angle, int amplitude)
    {
        Ensure.BetweenInclusive(0, ComboRingCollapse.LargestRadius, amplitude);
        int Component(TrigonometryAngle phase)
        {
            int sample = Trigonometry.SignedSine(phase);
            int magnitude = EightEight.Multiply(Math.Abs(sample), amplitude);
            return sample < 0 ? -magnitude : magnitude;
        }

        return new(Component(angle), Component(angle - TrigonometryAngle.QuarterTurn));
    }
}
