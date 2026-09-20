namespace SuperMetroid.Core.Game;

/// <summary>Private bank-$A2 instruction callbacks used by the tatori family.</summary>
internal static class MamaTurtleInstructionCodes
{
    /// <summary><c>Instruction_BabyTurtle_Crawl</c> at $A2:9381.</summary>
    internal const ushort Crawl = 0x9381;
    /// <summary><c>Instruction_BabyTurtle_LoopOrTurnAroundIfMovedTooFar</c> at $A2:9412.</summary>
    internal const ushort LoopOrTurnAroundIfMovedTooFar = 0x9412;
    /// <summary><c>Instruction_MamaTurtle_EnterShell</c> at $A2:9447.</summary>
    internal const ushort EnterShell = 0x9447;
    /// <summary><c>Instruction_MamaTurtle_RiseToHoverRightwards</c> at $A2:9451.</summary>
    internal const ushort RiseToHoverRightwards = 0x9451;
    /// <summary><c>Instruction_MamaTurtle_RiseToHoverLeftwards</c> at $A2:946B.</summary>
    internal const ushort RiseToHoverLeftwards = 0x946b;
    /// <summary><c>Instruction_BabyTurtle_LeaveShell</c> at $A2:9485.</summary>
    internal const ushort LeaveShell = 0x9485;
    /// <summary><c>Instruction_BabyTurtle_LeftShell</c> at $A2:94A1.</summary>
    internal const ushort LeftShell = 0x94a1;
    /// <summary><c>Instruction_BabyTurtle_Set_Spinning_Stoppable</c> at $A2:94C7.</summary>
    internal const ushort SetSpinningStoppable = 0x94c7;
    /// <summary><c>Instruction_MamaTurtle_PlaySpinningSFX</c> at $A2:94D1.</summary>
    internal const ushort PlaySpinningSound = 0x94d1;
}
