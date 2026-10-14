namespace SuperMetroid.Core.Game;

/// <summary>The hopper family's private bank-$A3 animation instructions.</summary>
internal enum HopperInstruction : ushort
{
    /// <summary><c>Instruction_Sidehopper_QueueSoundInY_Lib2_Max3</c> at $A3:AA68.</summary>
    SidehopperQueueSoundInY = 0xaa68,

    /// <summary><c>Instruction_Hopper_ReadyToHop</c> at $A3:AAFE.</summary>
    ReadyToHop = 0xaafe,
}
