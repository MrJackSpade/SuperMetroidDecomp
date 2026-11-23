using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Golden Torizo (#2146): the Torizo's shared participation, with Golden Torizo's fight and its own shot reaction.</summary>
/// <param name="species">The Torizo species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place Golden Torizo occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built, which its base holds (#2554).</param>
internal sealed partial class GoldenTorizoEntity(TorizoBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    TorizoEnemyState state)
    : TorizoEntity(species, occupant, place, common, state)
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Torizo species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place Golden Torizo occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="surroundings">The room, Samus, controller and camera the population load sets the Torizo up in.</param>
    internal GoldenTorizoEntity(TorizoBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        EnemyPopulationSurroundings surroundings)
        : this(species, occupant, place, common,
            species.Movement.InitializeTorizo(place, TorizoVariant.Golden, surroundings.Samus, surroundings.ControllerInput))
    {
    }

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunGoldenTorizoMainAi(Occupant, Place, State, frame);

    /// <summary>The shot reaction of this Torizo's own that its definition names (#2558).</summary>
    protected override EnemyReaction OwnShot => TorizoReactions.GoldenShot;

    /// <inheritdoc/>
    public override bool RunHurt(RoomEnemyFrame frame)
    {
        Species.Combat.ApplyGoldenTorizoHurt(Occupant, Place, State);
        return true;
    }

    /// <inheritdoc/>
    protected override EnemyShotReaction OwnShotReaction => field ??= TorizoCombat.GoldenTorizoShotReaction(this);

    /// <summary>Resolves a shot on Golden Torizo before its impact: his own catches, dodges and stuns.</summary>
    /// <param name="c">The shot meeting him.</param>
    /// <returns>Whether the resolution replaces the rest of the shot reaction.</returns>
    internal bool ResolveShotBeforeImpact(in EnemyShotContext c) => TorizoCombat.ResolveGoldenTorizoShotBeforeImpact(c, State);

    /// <inheritdoc/>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        Species.Combat.ReactToGoldenTorizoBomb(context, State);
        return true;
    }

    /// <summary>The Torizo shot reactions, which a normal bomb triggers as it does the common one, and Golden Torizo's own.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names.</param>
    public override bool OwnsPrivateShotReaction(EnemyReaction reaction) =>
        base.OwnsPrivateShotReaction(reaction) || reaction == TorizoReactions.GoldenShot;
}
