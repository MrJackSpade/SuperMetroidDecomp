namespace SuperMetroid.Core.Game;

/// <summary>Compiled cartridge mechanics for Samus's special beam attacks.</summary>
internal static class SamusComboMechanicsDefinitions
{
    /// <summary>
    /// CostOfSBAsInPowerBombs at $90:CC21: one Power Bomb for Wave, Ice,
    /// Spazer, and Plasma alone; zero for the eight non-combo beam indexes.
    /// </summary>
    /// <remarks>Issues #625 and #938 exact bit predicate: after validating b=0..11,
    /// return 1 iff b!=0 and (b&amp;(b-1))==0, otherwise zero. This recognizes
    /// precisely the four single-beam selections without storing twelve costs.
    /// LookupTableResearch verifies every result against GetPowerBombCost,
    /// the NTSC ROM and pinned bank_90.asm, including invalid-input rejection.
    /// Keep zero distinct from a power of two and do not accept unused indices 12..15.</remarks>
    private static ReadOnlySpan<ushort> PowerBombCosts =>
    [
        0, 1, 1, 0,
        1, 0, 0, 0,
        1, 0, 0, 0,
    ];

    /// <summary>
    /// IcePlasmaSBAProjectileOriginAngles at $90:CD08: four evenly spaced
    /// eight-bit angles assigned in projectile-slot order.
    /// </summary>
    /// <remarks>Issue #625 exact formula: 64*slot for validated slot=0..3.
    /// LookupTableResearch verifies all four compiled/native words and bounds.
    /// The adjacent unused diagonal-angle words are not further projectile slots;
    /// a future replacement must not extrapolate this progression into them.</remarks>
    private static ReadOnlySpan<ushort> OriginAngles => [0x00, 0x40, 0x80, 0xc0];

    /// <summary>Returns the native Power Bomb cost for a low-nibble beam index.</summary>
    internal static ushort GetPowerBombCost(int beamIndex)
    {
        if ((uint)beamIndex >= PowerBombCosts.Length)
            throw new ArgumentOutOfRangeException(nameof(beamIndex));
        return PowerBombCosts[beamIndex];
    }

    /// <summary>Returns the native Ice/Plasma combo origin angle for one projectile slot.</summary>
    internal static ushort GetOriginAngle(int projectileSlot)
    {
        if ((uint)projectileSlot >= OriginAngles.Length)
            throw new ArgumentOutOfRangeException(nameof(projectileSlot));
        return OriginAngles[projectileSlot];
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
