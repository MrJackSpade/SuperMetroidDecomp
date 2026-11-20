namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// A Powamp's body (#2163): it rests below, rises with its balloon, and runs its own touch, shot, bomb and Power Bomb
/// reactions where its definition leaves them to it, each carrying the hit on to its balloon.
/// </summary>
/// <param name="species">The Powamp species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class PowampBodyEntity(PowampBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The body's own shot reaction, built for this occupant.</summary>
    private readonly EnemyShotReaction ownShot = species.PowampBodyShotReaction(occupant, occupant.SpawnSettings<PowampSpawnSettings>());

    /// <inheritdoc/>
    public void Initialize() => species.InitializePowampBody(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunPowampMain(Occupant, Place, Place.RequireSpeciesState<PowampBodyState>(), frame.Level, frame.Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PowampBehavior.AdvancePowampBodyTypedAnimation(Place, Place.RequireSpeciesState<PowampBodyState>());

    /// <summary>Whether the body's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == PowampReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the body's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == PowampReactions.Touch)
            species.ReactToPowampBodyTouch(Occupant, Occupant.SpawnSettings<PowampSpawnSettings>(), context, context.Place.RequireSpeciesState<PowampBodyState>());
        else
            base.ReactToTouch(context);
    }

    /// <summary>The body's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == PowampReactions.Shot ? ownShot : base.ShotReaction;

    /// <summary>The Powamp's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == PowampReactions.Shot ? species.PowampBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The body's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction => LeavesPowerBombToSpecies ? species.PowampBodyPowerBombReaction : base.PowerBombReaction;

    /// <summary>The Powamp's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == PowampReactions.Shot;
}
