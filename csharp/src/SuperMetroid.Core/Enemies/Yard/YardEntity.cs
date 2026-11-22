using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>One Yard occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Yard species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed partial class YardEntity(YardEnemyBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, YardSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The species' YardShotReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyShotReaction OwnYardShotReaction => field ??= YardEnemyBehavior.YardShotReaction(KickShellIntoAir);

    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal YardSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly YardEnemyState state = YardEnemyBehavior.InitializeYard(settings, place);

    /// <summary>
    /// Kicks the struck Yard's shell into the air before vulnerability, after the shot's impact: a bomb or a power bomb goes
    /// on to its vulnerability damage instead.
    /// </summary>
    /// <param name="c">The shot meeting the Yard.</param>
    /// <returns>Whether the kick replaced the vulnerability damage.</returns>
    internal bool KickShellIntoAir(in EnemyShotContext c)
    {
        if (c.Family is SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb)
            return false;
        species.ResolveYardBeamLaunch(c.Enemy, state, c.Samus);
        return true;
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunYardMainAi(Place, state, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceYardTypedAnimation(Place, state);

    /// <summary>Whether the Yard's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == YardReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the Yard's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == YardReactions.Touch)
            species.ReactToYardTouch(Occupant, Settings, context, state);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Yard's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == YardReactions.Shot ? OwnYardShotReaction : base.ShotReaction;

    /// <summary>The Yard's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == YardReactions.Shot;
}
