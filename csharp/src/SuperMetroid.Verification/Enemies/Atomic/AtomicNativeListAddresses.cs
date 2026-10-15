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
    internal static readonly (AtomicProgram Program, ushort Entry)[] Entries =
    [
        (AtomicProgram.UpRight, 0xe310),
        (AtomicProgram.UpLeft, 0xe32c),
        (AtomicProgram.DownLeft, 0xe348),
        (AtomicProgram.DownRight, 0xe364),
    ];

}
