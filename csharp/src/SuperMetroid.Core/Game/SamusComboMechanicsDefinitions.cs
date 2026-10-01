namespace SuperMetroid.Core.Game;

/// <summary>Compiled cartridge mechanics for Samus's special beam attacks.</summary>
internal static class SamusComboMechanicsDefinitions
{
    /// <summary>
    /// CostOfSBAsInPowerBombs at $90:CC21: one Power Bomb for Wave, Ice,
    /// Spazer, and Plasma alone; zero for the eight non-combo beam indexes.
    /// </summary>
    /// <remarks>Independently reviewed for #1165 against NTSC J/U v1.0 and pinned
    /// bank_90.asm: after validating b=0..11,
    /// return 1 iff b!=0 and (b&amp;(b-1))==0, otherwise zero. This recognizes
    /// precisely the four single-beam selections without storing twelve costs.
    /// FireSBA masks equipped beams to a low nibble, then doubles it for a word
    /// lookup. This API receives the undoubled index; its proof covers all twelve costs.
    /// Keep zero distinct from a power of two and do not accept unused indices 12..15.</remarks>
    internal static ushort GetPowerBombCost(int beamIndex)
    {
        if ((uint)beamIndex >= 12)
            throw new ArgumentOutOfRangeException(nameof(beamIndex));
        return (ushort)(beamIndex != 0 && (beamIndex & (beamIndex - 1)) == 0 ? 1 : 0);
    }

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
