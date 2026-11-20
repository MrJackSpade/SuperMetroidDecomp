using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>One SporeSpawn occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The SporeSpawn species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class SporeSpawnEntity(SporeSpawnBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IEnemyPrivateShotReaction
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal SporeSpawnEnemyState State { get; } = species.InitializeSporeSpawn(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunSporeSpawnMainAi(Place, Encounter);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceSporeSpawnAnimation(Place, Encounter, context.Samus);

    /// <summary>The encounter his body holds from its initialization (#2231), which his own reactions act on (#2420).</summary>
    /// <exception cref="InvalidOperationException">The body has not initialized.</exception>
    internal SporeSpawnEnemyState Encounter => State;

    /// <summary>His own shot reaction, which resolves the rest of a shot on this encounter after the common damage.</summary>
    private EnemyShotReaction OwnShotReaction => field ??= species.SporeSpawnShotReactionOf(this);

    /// <summary>His own stages around common bomb damage, which resolve the rest of a bomb on this encounter.</summary>
    private EnemyBombReaction OwnBombReaction => field ??= species.SporeSpawnBombReactionOf(this);

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction)
        => SporeSpawnBehavior.FindSporeSpawnHitboxReaction(Place, probe, out reaction);

    /// <summary>Spore Spawn's touch reaction, which its definition leaves to it.</summary>
    public EnemyReaction? SpeciesTouch => SporeSpawnReactions.Touch;

    /// <summary>Spore Spawn's shot reaction, which its definition leaves to it.</summary>
    public EnemyReaction? SpeciesShot => SporeSpawnReactions.Shot;

    /// <summary>Whether Spore Spawn's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs Spore Spawn's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            species.ReactToSporeSpawnTouch(context, Encounter);
        else
            base.ReactToTouch(context);
    }

    /// <summary>Spore Spawn's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? OwnShotReaction : base.ShotReaction;

    /// <summary>
    /// Spore Spawn's dud shot for a bomb whose hitbox selects it, in place of common bomb damage once the ignoring check has
    /// passed.
    /// </summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReplaceCommonBombDamage(EnemyBombContext context)
    {
        if (context.SelectedShot != EnemyReaction.DudShot)
            return base.TryReplaceCommonBombDamage(context);
        species.CreateBombDudShot(context);
        return true;
    }

    /// <summary>Spore Spawn's own stages around common bomb damage, for a bomb that selects its shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == SporeSpawnReactions.Shot ? OwnBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>Spore Spawn's contact and shots both meet its own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>Spore Spawn's shot reaction and its dud shot, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == SporeSpawnReactions.Shot || reaction == EnemyReaction.DudShot;
}
