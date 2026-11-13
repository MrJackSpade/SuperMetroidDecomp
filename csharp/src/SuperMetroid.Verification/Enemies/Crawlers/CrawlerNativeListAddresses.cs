using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>
/// The native bank-$A3 lists the crawler ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class CrawlerNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<CrawlerSegment>[] Entries =
    [
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SciserUpsideRight, 0x967b),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SciserUpsideRightLoop, 0x967f),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SciserUpsideLeft, 0x9693),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SciserUpsideLeftLoop, 0x9697),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SciserUpsideDown, 0x96ab),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SciserUpsideDownLoop, 0x96af),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SciserUpsideUp, 0x96c3),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SciserUpsideUpLoop, 0x96c7),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.ZeroUpsideRight, 0x984b),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.ZeroUpsideRightLoop, 0x984f),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.ZeroUpsideLeft, 0x988b),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.ZeroUpsideLeftLoop, 0x988f),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.ZeroUpsideDown, 0x98ab),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.ZeroUpsideDownLoop, 0x98af),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.ZeroUpsideUp, 0x990b),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.ZeroUpsideUpLoop, 0x990f),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.ViolaUpsideDown, 0xb5d3),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.ViolaUpsideUp, 0xb5db),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.ViolaUpsideRight, 0xb5e3),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.ViolaUpsideLeft, 0xb5eb),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.ViolaNormal, 0xb5ef),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.HZoomerUpsideRight, 0xdfcb),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.HZoomerUpsideRightLoop, 0xdfcf),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.HZoomerUpsideLeft, 0xdfe7),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.HZoomerUpsideLeftLoop, 0xdfeb),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.HZoomerUpsideDown, 0xe003),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.HZoomerUpsideDownLoop, 0xe007),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.HZoomerUpsideUp, 0xe01f),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.HZoomerUpsideUpLoop, 0xe023),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SharedUpsideRight, 0xe25c),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SharedUpsideRightLoop, 0xe260),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SharedUpsideLeft, 0xe278),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SharedUpsideLeftLoop, 0xe27c),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SharedUpsideDown, 0xe294),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SharedUpsideDownLoop, 0xe298),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SharedUpsideUp, 0xe2b0),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SharedUpsideUpLoop, 0xe2b4),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, CrawlerNativeListAddressesInstruction> Instructions = new()
    {
        [0xdfc2] = new CrawlerNativeListAddressesInstruction(CrawlerCueKind.SetFunction, "E"),
        [0xe660] = new CrawlerNativeListAddressesInstruction(CrawlerCueKind.SetFunction, "E"),
    };
}
