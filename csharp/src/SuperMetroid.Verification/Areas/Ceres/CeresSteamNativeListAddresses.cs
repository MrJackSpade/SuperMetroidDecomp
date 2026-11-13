using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>
/// The native bank-$A6 lists the Ceres steam ROM oracle (#1290) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class CeresSteamNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa6;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<CeresSteamSegment>[] Entries =
    [
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.Up, 0xf04d),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.UpLoop, 0xf059),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.UpLoop2, 0xf061),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.Left, 0xf081),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.LeftLoop, 0xf08d),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.LeftLoop2, 0xf095),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.Down, 0xf0b5),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.DownLoop, 0xf0c1),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.DownLoop2, 0xf0c9),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.Right, 0xf0e9),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.RightLoop, 0xf0f5),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.RightLoop2, 0xf0fd),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, NativeCueShape<CeresSteamCue>> Instructions = new()
    {
        [0xf11d] = NativeCueShape<CeresSteamCue>.Exactly(new CeresSteamCue.Command(CeresSteamCueKind.Vanish)),
        [0xf127] = NativeCueShape<CeresSteamCue>.Of<CeresSteamCue.CountDownToActivation>("TA"),
        [0xf135] = NativeCueShape<CeresSteamCue>.Exactly(new CeresSteamCue.Command(CeresSteamCueKind.Appear)),
    };
}
