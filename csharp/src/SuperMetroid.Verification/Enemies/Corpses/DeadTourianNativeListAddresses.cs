using SuperMetroid.TestSupport.MovieReplay;
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

    /// <summary>
    /// Each typed program's native entry point, in authored order: a corpse pose's is the list of the first variant that
    /// lies so (#1810); <see cref="DeadTourianCorpseDefinitionsVerification"/> compares every variant's list by the pose it
    /// shows.
    /// </summary>
    internal static readonly NativeProgramEntry<DeadTourianSegment>[] Entries =
    [
        new NativeProgramEntry<DeadTourianSegment>(DeadTourianSegment.SidehopperHopping, 0xecac),
        new NativeProgramEntry<DeadTourianSegment>(DeadTourianSegment.SidehopperIdle, 0xece3),
        new NativeProgramEntry<DeadTourianSegment>(DeadTourianSegment.SidehopperCorpse, 0xece9),
        new NativeProgramEntry<DeadTourianSegment>(DeadTourianSegment.SidehopperInitiallyDead, 0xecef),
        new NativeProgramEntry<DeadTourianSegment>(DeadTourianSegment.ZoomerCorpseOnItsBack, 0xecf5),
        new NativeProgramEntry<DeadTourianSegment>(DeadTourianSegment.ZoomerCorpseUpright, 0xecfb),
        new NativeProgramEntry<DeadTourianSegment>(DeadTourianSegment.RipperCorpseLevel, 0xed07),
        new NativeProgramEntry<DeadTourianSegment>(DeadTourianSegment.RipperCorpseTilted, 0xed0d),
        new NativeProgramEntry<DeadTourianSegment>(DeadTourianSegment.SkreeCorpse, 0xed13),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, DeadTourianNativeListAddressesInstruction> Instructions = new()
    {
        [0xecd0] = new DeadTourianNativeListAddressesInstruction(DeadTourianCueKind.EndHop, ""),
    };
}
