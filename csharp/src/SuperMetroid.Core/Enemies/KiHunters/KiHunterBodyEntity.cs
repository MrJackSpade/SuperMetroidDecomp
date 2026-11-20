namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// One Ki-Hunter's body, green, yellow or red (#2146): its own AI, with the shared reactions and its own reaction to the
/// shots and bombs that may strip its wings.
/// </summary>
/// <param name="species">The Ki-Hunter species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KiHunterBodyEntity(KiHunterBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeKiHunter(Occupant, Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunKiHunterBodyMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceKiHunterBodyTypedAnimation(Occupant, Place);

    /// <summary>The Ki-Hunter's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == KiHunterReactions.Shot ? species.KiHunterShotReaction : base.ShotReaction;

    /// <summary>The Ki-Hunter's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == KiHunterReactions.Shot ? species.KiHunterBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Ki-Hunter's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == KiHunterReactions.Shot;
}
