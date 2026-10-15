using SuperMetroid.Core.Enemies.FuneNamihe;

namespace SuperMetroid.Verification.Enemies.FuneNamihe;

/// <summary>
/// The native bank-$A8 lists the Fune or Namihe ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class FuneNamiheNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa8;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (FuneNamiheProgram Program, ushort Entry)[] Entries =
    [
        (FuneNamiheProgram.FuneIdleLeft, 0x9399),
        (FuneNamiheProgram.FuneActiveLeft, 0x939f),
        (FuneNamiheProgram.FuneIdleRight, 0x93c9),
        (FuneNamiheProgram.FuneActiveRight, 0x93cf),
        (FuneNamiheProgram.NamiheIdleLeft, 0x95bd),
        (FuneNamiheProgram.NamiheActiveLeft, 0x95c3),
        (FuneNamiheProgram.NamiheIdleRight, 0x95f1),
        (FuneNamiheProgram.NamiheActiveRight, 0x95f7),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (FuneNamiheCueKind Kind, string Operands)> Instructions = new()
    {
        [0x9625] = (FuneNamiheCueKind.SpitSound, ""),
        [0x9631] = (FuneNamiheCueKind.NamiheFireLeft, ""),
        [0x964a] = (FuneNamiheCueKind.NamiheFireRight, ""),
        [0x9663] = (FuneNamiheCueKind.FuneFireLeft, ""),
        [0x967c] = (FuneNamiheCueKind.FuneFireRight, ""),
        [0x9695] = (FuneNamiheCueKind.FinishActivity, ""),
        [0x96b4] = (FuneNamiheCueKind.FinishActivity, ""),
    };
}
