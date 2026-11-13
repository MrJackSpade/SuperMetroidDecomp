using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Rios;

namespace SuperMetroid.Verification.Enemies.Rios;

/// <summary>
/// The native bank-$A2 lists the Rio ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class RioNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<RioSegment>[] Entries =
    [
        new NativeProgramEntry<RioSegment>(RioSegment.Idle, 0xbb4b),
        new NativeProgramEntry<RioSegment>(RioSegment.PostSwoopIdle, 0xbb53),
        new NativeProgramEntry<RioSegment>(RioSegment.SwoopingPart1, 0xbb7f),
        new NativeProgramEntry<RioSegment>(RioSegment.SwoopingPart2, 0xbb97),
        new NativeProgramEntry<RioSegment>(RioSegment.SwoopCooldown, 0xbba3),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, RioNativeListAddressesInstruction> Instructions = new()
    {
        [0xbbc3] = new RioNativeListAddressesInstruction(RioCueKind.AnimationFinished, ""),
    };
}
