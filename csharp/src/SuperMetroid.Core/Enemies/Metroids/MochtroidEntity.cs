namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>One Mochtroid occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Mochtroid species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MochtroidEntity(MochtroidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => MochtroidBehavior.InitializeMochtroid(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunMochtroidMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => MochtroidBehavior.AdvanceMochtroidTypedAnimation(Place);

    /// <summary>Whether the Mochtroid's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == MochtroidReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the Mochtroid's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == MochtroidReactions.Touch)
            species.ReactToMochtroidTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Mochtroid's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == MochtroidReactions.Shot ? MochtroidBehavior.MochtroidShotReaction : base.ShotReaction;

    /// <summary>The Mochtroid's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == MochtroidReactions.Shot;
}
