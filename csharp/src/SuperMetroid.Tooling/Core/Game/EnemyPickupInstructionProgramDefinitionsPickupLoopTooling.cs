namespace SuperMetroid.Core.Game;

/// <summary>Development-tool instance members of <see cref="EnemyPickupInstructionProgramDefinitions.PickupLoop"/>.</summary>
internal static class EnemyPickupInstructionProgramDefinitionsPickupLoopToolingExtensions
{
    /// <summary>Provides extension members for <c>EnemyPickupInstructionProgramDefinitions.PickupLoop</c>.</summary>
    extension(EnemyPickupInstructionProgramDefinitions.PickupLoop self)
    {
            /// <summary>Gets mechanics words.</summary>
            internal int MechanicsWords => self.Frames + (self.HasSleep ? 3 : 2);
    }
}
