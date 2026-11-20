namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// The reactions a Dragon's body and its wing share (#2557): where their definition leaves Samus's touch, shots, bombs or the
/// Power Bomb to the species, the Dragon's own reaction runs, and otherwise the shared one.
/// </summary>
/// <param name="species">The Dragon species owner.</param>
/// <param name="place">The place the part occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class DragonPartEntity(DragonBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyPrivateShotReaction
{
    /// <summary>The Dragon species owner.</summary>
    private protected DragonBehavior Species { get; } = species;

    /// <summary>Whether the Dragon's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the Dragon's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            Species.ReactToDragonTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Dragon's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? Species.DragonShotReaction : base.ShotReaction;

    /// <summary>The Dragon's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? Species.DragonBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Dragon's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction => LeavesPowerBombToSpecies ? Species.DragonPowerBombReaction : base.PowerBombReaction;

    /// <summary>The Dragon's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the Species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
