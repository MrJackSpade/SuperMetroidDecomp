namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// An Evir's arms (#2146), which share the Evir's definition and follow its body. They take neither its touch nor its
/// shot reaction, but their bombs and Power Bombs still take the Evir's, followed by the follow-up to the enemies after
/// them.
/// </summary>
/// <param name="species">The Evir species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the arms occupy.</param>
/// <param name="common">The shared participation.</param>
internal sealed class EvirArmsEntity(EvirBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeEvirArms(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => EvirBehavior.RunEvirArmsMainAi(Place);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => EvirBehavior.AdvanceEvirPartTypedAnimation<EvirArmsState>(Place);

    /// <summary>The Evir's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? species.EvirBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Evir's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction => LeavesPowerBombToSpecies ? species.EvirPowerBombReaction : base.PowerBombReaction;

    /// <summary>The Evir's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
