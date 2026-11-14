using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>
/// The native bank-$A6 lists the Ceres steam ROM oracle (#1290) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T the dormant wait replayed, A the puff played on expiry).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class CeresSteamNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa6;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<CeresSteamSegment>[] Entries =
    [
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.UpDormant, 0xf04d),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.UpResting, 0xf059),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.UpPuffing, 0xf061),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.LeftDormant, 0xf081),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.LeftResting, 0xf08d),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.LeftPuffing, 0xf095),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.DownDormant, 0xf0b5),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.DownResting, 0xf0c1),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.DownPuffing, 0xf0c9),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.RightDormant, 0xf0e9),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.RightResting, 0xf0f5),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.RightPuffing, 0xf0fd),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, NativeCueShape<CeresSteamCue>> Instructions = new()
    {
        [0xf11d] = NativeCueShape<CeresSteamCue>.Exactly(new CeresSteamCue.Signal(CeresSteamCueKind.Vanish)),
        [0xf127] = NativeCueShape<CeresSteamCue>.Of<CeresSteamCue.CountDownToActivation>("TA"),
        [0xf135] = NativeCueShape<CeresSteamCue>.Exactly(new CeresSteamCue.Signal(CeresSteamCueKind.Appear)),
    };
}
