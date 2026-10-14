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
    internal static readonly (CrawlerProgram Program, ushort Entry)[] Entries =
    [
        (CrawlerProgram.SciserUpsideRight, 0x967b),
        (CrawlerProgram.SciserUpsideRightLoop, 0x967f),
        (CrawlerProgram.SciserUpsideLeft, 0x9693),
        (CrawlerProgram.SciserUpsideLeftLoop, 0x9697),
        (CrawlerProgram.SciserUpsideDown, 0x96ab),
        (CrawlerProgram.SciserUpsideDownLoop, 0x96af),
        (CrawlerProgram.SciserUpsideUp, 0x96c3),
        (CrawlerProgram.SciserUpsideUpLoop, 0x96c7),
        (CrawlerProgram.ZeroUpsideRight, 0x984b),
        (CrawlerProgram.ZeroUpsideRightLoop, 0x984f),
        (CrawlerProgram.ZeroUpsideLeft, 0x988b),
        (CrawlerProgram.ZeroUpsideLeftLoop, 0x988f),
        (CrawlerProgram.ZeroUpsideDown, 0x98ab),
        (CrawlerProgram.ZeroUpsideDownLoop, 0x98af),
        (CrawlerProgram.ZeroUpsideUp, 0x990b),
        (CrawlerProgram.ZeroUpsideUpLoop, 0x990f),
        (CrawlerProgram.ViolaUpsideDown, 0xb5d3),
        (CrawlerProgram.ViolaUpsideUp, 0xb5db),
        (CrawlerProgram.ViolaUpsideRight, 0xb5e3),
        (CrawlerProgram.ViolaUpsideLeft, 0xb5eb),
        (CrawlerProgram.ViolaNormal, 0xb5ef),
        (CrawlerProgram.HZoomerUpsideRight, 0xdfcb),
        (CrawlerProgram.HZoomerUpsideRightLoop, 0xdfcf),
        (CrawlerProgram.HZoomerUpsideLeft, 0xdfe7),
        (CrawlerProgram.HZoomerUpsideLeftLoop, 0xdfeb),
        (CrawlerProgram.HZoomerUpsideDown, 0xe003),
        (CrawlerProgram.HZoomerUpsideDownLoop, 0xe007),
        (CrawlerProgram.HZoomerUpsideUp, 0xe01f),
        (CrawlerProgram.HZoomerUpsideUpLoop, 0xe023),
        (CrawlerProgram.SharedUpsideRight, 0xe25c),
        (CrawlerProgram.SharedUpsideRightLoop, 0xe260),
        (CrawlerProgram.SharedUpsideLeft, 0xe278),
        (CrawlerProgram.SharedUpsideLeftLoop, 0xe27c),
        (CrawlerProgram.SharedUpsideDown, 0xe294),
        (CrawlerProgram.SharedUpsideDownLoop, 0xe298),
        (CrawlerProgram.SharedUpsideUp, 0xe2b0),
        (CrawlerProgram.SharedUpsideUpLoop, 0xe2b4),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (CrawlerCueKind Kind, string Operands)> Instructions = new()
    {
        [0xdfc2] = (CrawlerCueKind.SetFunction, "E"),
        [0xe660] = (CrawlerCueKind.SetFunction, "E"),
    };
}
