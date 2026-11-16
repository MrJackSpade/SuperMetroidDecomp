using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>One SporeSpawn occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The SporeSpawn species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class SporeSpawnEntity(SporeSpawnBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeSporeSpawn(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunSporeSpawnMainAi(Place, Encounter);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceSporeSpawnAnimation(Place, Encounter, context.Samus);

    /// <summary>The encounter his body holds from its initialization (#2231).</summary>
    /// <exception cref="InvalidOperationException">The body has not initialized.</exception>
    private SporeSpawnEnemyState Encounter => Place.RequireSpeciesState<SporeSpawnEnemyState>();

    /// <summary>The encounter <paramref name="body"/> holds, which his initialization installed on it (#2231).</summary>
    /// <param name="body">A place holding Spore Spawn's body, initialized.</param>
    /// <exception cref="InvalidOperationException">The place holds no initialized Spore Spawn.</exception>
    internal static SporeSpawnEnemyState EncounterOf(RoomEnemyPlace body) =>
        Ensure.NotNull(body).Entity is SporeSpawnEntity entity
            ? entity.Encounter
            : throw new InvalidOperationException($"Enemy place {body.Ordinal} holds no Spore Spawn.");

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        return SporeSpawnBehavior.FindSporeSpawnHitboxReaction(Place, probe, out reaction);
    }

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
            species.ReactToSporeSpawnTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>Spore Spawn's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.SporeSpawnShotReaction : base.ShotReaction;

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
        context.SelectedShot == SporeSpawnReactions.Shot ? species.SporeSpawnBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>Spore Spawn's contact and shots both meet its own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>Spore Spawn's shot reaction and its dud shot, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == SporeSpawnReactions.Shot || reaction == EnemyReaction.DudShot;
}
