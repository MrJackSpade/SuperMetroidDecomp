namespace SuperMetroid.Core.Rooms.SpriteObjects;

/// <summary>The bank-$B4 room sprite-object instruction opcodes.</summary>
internal enum RoomSpriteObjectInstruction : ushort
{
    /// <summary><c>SpriteObject_Instr_RepeatLast</c> at $B4:BCF0.</summary>
    RepeatLast = 0xbcf0,

    /// <summary><c>SpriteObject_Instr_Terminate</c> at $B4:BD07.</summary>
    Terminate = 0xbd07,

    /// <summary><c>SpriteObject_Instr_Goto</c> at $B4:BD12.</summary>
    Goto = 0xbd12,
}
