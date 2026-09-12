namespace SuperMetroid.Core.Game;

/// <summary>Native signed 8.8 velocities for rocks spat by Kraid.</summary>
public static class KraidRockLaunchDefinitions
{
    /// <summary>$A7:BC65, rockSpitXVelocities; selected by bits one through three of the current RNG word.</summary>
    public static ReadOnlySpan<ushort> HorizontalVelocities =>
        [0xfc00, 0xfc40, 0xfb40, 0xfb80, 0xfb40, 0xfc00, 0xfb80, 0xfc40];
    /// <summary>Preserves the native even-byte-offset selector without advancing RNG.</summary>
    public static ushort FromRandom(ushort random) => HorizontalVelocities[(random & 0x000e) >> 1];
}
