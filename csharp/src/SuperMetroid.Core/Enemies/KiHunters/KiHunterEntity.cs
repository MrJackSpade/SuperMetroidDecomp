namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>One KiHunter occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The KiHunter species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KiHunterEntity(KiHunterBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.KihunterGreen or EnemyDefinitionId.KihunterYellow or EnemyDefinitionId.KihunterRed => KiHunterBehavior.RunKihunterGreenInitializationAi,
        EnemyDefinitionId.KihunterGreenWings or EnemyDefinitionId.KihunterYellowWings or EnemyDefinitionId.KihunterRedWings => species.RunKihunterGreenWingsInitializationAi,
        _ => throw new InvalidOperationException($"KiHunter entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunKiHunterMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceKiHunterTypedAnimation;
}
