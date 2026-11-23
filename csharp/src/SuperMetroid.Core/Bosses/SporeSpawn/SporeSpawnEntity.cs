using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>One SporeSpawn occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The SporeSpawn species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class SporeSpawnEntity(SporeSpawnBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, SporeSpawnEnemyState state)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IEnemyPrivateShotReaction
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The SporeSpawn species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the enemy occupies.</param>
    /// <param name="common">The shared participation.</param>
    internal SporeSpawnEntity(SporeSpawnBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
        : this(species, occupant, place, common, species.InitializeSporeSpawn(occupant, place))
    {
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunSporeSpawnMainAi(Place, Encounter);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceSporeSpawnAnimation(Place, Encounter, context.Samus);

    /// <summary>The encounter his body holds from its initialization (#2231), which his own reactions act on (#2420).</summary>
    /// <exception cref="InvalidOperationException">The body has not initialized.</exception>
    internal SporeSpawnEnemyState Encounter { get; } = state;

    /// <summary>His own shot reaction, which resolves the rest of a shot on this encounter after the common damage.</summary>
    private EnemyShotReaction OwnShotReaction => field ??= species.SporeSpawnShotReactionOf(this);

    /// <summary>His own stages around common bomb damage, which resolve the rest of a bomb on this encounter.</summary>
    private EnemyBombReaction OwnBombReaction => field ??= SporeSpawnBehavior.SporeSpawnBombReactionOf(this);

    /// <summary>Resolves the rest of a bomb on this encounter after its common damage.</summary>
    internal void ResolveShotAfterCommon(EnemyBombHitContext context) =>
        species.ResolveSporeSpawnShotAfterCommon(Ensure.NotNull(context).Enemy, Encounter);

    /// <summary>Resolves the rest of a shot on this encounter after its common damage.</summary>
    internal void ResolveShotAfterCommon(EnemyShotHitContext context) =>
        species.ResolveSporeSpawnShotAfterCommon(Ensure.NotNull(context).Enemy, Encounter);

    /// <inheritdoc/>
    public EnemyHitboxContact FindHitboxReaction(EnemyHitboxProbe probe)
        => SporeSpawnBehavior.FindSporeSpawnHitboxReaction(Place, probe);

    /// <summary>Whether Spore Spawn's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == SporeSpawnReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs Spore Spawn's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == SporeSpawnReactions.Touch)
            species.ReactToSporeSpawnTouch(context, Encounter);
        else
            base.ReactToTouch(context);
    }

    /// <summary>Spore Spawn's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == SporeSpawnReactions.Shot ? OwnShotReaction : base.ShotReaction;

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
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == SporeSpawnReactions.Shot ? OwnBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>Spore Spawn's contact and shots both meet its own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>Spore Spawn's shot reaction and its dud shot, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) =>
        reaction == SporeSpawnReactions.Shot || reaction == EnemyReaction.DudShot;
}
