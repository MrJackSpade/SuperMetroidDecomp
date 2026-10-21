using SuperMetroid.Verification.Bosses.MotherBrain;
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
    internal static readonly NativeProgramEntry<CrawlerProgram>[] Entries =
    [
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.SciserUpsideRight, 0x967b),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.SciserUpsideRightLoop, 0x967f),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.SciserUpsideLeft, 0x9693),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.SciserUpsideLeftLoop, 0x9697),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.SciserUpsideDown, 0x96ab),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.SciserUpsideDownLoop, 0x96af),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.SciserUpsideUp, 0x96c3),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.SciserUpsideUpLoop, 0x96c7),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.ZeroUpsideRight, 0x984b),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.ZeroUpsideRightLoop, 0x984f),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.ZeroUpsideLeft, 0x988b),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.ZeroUpsideLeftLoop, 0x988f),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.ZeroUpsideDown, 0x98ab),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.ZeroUpsideDownLoop, 0x98af),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.ZeroUpsideUp, 0x990b),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.ZeroUpsideUpLoop, 0x990f),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.ViolaUpsideDown, 0xb5d3),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.ViolaUpsideUp, 0xb5db),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.ViolaUpsideRight, 0xb5e3),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.ViolaUpsideLeft, 0xb5eb),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.ViolaNormal, 0xb5ef),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.HZoomerUpsideRight, 0xdfcb),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.HZoomerUpsideRightLoop, 0xdfcf),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.HZoomerUpsideLeft, 0xdfe7),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.HZoomerUpsideLeftLoop, 0xdfeb),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.HZoomerUpsideDown, 0xe003),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.HZoomerUpsideDownLoop, 0xe007),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.HZoomerUpsideUp, 0xe01f),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.HZoomerUpsideUpLoop, 0xe023),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.SharedUpsideRight, 0xe25c),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.SharedUpsideRightLoop, 0xe260),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.SharedUpsideLeft, 0xe278),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.SharedUpsideLeftLoop, 0xe27c),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.SharedUpsideDown, 0xe294),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.SharedUpsideDownLoop, 0xe298),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.SharedUpsideUp, 0xe2b0),
        new NativeProgramEntry<CrawlerProgram>(CrawlerProgram.SharedUpsideUpLoop, 0xe2b4),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, CrawlerNativeListAddressesInstruction> Instructions = new()
    {
        [0xdfc2] = new CrawlerNativeListAddressesInstruction(CrawlerCueKind.SetFunction, "E"),
        [0xe660] = new CrawlerNativeListAddressesInstruction(CrawlerCueKind.SetFunction, "E"),
    };
}
