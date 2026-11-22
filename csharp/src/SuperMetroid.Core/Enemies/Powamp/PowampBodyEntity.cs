namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// A Powamp's body (#2163): it rests below, rises with its balloon, and runs its own touch, shot, bomb and Power Bomb
/// reactions where its definition leaves them to it, each carrying the hit on to its balloon.
/// </summary>
/// <param name="species">The Powamp species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed partial class PowampBodyEntity(PowampBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, PowampBodySpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal PowampBodySpawnSettings Settings { get; } = settings;

    /// <summary>The balloon the room lists right before the body, which the body receives as it is built (#2554).</summary>
    private readonly PowampBalloonEntity balloon = species.PlacedBalloon(place);

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly PowampBodyState state = PowampBehavior.InitializePowampBody(place);

    /// <summary>The body's own shot reaction, built once for this body.</summary>
    private EnemyShotReaction OwnShot => field ??= PowampBehavior.PowampBodyShotReaction(this);

    /// <summary>The body's own bomb reaction, built once for this body.</summary>
    private EnemyBombReaction OwnBomb => field ??= PowampBehavior.PowampBodyBombReaction(this);

    /// <summary>The body's own Power Bomb reaction, built once for this body.</summary>
    private EnemyPowerBombReaction OwnPowerBomb => field ??= PowampBehavior.PowampBodyPowerBombReaction(this);

    /// <summary>Whether the body refuses shots: its placement refuses them, or it is dying (#2474).</summary>
    /// <param name="body">The place the body occupies.</param>
    internal bool RefusesContact(RoomEnemyPlace body) => PowampBehavior.BodyRefusesContact(Settings, state);

    /// <summary>Carries a shot's freeze, hurt flash and fatal damage on from the body to its balloon, after the common damage.</summary>
    /// <param name="context">The shot's hit.</param>
    internal void CarryShotOnToBalloon(EnemyShotHitContext context) =>
        PowampBehavior.ResolvePowampShotAfterCommon(Ensure.NotNull(context).Enemy, state, balloon.Place);

    /// <summary>Carries a bomb's freeze, hurt flash and fatal damage on from the body to its balloon, after the common damage.</summary>
    /// <param name="context">The bomb's hit.</param>
    internal void CarryShotOnToBalloon(EnemyBombHitContext context) =>
        PowampBehavior.ResolvePowampShotAfterCommon(Ensure.NotNull(context).Enemy, state, balloon.Place);

    /// <summary>Carries a Power Bomb's hit on from the body to its balloon, after the common damage.</summary>
    /// <param name="context">The blast meeting the body.</param>
    internal void CarryPowerBombOnToBalloon(EnemyPowerBombContext context) => EnemyPartnerHits.CarryOnto(context.Enemy, balloon.Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunPowampMain(Occupant, Place, state, balloon, frame.Level, frame.Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PowampBehavior.AdvancePowampBodyTypedAnimation(Place, state);

    /// <summary>Whether the body's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == PowampReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the body's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == PowampReactions.Touch)
            species.ReactToPowampBodyTouch(Occupant, Settings, context, state, balloon.Place);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The body's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == PowampReactions.Shot ? OwnShot : base.ShotReaction;

    /// <summary>The Powamp's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == PowampReactions.Shot ? OwnBomb : base.CommonBombDamageReaction(context);

    /// <summary>The body's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction => LeavesPowerBombToSpecies ? OwnPowerBomb : base.PowerBombReaction;

    /// <summary>The Powamp's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == PowampReactions.Shot;
}
