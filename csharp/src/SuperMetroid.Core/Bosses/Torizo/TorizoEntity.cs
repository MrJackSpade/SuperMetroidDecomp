using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// One Torizo, Bomb Torizo (<see cref="BombTorizoEntity"/>) or Golden Torizo (<see cref="GoldenTorizoEntity"/>): both
/// set up and walk by the shared movement, are met by their own frames' hitboxes, and run their own hurt, touch, shot
/// and bomb reactions; each runs its own fight.
/// </summary>
/// <param name="species">The Torizo species owner.</param>
/// <param name="place">The place the Torizo occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class TorizoEntity(TorizoBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemySurroundingsInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IEnemyPrivateShotReaction, ILitTorizo
{
    /// <summary>The Torizo species owner.</summary>
    protected TorizoBehavior Species => species;

    /// <summary>Whether this Torizo has no health left: its belly's glow ends on the first update it sees so (#1700).</summary>
    public bool IsDefeated => Place.Health == 0;

    /// <inheritdoc/>
    public void Initialize(EnemyPopulationSurroundings surroundings) => Species.Movement.RunTorizoInitializationAi(Place, Variant, surroundings);

    /// <summary>Which Torizo this is, Crateria's Bomb Torizo or Norfair's Golden Torizo, whose values his fight reads (#2301, #2410).</summary>
    protected abstract TorizoVariant Variant { get; }

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.AdvanceTorizoTypedAnimation(Place, this, context);

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction)
        => TorizoCombat.FindTorizoHitboxReaction(Place, probe, out reaction);

    /// <summary>Both Torizos' contact and shots meet their own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>Both Torizos' touch reaction, which their definitions leave to them.</summary>
    public EnemyReaction? SpeciesTouch => TorizoReactions.Touch;

    /// <summary>The shot reaction the Torizo's definition leaves to it.</summary>
    public abstract EnemyReaction? SpeciesShot { get; }

    /// <summary>
    /// The Torizo's own hurt behaviour (#2240); the common animation runner still advances after it, as the enemies'
    /// update does.
    /// </summary>
    /// <param name="frame">The enemy frame.</param>
    public abstract override bool RunHurt(RoomEnemyFrame frame);

    /// <summary>Whether the Torizo's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the Torizo's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            Species.Combat.ReactToTorizoTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Torizo's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? OwnShotReaction : base.ShotReaction;

    /// <summary>The Torizo's own shot reaction (#2240).</summary>
    protected abstract EnemyShotReaction OwnShotReaction { get; }

    /// <summary>Runs the Torizo's own reaction to every bomb that overlaps it, before the ignoring check (#2240).</summary>
    /// <param name="context">The bomb contact.</param>
    public abstract override bool TryReactToBomb(EnemyBombContext context);

    /// <summary>Shows a step of the belly glow this Torizo's fight start lit, on this Torizo (#2240).</summary>
    /// <param name="frame">The glow's step.</param>
    public void ShowGlow(ColourEffectFrame frame) => TorizoBehavior.ShowBellyGlow(Place, frame);

    /// <summary>The Torizo shot reactions both Torizos own, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names, or null.</param>
    public virtual bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == TorizoReactions.Shot || reaction == TorizoReactions.StandUpSitDownShot;
}
