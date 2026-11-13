namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// One Bull occupant: its initialization, main AI, typed animation and immune-hit recoil after shots and bombs, with the
/// shared reactions otherwise. Its state inherits the shot-reaction guard of the previous Bull in its slot.
/// </summary>
/// <param name="species">The Bull species owner.</param>
/// <param name="slot">The slot the Bull occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BullEntity(BullBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.InitializeBull;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => RunMain;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => BullBehavior.AdvanceBullAnimation;

    /// <summary>
    /// Bull's recoil reaction, ahead of the shared ones; only reactions for other species precede it in the shared list,
    /// and the shared generic reactions follow it.
    /// </summary>
    public override EnemyShotReaction? FindShot() =>
        species.ShotReaction.Matches(Slot) ? species.ShotReaction : base.FindShot();

    /// <summary>Bull's bomb recoil, ahead of the shared common bomb reactions, which follow it in the shared list.</summary>
    public override EnemyBombReaction? FindBombCommon(EnemyBombContext context) =>
        species.BombReaction.Matches(context) ? species.BombReaction : base.FindBombCommon(context);

    /// <summary>Runs a Bull's per-frame behaviour.</summary>
    private void RunMain(RoomEnemySlot slot, IEnemyAiFrame frame) =>
        BullBehavior.RunBullMain(slot, slot.RequireSpeciesState<BullEnemyState>(), frame.Samus);

    /// <summary>The Bull's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == EnemyReaction.BullShot;
}
