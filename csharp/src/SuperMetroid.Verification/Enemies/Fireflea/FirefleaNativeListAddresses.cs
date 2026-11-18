using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Fireflea;

namespace SuperMetroid.Verification.Enemies.Fireflea;

/// <summary>
/// The native bank-$A3 lists the Fireflea ROM oracle (#641) walks: each program's entry point.
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class FirefleaNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>
    /// Each typed program's native entry point, in authored order, then where the brightening half of the glow lays out each
    /// glow's wingbeat again (#2477).
    /// </summary>
    internal static readonly NativeProgramEntry<FirefleaSegment>[] Entries =
    [
        new NativeProgramEntry<FirefleaSegment>(FirefleaSegment.BrightestWingbeat, 0x8c2f),
        new NativeProgramEntry<FirefleaSegment>(FirefleaSegment.BrighterWingbeat, 0x8c4f),
        new NativeProgramEntry<FirefleaSegment>(FirefleaSegment.BrightWingbeat, 0x8c5f),
        new NativeProgramEntry<FirefleaSegment>(FirefleaSegment.MidWingbeat, 0x8c6f),
        new NativeProgramEntry<FirefleaSegment>(FirefleaSegment.DimWingbeat, 0x8c7f),
        new NativeProgramEntry<FirefleaSegment>(FirefleaSegment.DimmerWingbeat, 0x8c8f),
        new NativeProgramEntry<FirefleaSegment>(FirefleaSegment.DimmestWingbeat, 0x8c9f),
        new NativeProgramEntry<FirefleaSegment>(FirefleaSegment.DimmerWingbeat, 0x8caf),
        new NativeProgramEntry<FirefleaSegment>(FirefleaSegment.DimWingbeat, 0x8cbf),
        new NativeProgramEntry<FirefleaSegment>(FirefleaSegment.MidWingbeat, 0x8ccf),
        new NativeProgramEntry<FirefleaSegment>(FirefleaSegment.BrightWingbeat, 0x8cdf),
        new NativeProgramEntry<FirefleaSegment>(FirefleaSegment.BrighterWingbeat, 0x8cef),
    ];

}
