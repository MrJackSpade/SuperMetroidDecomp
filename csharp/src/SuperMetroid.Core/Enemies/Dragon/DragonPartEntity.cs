namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// The reactions a Dragon's body and its wing share (#2557): where their definition leaves Samus's touch, shots, bombs or the
/// Power Bomb to the species, the Dragon's own reaction runs, and otherwise the shared one. The definition leaves Samus's
/// touch or shots to the species by naming <see cref="DragonReactions"/>' own (#2558).
/// </summary>
/// <param name="species">The Dragon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the part occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract partial class DragonPartEntity(DragonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyPrivateShotReaction
{
    /// <summary>The Dragon species owner.</summary>
    private protected DragonBehavior Species => species;

    /// <summary>Whether the part's definition leaves Samus's touch to the species.</summary>
    private bool LeavesTouchToDragon => Occupant.Definition.Touch == DragonReactions.Touch;

    /// <summary>Whether the Dragon's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToDragon || base.ReactsToTouch;

    /// <summary>Runs the Dragon's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToDragon)
            Species.ReactToDragonTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Dragon's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == DragonReactions.Shot ? Species.DragonShotReaction : base.ShotReaction;

    /// <summary>The Dragon's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == DragonReactions.Shot ? Species.DragonBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Dragon's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction => LeavesPowerBombToSpecies ? Species.DragonPowerBombReaction : base.PowerBombReaction;

    /// <summary>The Dragon's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == DragonReactions.Shot;
}
