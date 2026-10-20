using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.SpriteObjects;

namespace SuperMetroid.Verification.Enemies.Deaths;

/// <summary>How the death and drop programs match their native lists: each command, its operand bytes and the cue it becomes.</summary>
internal static class EnemyDeathProjectileCueRules
{
    /// <summary>Each command the lists use, the cue it becomes and its operand byte count (the spawns carry a sprite object).</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, (EnemyDeathProjectileCueKind Kind, int OperandBytes)> Commands = new()
    {
        [NativeEnemyProjectileInstruction.Delete] = (EnemyDeathProjectileCueKind.Delete, 0),
        [NativeEnemyProjectileInstruction.EnemyDeathExpl_SpawnSpriteObjectInY_20] = (EnemyDeathProjectileCueKind.SpawnSpriteWithin32, 2),
        [NativeEnemyProjectileInstruction.EnemyDeathExpl_SpawnSpriteObjectInY_10] = (EnemyDeathProjectileCueKind.SpawnSpriteWithin16, 2),
        [NativeEnemyProjectileInstruction.EnemyDeathExpl_QueueEnemyKilledSoundFX] = (EnemyDeathProjectileCueKind.PlayEnemyKilledSound, 0),
        [NativeEnemyProjectileInstruction.EDeathExplo_QueueSmallExplosionSoundFX] = (EnemyDeathProjectileCueKind.PlaySmallExplosionSound, 0),
        [NativeEnemyProjectileInstruction.EDeathExplo_QueueContactKilledSoundFX] = (EnemyDeathProjectileCueKind.PlayContactKilledSound, 0),
        [NativeEnemyProjectileInstruction.EnemyDeathExplosion_BecomePickup] = (EnemyDeathProjectileCueKind.BecomePickup, 0),
        [NativeEnemyProjectileInstruction.Pickup_HandleRespawningEnemy] = (EnemyDeathProjectileCueKind.HandleRespawningEnemy, 0),
    };

    /// <summary>The oracle rules for the death and drop programs.</summary>
    /// <remarks>A deletion ends its list, and the conversion always continues at the drop or respawn tail it chooses.</remarks>
    internal static NativeEnemyProjectileCueRules<EnemyDeathProjectileCue> Rules { get; } = new(
        word => Commands.TryGetValue((NativeEnemyProjectileInstruction)word, out var command) ? command.OperandBytes : null,
        (word, operands, cue, at) =>
        {
            var command = Commands[(NativeEnemyProjectileInstruction)word];
            AssertEqual(command.Kind, cue.Kind, $"{at}: cue");
            RoomSpriteObjectKind spriteObject = command.OperandBytes == 2 ? (RoomSpriteObjectKind)SnesLittleEndian.Word(operands[0], operands[1]) : default;
            AssertEqual(spriteObject, cue.SpriteObject, $"{at}: sprite object");
        },
        cue => cue.Kind is EnemyDeathProjectileCueKind.Delete or EnemyDeathProjectileCueKind.BecomePickup);
}
