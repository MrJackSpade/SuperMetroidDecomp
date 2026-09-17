namespace SuperMetroid.Core.Game;

/// <summary>Compiled cartridge mechanics for Samus's special beam attacks.</summary>
internal static class SamusComboMechanicsDefinitions
{
    /// <summary>
    /// CostOfSBAsInPowerBombs at $90:CC21: one Power Bomb for Wave, Ice,
    /// Spazer, and Plasma alone; zero for the eight non-combo beam indexes.
    /// </summary>
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
}
