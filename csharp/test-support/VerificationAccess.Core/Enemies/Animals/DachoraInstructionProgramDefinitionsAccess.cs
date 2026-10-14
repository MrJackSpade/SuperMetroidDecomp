namespace SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Programs;

/// <summary>Verification access to <see cref="DachoraInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class DachoraInstructionProgramDefinitionsAccess
{
    /// <summary>The fifteen production entry programs, in native order.</summary>
    private static readonly ushort[] ProgramEntries =
    [
        DachoraInstructionProgramDefinitions.RunningLeft, DachoraInstructionProgramDefinitions.RunningLeftFast,
        DachoraInstructionProgramDefinitions.RunningLeftVeryFast, DachoraInstructionProgramDefinitions.IdleLeft,
        DachoraInstructionProgramDefinitions.BlinkLeft, DachoraInstructionProgramDefinitions.EchoLeft,
        DachoraInstructionProgramDefinitions.FallingLeft, DachoraInstructionProgramDefinitions.RunningRight,
        DachoraInstructionProgramDefinitions.RunningRightFast, DachoraInstructionProgramDefinitions.RunningRightVeryFast,
        DachoraInstructionProgramDefinitions.IdleRight, DachoraInstructionProgramDefinitions.BlinkRight,
        DachoraInstructionProgramDefinitions.ChargeRight, DachoraInstructionProgramDefinitions.EchoRight,
        DachoraInstructionProgramDefinitions.FallingRight,
    ];

    /// <summary>Indexes the compiled Dachora instruction programs and their native entry points.</summary>
    extension(DachoraInstructionProgramDefinitions)
    {
        /// <summary>Count of the fifteen native Dachora entry programs above.</summary>
        internal static int ProgramCount => ProgramEntries.Length;

        /// <summary>Reads a program root, timed-frame count, and whether its terminator loops.</summary>
        internal static DachoraInstructionProgram Program(int index)
        {
            ushort entry = ProgramEntries[index];
            (int frames, ushort terminator) = DachoraInstructionProgramDefinitions.Layout.FramesFrom(entry);
            return new(entry, frames, terminator == (ushort)CommonEnemyInstruction.Goto);
        }
    }
}
