using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>
/// The native bank-$A9 lists the dead Tourian enemy ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class DeadTourianNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa9;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<DeadTourianProgram>[] Entries =
    [
        new NativeProgramEntry<DeadTourianProgram>(DeadTourianProgram.SidehopperHopping, 0xecac),
        new NativeProgramEntry<DeadTourianProgram>(DeadTourianProgram.SidehopperIdle, 0xece3),
        new NativeProgramEntry<DeadTourianProgram>(DeadTourianProgram.SidehopperCorpse, 0xece9),
        new NativeProgramEntry<DeadTourianProgram>(DeadTourianProgram.SidehopperInitiallyDead, 0xecef),
        new NativeProgramEntry<DeadTourianProgram>(DeadTourianProgram.ZoomerCorpse0, 0xecf5),
        new NativeProgramEntry<DeadTourianProgram>(DeadTourianProgram.ZoomerCorpse1, 0xecfb),
        new NativeProgramEntry<DeadTourianProgram>(DeadTourianProgram.ZoomerCorpse2, 0xed01),
        new NativeProgramEntry<DeadTourianProgram>(DeadTourianProgram.RipperCorpse0, 0xed07),
        new NativeProgramEntry<DeadTourianProgram>(DeadTourianProgram.RipperCorpse1, 0xed0d),
        new NativeProgramEntry<DeadTourianProgram>(DeadTourianProgram.SkreeCorpse0, 0xed13),
        new NativeProgramEntry<DeadTourianProgram>(DeadTourianProgram.SkreeCorpse1, 0xed19),
        new NativeProgramEntry<DeadTourianProgram>(DeadTourianProgram.SkreeCorpse2, 0xed1f),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, DeadTourianNativeListAddressesInstruction> Instructions = new()
    {
        [0xecd0] = new DeadTourianNativeListAddressesInstruction(DeadTourianCueKind.EndHop, ""),
    };
}
