namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>
/// One Ripper: the grapple Ripper (<see cref="GrappleRipperEntity"/>), the jet Ripper (<see cref="JetRipperEntity"/>) or
/// the ordinary Ripper (<see cref="OrdinaryRipperEntity"/>). Each sets up and flies by its own AI, animates by the shared
/// Ripper animation, and takes the Rippers' own shot and bomb stages where its definition leaves shots to it.
/// </summary>
/// <param name="species">The Ripper species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the Ripper occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class RipperEntity(RipperBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The Ripper species owner.</summary>
    protected RipperBehavior Species => species;

    /// <inheritdoc/>
    public abstract void Initialize();

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => RipperBehavior.AdvanceRipperTypedAnimation(Place, Place.RequireSpeciesState<RipperEnemyState>());

    /// <summary>The grapple Ripper and jet Ripper's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == RipperReactions.Shot ? RipperBehavior.RipperShotReaction : base.ShotReaction;

    /// <summary>The grapple Ripper and jet Ripper's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == RipperReactions.Shot ? RipperBehavior.RipperBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The grapple Ripper and jet Ripper's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) =>
        reaction == RipperReactions.Shot && Occupant.Definition.Shot == RipperReactions.Shot;
}
