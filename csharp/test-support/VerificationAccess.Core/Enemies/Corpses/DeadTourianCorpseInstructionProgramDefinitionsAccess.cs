
namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Verification access to <see cref="DeadTourianCorpseInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class DeadTourianCorpseInstructionProgramDefinitionsAccess
{
    /// <summary>Finds each dead Tourian corpse program's sleep operand at entry plus four.</summary>
    extension(DeadTourianCorpseInstructionProgramDefinitions)
    {
        /// <summary>Locates the sleep command after a Dead Tourian corpse program's first frame.</summary>
        internal static ushort SleepWordAddress(int index) => (ushort)(DeadTourianCorpseInstructionProgramDefinitions.Program(index)+4);
    }
}
