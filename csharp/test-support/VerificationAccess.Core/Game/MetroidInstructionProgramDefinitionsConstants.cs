using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="MetroidInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class MetroidInstructionProgramDefinitionsConstants
{
    /// <summary><c>BombedOffVelocities</c>, adjacent non-instruction data at $A3:EA3F.</summary>
    public const ushort AdjacentBombedOffVelocities = 0xea3f;
    /// <summary><c>Instruction_Metroid_PlayRandomMetroidSFX</c> entry at $A3:EA1F.</summary>
    public const ushort ChasingSoundCallback = 0xea1f;

    /// <summary>Provides the nearby bombed-off velocities and chase sound callback for Metroid program checks.</summary>
    extension(MetroidInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="MetroidInstructionProgramDefinitionsConstants.AdjacentBombedOffVelocities"/>
        internal static ushort AdjacentBombedOffVelocities => MetroidInstructionProgramDefinitionsConstants.AdjacentBombedOffVelocities;
        /// <inheritdoc cref="MetroidInstructionProgramDefinitionsConstants.ChasingSoundCallback"/>
        internal static ushort ChasingSoundCallback => MetroidInstructionProgramDefinitionsConstants.ChasingSoundCallback;
    }
}
