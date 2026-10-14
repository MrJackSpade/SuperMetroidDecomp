namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>Boyon's private bank-$A2 animation instructions.</summary>
internal enum BoyonInstruction : ushort
{
    /// <summary><c>RTL_A288C5</c> at $A2:88C5.</summary>
    Return = 0x88c5,

    /// <summary><c>Instruction_Boyon_88C6</c> at $A2:88C6.</summary>
    StartBounce = 0x88c6,
}
