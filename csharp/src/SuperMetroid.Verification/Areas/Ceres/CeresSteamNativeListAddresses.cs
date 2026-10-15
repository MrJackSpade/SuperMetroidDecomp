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
    internal static readonly (CeresSteamProgram Program, ushort Entry)[] Entries =
    [
        (CeresSteamProgram.Up, 0xf04d),
        (CeresSteamProgram.UpLoop, 0xf059),
        (CeresSteamProgram.UpLoop2, 0xf061),
        (CeresSteamProgram.Left, 0xf081),
        (CeresSteamProgram.LeftLoop, 0xf08d),
        (CeresSteamProgram.LeftLoop2, 0xf095),
        (CeresSteamProgram.Down, 0xf0b5),
        (CeresSteamProgram.DownLoop, 0xf0c1),
        (CeresSteamProgram.DownLoop2, 0xf0c9),
        (CeresSteamProgram.Right, 0xf0e9),
        (CeresSteamProgram.RightLoop, 0xf0f5),
        (CeresSteamProgram.RightLoop2, 0xf0fd),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (CeresSteamCueKind Kind, string Operands)> Instructions = new()
    {
        [0xf11d] = (CeresSteamCueKind.Vanish, ""),
        [0xf127] = (CeresSteamCueKind.CountDownToActivation, "TA"),
        [0xf135] = (CeresSteamCueKind.Appear, ""),
    };
}
