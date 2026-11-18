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
        // The idle's list shows one tucked wingbeat and runs on into the post-swoop list, which writes the same wingbeat out
        // three times and then its hanging wingbeat twice (#2451, #2512).
        new NativeProgramEntry<RioSegment>(RioSegment.TuckedWingbeat, 0xbb4b),
        new NativeProgramEntry<RioSegment>(RioSegment.TuckedWingbeat, PostSwoopHoverEntry),
        new NativeProgramEntry<RioSegment>(RioSegment.HangingWingbeat, 0xbb6b),
        new NativeProgramEntry<RioSegment>(RioSegment.SwoopOpening, 0xbb7f),
        new NativeProgramEntry<RioSegment>(RioSegment.SwoopLoop, 0xbb97),
        new NativeProgramEntry<RioSegment>(RioSegment.SwoopCooldown, 0xbba3),
    ];

    /// <summary>The post-swoop list's entry, where the hover a swoop ends in starts (#2512).</summary>
    internal const ushort PostSwoopHoverEntry = 0xbb53;

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, RioNativeListAddressesInstruction> Instructions = new()
    {
        [0xbbc3] = new RioNativeListAddressesInstruction(RioCueKind.AnimationFinished, ""),
    };
}
