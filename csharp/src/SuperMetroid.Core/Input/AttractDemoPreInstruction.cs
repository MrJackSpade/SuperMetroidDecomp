namespace SuperMetroid.Core.Input;

/// <summary>The bank-$91 pre-instructions a title-demo input object can hold.</summary>
public enum AttractDemoPreInstruction : ushort
{
    /// <summary>$91:83BF, the return-only routine installed by the demo object definition.</summary>
    NoOp = DemoInputRomData.Routines.NoOp,
    /// <summary>$91:8447, the return-only routine installed when the pre-instruction is cleared.</summary>
    Cleared = DemoInputRomData.Routines.ClearedPreInstruction,
    /// <summary>$91:8A9B, DemoPreInstr_CheckLeaveDemo: delete during game state $2C.</summary>
    CheckLeave = 0x8a9b,
    /// <summary>$91:8AB0, DemoPreInstr_8AB0: redirects unless movement type is $1A.</summary>
    Shinespark = 0x8ab0,
}
