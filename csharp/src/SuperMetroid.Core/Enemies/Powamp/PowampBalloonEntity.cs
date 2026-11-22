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
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed partial class PowampBalloonEntity(PowampBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, PowampBalloonSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal PowampBalloonSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal PowampBalloonState State { get; } = PowampBehavior.InitializePowampBalloon(place, settings);

    /// <summary>The balloon's own shot reaction, built once for this balloon.</summary>
    private EnemyShotReaction OwnShot => field ??= PowampBehavior.PowampBalloonShotReaction(this);

    /// <summary>The balloon's own bomb reaction, built once for this balloon.</summary>
    private EnemyBombReaction OwnBomb => field ??= PowampBehavior.PowampBalloonBombReaction(this);

    /// <summary>Whether the balloon refuses shots: it refuses contact (#2474), as a grappled balloon does.</summary>
    /// <param name="balloon">The place the balloon occupies.</param>
    internal bool RefusesContact(RoomEnemyPlace balloon) => State.Contact == PowampContact.Refuses;

    /// <summary>Carries a shot on from the balloon to the enemy the room lists before it, after the common damage.</summary>
    /// <param name="context">The shot's hit.</param>
    internal void CarryShotOnBefore(EnemyShotHitContext context) => species.ResolvePowampBalloonShotAfterCommon(Ensure.NotNull(context).Enemy, State);

    /// <summary>Carries a bomb on from the balloon to the enemy the room lists before it, after the common damage.</summary>
    /// <param name="context">The bomb's hit.</param>
    internal void CarryShotOnBefore(EnemyBombHitContext context) => species.ResolvePowampBalloonShotAfterCommon(Ensure.NotNull(context).Enemy, State);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PowampBehavior.AdvancePowampBalloonTypedAnimation(Place, State);

    /// <summary>Whether the balloon's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == PowampReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the balloon's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == PowampReactions.Touch)
            species.ReactToPowampBalloonTouch(Occupant, State, context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The balloon's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == PowampReactions.Shot ? OwnShot : base.ShotReaction;

    /// <summary>The Powamp's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == PowampReactions.Shot ? OwnBomb : base.CommonBombDamageReaction(context);

    /// <summary>The balloon's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction => LeavesPowerBombToSpecies ? species.PowampBalloonPowerBombReaction : base.PowerBombReaction;

    /// <summary>The Powamp's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == PowampReactions.Shot;
}
