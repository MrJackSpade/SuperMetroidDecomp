namespace SuperMetroid.Core.Game;

/// <summary>Proven composable bits in the Torizo behavioral-properties word.</summary>
internal static class TorizoBehaviorBits
{
    /// <summary>$AA:D67F: Golden Torizo has caught a Super Missile; subsequent shots bypass capture.</summary>
    public const ushort CaughtSuperMissile = 0x1000;
    /// <summary>$AA:D6A0: stun/counterattack latch consumed by the health-gated instruction branch.</summary>
    public const ushort Counterattack = 0x2000;
}
