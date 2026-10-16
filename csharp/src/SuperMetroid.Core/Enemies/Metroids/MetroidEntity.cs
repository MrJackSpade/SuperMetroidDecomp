namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>One Metroid occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Metroid species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MetroidEntity(MetroidBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunMetroidInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunMetroidMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceMetroidTypedAnimation;

    /// <summary>The Metroid's own shot callback, which normal bombs dispatch like the common one.</summary>
    public override bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback, ushort callbackPointer) =>
        callback == EnemyInteractionCallback.MetroidShot || base.OwnsPrivateShotCallback(callback, callbackPointer);
}
