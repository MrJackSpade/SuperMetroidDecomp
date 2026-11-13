using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpriteObjects;

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
    internal static readonly DraygonNativeListAddressesEntry[] Entries =
    [
        new DraygonNativeListAddressesEntry(DraygonSegment.Sleep, 0x97b9),
        new DraygonNativeListAddressesEntry(DraygonSegment.BodyFacingLeftReset, 0x97bb),
        new DraygonNativeListAddressesEntry(DraygonSegment.BodyFacingRightReset, 0x97d1),
        new DraygonNativeListAddressesEntry(DraygonSegment.ArmsFacingLeftIdle, 0x97e7),
        new DraygonNativeListAddressesEntry(DraygonSegment.ArmsFacingLeftNearSwoopApex, 0x9813),
        new DraygonNativeListAddressesEntry(DraygonSegment.ArmsFacingLeftGrab, 0x9845),
        new DraygonNativeListAddressesEntry(DraygonSegment.ArmsFacingLeftDying, 0x9867),
        new DraygonNativeListAddressesEntry(DraygonSegment.BodyFacingLeftIdle, 0x9889),
        new DraygonNativeListAddressesEntry(DraygonSegment.BodyDying, 0x989b),
        new DraygonNativeListAddressesEntry(DraygonSegment.BodyDyingExplosions, 0x98a5),
        new DraygonNativeListAddressesEntry(DraygonSegment.BodyDyingExplosionsLoop, 0x98bf),
        new DraygonNativeListAddressesEntry(DraygonSegment.Delete, 0x98ed),
        new DraygonNativeListAddressesEntry(DraygonSegment.BodyFacingLeftFireGoop, 0x98fe),
        new DraygonNativeListAddressesEntry(DraygonSegment.BodyFacingLeftRoar, 0x9922),
        new DraygonNativeListAddressesEntry(DraygonSegment.EyeFacingLeftIdle, 0x9944),
        new DraygonNativeListAddressesEntry(DraygonSegment.EyeFacingLeftDying, 0x997a),
        new DraygonNativeListAddressesEntry(DraygonSegment.EyeFacingLeftDyingLoop, 0x997e),
        new DraygonNativeListAddressesEntry(DraygonSegment.EyeFacingLeftDead, 0x999c),
        new DraygonNativeListAddressesEntry(DraygonSegment.EyeFacingLeftLookingLeft, 0x99ae),
        new DraygonNativeListAddressesEntry(DraygonSegment.EyeFacingLeftLookingRight, 0x99b4),
        new DraygonNativeListAddressesEntry(DraygonSegment.EyeFacingLeftLookingUp, 0x99ba),
        new DraygonNativeListAddressesEntry(DraygonSegment.EyeFacingLeftLookingDown, 0x99c0),
        new DraygonNativeListAddressesEntry(DraygonSegment.TailFacingLeftIdle, 0x99c6),
        new DraygonNativeListAddressesEntry(DraygonSegment.TailFacingLeftInitialFakeWhip, 0x99fc),
        new DraygonNativeListAddressesEntry(DraygonSegment.TailFacingLeftFinalWhips, 0x9a68),
        new DraygonNativeListAddressesEntry(DraygonSegment.TailFacingLeftFinalWhipsLoop, 0x9a6c),
        new DraygonNativeListAddressesEntry(DraygonSegment.TailFacingLeftWhip, 0x9ae8),
        new DraygonNativeListAddressesEntry(DraygonSegment.TailFacingLeftFlail, 0x9b5a),
        new DraygonNativeListAddressesEntry(DraygonSegment.ArmsFacingRightIdle, 0x9bda),
        new DraygonNativeListAddressesEntry(DraygonSegment.ArmsFacingRightNearSwoopApex, 0x9c06),
        new DraygonNativeListAddressesEntry(DraygonSegment.ArmsFacingRightGrab, 0x9c38),
        new DraygonNativeListAddressesEntry(DraygonSegment.ArmsFacingRightDying, 0x9c5a),
        new DraygonNativeListAddressesEntry(DraygonSegment.BodyFacingRightIdle, 0x9c7e),
        new DraygonNativeListAddressesEntry(DraygonSegment.BodyFacingRightFireGoop, 0x9c90),
        new DraygonNativeListAddressesEntry(DraygonSegment.BodyFacingRightRoar, 0x9cb4),
        new DraygonNativeListAddressesEntry(DraygonSegment.EyeFacingRightIdle, 0x9cd6),
        new DraygonNativeListAddressesEntry(DraygonSegment.EyeFacingRightDying, 0x9d1c),
        new DraygonNativeListAddressesEntry(DraygonSegment.EyeFacingRightDyingLoop, 0x9d20),
        new DraygonNativeListAddressesEntry(DraygonSegment.EyeFacingRightDead, 0x9d3e),
        new DraygonNativeListAddressesEntry(DraygonSegment.EyeFacingRightLookingRight, 0x9d50),
        new DraygonNativeListAddressesEntry(DraygonSegment.EyeFacingRightLookingLeft, 0x9d56),
        new DraygonNativeListAddressesEntry(DraygonSegment.EyeFacingRightLookingUp, 0x9d5c),
        new DraygonNativeListAddressesEntry(DraygonSegment.EyeFacingRightLookingDown, 0x9d62),
        new DraygonNativeListAddressesEntry(DraygonSegment.TailFacingRightIdle, 0x9d68),
        new DraygonNativeListAddressesEntry(DraygonSegment.TailFacingRightFinalWhips, 0x9e21),
        new DraygonNativeListAddressesEntry(DraygonSegment.TailFacingRightFinalWhipsLoop, 0x9e25),
        new DraygonNativeListAddressesEntry(DraygonSegment.TailFacingRightWhip, 0x9ea1),
        new DraygonNativeListAddressesEntry(DraygonSegment.TailFacingRightFlail, 0x9f15),
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
    internal static DraygonSegment? ProgramAt(ushort address) =>
        Entries.Where(item => item.Entry == address).Select(item => (DraygonSegment?)item.Program).SingleOrDefault();

    /// <summary>The native entry point of <paramref name="program"/>.</summary>
    internal static ushort Entry(DraygonSegment program) => Entries.Single(item => item.Program == program).Entry;

    /// <summary>Whether the native entries ascend, so the typed declaration order is the authored order.</summary>
    internal static bool EntriesAscend => Entries.Zip(Entries.Skip(1), (first, second) => first.Entry < second.Entry).All(ascends => ascends);

    /// <summary>
    /// Whether a native callback only selects the screen split that draws the HUD. The port's display draws the HUD
    /// without it, so these callbacks have no typed step.
    /// </summary>
    internal static bool SelectsHudSplit(ushort callback) => callback is 0x9895 or 0x9c8a;

    /// <summary>The cue a native callback with <paramref name="operands"/> must become.</summary>
    internal static DraygonCue CueOf(ushort callback, ushort[] operands) => callback switch
    {
        0x94dd => new DraygonCue.StartPartAnimations(Program(operands[0]), Program(operands[1]), Program(operands[2]), Program(operands[3])),
        0xc47b => new DraygonCue.SetEyeFunction(DraygonNativeWords.EyeFunctions.Decode(operands[0], "Draygon eye function")),
        0x9736 => new DraygonCue.SetOwnEyeFunction(DraygonNativeWords.EyeFunctions.Decode(operands[0], "Draygon eye function")),
        0x9e0a => new DraygonCue.DisplaceBodyGraphics(new(unchecked((short)operands[0]), unchecked((short)operands[1]))),
        0x9f60 => new DraygonCue.QueueSound(SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, operands[0])),
        0x9f6e => new DraygonCue.QueueSound(SoundEffectCommands.Decode(SoundEffectLibrary.Room, operands[0])),
        0x973f => new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.DustCloud),
        0x9752 => new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.SporeSpawnDyingExplosion),
        0x9765 => new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.BotwoonLargeExplosion),
        0x9778 => new DraygonCue.SpawnDyingObject(RoomSpriteObjectKind.DraygonBreathBubble),
        0x98d3 => DraygonCue.ParalyseTailAndArms.Instance,
        0x98ef => DraygonCue.SetBodyIntangible.Instance,
        0x9f57 => new DraygonCue.SetBodyFunction(BodyFunctions[operands[0]]),
        0x9b9a => DraygonCue.TailWhipHit.Instance,
        0x9f7c => new DraygonCue.SpawnGoop(HorizontalDirection.Left),
        0x9fae => new DraygonCue.SpawnGoop(HorizontalDirection.Right),
        _ => throw new InvalidDataException($"Draygon callback ${callback:X4} is not a known callback."),
    };

    /// <summary>The program a callback operand names.</summary>
    private static DraygonSegment Program(ushort address) =>
        ProgramAt(address) ?? throw new InvalidDataException($"Draygon list ${address:X4} is not a typed program.");

    /// <summary>Each typed projectile program's native entry point in bank $86.</summary>
    internal static readonly ProjectileEntry[] ProjectileEntries =
    [
        new ProjectileEntry(DraygonProjectileSegment.GoopTouch, 0x8c38),
        new ProjectileEntry(DraygonProjectileSegment.Goop, 0x8c3a),
        new ProjectileEntry(DraygonProjectileSegment.GoopShot, 0x8c58),
        new ProjectileEntry(DraygonProjectileSegment.WallTurretBloom, 0x8ca4),
        new ProjectileEntry(DraygonProjectileSegment.WallTurretFlight, 0x8ce6),
    ];

    /// <summary>The bank-$86 callbacks the projectile lists use and the cue each must become.</summary>
    internal static readonly Dictionary<ushort, DraygonProjectileCue> ProjectileCallbacks = new()
    {
        [(ushort)NativeEnemyProjectileInstruction.DraygonGoop_SamusCollision] = DraygonProjectileCue.StickToSamus,
        [(ushort)NativeEnemyProjectileInstruction.SpawnEnemyDropsWithDraygonEyeChances] = DraygonProjectileCue.SpawnDrops,
        [(ushort)NativeEnemyProjectileInstruction.Delete] = DraygonProjectileCue.Delete,
        [(ushort)NativeEnemyProjectileInstruction.SetPreInst_DraygonsWallTurretProjectile_Fired] = DraygonProjectileCue.Fire,
    };

    /// <summary>The projectile program whose native list starts at <paramref name="address"/>, if one does.</summary>
    internal static DraygonProjectileSegment? ProjectileProgramAt(ushort address) =>
        ProjectileEntries.Where(item => item.Entry == address).Select(item => (DraygonProjectileSegment?)item.Program).SingleOrDefault();

    /// <summary>Whether a bank-$86 word is the projectile goto.</summary>
    internal static bool IsProjectileGoTo(ushort word) => ProjectileGoTo.Contains(word);

    /// <summary>The projectile goto opcode.</summary>
    private static readonly HashSet<ushort> ProjectileGoTo = [(ushort)NativeEnemyProjectileInstruction.GotoY];
}
