using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>One Botwoon occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Botwoon species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BotwoonEntity(BotwoonBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeBotwoon(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunBotwoonMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceBotwoonTypedAnimation(Place, context);

    /// <summary>Whether Botwoon's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs Botwoon's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            species.ReactToBotwoonTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>Botwoon's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.BotwoonShotReaction : base.ShotReaction;

    /// <summary>Botwoon's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? species.BotwoonBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>Botwoon's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction => LeavesPowerBombToSpecies ? species.BotwoonPowerBombReaction : base.PowerBombReaction;

    /// <summary>Botwoon's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
