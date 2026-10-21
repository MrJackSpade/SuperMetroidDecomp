using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Cacatac;

namespace SuperMetroid.Verification.Enemies.Cacatac;

/// <summary>
/// The native bank-$A2 lists the Cacatac ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class CacatacNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<CacatacProgram>[] Entries =
    [
        new NativeProgramEntry<CacatacProgram>(CacatacProgram.UpsideUpIdle, 0x9e8a),
        new NativeProgramEntry<CacatacProgram>(CacatacProgram.UpsideUpAttack, 0x9eb0),
        new NativeProgramEntry<CacatacProgram>(CacatacProgram.UpsideDownIdle, 0x9eda),
        new NativeProgramEntry<CacatacProgram>(CacatacProgram.UpsideDownIdleLoop, 0x9edc),
        new NativeProgramEntry<CacatacProgram>(CacatacProgram.UpsideDownAttack, 0x9f00),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, CacatacNativeListAddressesInstruction> Instructions = new()
    {
        [0x9f2a] = new CacatacNativeListAddressesInstruction(CacatacCueKind.PlaySpikeSound, ""),
        [0xa095] = new CacatacNativeListAddressesInstruction(CacatacCueKind.RestorePatrol, ""),
        [0xa0a7] = new CacatacNativeListAddressesInstruction(CacatacCueKind.SpawnSpike, "V"),
    };
}
