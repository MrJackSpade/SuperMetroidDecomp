using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>
/// The native bank-$A6 lists the Ceres steam ROM oracle (#1290) walks: each program's entry point, the owner instructions
/// the lists use with the cue each must become, and the records the typed segments read differently (#2480): the dormant
/// lists' count-downs, which the vents' owner holds as waits, and the rests' hidden frame records, which are pauses.
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

    /// <summary>
    /// The rests between puffs, which no owner starts and no cue branches to: a dormant or puffing vent's animation only
    /// runs on into them (#2096).
    /// </summary>
    internal static readonly IReadOnlySet<CeresSteamSegment> OnlyRunOnInto = new HashSet<CeresSteamSegment>
    {
        CeresSteamSegment.UpResting,
        CeresSteamSegment.LeftResting,
        CeresSteamSegment.DownResting,
        CeresSteamSegment.RightResting,
    };

    /// <summary>
    /// The frame record each dormant list shows after hiding its vent (#2480): the port's owner takes the vent over there,
    /// holding it hidden for its drawn wait, so the dormant segment ends holding and the record and the count-down after it
    /// are read on their own rather than beat by beat.
    /// </summary>
    internal static readonly IReadOnlySet<ushort> OwnerTakesOver = new HashSet<ushort> { 0xf04f, 0xf083, 0xf0b7, 0xf0eb };

    /// <summary>The frame record each rest shows while its vent is hidden (#2480), which the typed rest waits as a pause.</summary>
    internal static readonly IReadOnlySet<ushort> UnseenFrames = new HashSet<ushort> { 0xf05b, 0xf08f, 0xf0c3, 0xf0f7 };

    /// <summary>The routine that counts a dormant vent's wait down, replaying its list until it runs out.</summary>
    internal const ushort CountDownRoutine = 0xf127;

    /// <summary>Each owner instruction the segments' cues stand for, keyed by its native routine; neither takes an operand.</summary>
    internal static readonly Dictionary<ushort, CeresSteamCueKind> Instructions = new()
    {
        [0xf11d] = CeresSteamCueKind.Vanish,
        [0xf135] = CeresSteamCueKind.Appear,
    };
}
