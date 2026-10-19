namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>One MorphBallEye occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The MorphBallEye species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MorphBallEyeEntity(MorphBallEyeBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunMorphBallEyeInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunMorphBallEyeMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => MorphBallEyeBehavior.AdvanceMorphBallEyeTypedAnimation;
}
