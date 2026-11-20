namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// A Powamp's balloon (#2163), which shares the Powamp's definition and which its body, listed right after it, moves. It
/// has no AI of its own, plays its inflate and sink animations, and keeps the Powamp's reactions where its definition
/// leaves them to it; its intangibility normally keeps Samus and her shots from reaching it.
/// </summary>
/// <param name="species">The Powamp species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the balloon occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class PowampBalloonEntity(PowampBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The balloon's own shot reaction, built for this occupant.</summary>
    private readonly EnemyShotReaction ownShot = species.PowampBalloonShotReaction(occupant);

    /// <inheritdoc/>
    public void Initialize() => PowampBehavior.InitializePowampBalloon(Place);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PowampBehavior.AdvancePowampBalloonTypedAnimation(Place, Place.RequireSpeciesState<PowampBalloonState>());

    /// <summary>Whether the balloon's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == PowampReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the balloon's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == PowampReactions.Touch)
            species.ReactToPowampBalloonTouch(Occupant, context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The balloon's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == PowampReactions.Shot ? ownShot : base.ShotReaction;

    /// <summary>The Powamp's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == PowampReactions.Shot ? species.PowampBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The balloon's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction => LeavesPowerBombToSpecies ? species.PowampBalloonPowerBombReaction : base.PowerBombReaction;

    /// <summary>The Powamp's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == PowampReactions.Shot;
}
