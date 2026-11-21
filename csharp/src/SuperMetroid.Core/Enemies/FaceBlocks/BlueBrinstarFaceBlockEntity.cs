namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>One blue Brinstar face block occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The blue Brinstar face block species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="surroundings">The room, Samus, controller and camera the population load sets the enemy up in.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class BlueBrinstarFaceBlockEntity(BlueBrinstarFaceBlockBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemyPopulationSurroundings surroundings, FaceBlockSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal FaceBlockSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly BlueBrinstarFaceBlockEnemyState state = species.InitializeBlueBrinstarFaceBlock(place, surroundings.Samus);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunBlueBrinstarFaceBlockMainAi(Settings, Place, state, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => BlueBrinstarFaceBlockBehavior.AdvanceBlueBrinstarFaceBlockTypedAnimation(Place, state);

    /// <summary>The pass-through shot reaction where the face block's definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction =>
        Occupant.Definition.Shot == FaceBlockReactions.Shot ? BlueBrinstarFaceBlockBehavior.PassThroughShot : base.ShotReaction;

    /// <summary>
    /// Passes a bomb that selects the face block's own shot reaction through it in place of common bomb damage, once the
    /// ignoring check has passed.
    /// </summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReplaceCommonBombDamage(EnemyBombContext context)
    {
        if (context.SelectedShot != FaceBlockReactions.Shot)
            return base.TryReplaceCommonBombDamage(context);
        ProjectilePassThrough.Bomb(context);
        return true;
    }

    /// <summary>The face block's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == FaceBlockReactions.Shot;
}
