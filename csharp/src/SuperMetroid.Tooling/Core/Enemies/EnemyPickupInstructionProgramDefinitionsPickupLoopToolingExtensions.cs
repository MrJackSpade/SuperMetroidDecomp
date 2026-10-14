namespace SuperMetroid.Core.Enemies;

/// <summary>Development-tool instance members of <see cref="EnemyPickupInstructionProgramDefinitions.PickupLoop"/>.</summary>
internal static class EnemyPickupInstructionProgramDefinitionsPickupLoopToolingExtensions
{
    /// <summary>Exposes pickup-loop program length to development-time instruction checks.</summary>
    extension(EnemyPickupInstructionProgramDefinitions.PickupLoop self)
    {
            /// <summary>Counts frame words plus the loop terminator, with an additional sleep word when the pickup waits for activation.</summary>
            internal int MechanicsWords => self.Frames + (self.HasSleep ? 3 : 2);
    }
}
