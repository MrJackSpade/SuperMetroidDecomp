using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One Ceres steam occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Ceres steam species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class CeresSteamEntity(CeresSteamBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeCeresSteam(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => CeresSteamBehavior.RunCeresSteamMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => CeresSteamBehavior.AdvanceCeresSteamAnimation(Slot);

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction) => CeresSteamBehavior.FindHitboxReaction(Slot, probe, out reaction);

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
