using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Skree;

namespace SuperMetroid.Verification.Enemies.Skree;

/// <summary>
/// The native bank-$A3 lists the Skree or Metaree ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class SkreeMetareeNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<SkreeMetareeSegment>[] Entries =
    [
        new NativeProgramEntry<SkreeMetareeSegment>(SkreeMetareeSegment.MetareeIdling, 0x8910),
        new NativeProgramEntry<SkreeMetareeSegment>(SkreeMetareeSegment.MetareePreparingAttack, 0x8924),
        new NativeProgramEntry<SkreeMetareeSegment>(SkreeMetareeSegment.MetareeDiving, 0x8930),
        new NativeProgramEntry<SkreeMetareeSegment>(SkreeMetareeSegment.MetareeDivingLoop, 0x8932),
        new NativeProgramEntry<SkreeMetareeSegment>(SkreeMetareeSegment.SkreeIdling, 0xc65e),
        new NativeProgramEntry<SkreeMetareeSegment>(SkreeMetareeSegment.SkreePreparingAttack, 0xc672),
        new NativeProgramEntry<SkreeMetareeSegment>(SkreeMetareeSegment.SkreeDiving, 0xc67e),
        new NativeProgramEntry<SkreeMetareeSegment>(SkreeMetareeSegment.SkreeDivingLoop, 0xc680),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, SkreeMetareeNativeListAddressesInstruction> Instructions = new()
    {
        [0x8173] = new SkreeMetareeNativeListAddressesInstruction(SkreeMetareeCueKind.ProcessOffScreen, ""),
        [0x8956] = new SkreeMetareeNativeListAddressesInstruction(SkreeMetareeCueKind.SetAttackReady, ""),
        [0xc6a4] = new SkreeMetareeNativeListAddressesInstruction(SkreeMetareeCueKind.SetAttackReady, ""),
    };
}
