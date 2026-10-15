namespace SuperMetroid.Core.Enemies;

/// <summary>Chozo-statue palette and motion tables in bank $AA.</summary>
internal static class EnemyRomTablePointersChozoStatue
{
    /// <summary>Thirty-two signed statue movement velocities at $AA:E630 (64 bytes).</summary>
    public const int CarryVelocityWords = 0xaae630;
    /// <summary>Thirty-two carried-Samus X offsets at $AA:E670 (64 bytes).</summary>
    public const int CarriedSamusXOffsetWords = 0xaae670;
    /// <summary>Thirty-two carried-Samus Y offsets at $AA:E6B0 (64 bytes).</summary>
    public const int CarriedSamusYOffsetWords = 0xaae6b0;
}
