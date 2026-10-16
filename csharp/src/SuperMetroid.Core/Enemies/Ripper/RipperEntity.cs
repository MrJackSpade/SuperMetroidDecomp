namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>One Ripper occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Ripper species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class RipperEntity(RipperBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.GRipper => species.RunGRipperInitializationAi,
        EnemyDefinitionId.Ripper2 => species.RunRipper2InitializationAi,
        EnemyDefinitionId.Ripper => species.RunRipperInitializationAi,
        _ => throw new InvalidOperationException($"Ripper entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.GRipper => species.RunGRipperMainAi,
        EnemyDefinitionId.Ripper2 => species.RunRipper2MainAi,
        EnemyDefinitionId.Ripper => species.RunRipperMainAi,
        _ => throw new InvalidOperationException($"Ripper entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceRipperTypedAnimation;
}
