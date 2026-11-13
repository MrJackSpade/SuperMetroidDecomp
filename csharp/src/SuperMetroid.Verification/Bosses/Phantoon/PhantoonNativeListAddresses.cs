using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.TestSupport.MovieReplay;

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
    internal static readonly NativeProgramEntry<PhantoonPartSegment>[] PartEntries =
    [
        new(PhantoonPartSegment.InvulnerableBody, 0xcc41),
        new(PhantoonPartSegment.FullHitboxBody, 0xcc47),
        new(PhantoonPartSegment.EyeHitboxBody, 0xcc4d),
        new(PhantoonPartSegment.EyeOpen, 0xcc53),
        new(PhantoonPartSegment.EyeClosed, 0xcc7b),
        new(PhantoonPartSegment.EyeCloseAndPickNewPattern, 0xcc81),
        new(PhantoonPartSegment.EyeClose, 0xcc91),
        new(PhantoonPartSegment.EyeballCentered, 0xcc9d),
        new(PhantoonPartSegment.EyeLookingUp, 0xcca7),
        new(PhantoonPartSegment.EyeLookingUpRight, 0xccad),
        new(PhantoonPartSegment.EyeLookingRight, 0xccb3),
        new(PhantoonPartSegment.EyeLookingDownRight, 0xccb9),
        new(PhantoonPartSegment.EyeLookingDown, 0xccbf),
        new(PhantoonPartSegment.EyeLookingDownLeft, 0xccc5),
        new(PhantoonPartSegment.EyeLookingLeft, 0xcccb),
        new(PhantoonPartSegment.EyeLookingUpLeft, 0xccd1),
        new(PhantoonPartSegment.Tentacles, 0xccd7),
        new(PhantoonPartSegment.MouthSpawnFlame, 0xcceb),
        new(PhantoonPartSegment.MouthInitial, 0xccf7),
    ];

    /// <summary>The bank-$A7 routines Phantoon's part lists call and the cue each must become.</summary>
    internal static readonly Dictionary<ushort, PhantoonPartCue> PartCallbacks = new()
    {
        [0xceed] = PhantoonPartCue.PlayMaterializationSound,
        [0xd03f] = PhantoonPartCue.BeginEyeTracking,
        [0xd076] = PhantoonPartCue.PickNewPattern,
        [0xcf5e] = PhantoonPartCue.SpawnCasualFlame,
    };

    /// <summary>The native entry point of <paramref name="program"/>.</summary>
    internal static ushort PartEntry(PhantoonPartSegment program) => PartEntries.Single(item => item.Program == program).Entry;

    /// <summary>Whether the native part entries ascend, so the typed declaration order is the authored order.</summary>
    internal static bool PartEntriesAscend => PartEntries.Zip(PartEntries.Skip(1), (first, second) => first.Entry < second.Entry).All(ascends => ascends);

    /// <summary>Each typed flame program's native entry point in bank $86.</summary>
    internal static readonly FlameEntry[] FlameEntries =
    [
        new FlameEntry(PhantoonFlameSegment.DestroyableIdle, 0x975c),
        new FlameEntry(PhantoonFlameSegment.CasualHitGround, 0x976c),
        new FlameEntry(PhantoonFlameSegment.CasualBouncing, 0x9772),
        new FlameEntry(PhantoonFlameSegment.CasualResting, 0x9782),
        new FlameEntry(PhantoonFlameSegment.Dying, 0x979a),
        new FlameEntry(PhantoonFlameSegment.RainImpact, 0x97ac),
        new FlameEntry(PhantoonFlameSegment.CasualFalling0, 0x97b4),
        new FlameEntry(PhantoonFlameSegment.StartingFlame, 0x97e8),
        new FlameEntry(PhantoonFlameSegment.Delete, 0x97f8),
        new FlameEntry(PhantoonFlameSegment.DestroyableShot, 0x97fa),
    ];

    /// <summary><c>InstList_EnemyProjectile_PrePhantoonRoom</c> at $86:A3AA.</summary>
    internal const ushort PrePhantoonRoomEntry = 0xa3aa;
}
