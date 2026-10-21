using SuperMetroid.Verification.Bosses.MotherBrain;
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
    internal static readonly NativeProgramEntry<FuneNamiheProgram>[] Entries =
    [
        new NativeProgramEntry<FuneNamiheProgram>(FuneNamiheProgram.FuneIdleLeft, 0x9399),
        new NativeProgramEntry<FuneNamiheProgram>(FuneNamiheProgram.FuneActiveLeft, 0x939f),
        new NativeProgramEntry<FuneNamiheProgram>(FuneNamiheProgram.FuneIdleRight, 0x93c9),
        new NativeProgramEntry<FuneNamiheProgram>(FuneNamiheProgram.FuneActiveRight, 0x93cf),
        new NativeProgramEntry<FuneNamiheProgram>(FuneNamiheProgram.NamiheIdleLeft, 0x95bd),
        new NativeProgramEntry<FuneNamiheProgram>(FuneNamiheProgram.NamiheActiveLeft, 0x95c3),
        new NativeProgramEntry<FuneNamiheProgram>(FuneNamiheProgram.NamiheIdleRight, 0x95f1),
        new NativeProgramEntry<FuneNamiheProgram>(FuneNamiheProgram.NamiheActiveRight, 0x95f7),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, FuneNamiheNativeListAddressesInstruction> Instructions = new()
    {
        [0x9625] = new FuneNamiheNativeListAddressesInstruction(FuneNamiheCueKind.SpitSound, ""),
        [0x9631] = new FuneNamiheNativeListAddressesInstruction(FuneNamiheCueKind.NamiheFireLeft, ""),
        [0x964a] = new FuneNamiheNativeListAddressesInstruction(FuneNamiheCueKind.NamiheFireRight, ""),
        [0x9663] = new FuneNamiheNativeListAddressesInstruction(FuneNamiheCueKind.FuneFireLeft, ""),
        [0x967c] = new FuneNamiheNativeListAddressesInstruction(FuneNamiheCueKind.FuneFireRight, ""),
        [0x9695] = new FuneNamiheNativeListAddressesInstruction(FuneNamiheCueKind.FinishActivity, ""),
        [0x96b4] = new FuneNamiheNativeListAddressesInstruction(FuneNamiheCueKind.FinishActivity, ""),
    };
}
