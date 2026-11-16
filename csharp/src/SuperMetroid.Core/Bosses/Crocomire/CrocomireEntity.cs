using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// One of Crocomire's occupants, his body (<see cref="CrocomireBodyEntity"/>) or his tongue
/// (<see cref="CrocomireTongueEntity"/>): both meet Samus and her shots by their frames' hitboxes, route the touch and
/// shot their definitions lead to through Crocomire's component contact, and own his hitboxes' shot reactions.
/// </summary>
/// <param name="species">The Crocomire species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class CrocomireEntity(CrocomireBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IEnemyPrivateShotReaction
{
    /// <summary>The Crocomire species owner.</summary>
    protected CrocomireBehavior Species => species;

    /// <inheritdoc/>
    public abstract void Initialize();

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public abstract void AdvanceTypedAnimation(EnemyAnimationCueContext context);

    /// <inheritdoc/>
    public abstract bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction);

    /// <summary>None: the body's definition names the ignoring touch and the tongue's the ordinary one.</summary>
    public EnemyReaction? SpeciesTouch => null;

    /// <summary>None: the body's definition names no shot reaction, and every live hitbox names its own.</summary>
    public EnemyReaction? SpeciesShot => null;

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
    public override EnemyShotReaction? ShotReaction => OwnsShot ? Species.Combat.CrocomireShotReaction : base.ShotReaction;

    /// <summary>The body's and tongue's contact and shots both meet their own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>Crocomire's own hitbox shot reactions.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == CrocomireReactions.AbsorbedShot || reaction == CrocomireReactions.DustShot ||
        reaction == CrocomireReactions.MouthShot;
}
