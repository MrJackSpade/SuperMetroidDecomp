using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One Kraid occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Kraid species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KraidEntity(KraidBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyExtendedHitboxes
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.Kraid => species.RunKraidInitializationAi,
        EnemyDefinitionId.KraidArm => species.RunKraidArmInitializationAi,
        EnemyDefinitionId.KraidLintTop => species.RunKraidLintTopInitializationAi,
        EnemyDefinitionId.KraidLintMiddle => species.RunKraidLintMiddleInitializationAi,
        EnemyDefinitionId.KraidLintBottom => species.RunKraidLintBottomInitializationAi,
        EnemyDefinitionId.KraidFoot => species.RunKraidFootInitializationAi,
        EnemyDefinitionId.KraidNail => species.RunKraidNailInitializationAi,
        EnemyDefinitionId.KraidNailBad => species.RunKraidNailBadInitializationAi,
        _ => throw new InvalidOperationException($"Kraid entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.Kraid => species.RunKraidMainAi,
        EnemyDefinitionId.KraidArm => species.RunKraidArmMainAi,
        EnemyDefinitionId.KraidLintTop or EnemyDefinitionId.KraidLintMiddle or EnemyDefinitionId.KraidLintBottom => species.RunKraidLintTopMainAi,
        EnemyDefinitionId.KraidFoot => species.RunKraidFootMainAi,
        EnemyDefinitionId.KraidNail or EnemyDefinitionId.KraidNailBad => species.RunKraidNailMainAi,
        _ => throw new InvalidOperationException($"Kraid entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.KraidArm or EnemyDefinitionId.KraidLintTop or EnemyDefinitionId.KraidLintMiddle or EnemyDefinitionId.KraidLintBottom or EnemyDefinitionId.KraidFoot or EnemyDefinitionId.KraidNail or EnemyDefinitionId.KraidNailBad => species.AdvanceKraidTypedAnimation,
        EnemyDefinitionId.Kraid => Common.FindTypedAnimation(Slot),
        _ => throw new InvalidOperationException($"Kraid entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out ushort callback)
    {
        callback = 0;
        return KraidBehavior.FindKraidFootHitboxCallback(Slot, probe, out callback) ??
            KraidBehavior.FindKraidArmHitboxCallback(Slot, probe, out callback);
    }
}
