using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Native NTSC firing delays, independent of projectile presentation assets.</summary>
internal static class SamusProjectileCooldownDefinitions
{
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

    internal static byte ReadByte(ISnesAddressSpace bus, int address)
    {
        int index = address - SamusProjectileRomData.Beams.UnchargedCooldowns;
        // Select by exact address before table identity. Extended indices can read
        // neighboring cooldown rows or continue beyond them into presentation SFX.
        return index >= 0 && index < Delays.Length ? Delays[index] : bus.ReadByte(address);
    }
}
