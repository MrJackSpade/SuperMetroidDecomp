using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.SpriteObjects;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>
/// The native bank-$A5 and bank-$86 lists the Draygon ROM oracle (#1283) walks: list entry points, callbacks and
/// their operands, and the cue each callback must become. Only verification reads them; the port runs typed programs.
/// </summary>
internal static class DraygonNativeListAddresses
{
    /// <summary>Draygon's instruction bank, $A5.</summary>
    internal const byte BodyBankNumber = 0xa5;
    /// <summary>The bank-$86 projectile instruction lists, as a long-address base.</summary>
    internal const int ProjectileBank = 0x860000;

    /// <summary>Each typed program's native entry point in bank $A5, in authored order.</summary>
    internal static readonly (DraygonProgram Program, ushort Entry)[] Entries =
    [
        (DraygonProgram.Sleep, 0x97b9),
        (DraygonProgram.BodyFacingLeftReset, 0x97bb),
        (DraygonProgram.BodyFacingRightReset, 0x97d1),
        (DraygonProgram.ArmsFacingLeftIdle, 0x97e7),
        (DraygonProgram.ArmsFacingLeftNearSwoopApex, 0x9813),
        (DraygonProgram.ArmsFacingLeftGrab, 0x9845),
        (DraygonProgram.ArmsFacingLeftDying, 0x9867),
        (DraygonProgram.BodyFacingLeftIdle, 0x9889),
        (DraygonProgram.BodyDying, 0x989b),
        (DraygonProgram.BodyDyingExplosions, 0x98a5),
        (DraygonProgram.BodyDyingExplosionsLoop, 0x98bf),
        (DraygonProgram.Delete, 0x98ed),
        (DraygonProgram.BodyFacingLeftFireGoop, 0x98fe),
        (DraygonProgram.BodyFacingLeftRoar, 0x9922),
        (DraygonProgram.EyeFacingLeftIdle, 0x9944),
        (DraygonProgram.EyeFacingLeftDying, 0x997a),
        (DraygonProgram.EyeFacingLeftDyingLoop, 0x997e),
        (DraygonProgram.EyeFacingLeftDead, 0x999c),
        (DraygonProgram.EyeFacingLeftLookingLeft, 0x99ae),
        (DraygonProgram.EyeFacingLeftLookingRight, 0x99b4),
        (DraygonProgram.EyeFacingLeftLookingUp, 0x99ba),
        (DraygonProgram.EyeFacingLeftLookingDown, 0x99c0),
        (DraygonProgram.TailFacingLeftIdle, 0x99c6),
        (DraygonProgram.TailFacingLeftInitialFakeWhip, 0x99fc),
        (DraygonProgram.TailFacingLeftFinalWhips, 0x9a68),
        (DraygonProgram.TailFacingLeftFinalWhipsLoop, 0x9a6c),
        (DraygonProgram.TailFacingLeftWhip, 0x9ae8),
        (DraygonProgram.TailFacingLeftFlail, 0x9b5a),
        (DraygonProgram.ArmsFacingRightIdle, 0x9bda),
        (DraygonProgram.ArmsFacingRightNearSwoopApex, 0x9c06),
        (DraygonProgram.ArmsFacingRightGrab, 0x9c38),
        (DraygonProgram.ArmsFacingRightDying, 0x9c5a),
        (DraygonProgram.BodyFacingRightIdle, 0x9c7e),
        (DraygonProgram.BodyFacingRightFireGoop, 0x9c90),
        (DraygonProgram.BodyFacingRightRoar, 0x9cb4),
        (DraygonProgram.EyeFacingRightIdle, 0x9cd6),
        (DraygonProgram.EyeFacingRightDying, 0x9d1c),
        (DraygonProgram.EyeFacingRightDyingLoop, 0x9d20),
        (DraygonProgram.EyeFacingRightDead, 0x9d3e),
        (DraygonProgram.EyeFacingRightLookingRight, 0x9d50),
        (DraygonProgram.EyeFacingRightLookingLeft, 0x9d56),
        (DraygonProgram.EyeFacingRightLookingUp, 0x9d5c),
        (DraygonProgram.EyeFacingRightLookingDown, 0x9d62),
        (DraygonProgram.TailFacingRightIdle, 0x9d68),
        (DraygonProgram.TailFacingRightFinalWhips, 0x9e21),
        (DraygonProgram.TailFacingRightFinalWhipsLoop, 0x9e25),
        (DraygonProgram.TailFacingRightWhip, 0x9ea1),
        (DraygonProgram.TailFacingRightFlail, 0x9f15),
    ];

    /// <summary>The operand words each bank-$A5 Draygon callback takes.</summary>
    private static readonly Dictionary<ushort, int> CallbackOperandWords = new()
    {
        [0x94dd] = 4, [0x9895] = 0, [0x9c8a] = 0, [0xc47b] = 1, [0x9736] = 1, [0x9e0a] = 2, [0x9f60] = 1, [0x9f6e] = 1,
        [0x973f] = 0, [0x9752] = 0, [0x9765] = 0, [0x9778] = 0, [0x98d3] = 0, [0x98ef] = 0, [0x9f57] = 1, [0x9b9a] = 0,
        [0x9f7c] = 0, [0x9fae] = 0,
    };

    /// <summary>The body functions callbacks name, by native routine address.</summary>
    private static readonly Dictionary<ushort, DraygonAiFunction> BodyFunctions = new()
    {
        [0x9128] = DraygonAiFunction.ReleaseSamus,
    };

    /// <summary>The operand words a native callback takes.</summary>
    internal static int OperandWords(ushort callback) =>
        CallbackOperandWords.TryGetValue(callback, out int count)
            ? count
            : throw new InvalidDataException($"Draygon callback ${callback:X4} is not a known callback.");

    /// <summary>The program whose native list starts at <paramref name="address"/>, if one does.</summary>
    internal static DraygonProgram? ProgramAt(ushort address) =>
        Entries.Where(item => item.Entry == address).Select(item => (DraygonProgram?)item.Program).SingleOrDefault();

    /// <summary>The native entry point of <paramref name="program"/>.</summary>
    internal static ushort Entry(DraygonProgram program) => Entries.Single(item => item.Program == program).Entry;

    /// <summary>Whether the native entries ascend, so the typed declaration order is the authored order.</summary>
    internal static bool EntriesAscend => Entries.Zip(Entries.Skip(1)).All(pair => pair.First.Entry < pair.Second.Entry);

    /// <summary>The cue a native callback with <paramref name="operands"/> must become.</summary>
    internal static DraygonCue CueOf(ushort callback, ushort[] operands) => callback switch
    {
        0x94dd => new DraygonCue.StartPartPrograms(Program(operands[0]), Program(operands[1]), Program(operands[2]), Program(operands[3])),
        0x9895 or 0x9c8a => DraygonCue.BeginHudDraw.Instance,
        0xc47b => new DraygonCue.SetEyeFunction(ClosedNativeWords.Decode<DraygonEyeFunction>(operands[0], "Draygon eye function")),
        0x9736 => new DraygonCue.SetOwnEyeFunction(ClosedNativeWords.Decode<DraygonEyeFunction>(operands[0], "Draygon eye function")),
        0x9e0a => new DraygonCue.DisplaceBodyGraphics(new(unchecked((short)operands[0]), unchecked((short)operands[1]))),
        0x9f60 => new DraygonCue.QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, operands[0])),
        0x9f6e => new DraygonCue.QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, operands[0])),
        0x973f => new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.DustCloud),
        0x9752 => new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.SporeSpawnDyingExplosion),
        0x9765 => new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.BotwoonLargeExplosion),
        0x9778 => new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.DraygonBreathBubble),
        0x98d3 => DraygonCue.ParalyseTailAndArms.Instance,
        0x98ef => DraygonCue.SetBodyIntangible.Instance,
        0x9f57 => new DraygonCue.SetBodyFunction(BodyFunctions[operands[0]]),
        0x9b9a => DraygonCue.TailWhipHit.Instance,
        0x9f7c => new DraygonCue.SpawnGoop(MovingRight: false),
        0x9fae => new DraygonCue.SpawnGoop(MovingRight: true),
        _ => throw new InvalidDataException($"Draygon callback ${callback:X4} is not a known callback."),
    };

    /// <summary>The program a callback operand names.</summary>
    private static DraygonProgram Program(ushort address) =>
        ProgramAt(address) ?? throw new InvalidDataException($"Draygon list ${address:X4} is not a typed program.");

    /// <summary>Each typed projectile program's native entry point in bank $86.</summary>
    internal static readonly (DraygonProjectileProgram Program, ushort Entry)[] ProjectileEntries =
    [
        (DraygonProjectileProgram.GoopTouch, 0x8c38),
        (DraygonProjectileProgram.Goop, 0x8c3a),
        (DraygonProjectileProgram.GoopShot, 0x8c58),
        (DraygonProjectileProgram.WallTurretBloom, 0x8ca4),
        (DraygonProjectileProgram.WallTurretFlight, 0x8ce6),
    ];

    /// <summary>The bank-$86 callbacks the projectile lists use and the cue each must become.</summary>
    internal static readonly Dictionary<ushort, DraygonProjectileCue> ProjectileCallbacks = new()
    {
        [(ushort)EnemyProjectileInstruction.DraygonGoop_SamusCollision] = DraygonProjectileCue.StickToSamus,
        [(ushort)EnemyProjectileInstruction.SpawnEnemyDropsWithDraygonEyeChances] = DraygonProjectileCue.SpawnDrops,
        [(ushort)EnemyProjectileInstruction.Delete] = DraygonProjectileCue.Delete,
        [(ushort)EnemyProjectileInstruction.SetPreInst_DraygonsWallTurretProjectile_Fired] = DraygonProjectileCue.Fire,
    };

    /// <summary>The projectile program whose native list starts at <paramref name="address"/>, if one does.</summary>
    internal static DraygonProjectileProgram? ProjectileProgramAt(ushort address) =>
        ProjectileEntries.Where(item => item.Entry == address).Select(item => (DraygonProjectileProgram?)item.Program).SingleOrDefault();

    /// <summary>Whether a bank-$86 word is the projectile goto.</summary>
    internal static bool IsProjectileGoTo(ushort word) => ProjectileGoTo.Contains(word);

    /// <summary>The projectile goto opcode.</summary>
    private static readonly HashSet<ushort> ProjectileGoTo = [(ushort)EnemyProjectileInstruction.GotoY];
}
