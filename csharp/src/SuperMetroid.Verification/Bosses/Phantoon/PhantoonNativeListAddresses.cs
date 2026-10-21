using SuperMetroid.Core.Bosses.Phantoon;

namespace SuperMetroid.Verification.Bosses.Phantoon;

/// <summary>
/// The native bank-$A7 and bank-$86 lists the Phantoon ROM oracle (#1286) walks: the part and flame list entry points,
/// and their callbacks with the cue each must become. Only verification reads them; the port runs typed programs.
/// </summary>
internal static class PhantoonNativeListAddresses
{
    /// <summary>Phantoon's instruction bank, $A7.</summary>
    internal const byte PartBankNumber = 0xa7;

    /// <summary><c>Instruction_CommonA7_CallFunctionInY</c> at $A7:808A: Phantoon's part callbacks name their routine in its operand.</summary>
    internal const ushort CallFunctionInY = 0x808a;

    /// <summary>Each typed part program's native entry point in bank $A7, in authored order.</summary>
    internal static readonly PartEntryValue2[] PartEntries =
    [
        new PartEntryValue2(PhantoonPartProgram.InvulnerableBody, 0xcc41),
        new PartEntryValue2(PhantoonPartProgram.FullHitboxBody, 0xcc47),
        new PartEntryValue2(PhantoonPartProgram.EyeHitboxBody, 0xcc4d),
        new PartEntryValue2(PhantoonPartProgram.EyeOpen, 0xcc53),
        new PartEntryValue2(PhantoonPartProgram.EyeClosed, 0xcc7b),
        new PartEntryValue2(PhantoonPartProgram.EyeCloseAndPickNewPattern, 0xcc81),
        new PartEntryValue2(PhantoonPartProgram.EyeClose, 0xcc91),
        new PartEntryValue2(PhantoonPartProgram.EyeballCentered, 0xcc9d),
        new PartEntryValue2(PhantoonPartProgram.EyeLookingUp, 0xcca7),
        new PartEntryValue2(PhantoonPartProgram.EyeLookingUpRight, 0xccad),
        new PartEntryValue2(PhantoonPartProgram.EyeLookingRight, 0xccb3),
        new PartEntryValue2(PhantoonPartProgram.EyeLookingDownRight, 0xccb9),
        new PartEntryValue2(PhantoonPartProgram.EyeLookingDown, 0xccbf),
        new PartEntryValue2(PhantoonPartProgram.EyeLookingDownLeft, 0xccc5),
        new PartEntryValue2(PhantoonPartProgram.EyeLookingLeft, 0xcccb),
        new PartEntryValue2(PhantoonPartProgram.EyeLookingUpLeft, 0xccd1),
        new PartEntryValue2(PhantoonPartProgram.Tentacles, 0xccd7),
        new PartEntryValue2(PhantoonPartProgram.MouthSpawnFlame, 0xcceb),
        new PartEntryValue2(PhantoonPartProgram.MouthInitial, 0xccf7),
    ];

    /// <summary>The bank-$A7 routines Phantoon's part lists call and the cue each must become.</summary>
    internal static readonly Dictionary<ushort, PhantoonPartCue> PartCallbacks = new()
    {
        [0xceed] = PhantoonPartCue.PlayMaterializationSound,
        [0xd03f] = PhantoonPartCue.BeginEyeTracking,
        [0xd076] = PhantoonPartCue.PickNewPattern,
        [0xcf5e] = PhantoonPartCue.SpawnCasualFlame,
    };

    /// <summary>The part program whose native list starts at <paramref name="address"/>, if one does.</summary>
    internal static PhantoonPartProgram? PartProgramAt(ushort address) =>
        PartEntries.Where(item => item.Entry == address).Select(item => (PhantoonPartProgram?)item.Program).SingleOrDefault();

    /// <summary>The native entry point of <paramref name="program"/>.</summary>
    internal static ushort PartEntry(PhantoonPartProgram program) => PartEntries.Single(item => item.Program == program).Entry;

    /// <summary>Whether the native part entries ascend, so the typed declaration order is the authored order.</summary>
    internal static bool PartEntriesAscend => PartEntries.Zip(PartEntries.Skip(1), (first, second) => first.Entry < second.Entry).All(ascends => ascends);

    /// <summary>Each typed flame program's native entry point in bank $86.</summary>
    internal static readonly FlameEntry[] FlameEntries =
    [
        new FlameEntry(PhantoonFlameProgram.DestroyableIdle, 0x975c),
        new FlameEntry(PhantoonFlameProgram.CasualHitGround, 0x976c),
        new FlameEntry(PhantoonFlameProgram.CasualBouncing, 0x9772),
        new FlameEntry(PhantoonFlameProgram.CasualResting, 0x9782),
        new FlameEntry(PhantoonFlameProgram.Dying, 0x979a),
        new FlameEntry(PhantoonFlameProgram.RainImpact, 0x97ac),
        new FlameEntry(PhantoonFlameProgram.CasualFalling0, 0x97b4),
        new FlameEntry(PhantoonFlameProgram.StartingFlame, 0x97e8),
        new FlameEntry(PhantoonFlameProgram.Delete, 0x97f8),
        new FlameEntry(PhantoonFlameProgram.DestroyableShot, 0x97fa),
    ];

    /// <summary><c>InstList_EnemyProjectile_PrePhantoonRoom</c> at $86:A3AA.</summary>
    internal const ushort PrePhantoonRoomEntry = 0xa3aa;
}
