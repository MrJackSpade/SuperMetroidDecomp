namespace SuperMetroid.Core.Enemies;

/// <summary>Verified bit tests on the cartridge enemy AI-handler word.</summary>
internal static class EnemyAiHandlerMasks
{
    /// <summary>
    /// $A0:8EB6 admits actors with AI-handler bit $0004 even outside the camera
    /// window, keeping frozen processing and collision-list membership active.
    /// </summary>
    public const ushort Frozen = 0x0004;

    /// <summary>AI-handler bit $0002: the enemy runs its hurt AI, set when a shot damages it.</summary>
    public const ushort Hurt = 0x0002;
}
