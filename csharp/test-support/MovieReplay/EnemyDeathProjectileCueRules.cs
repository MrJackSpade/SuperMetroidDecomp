using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>How the death and drop programs match their native lists: each command, its operand bytes and the cue it becomes.</summary>
internal static class EnemyDeathProjectileCueRules
{
    /// <summary>Each command the lists use, the cue it becomes and its operand byte count (the spawns carry a sprite object).</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, EnemyDeathProjectileCueRulesCommand> Commands = new()
    {
        [NativeEnemyProjectileInstruction.Delete] = new EnemyDeathProjectileCueRulesCommand(EnemyDeathProjectileCueKind.Delete, null),
        [NativeEnemyProjectileInstruction.EnemyDeathExpl_SpawnSpriteObjectInY_20] = new EnemyDeathProjectileCueRulesCommand(null, EnemyDeathExplosionDefinitions.WideSpriteSpread),
        [NativeEnemyProjectileInstruction.EnemyDeathExpl_SpawnSpriteObjectInY_10] = new EnemyDeathProjectileCueRulesCommand(null, EnemyDeathExplosionDefinitions.NarrowSpriteSpread),
        [NativeEnemyProjectileInstruction.EnemyDeathExpl_QueueEnemyKilledSoundFX] = new EnemyDeathProjectileCueRulesCommand(EnemyDeathProjectileCueKind.PlayEnemyKilledSound, null),
        [NativeEnemyProjectileInstruction.EDeathExplo_QueueSmallExplosionSoundFX] = new EnemyDeathProjectileCueRulesCommand(EnemyDeathProjectileCueKind.PlaySmallExplosionSound, null),
        [NativeEnemyProjectileInstruction.EDeathExplo_QueueContactKilledSoundFX] = new EnemyDeathProjectileCueRulesCommand(EnemyDeathProjectileCueKind.PlayContactKilledSound, null),
        [NativeEnemyProjectileInstruction.EnemyDeathExplosion_BecomePickup] = new EnemyDeathProjectileCueRulesCommand(EnemyDeathProjectileCueKind.BecomePickup, null),
        [NativeEnemyProjectileInstruction.Pickup_HandleRespawningEnemy] = new EnemyDeathProjectileCueRulesCommand(EnemyDeathProjectileCueKind.HandleRespawningEnemy, null),
    };

    /// <summary>The oracle rules for the death and drop programs.</summary>
    /// <remarks>A deletion ends its list, and the conversion always continues at the drop or respawn tail it chooses.</remarks>
    internal static NativeEnemyProjectileCueRules<EnemyDeathProjectileCue> Rules { get; } = new(
        word => Commands.TryGetValue((NativeEnemyProjectileInstruction)word, out var command) ? command.OperandBytes : null,
        (word, operands, cue, at) =>
        {
            var command = Commands[(NativeEnemyProjectileInstruction)word];
            EnemyDeathProjectileCue expected = command.Spread is { } spread
                ? new EnemyDeathProjectileCue.SpawnEffectActor(
                    RoomEffectActorNativeWords.Kinds.Decode(LittleEndianBytes.Word(operands[0], operands[1]), "death sprite object"), spread)
                : new EnemyDeathProjectileCue.Signal(command.Kind ?? throw new InvalidDataException($"{at}: command names neither a signal nor a spread."));
            VerificationAssertions.AssertEqual(expected, cue, $"{at}: cue");
        },
        cue => cue is EnemyDeathProjectileCue.Signal { Kind: EnemyDeathProjectileCueKind.Delete or EnemyDeathProjectileCueKind.BecomePickup });
}
