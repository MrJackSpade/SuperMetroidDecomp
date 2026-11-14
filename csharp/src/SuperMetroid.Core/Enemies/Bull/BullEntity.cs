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
    public void Initialize() => species.InitializeBull(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) =>
        BullBehavior.RunBullMain(Slot, Slot.RequireSpeciesState<BullEnemyState>(), frame.Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => BullBehavior.AdvanceBullAnimation(Slot, context);

    /// <summary>The Bull's own recoil reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.ShotReaction : base.ShotReaction;

    /// <summary>The Bull's own bomb recoil around common bomb damage, for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? species.BombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Bull's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
