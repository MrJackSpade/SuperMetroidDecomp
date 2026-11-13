using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Shutters;

namespace SuperMetroid.Verification.Enemies.Shutters;

/// <summary>
/// The native bank-$A2 lists the shutter ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class ShutterNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<ShutterSegment>[] Entries =
    [
        new NativeProgramEntry<ShutterSegment>(ShutterSegment.GrowingTenPixels, 0xe998),
        new NativeProgramEntry<ShutterSegment>(ShutterSegment.GrowingTwentyPixels, 0xe99e),
        new NativeProgramEntry<ShutterSegment>(ShutterSegment.GrowingThirtyPixels, 0xe9a4),
        new NativeProgramEntry<ShutterSegment>(ShutterSegment.FortyPixels, 0xe9aa),
        new NativeProgramEntry<ShutterSegment>(ShutterSegment.KamerPlatform, 0xede7),
    ];

}
