using SuperMetroid.Verification.Bosses.MotherBrain;
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
    internal static readonly NativeProgramEntry<PipeBugProgram>[] Entries =
    [
        new NativeProgramEntry<PipeBugProgram>(PipeBugProgram.ZebRisingLeft, 0x87ab),
        new NativeProgramEntry<PipeBugProgram>(PipeBugProgram.ZebShootingLeft, 0x87cf),
        new NativeProgramEntry<PipeBugProgram>(PipeBugProgram.ZebRisingRight, 0x87eb),
        new NativeProgramEntry<PipeBugProgram>(PipeBugProgram.ZebShootingRight, 0x880f),
        new NativeProgramEntry<PipeBugProgram>(PipeBugProgram.ZebboRisingLeft, 0x8a1d),
        new NativeProgramEntry<PipeBugProgram>(PipeBugProgram.ZebboShootingLeft, 0x8a31),
        new NativeProgramEntry<PipeBugProgram>(PipeBugProgram.ZebboRisingRight, 0x8a45),
        new NativeProgramEntry<PipeBugProgram>(PipeBugProgram.ZebboShootingRight, 0x8a59),
        new NativeProgramEntry<PipeBugProgram>(PipeBugProgram.GametRisingLeft, 0x8ae1),
        new NativeProgramEntry<PipeBugProgram>(PipeBugProgram.GametFlyingLeft, 0x8b05),
        new NativeProgramEntry<PipeBugProgram>(PipeBugProgram.GametRisingRight, 0x8b21),
        new NativeProgramEntry<PipeBugProgram>(PipeBugProgram.GametFlyingRight, 0x8b45),
        new NativeProgramEntry<PipeBugProgram>(PipeBugProgram.GeegaFlyingLeft, 0x8efc),
        new NativeProgramEntry<PipeBugProgram>(PipeBugProgram.GeegaArcingLeft, 0x8f10),
        new NativeProgramEntry<PipeBugProgram>(PipeBugProgram.GeegaFlyingRight, 0x8f24),
        new NativeProgramEntry<PipeBugProgram>(PipeBugProgram.GeegaArcingRight, 0x8f38),
    ];

}
