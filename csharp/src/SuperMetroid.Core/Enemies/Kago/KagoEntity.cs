using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>One Kago occupant: its own AI, with the shared reactions.</summary>
<<<<<<< HEAD
/// <param name="species">The Kago species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells it: the hits it takes.</param>
internal sealed class KagoEntity(KagoBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, KagoSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyTypedAnimation, IEnemyPrivateShotReaction
||||||| 418ca79935
/// <param name="species">The Kago species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KagoEntity(KagoBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyTypedAnimation, IEnemyPrivateShotReaction
=======
[RoomLifetime]
internal sealed partial class KagoEntity
    : CommonParticipationEnemyEntity, IEnemyInitialization, IEnemyTypedAnimation, IEnemyPrivateShotReaction
>>>>>>> origin/main
{
    /// <summary>Creates a <see cref="KagoEntity"/>.</summary>
    /// <param name="species">The Kago species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the enemy occupies.</param>
    /// <param name="common">The shared participation.</param>
    public KagoEntity(KagoBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
        : base(occupant, place, common)
    {
        this.species = species;
        this.ownShot = KagoBehavior.KagoShotReaction(this);
        this.ownBomb = KagoBehavior.KagoBombReaction(this);
    }

    /// <summary>The Kago species owner, whose shell response the Kago's own reactions run.</summary>
    private readonly KagoBehavior species;

    /// <summary>Applies Kago's shell response after a bomb's common damage on this Kago.</summary>
    internal void ResolveShellAfterCommon(EnemyBombHitContext context) =>
        species.ResolveKagoShotAfterCommon(Occupant, Ensure.NotNull(context).Enemy, context.Enemy.RequireSpeciesState<KagoEnemyState>());

    /// <summary>Applies Kago's shell response after a shot's common damage on this Kago.</summary>
    internal void ResolveShellAfterCommon(EnemyShotHitContext context) =>
        species.ResolveKagoShotAfterCommon(Occupant, Ensure.NotNull(context).Enemy, context.Enemy.RequireSpeciesState<KagoEnemyState>());

    /// <summary>The Kago's own shot reaction, built for this occupant.</summary>
<<<<<<< HEAD
    private EnemyShotReaction OwnShot => field ??= species.KagoShotReaction(Occupant, settings, state);
||||||| 418ca79935
    private readonly EnemyShotReaction ownShot = species.KagoShotReaction(occupant);
=======
    private readonly EnemyShotReaction ownShot;
>>>>>>> origin/main

    /// <summary>The Kago's own stages around common bomb damage, built for this occupant.</summary>
<<<<<<< HEAD
    private EnemyBombReaction OwnBomb => field ??= species.KagoBombReaction(Occupant, settings, state);

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly KagoEnemyState state = KagoBehavior.InitializeKago(place);
||||||| 418ca79935
    private readonly EnemyBombReaction ownBomb = species.KagoBombReaction(occupant);
=======
    private readonly EnemyBombReaction ownBomb;
>>>>>>> origin/main

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => KagoBehavior.AdvanceKagoTypedAnimation(Place, state);

    /// <summary>The Kago's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == KagoReactions.Shot ? OwnShot : base.ShotReaction;

    /// <summary>The Kago's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == KagoReactions.Shot ? OwnBomb : base.CommonBombDamageReaction(context);

    /// <summary>The Kago's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == KagoReactions.Shot;
}
