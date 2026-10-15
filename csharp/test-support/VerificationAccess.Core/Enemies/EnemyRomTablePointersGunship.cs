namespace SuperMetroid.Core.Enemies;

/// <summary>Landing Site gunship graphics-transfer tables.</summary>
internal static class EnemyRomTablePointersGunship
{
    /// <summary>Six signed liftoff dust X-offset words at $86:A2D6 (12 bytes).</summary>
    public const int DustXOffsetWords = 0x86a2d6;
    /// <summary>Six liftoff dust instruction-list pointers at $86:A2E2 (12 bytes).</summary>
    public const int DustInstructionPointers = 0x86a2e2;
    /// <summary>Five graphics source pointers at $A2:AC07 (10 bytes).</summary>
    public const int LiftoffGraphicsSourceWords = 0xa2ac07;
    /// <summary>Five VRAM destination words at $A2:AC11 (10 bytes).</summary>
    public const int LiftoffVramDestinationWords = 0xa2ac11;
}
