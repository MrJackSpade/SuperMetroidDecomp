using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One Ceres steam occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Ceres steam species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="motions">The motions the room gives its vents, which give this vent the one its variant names.</param>
internal sealed class CeresSteamEntity(CeresSteamBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, CeresSteamVentMotions motions)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyFrameHitboxes
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal CeresSteamEnemyState State { get; } = species.InitializeCeresSteam(place, motions);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => CeresSteamBehavior.RunCeresSteamMainAi(Place);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => CeresSteamBehavior.AdvanceCeresSteamAnimation(Place);

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction) => CeresSteamBehavior.FindHitboxReaction(Place, probe, out reaction);

    /// <summary>The steam's touch reaction, which its definition leaves to it.</summary>
    public EnemyReaction? SpeciesTouch => CeresSteamReactions.Touch;

    /// <summary>None: the steam's definition names the ignoring shot reaction.</summary>
    public EnemyReaction? SpeciesShot => null;

    /// <summary>Whether the steam's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the steam's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            species.ReactToCeresSteamTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The steam's contact and shots both meet its own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;
}
