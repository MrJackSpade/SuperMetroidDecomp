namespace SuperMetroid.Core.Enemies;

/// <summary>Crocomire death graphics-transfer tables.</summary>
internal static class EnemyRomTablePointersCrocomire
{
    /// <summary>Seven VRAM destination offsets at $A4:99CB (14 bytes).</summary>
    public const int DeathVramDestinationWords = 0xa499cb;
    /// <summary>Seven source pointers at $A4:99D9 (14 bytes).</summary>
    public const int DeathGraphicsSourceWords = 0xa499d9;
}
