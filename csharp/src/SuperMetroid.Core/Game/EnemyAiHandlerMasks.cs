namespace SuperMetroid.Core.Game;

/// <summary>Verified bit tests on the cartridge enemy AI-handler word.</summary>
internal static class EnemyAiHandlerMasks
{
    /// <summary>
    /// $A0:8EB6 admits actors with AI-handler bit $0004 even outside the camera
    /// window, keeping frozen processing and collision-list membership active.
    /// </summary>
    public const ushort Frozen = 0x0004;
}
