using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="GoldenTorizoWalkingInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoWalkingInstructionProgramDefinitionsConstants
{
    /// <summary>Instruction boundary at $D2AD after the walking program.</summary>
    public const ushort End = 0xd2ad;
    /// <summary>First left-right-leg walking instruction in the combat pointer catalog.</summary>
    public const ushort Start = GoldenTorizoCombatInstructionPointers.WalkingLeftRightLeg;

    /// <summary>Provides the native walking program span for Golden Torizo instruction checks.</summary>
    extension(GoldenTorizoWalkingInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoWalkingInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => GoldenTorizoWalkingInstructionProgramDefinitionsConstants.End;
        /// <inheritdoc cref="GoldenTorizoWalkingInstructionProgramDefinitionsConstants.Start"/>
        internal static ushort Start => GoldenTorizoWalkingInstructionProgramDefinitionsConstants.Start;
    }
}
