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

    /// <summary>
    /// Each crawl's native entry point, in authored order. A surface's selector names an entry before it that only
    /// installs the movement routine the port omits, then leads into the crawl's list (#2061). The cartridge keeps each
    /// spiny crawl twice, the orange Zoomer's copy first, and the port plays one (#2283).
    /// </summary>
    internal static readonly NativeProgramEntry<CrawlerSegment>[] Entries =
    [
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SciserOnLeftWall, 0x967f),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SciserOnRightWall, 0x9697),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SciserOnCeiling, 0x96af),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SciserOnFloor, 0x96c7),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.ZeroOnLeftWall, 0x984f),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.ZeroOnRightWall, 0x988f),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.ZeroOnCeiling, 0x98af),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.ZeroOnFloor, 0x990f),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.ViolaIdle, 0xb5ef),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SpinyCrawlerOnLeftWall, 0xdfcf),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SpinyCrawlerOnRightWall, 0xdfeb),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SpinyCrawlerOnCeiling, 0xe007),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SpinyCrawlerOnFloor, 0xe023),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SpinyCrawlerOnLeftWall, 0xe260),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SpinyCrawlerOnRightWall, 0xe27c),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SpinyCrawlerOnCeiling, 0xe298),
        new NativeProgramEntry<CrawlerSegment>(CrawlerSegment.SpinyCrawlerOnFloor, 0xe2b4),
    ];

    /// <summary>
    /// The owner instructions the lists use, each installing a movement routine from its one operand word: the port's
    /// animations omit them, as the surface a crawler shows sets its crawl (#2022).
    /// </summary>
    internal static readonly HashSet<ushort> OmittedInstructions = [0xdfc2, 0xe660];
}
