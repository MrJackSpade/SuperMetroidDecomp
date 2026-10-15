using SuperMetroid.Core.Enemies.PipeBugs;

namespace SuperMetroid.Verification.Enemies.PipeBugs;

/// <summary>
/// The native bank-$B3 lists the Pipe Bug ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class PipeBugNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xb3;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (PipeBugProgram Program, ushort Entry)[] Entries =
    [
        (PipeBugProgram.ZebRisingLeft, 0x87ab),
        (PipeBugProgram.ZebShootingLeft, 0x87cf),
        (PipeBugProgram.ZebRisingRight, 0x87eb),
        (PipeBugProgram.ZebShootingRight, 0x880f),
        (PipeBugProgram.ZebboRisingLeft, 0x8a1d),
        (PipeBugProgram.ZebboShootingLeft, 0x8a31),
        (PipeBugProgram.ZebboRisingRight, 0x8a45),
        (PipeBugProgram.ZebboShootingRight, 0x8a59),
        (PipeBugProgram.GametRisingLeft, 0x8ae1),
        (PipeBugProgram.GametFlyingLeft, 0x8b05),
        (PipeBugProgram.GametRisingRight, 0x8b21),
        (PipeBugProgram.GametFlyingRight, 0x8b45),
        (PipeBugProgram.GeegaFlyingLeft, 0x8efc),
        (PipeBugProgram.GeegaArcingLeft, 0x8f10),
        (PipeBugProgram.GeegaFlyingRight, 0x8f24),
        (PipeBugProgram.GeegaArcingRight, 0x8f38),
    ];

}
