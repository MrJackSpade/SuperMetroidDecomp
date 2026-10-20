namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>One BlueBrinstarFaceBlock occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The BlueBrinstarFaceBlock species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BlueBrinstarFaceBlockEntity(BlueBrinstarFaceBlockBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemySurroundingsInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemySurroundingsInitializationAiHandler? FindSurroundingsInitialization() => species.RunBlueBrinstarFaceBlockInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunBlueBrinstarFaceBlockMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => BlueBrinstarFaceBlockBehavior.AdvanceBlueBrinstarFaceBlockTypedAnimation;

    /// <summary>The pass-through shot reaction, ahead of the shared ones (only other species' precede it there).</summary>
    public override EnemyShotReaction? FindShot() =>
        BlueBrinstarFaceBlockBehavior.PassThroughShot.Matches(Slot) ? BlueBrinstarFaceBlockBehavior.PassThroughShot : base.FindShot();

    /// <summary>The bomb pass-through after the no-op check, ahead of the shared ones.</summary>
    public override Action<EnemyBombContext>? FindBombSpecialAfterNoOp(EnemyBombContext context) =>
        context.SelectedShotAi == EnemyInteractionCallback.BlueBrinstarFaceBlockShot ? ProjectilePassThrough.Bomb : base.FindBombSpecialAfterNoOp(context);

    /// <summary>The face block's own shot callback, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        callback == EnemyInteractionCallback.BlueBrinstarFaceBlockShot;
}
