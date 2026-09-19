namespace SuperMetroid.Core.Game;

/// <summary>Native NTSC firing delays, independent of projectile presentation assets.</summary>
internal static class SamusProjectileCooldownDefinitions
{
    /// <summary>
    /// Bank-$90 address reached when the bounded SpaceTime Beam setup fires its corrupt
    /// beam word. Native indexes three bytes beyond the ordinary cooldown table and
    /// observes the low byte <c>$0D</c> of the adjacent instruction operand.
    /// </summary>
    internal const int SpacetimeBeamCooldownAddress = 0x90C291;

    /// <summary>
    /// $90:C254..C28E ProjectileCooldown and BeamAutoFireCooldowns: sixteen uncharged,
    /// sixteen charged, six padding, nine non-beam, and twelve auto-fire bytes.
    /// Preserve zero padding because combo and out-of-table indices can reach it.
    /// </summary>
    private static ReadOnlySpan<byte> Delays =>
    [
        15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 12, 15, 0, 0, 0, 0,
        30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0,
        0, 10, 20, 40, 0, 16, 0, 0, 0,
        25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
    ];

    internal static byte ReadByte(int address)
    {
        int index = address - SamusProjectileRomData.Beams.UnchargedCooldowns;
        if (index >= 0 && index < Delays.Length)
            return Delays[index];
        if (address == SpacetimeBeamCooldownAddress)
            return 0x0D;

        throw new InvalidDataException(
            $"Projectile cooldown byte ${address:X6} is outside the compiled definitions.");
    }
}
