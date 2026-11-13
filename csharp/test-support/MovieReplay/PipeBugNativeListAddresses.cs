using SuperMetroid.Core.Enemies.PipeBugs;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$B3 lists the Pipe Bug ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class PipeBugNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xb3;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<PipeBugSegment>[] Entries =
    [
        new NativeProgramEntry<PipeBugSegment>(PipeBugSegment.ZebRisingLeft, 0x87ab),
        new NativeProgramEntry<PipeBugSegment>(PipeBugSegment.ZebShootingLeft, 0x87cf),
        new NativeProgramEntry<PipeBugSegment>(PipeBugSegment.ZebRisingRight, 0x87eb),
        new NativeProgramEntry<PipeBugSegment>(PipeBugSegment.ZebShootingRight, 0x880f),
        new NativeProgramEntry<PipeBugSegment>(PipeBugSegment.ZebboRisingLeft, 0x8a1d),
        new NativeProgramEntry<PipeBugSegment>(PipeBugSegment.ZebboShootingLeft, 0x8a31),
        new NativeProgramEntry<PipeBugSegment>(PipeBugSegment.ZebboRisingRight, 0x8a45),
        new NativeProgramEntry<PipeBugSegment>(PipeBugSegment.ZebboShootingRight, 0x8a59),
        new NativeProgramEntry<PipeBugSegment>(PipeBugSegment.GametRisingLeft, 0x8ae1),
        new NativeProgramEntry<PipeBugSegment>(PipeBugSegment.GametFlyingLeft, 0x8b05),
        new NativeProgramEntry<PipeBugSegment>(PipeBugSegment.GametRisingRight, 0x8b21),
        new NativeProgramEntry<PipeBugSegment>(PipeBugSegment.GametFlyingRight, 0x8b45),
        new NativeProgramEntry<PipeBugSegment>(PipeBugSegment.GeegaFlyingLeft, 0x8efc),
        new NativeProgramEntry<PipeBugSegment>(PipeBugSegment.GeegaArcingLeft, 0x8f10),
        new NativeProgramEntry<PipeBugSegment>(PipeBugSegment.GeegaFlyingRight, 0x8f24),
        new NativeProgramEntry<PipeBugSegment>(PipeBugSegment.GeegaArcingRight, 0x8f38),
    ];

}
