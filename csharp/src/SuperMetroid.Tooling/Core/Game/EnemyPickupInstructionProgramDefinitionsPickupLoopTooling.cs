namespace SuperMetroid.Core.Game;

/// <summary>Development-tool instance members of <see cref="EnemyPickupInstructionProgramDefinitions.PickupLoop"/>.</summary>
internal static class EnemyPickupInstructionProgramDefinitionsPickupLoopToolingExtensions
{
    /// <summary>Adds tooling-only inspection of the compiled mechanics-word count for one pickup animation loop.</summary>
    extension(EnemyPickupInstructionProgramDefinitions.PickupLoop self)
    {
            /// <summary>Counts timed-frame duration words, the loop command and target, and any trailing sleep command.</summary>
            internal int MechanicsWords => self.Frames + (self.HasSleep ? 3 : 2);
    }
}
