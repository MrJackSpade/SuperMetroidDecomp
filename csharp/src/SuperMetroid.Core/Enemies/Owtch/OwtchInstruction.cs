namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>Owtch's private bank-$A2 animation instructions.</summary>
internal enum OwtchInstruction : ushort
{
    /// <summary><c>Instruction_Owtch_0</c> at $A2:A56D.</summary>
    SetMovingLeft = 0xa56d,

    /// <summary><c>Instruction_Owtch_1</c> at $A2:A571.</summary>
    SetMovingRight = 0xa571,
}
