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
    /// FireSBA masks equipped beams to a low nibble, then doubles it for a word
    /// lookup. Rows 12..15 are outside the table.</remarks>
    internal static int GetPowerBombCost(SamusBeamCombination beam) =>
        ComboKind(beam) is null ? 0 : 1;

    /// <summary>
    /// The Ice and Plasma special beam attacks' origin angles: four evenly spaced
    /// eight-bit angles assigned in projectile-slot order.
    /// </summary>
    /// <remarks>Independently reviewed for #1165 against NTSC J/U v1.0:
    /// 64*slot for validated slot=0..3. Ice/Plasma initialization
    /// reads word offsets 6,4,2,0; the caller supplies undoubled projectile slots.
    /// The adjacent unused diagonal-angle words are not further projectile slots;
    /// the bounded progression does not extrapolate into them.</remarks>
    internal static ByteAngle GetOriginAngle(int projectileSlot)
    {
        if ((uint)projectileSlot >= 4)
            throw new ArgumentOutOfRangeException(nameof(projectileSlot));
        return new ByteAngle(0).AddTableUnits(ByteAngle.QuarterTurn.RawValue * projectileSlot);
    }

    /// <summary>
    /// Applies the unsigned hardware-multiply sequence to the low
    /// bytes of an angle and amplitude. The game negates the truncated positive
    /// magnitude, so this deliberately truncates toward zero in all quadrants.
    /// </summary>
    /// <remarks>Independently reviewed for #1165 against the combos' sine and cosine and all
    /// original positive sine words. Multiplying a whole sample and
    /// shifting eight bits equals the low-byte product high byte plus
    /// the high-byte product; restore sign afterward, including the exact 256 peak.
    /// Ice, Plasma and Spazer callers supply byte-cycle angles; Y subtracts 64
    /// with byte wrap. Amplitude uses its low byte as the hardware multiplier does.
    /// This is a consumer view of the reviewed signed sine mapping, with no stored
    /// lookup of its own; preserve this existing algorithm.</remarks>
    internal static Offset16<RoomPixels> GetSineOffset(ByteAngle angle, int amplitude)
    {
        Ensure.BetweenInclusive(0, OrbitRadiusCycle - 1, amplitude);
        int Component(int phase)
        {
            int sample = Trigonometry.SignedSine(new ByteAngle(phase));
            int magnitude = EightEight.Multiply(Math.Abs(sample), amplitude);
            return sample < 0 ? -magnitude : magnitude;
        }

        return new(Component(angle.RawValue), Component(angle.AddTableUnits(-ByteAngle.QuarterTurn.RawValue).RawValue));
    }

    /// <summary>The orbit radii a combo particle holds, in pixels; a radius grown past the last restarts from zero.</summary>
    private const int OrbitRadiusCycle = 256;

    /// <summary>
    /// The orbit radius after growing <paramref name="radius"/> by <paramref name="step"/> pixels. An orbit radius is held
    /// within a cycle of 256 pixels, so an Ice or Plasma ring that is still on screen when its radius passes 255
    /// collapses onto Samus and grows outward again.
    /// </summary>
    /// <param name="radius">The current radius, 0 through 255; other values are rejected.</param>
    /// <param name="step">The signed growth; a shrink must not pass below zero.</param>
    internal static int GrownOrbitRadius(int radius, int step)
    {
        int grown = Ensure.BetweenInclusive(0, OrbitRadiusCycle - 1, radius) + step;
        return grown >= OrbitRadiusCycle ? grown - OrbitRadiusCycle : Ensure.BetweenInclusive(0, OrbitRadiusCycle - 1, grown);
    }
}
