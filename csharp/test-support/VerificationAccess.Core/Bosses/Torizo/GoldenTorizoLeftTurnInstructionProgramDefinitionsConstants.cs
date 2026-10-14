
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values of <see cref="GoldenTorizoLeftTurnInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoLeftTurnInstructionProgramDefinitionsConstants
{
    /// <summary>First byte after the left-turn lists, $AA:D20D.</summary>
    public const ushort End = GoldenTorizoCombatInstructionPointers.WalkingLeftRightLeg;
    /// <summary>
    /// <c>ExtendedSpritemaps_Torizo_FacingScreen_Turning_Dodging</c> at
    /// $AA:A4F0, also used by both turning-right lists.
    /// </summary>
    public const ushort FacingScreenFrame = 0xa4f0;

    /// <summary>Provides the left-turn end boundary and screen-facing frame used to check Golden Torizo's turn program.</summary>
    extension(GoldenTorizoLeftTurnInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoLeftTurnInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => GoldenTorizoLeftTurnInstructionProgramDefinitionsConstants.End;
        /// <inheritdoc cref="GoldenTorizoLeftTurnInstructionProgramDefinitionsConstants.FacingScreenFrame"/>
        internal static ushort FacingScreenFrame => GoldenTorizoLeftTurnInstructionProgramDefinitionsConstants.FacingScreenFrame;
    }
}
