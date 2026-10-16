using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One BombTorizo occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The BombTorizo species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BombTorizoEntity(BombTorizoBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyExtendedHitboxes
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunBombTorizoInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.BombTorizo => species.RunBombTorizoMainAi,
        EnemyDefinitionId.GoldenTorizo => species.RunGoldenTorizoMainAi,
        _ => throw new InvalidOperationException($"BombTorizo entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceBombTorizoTypedAnimation;

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out ushort callback)
    {
        callback = 0;
        return BombTorizoBehavior.FindTorizoHitboxCallback(Slot, probe, out callback);
    }
}
