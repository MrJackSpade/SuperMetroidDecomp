using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Verification.Enemies;

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
    internal static readonly NativeProgramEntry<CeresSteamProgram>[] Entries =
    [
        new NativeProgramEntry<CeresSteamProgram>(CeresSteamProgram.Up, 0xf04d),
        new NativeProgramEntry<CeresSteamProgram>(CeresSteamProgram.UpLoop, 0xf059),
        new NativeProgramEntry<CeresSteamProgram>(CeresSteamProgram.UpLoop2, 0xf061),
        new NativeProgramEntry<CeresSteamProgram>(CeresSteamProgram.Left, 0xf081),
        new NativeProgramEntry<CeresSteamProgram>(CeresSteamProgram.LeftLoop, 0xf08d),
        new NativeProgramEntry<CeresSteamProgram>(CeresSteamProgram.LeftLoop2, 0xf095),
        new NativeProgramEntry<CeresSteamProgram>(CeresSteamProgram.Down, 0xf0b5),
        new NativeProgramEntry<CeresSteamProgram>(CeresSteamProgram.DownLoop, 0xf0c1),
        new NativeProgramEntry<CeresSteamProgram>(CeresSteamProgram.DownLoop2, 0xf0c9),
        new NativeProgramEntry<CeresSteamProgram>(CeresSteamProgram.Right, 0xf0e9),
        new NativeProgramEntry<CeresSteamProgram>(CeresSteamProgram.RightLoop, 0xf0f5),
        new NativeProgramEntry<CeresSteamProgram>(CeresSteamProgram.RightLoop2, 0xf0fd),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, NativeCueShape<CeresSteamCue>> Instructions = new()
    {
        [0xf11d] = NativeCueShape<CeresSteamCue>.Exactly(new CeresSteamCue.Command(CeresSteamCueKind.Vanish)),
        [0xf127] = NativeCueShape<CeresSteamCue>.Of<CeresSteamCue.CountDownToActivation>("TA"),
        [0xf135] = NativeCueShape<CeresSteamCue>.Exactly(new CeresSteamCue.Command(CeresSteamCueKind.Appear)),
    };
}
