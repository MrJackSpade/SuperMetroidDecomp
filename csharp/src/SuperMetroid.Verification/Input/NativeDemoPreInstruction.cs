using SuperMetroid.Core.Input;

namespace SuperMetroid.Verification.Input;

/// <summary>
/// The bank-$91 pre-instruction routines a demo-input object can hold, by their native addresses: the native reference
/// interpreter's identities, and the oracle's translation of a typed <see cref="DemoInputPreInstruction"/>.
/// </summary>
internal enum NativeDemoPreInstruction : ushort
{
    /// <summary>$91:83BF, the return-only routine object definitions install.</summary>
    ReturnOnly = DemoInputRomData.Routines.NoOp,
    /// <summary>$91:8447, the return-only routine a list's clear instruction installs.</summary>
    Cleared = DemoInputRomData.Routines.ClearedPreInstruction,
    /// <summary>$91:8A9B, DemoPreInstr_CheckLeaveDemo: delete during game state $2C.</summary>
    AttractCheckLeave = 0x8a9b,
    /// <summary>$91:8AB0, DemoPreInstr_8AB0: redirects unless movement type is $1A.</summary>
    AttractShinespark = 0x8ab0,
    /// <summary>$91:864F, the Baby-discovery running-left routine.</summary>
    IntroBabyRunningLeft = 0x864f,
    /// <summary>$91:866A, the Baby-discovery stop-and-look routine.</summary>
    IntroBabyStopAndLook = 0x866a,
}
