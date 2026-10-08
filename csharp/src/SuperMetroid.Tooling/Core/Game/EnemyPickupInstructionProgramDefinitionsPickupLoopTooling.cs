namespace SuperMetroid.Core.Game;

/// <summary>Development-tool instance members of <see cref="EnemyPickupInstructionProgramDefinitions.PickupLoop"/>.</summary>
internal static class EnemyPickupInstructionProgramDefinitionsPickupLoopToolingExtensions
{
    extension(EnemyPickupInstructionProgramDefinitions.PickupLoop self)
    {
            internal int MechanicsWords => self.Frames + (self.HasSleep ? 3 : 2);
    }
}
