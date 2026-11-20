using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// How the death explosion and drop programs match their native lists (#2554): each command, its operand bytes and the cue
/// it becomes, the explosions' and the drops' apart.
/// </summary>
internal static class EnemyDeathProjectileCueRules
{
    /// <summary>The explosions' signalling commands and the cue kinds they become.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, EnemyDeathExplosionCueKind> ExplosionSignals = new()
    {
        [NativeEnemyProjectileInstruction.EnemyDeathExpl_QueueEnemyKilledSoundFX] = EnemyDeathExplosionCueKind.PlayEnemyKilledSound,
        [NativeEnemyProjectileInstruction.EDeathExplo_QueueSmallExplosionSoundFX] = EnemyDeathExplosionCueKind.PlaySmallExplosionSound,
        [NativeEnemyProjectileInstruction.EDeathExplo_QueueContactKilledSoundFX] = EnemyDeathExplosionCueKind.PlayContactKilledSound,
        [NativeEnemyProjectileInstruction.EnemyDeathExplosion_BecomePickup] = EnemyDeathExplosionCueKind.BecomePickup,
    };

    /// <summary>The explosions' sprite-spawning commands, each carrying a sprite object word, and the spreads they land in.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, EnemyDeathSpriteSpread> ExplosionSpawns = new()
    {
        [NativeEnemyProjectileInstruction.EnemyDeathExpl_SpawnSpriteObjectInY_20] = EnemyDeathExplosionDefinitions.WideSpriteSpread,
        [NativeEnemyProjectileInstruction.EnemyDeathExpl_SpawnSpriteObjectInY_10] = EnemyDeathExplosionDefinitions.NarrowSpriteSpread,
    };

    /// <summary>The drops' commands and the cue kinds they become.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, EnemyDeathAndDropCueKind> DropSignals = new()
    {
        [NativeEnemyProjectileInstruction.Delete] = EnemyDeathAndDropCueKind.Delete,
        [NativeEnemyProjectileInstruction.Pickup_HandleRespawningEnemy] = EnemyDeathAndDropCueKind.RespawnEnemy,
    };

    /// <summary>The oracle rules for the explosion programs.</summary>
    /// <remarks>The conversion ends the explosion's list: the projectile goes on in the drop or respawn tail it chooses.</remarks>
    internal static NativeEnemyProjectileCueRules<EnemyDeathExplosionCue> ExplosionRules { get; } = new(
        word => ExplosionSignals.ContainsKey((NativeEnemyProjectileInstruction)word) ? 0
            : ExplosionSpawns.ContainsKey((NativeEnemyProjectileInstruction)word) ? 2 : null,
        (word, operands, cue, at) =>
        {
            var instruction = (NativeEnemyProjectileInstruction)word;
            EnemyDeathExplosionCue expected = ExplosionSpawns.TryGetValue(instruction, out EnemyDeathSpriteSpread spread)
                ? new EnemyDeathExplosionCue.SpawnEffectActor(
                    RoomEffectActorNativeWords.Kinds.Decode(LittleEndianBytes.Word(operands[0], operands[1]), "death sprite object"), spread)
                : new EnemyDeathExplosionCue.Signal(ExplosionSignals[instruction]);
            VerificationAssertions.AssertEqual(expected, cue, $"{at}: cue");
        },
        cue => cue is EnemyDeathExplosionCue.Signal { Kind: EnemyDeathExplosionCueKind.BecomePickup });

    /// <summary>The oracle rules for the drop programs.</summary>
    /// <remarks>A deletion ends its list.</remarks>
    internal static NativeEnemyProjectileCueRules<EnemyDeathAndDropCue> DropRules { get; } = new(
        word => DropSignals.ContainsKey((NativeEnemyProjectileInstruction)word) ? 0 : null,
        (word, _, cue, at) => VerificationAssertions.AssertEqual(
            new EnemyDeathAndDropCue.Signal(DropSignals[(NativeEnemyProjectileInstruction)word]), cue, $"{at}: cue"),
        cue => cue is EnemyDeathAndDropCue.Signal { Kind: EnemyDeathAndDropCueKind.Delete });
}
