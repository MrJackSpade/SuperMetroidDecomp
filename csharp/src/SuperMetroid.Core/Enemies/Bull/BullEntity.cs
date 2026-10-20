namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// One Bull occupant: its initialization, main AI, typed animation and immune-hit recoil after shots and bombs, with the
/// shared reactions otherwise. Its state reads the slot's <see cref="BullExtensionWords"/>, which outlive the occupant.
/// </summary>
/// <param name="species">The Bull species owner.</param>
/// <param name="slot">The slot the Bull occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BullEntity(BullBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotCallback
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

    /// <summary>Ports <c>MainAI_Bull</c>.</summary>
    private void RunMain(RoomEnemySlot slot, IEnemyAiFrame frame) =>
        BullBehavior.RunBullMain(slot, slot.RequireSpeciesState<BullEnemyState>(), frame.Samus);

    /// <summary>The Bull's own shot callback, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        callback == EnemyInteractionCallback.BullShot;
}
