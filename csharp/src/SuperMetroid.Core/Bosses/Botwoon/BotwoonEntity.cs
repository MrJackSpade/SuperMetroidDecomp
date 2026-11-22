using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>One Botwoon occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Botwoon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed partial class BotwoonEntity(BotwoonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The species' BotwoonShotReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyShotReaction OwnBotwoonShotReaction => field ??= BotwoonBehavior.BotwoonShotReaction(state);

    /// <summary>The species' BotwoonPowerBombReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyPowerBombReaction OwnBotwoonPowerBombReaction => field ??= BotwoonBehavior.BotwoonPowerBombReaction(state);

    /// <summary>The species' BotwoonBombReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyBombReaction OwnBotwoonBombReaction => field ??= BotwoonBehavior.BotwoonBombReaction(state);

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly BotwoonEnemyState state = species.InitializeBotwoon(occupant, place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunBotwoonMainAi(Occupant, Place, state, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceBotwoonTypedAnimation(Place, state);

    /// <summary>Whether Botwoon's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == BotwoonReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs Botwoon's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == BotwoonReactions.Touch)
            species.ReactToBotwoonTouch(context, state);
        else
            base.ReactToTouch(context);
    }

    /// <summary>Botwoon's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == BotwoonReactions.Shot ? OwnBotwoonShotReaction : base.ShotReaction;

    /// <summary>Botwoon's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == BotwoonReactions.Shot ? OwnBotwoonBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>Botwoon's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction => LeavesPowerBombToSpecies ? OwnBotwoonPowerBombReaction : base.PowerBombReaction;

    /// <summary>Botwoon's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == BotwoonReactions.Shot;
}
