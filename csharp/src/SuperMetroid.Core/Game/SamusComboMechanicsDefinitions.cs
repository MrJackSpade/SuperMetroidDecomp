namespace SuperMetroid.Core.Game;

/// <summary>The four special beam attacks: one per beam equipped alone.</summary>
internal enum SamusComboKind
{
    /// <summary>Wave alone.</summary>
    Wave,
    /// <summary>Ice alone.</summary>
    Ice,
    /// <summary>Spazer alone.</summary>
    Spazer,
    /// <summary>Plasma alone.</summary>
    Plasma,
}

/// <summary>Compiled cartridge mechanics for Samus's special beam attacks.</summary>
internal static class SamusComboMechanicsDefinitions
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
    /// CostOfSBAsInPowerBombs at $90:CC21: one Power Bomb for Wave, Ice,
    /// Spazer, and Plasma alone; zero for the eight non-combo beam indexes.
    /// </summary>
    /// <remarks>Independently reviewed for #1165 against NTSC J/U v1.0 and pinned
    /// bank_90.asm: the four single-beam rows cost one and the other eight cost zero.
    /// FireSBA masks equipped beams to a low nibble, then doubles it for a word
    /// lookup. Rows 12..15 are outside the table.</remarks>
    internal static ushort GetPowerBombCost(SamusBeamCombination beam) =>
        ComboKind(beam) is null ? (ushort)0 : (ushort)1;

    /// <summary>
    /// IcePlasmaSBAProjectileOriginAngles at $90:CD08: four evenly spaced
    /// eight-bit angles assigned in projectile-slot order.
    /// </summary>
    /// <remarks>Independently reviewed for #1165 against NTSC J/U v1.0 and pinned
    /// bank_90.asm: 64*slot for validated slot=0..3. Native Ice/Plasma initialization
    /// reads word offsets 6,4,2,0; the caller supplies undoubled projectile slots.
    /// The adjacent unused diagonal-angle words are not further projectile slots;
    /// the bounded progression does not extrapolate into them.</remarks>
    internal static ushort GetOriginAngle(int projectileSlot)
    {
        if ((uint)projectileSlot >= 4)
            throw new ArgumentOutOfRangeException(nameof(projectileSlot));
        return (ushort)(64 * projectileSlot);
    }

    /// <summary>
    /// Applies the unsigned hardware-multiply sequence at $90:CC39 to the low
    /// bytes of an angle and amplitude. Native negates the truncated positive
    /// magnitude, so this deliberately truncates toward zero in all quadrants.
    /// </summary>
    /// <remarks>Independently reviewed for #1165 against $90:CC39/$CC8A and all
    /// original positive sine words at $A0:B443. Multiplying a whole sample and
    /// shifting eight bits equals the native low-byte product high byte plus
    /// the high-byte product; restore sign afterward, including the exact 256 peak.
    /// Ice, Plasma and Spazer callers supply byte-cycle angles; Y subtracts 64
    /// with byte wrap. Amplitude uses its low byte as the hardware multiplier does.
    /// This is a consumer view of the reviewed signed sine mapping, with no stored
    /// lookup of its own; preserve this existing algorithm.</remarks>
    internal static (ushort X, ushort Y) GetSineOffset(ushort angle, ushort amplitude)
    {
        ushort Component(byte phase)
        {
            short sample = EnemyTrigonometryTables.SignedSine(phase);
            int magnitude = Math.Abs((int)sample) * (byte)amplitude >> 8;
            return unchecked((ushort)(sample < 0 ? -magnitude : magnitude));
        }

        return (Component((byte)angle), Component(unchecked((byte)(angle - 64))));
    }
}
