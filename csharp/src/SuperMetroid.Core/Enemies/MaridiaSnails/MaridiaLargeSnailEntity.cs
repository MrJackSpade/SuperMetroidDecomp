namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>One MaridiaLargeSnail occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The MaridiaLargeSnail species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MaridiaLargeSnailEntity(MaridiaLargeSnailBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyExtendedHitboxes
{
    /// <summary>The occupant's MaridiaLargeSnailEnemyState, held by its physical slot.</summary>
    internal MaridiaLargeSnailEnemyState? State
    {
        get => Slot.SpeciesState<MaridiaLargeSnailEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => MaridiaLargeSnailBehavior.RunMaridiaLargeSnailInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunMaridiaLargeSnailMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceMaridiaLargeSnailTypedAnimation;

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out ushort callback) => MaridiaLargeSnailBehavior.FindHitboxCallback(Slot, probe, out callback);
}
