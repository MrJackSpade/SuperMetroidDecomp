using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// One of Crocomire's occupants, his body (<see cref="CrocomireBodyEntity"/>) or his tongue
/// (<see cref="CrocomireTongueEntity"/>): both meet Samus and her shots by their frames' hitboxes, route the touch and
/// shot their definitions lead to through Crocomire's component contact, and own his hitboxes' shot reactions.
/// </summary>
/// <param name="species">The Crocomire species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class CrocomireEntity(CrocomireBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IEnemyPrivateShotReaction
{
    /// <summary>The Crocomire species owner.</summary>
    protected CrocomireBehavior Species => species;

    /// <summary>The encounter the part is part of, which it holds from its setup (#2177) and its own reactions act on (#2420).</summary>
    internal abstract CrocomireEnemyState Encounter { get; }

    /// <summary>The shot reaction that routes the part's hitboxes' reactions to its encounter.</summary>
    private EnemyShotReaction OwnShotReaction => field ??= Species.Combat.CrocomireShotReactionOf(this);


    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public abstract void AdvanceTypedAnimation(EnemyAnimationCueContext context);

    /// <inheritdoc/>
    public abstract EnemyHitboxContact FindHitboxReaction(EnemyHitboxProbe probe);

    /// <summary>Whether the touch the part's definition names leads to Crocomire's component contact, which his frames' hitboxes decide.</summary>
    protected abstract bool OwnsTouch { get; }

    /// <summary>Whether the shot the part's definition names leads to Crocomire's hitboxes' reactions.</summary>
    protected abstract bool OwnsShot { get; }

    /// <summary>Whether Crocomire's component contact or a shared touch applies.</summary>
    public override bool ReactsToTouch => OwnsTouch || base.ReactsToTouch;

    /// <summary>Runs Crocomire's component contact, or the shared touch.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (OwnsTouch)
            Species.Combat.ReactToCrocomireTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The shot reaction that routes the part's hitboxes' reactions where its definition leads to them, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => OwnsShot ? OwnShotReaction : base.ShotReaction;

    /// <summary>The body's and tongue's contact and shots both meet their own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>Crocomire's own hitbox shot reactions.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) =>
        reaction == CrocomireReactions.AbsorbedShot || reaction == CrocomireReactions.DustShot ||
        reaction == CrocomireReactions.MouthShot;
}
