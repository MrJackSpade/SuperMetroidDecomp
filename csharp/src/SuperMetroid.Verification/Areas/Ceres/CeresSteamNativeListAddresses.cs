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

    /// <summary>Each way a vent blows, with its typed programs' native entry points in authored order.</summary>
    internal static readonly (string Name, CeresSteamWay Way, NativeProgramEntry<CeresSteamSegment>[] Entries)[] Ways =
    [
        ("up", CeresSteamWay.Up, Programs(0xf04d, 0xf059, 0xf061)),
        ("left", CeresSteamWay.Left, Programs(0xf081, 0xf08d, 0xf095)),
        ("down", CeresSteamWay.Down, Programs(0xf0b5, 0xf0c1, 0xf0c9)),
        ("right", CeresSteamWay.Right, Programs(0xf0e9, 0xf0f5, 0xf0fd)),
    ];

    /// <summary>
    /// The rests between puffs, which no owner starts and no cue branches to: a dormant or puffing vent's animation only
    /// runs on into them (#2096).
    /// </summary>
    internal static readonly IReadOnlySet<CeresSteamSegment> OnlyRunOnInto = new HashSet<CeresSteamSegment> { CeresSteamSegment.Resting };

    /// <summary>The native entry points of the way whose vent animation is <paramref name="way"/>.</summary>
    internal static NativeProgramEntry<CeresSteamSegment>[] EntriesOf(CeresSteamWay way) => Ways.Single(listed => listed.Way == way).Entries;

    /// <summary>One way's dormant, resting and puffing programs' entry points.</summary>
    private static NativeProgramEntry<CeresSteamSegment>[] Programs(ushort dormant, ushort resting, ushort puffing) =>
    [
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.Dormant, dormant),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.Resting, resting),
        new NativeProgramEntry<CeresSteamSegment>(CeresSteamSegment.Puffing, puffing),
    ];

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
