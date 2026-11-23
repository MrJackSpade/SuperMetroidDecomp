using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One Fake Kraid occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Fake Kraid species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class FakeKraidEntity(FakeKraidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, FakeKraidEnemyState state)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Fake Kraid species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the enemy occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="surroundings">The room, Samus, controller and camera the population load sets the enemy up in.</param>
    internal FakeKraidEntity(FakeKraidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        EnemyPopulationSurroundings surroundings)
        : this(species, occupant, place, common, species.InitializeFakeKraid(place, surroundings.Samus))
    {
    }

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly FakeKraidEnemyState state = state;

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunFakeKraidMainAi(Occupant, Place, state, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceFakeKraidTypedAnimation(Occupant, Place, state, context);

    /// <summary>Whether the fake Kraid's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == KraidReactions.FakeKraidTouch || base.ReactsToTouch;

    /// <summary>Runs the fake Kraid's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == KraidReactions.FakeKraidTouch)
            species.ReactToFakeKraidTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The fake Kraid's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == KraidReactions.FakeKraidShot ? species.FakeKraidShotReaction : base.ShotReaction;

    /// <summary>The fake Kraid's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) => species.FakeKraidBombReaction;

    /// <summary>The fake Kraid's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction => LeavesPowerBombToSpecies ? species.FakeKraidPowerBombReaction : base.PowerBombReaction;

    /// <summary>The fake Kraid's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == KraidReactions.FakeKraidShot;
}
