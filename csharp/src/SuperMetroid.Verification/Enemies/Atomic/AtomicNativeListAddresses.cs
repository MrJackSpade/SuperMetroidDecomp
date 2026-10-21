using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Atomic;

namespace SuperMetroid.Verification.Enemies.Atomic;

/// <summary>
/// The native bank-$A8 lists the Atomic ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class AtomicNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<AtomicProgram>[] Entries =
    [
        new NativeProgramEntry<AtomicProgram>(AtomicProgram.UpRight, 0xe310),
        new NativeProgramEntry<AtomicProgram>(AtomicProgram.UpLeft, 0xe32c),
        new NativeProgramEntry<AtomicProgram>(AtomicProgram.DownLeft, 0xe348),
        new NativeProgramEntry<AtomicProgram>(AtomicProgram.DownRight, 0xe364),
    ];

}
