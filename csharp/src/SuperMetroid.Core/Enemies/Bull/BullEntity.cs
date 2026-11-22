namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// One Bull occupant: its initialization, main AI, typed animation and immune-hit recoil after shots and bombs, with the
/// shared reactions otherwise. Its state inherits the shot-reaction guard of the previous Bull in its place.
/// </summary>
/// <param name="species">The Bull species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the Bull occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed partial class BullEntity(BullBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, BullSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The constructor's <c>species</c>, kept so a debugger state rebuilds the object through it (#2602).</summary>
    private readonly BullBehavior species = species;

    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal BullSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly BullEnemyState state = species.InitializeBull(settings, place);

    /// <summary>Kicks the Bull straight up from a bomb that left its health alone.</summary>
    /// <param name="context">The bomb's hit.</param>
    internal void KickFromUndamagingHit(EnemyBombHitContext context)
    {
        if (Ensure.NotNull(context).Enemy.Health == context.HealthBefore)
            BullBehavior.ResolveBullImmuneShot(context.Enemy, state, BullMovementDefinitions.BombKickAngle);
    }

    /// <summary>Kicks the Bull along a shot that froze it or left its health alone.</summary>
    /// <param name="context">The shot's hit.</param>
    internal void KickFromUndamagingHit(EnemyShotHitContext context)
    {
        if (Ensure.NotNull(context).Froze || context.Enemy.Health == context.HealthBefore)
            BullBehavior.ResolveBullImmuneShot(context.Enemy, state, BullMovementDefinitions.ShotAngles.For(context.ProjectileDirection));
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) =>
        BullBehavior.RunBullMain(Settings, Place, state, frame.Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => BullBehavior.AdvanceBullAnimation(Place, state);

    /// <summary>The Bull's own recoil reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == BullReactions.Shot ? OwnShot : base.ShotReaction;

    /// <summary>The Bull's own shot reaction, built once for this Bull with its state (#2554).</summary>
    private EnemyShotReaction OwnShot => field ??= BullBehavior.ShotReaction(KickFromUndamagingHit);

    /// <summary>The Bull's own bomb stages, built once for this Bull with its state (#2554).</summary>
    private EnemyBombReaction OwnBomb => field ??= BullBehavior.BombReaction(KickFromUndamagingHit);

    /// <summary>The Bull's own bomb recoil around common bomb damage, for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == BullReactions.Shot ? OwnBomb : base.CommonBombDamageReaction(context);

    /// <summary>The Bull's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == BullReactions.Shot;
}
