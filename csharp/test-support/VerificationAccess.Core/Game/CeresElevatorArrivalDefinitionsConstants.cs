using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="CeresElevatorArrivalDefinitions"/> that only verification reads.</summary>
internal static class CeresElevatorArrivalDefinitionsConstants
{
    /// <summary>Bank containing the two definitions and their instruction programs.</summary>
    public const int BankBase = 0x860000;
    /// <summary>$86:8154, Instruction_Delete opcode stored at $86:A28B.</summary>
    public const ushort DeleteOpcode = 0x8154;
    /// <summary>$86:81AB, Instruction_Goto opcode used by both looping programs.</summary>
    public const ushort GotoOpcode = 0x81ab;

    /// <summary>Provides the bank base and control opcodes used to decode Ceres elevator arrival instructions.</summary>
    extension(CeresElevatorArrivalDefinitions)
    {
        /// <inheritdoc cref="CeresElevatorArrivalDefinitionsConstants.BankBase"/>
        internal static int BankBase => CeresElevatorArrivalDefinitionsConstants.BankBase;
        /// <inheritdoc cref="CeresElevatorArrivalDefinitionsConstants.DeleteOpcode"/>
        internal static ushort DeleteOpcode => CeresElevatorArrivalDefinitionsConstants.DeleteOpcode;
        /// <inheritdoc cref="CeresElevatorArrivalDefinitionsConstants.GotoOpcode"/>
        internal static ushort GotoOpcode => CeresElevatorArrivalDefinitionsConstants.GotoOpcode;
    }
}
