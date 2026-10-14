namespace SuperMetroid.Core.Game;

/// <summary>Yard's private bank-$A3 animation instructions.</summary>
internal enum YardInstruction : ushort
{
    /// <summary><c>Instruction_Yard_MovementFunctionInY</c> at $A3:CC36.</summary>
    MovementFunctionInY = 0xcc36,

    /// <summary><c>Instruction_Yard_HidingInstListInY</c> at $A3:CC3F.</summary>
    HidingInstListInY = 0xcc3f,

    /// <summary><c>Instruction_Yard_DirectionInY</c> at $A3:CC48.</summary>
    DirectionInY = 0xcc48,

    /// <summary><c>Instruction_Yard_MoveByPixelsInY</c> at $A3:CC5F.</summary>
    MoveByPixelsInY = 0xcc5f,

    /// <summary><c>Instruction_Yard_GoBack4BytesIfHidingOr50PercentChance</c> at $A3:CC78.</summary>
    GoBack4BytesIfHidingOr50PercentChance = 0xcc78,
}
