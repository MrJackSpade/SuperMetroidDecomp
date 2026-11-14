using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The kind, Power Bomb cost and spread of each charged beam combo.</summary>
internal static class SamusComboMechanics
{
    /// <summary>
    /// The special beam attack a retail combination fires, or null when it fires none: only
    /// Wave, Ice, Spazer or Plasma equipped alone has one.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination is outside the twelve retail rows.</exception>
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
            throw new ArgumentOutOfRangeException(nameof(beam), beam, "The combo tables hold twelve retail rows."),
        _ => throw new ArgumentOutOfRangeException(nameof(beam), beam, "Undefined beam combination."),
    };

    /// <summary>
    /// The special beam attacks' cost in Power Bombs: one Power Bomb for Wave, Ice,
    /// Spazer, and Plasma alone; zero for the eight non-combo beam indexes.
    /// </summary>
    /// <remarks>Independently reviewed for #1165 against NTSC J/U v1.0:
    /// the four single-beam rows cost one and the other eight cost zero.
    /// The game keeps the equipped beams' lowest four bits as the row.
    /// Rows 12..15 are outside the table.</remarks>
    internal static int GetPowerBombCost(SamusBeamCombination beam) =>
        ComboKind(beam) is null ? 0 : 1;

    /// <summary>
    /// Scales the sine and cosine of an angle by an amplitude, as the combos place their
    /// particles. The game negates the truncated positive magnitude, so this deliberately
    /// truncates toward zero in all quadrants.
    /// </summary>
    /// <remarks>Independently reviewed for #1165 against the combos' sine and cosine and all
    /// original positive sine values. The positive sample times the amplitude, divided by 256
    /// and truncated, gives the magnitude; the sign is restored afterward, including at the exact 256 peak.
    /// Ice, Plasma and Spazer callers supply angles of 256 steps; Y turns back a quarter turn
    /// with the same wrap. The amplitude is 0 through 255.
    /// This is a consumer view of the reviewed signed sine mapping, with no stored
    /// lookup of its own; preserve this existing algorithm.</remarks>
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
