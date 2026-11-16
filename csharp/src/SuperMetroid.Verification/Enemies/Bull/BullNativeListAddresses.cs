using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Bull;

namespace SuperMetroid.Verification.Enemies.Bull;

/// <summary>
/// The native bank-$A8 lists the Bull ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class BullNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<BullSegment>[] Entries =
    [
        new NativeProgramEntry<BullSegment>(BullSegment.Normal, 0xd841),
        new NativeProgramEntry<BullSegment>(BullSegment.Shot, 0xd859),
    ];

    /// <summary>The native entry the shot reaction starts at (#2061): it loads the quick cycle's count and runs on into it.</summary>
    internal const ushort ShotStart = 0xd855;

}
