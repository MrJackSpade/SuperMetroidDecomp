namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>One WallSpacePirate occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The WallSpacePirate species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class WallSpacePirateEntity(WallSpacePirateBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyExtendedHitboxes
{
    /// <summary>The occupant's WallSpacePirateEnemyState, held by its physical slot.</summary>
    internal WallSpacePirateEnemyState? State
    {
        get => Slot.SpeciesState<WallSpacePirateEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => WallSpacePirateBehavior.RunWallSpacePirateInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => WallSpacePirateBehavior.RunWallSpacePirateMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceWallSpacePirateTypedAnimation;

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out ushort callback) => SpacePirateHitboxes.Find(Slot, probe, out callback);
}
