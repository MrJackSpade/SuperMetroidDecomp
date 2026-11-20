using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// One Torizo, Bomb Torizo (<see cref="BombTorizoEntity"/>) or Golden Torizo (<see cref="GoldenTorizoEntity"/>): both
/// set up and walk by the shared movement, are met by their own frames' hitboxes, and run their own hurt, touch, shot
/// and bomb reactions; each runs its own fight.
/// </summary>
/// <param name="species">The Torizo species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the Torizo occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class TorizoEntity(TorizoBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemySurroundingsInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IEnemyPrivateShotReaction, ILitTorizo
{
    /// <summary>The Torizo species owner.</summary>
    protected TorizoBehavior Species => species;

    /// <summary>Whether this Torizo has no health left: its belly's glow ends on the first update it sees so (#1700).</summary>
    public bool IsDefeated => Place.Health == 0;

    /// <inheritdoc/>
    public void Initialize(EnemyPopulationSurroundings surroundings) => Species.Movement.InitializeTorizo(Place, Variant, surroundings.Samus, surroundings.ControllerInput);

    /// <summary>Which Torizo this is, Crateria's Bomb Torizo or Norfair's Golden Torizo, whose values his fight reads (#2301, #2410).</summary>
    protected abstract TorizoVariant Variant { get; }

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.AdvanceTorizoTypedAnimation(Occupant, Place, Place.RequireSpeciesState<TorizoEnemyState>(), this, context);

    /// <inheritdoc/>
    public EnemyHitboxContact FindHitboxReaction(EnemyHitboxProbe probe)
        => TorizoCombat.FindTorizoHitboxReaction(Place, probe);

    /// <summary>Both Torizos' contact and shots meet their own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>
    /// The Torizo's own hurt behaviour (#2240); the common animation runner still advances after it, as the enemies'
    /// update does.
    /// </summary>
    /// <param name="frame">The enemy frame.</param>
    public abstract override bool RunHurt(RoomEnemyFrame frame);

    /// <summary>Whether the Torizo's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == TorizoReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the Torizo's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == TorizoReactions.Touch)
            Species.Combat.ReactToTorizoTouch(context, context.Place.RequireSpeciesState<TorizoEnemyState>());
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Torizo's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == OwnShot ? OwnShotReaction : base.ShotReaction;

    /// <summary>The Torizo's own shot reaction (#2240).</summary>
    protected abstract EnemyShotReaction OwnShotReaction { get; }

    /// <summary>The shot reaction of the Torizo's own that its definition names (#2558).</summary>
    protected abstract EnemyReaction OwnShot { get; }

    /// <summary>Runs the Torizo's own reaction to every bomb that overlaps it, before the ignoring check (#2240).</summary>
    /// <param name="context">The bomb contact.</param>
    public abstract override bool TryReactToBomb(EnemyBombContext context);

    /// <summary>Shows a step of the belly glow this Torizo's fight start lit, on this Torizo (#2240).</summary>
    /// <param name="frame">The glow's step.</param>
    public void ShowGlow(ColourEffectFrame frame) => TorizoBehavior.ShowBellyGlow(Place, Place.RequireSpeciesState<TorizoEnemyState>(), frame);

    /// <summary>The Torizo shot reactions both Torizos own, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names.</param>
    public virtual bool OwnsPrivateShotReaction(EnemyReaction reaction) =>
        reaction == TorizoReactions.Shot || reaction == TorizoReactions.StandUpSitDownShot;
}
