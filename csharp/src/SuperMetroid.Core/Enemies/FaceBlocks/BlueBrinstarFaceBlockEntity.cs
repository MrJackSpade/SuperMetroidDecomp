namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>One BlueBrinstarFaceBlock occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The BlueBrinstarFaceBlock species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BlueBrinstarFaceBlockEntity(BlueBrinstarFaceBlockBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's state, set by its initializer.</summary>
    internal BlueBrinstarFaceBlockEnemyState? State { get; set; }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunBlueBrinstarFaceBlockInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunBlueBrinstarFaceBlockMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => BlueBrinstarFaceBlockBehavior.AdvanceBlueBrinstarFaceBlockTypedAnimation;
}
