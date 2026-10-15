namespace SuperMetroid.Core.Enemies;

/// <summary>Bomb/Golden Torizo actor and projectile tables.</summary>
internal static class EnemyRomTablePointersTorizo
{
    /// <summary>Two Golden Torizo super-missile list pointers at $86:B209 (4 bytes).</summary>
    public const int SuperMissileInstructionPointers = 0x86b209;
    /// <summary>Two Bomb Torizo wake positions at $AA:C95F (4 bytes).</summary>
    public const int WakeXPositions = 0xaac95f;
    /// <summary>Two Bomb Torizo wake positions at $AA:C963 (4 bytes).</summary>
    public const int WakeYPositions = 0xaac963;
    /// <summary>Two Bomb Torizo wake instruction lists at $AA:C967 (4 bytes).</summary>
    public const int WakeInstructionLists = 0xaac967;
    /// <summary>Two Bomb Torizo wake property masks at $AA:C96B (4 bytes).</summary>
    public const int WakePropertyMasks = 0xaac96b;
    /// <summary>Two Bomb Torizo wake X radii at $AA:C96F (4 bytes).</summary>
    public const int WakeXRadii = 0xaac96f;
    /// <summary>Two Bomb Torizo wake Y radii at $AA:C973 (4 bytes).</summary>
    public const int WakeYRadii = 0xaac973;
    /// <summary>Twenty Golden Torizo walking displacement words at $AA:D59A (40 bytes).</summary>
    public const int WalkHorizontalVelocityWords = 0xaad59a;
    /// <summary>Bomb Torizo palette-FX color words at $84:8032.</summary>
    public const int BodyPaletteFxColors = 0x848032;
    /// <summary>Bomb Torizo belly palette-FX color words at $84:8132.</summary>
    public const int BellyPaletteFxColors = 0x848132;
}
