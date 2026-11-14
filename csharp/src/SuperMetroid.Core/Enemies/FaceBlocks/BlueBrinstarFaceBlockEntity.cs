namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>One blue Brinstar face block occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The blue Brinstar face block species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BlueBrinstarFaceBlockEntity(BlueBrinstarFaceBlockBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemySurroundingsInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize(EnemySpawnSurroundings surroundings) => species.RunBlueBrinstarFaceBlockInitializationAi(Slot, surroundings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunBlueBrinstarFaceBlockMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => BlueBrinstarFaceBlockBehavior.AdvanceBlueBrinstarFaceBlockTypedAnimation(Slot, context);

    /// <summary>The pass-through shot reaction where the face block's definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction =>
        LeavesShotToSpecies ? BlueBrinstarFaceBlockBehavior.PassThroughShot : base.ShotReaction;

    /// <summary>
    /// Passes a bomb that selects the face block's own shot reaction through it in place of common bomb damage, once the
    /// ignoring check has passed.
    /// </summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReplaceCommonBombDamage(EnemyBombContext context)
    {
        if (context.SelectedShot is not null)
            return base.TryReplaceCommonBombDamage(context);
        ProjectilePassThrough.Bomb(context);
        return true;
    }

    /// <summary>The face block's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
