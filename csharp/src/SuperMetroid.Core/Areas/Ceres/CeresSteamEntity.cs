using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One Ceres steam occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Ceres steam species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class CeresSteamEntity(CeresSteamBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, CeresSteamEnemyState state)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyFrameHitboxes
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Ceres steam species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the enemy occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="motions">The motions the room gives its vents, which give this vent the one its variant names.</param>
    /// <param name="settings">What the enemy's population entry tells its species.</param>
    internal CeresSteamEntity(CeresSteamBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        CeresSteamVentMotions motions, CeresSteamSpawnSettings settings)
        : this(species, occupant, place, common, species.InitializeCeresSteam(settings, place, motions))
    {
    }

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly CeresSteamEnemyState state = state;

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => CeresSteamBehavior.RunCeresSteamMainAi(Place, state);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => CeresSteamBehavior.AdvanceCeresSteamAnimation(Place, state);

    /// <inheritdoc/>
    public EnemyHitboxContact FindHitboxReaction(EnemyHitboxProbe probe) => CeresSteamBehavior.FindHitboxReaction(Place, probe);

    /// <summary>Whether the steam's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == CeresSteamReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the steam's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == CeresSteamReactions.Touch)
            species.ReactToCeresSteamTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The steam's contact and shots both meet its own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;
}
