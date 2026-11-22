using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb Torizo (#2146): the Torizo's shared participation, with Bomb Torizo's fight and shot reaction.</summary>
/// <param name="species">The Torizo species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place Bomb Torizo occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built, which its base holds (#2554).</param>
internal sealed partial class BombTorizoEntity(TorizoBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    TorizoEnemyState state)
    : TorizoEntity(species, occupant, place, common, state)
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Torizo species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place Bomb Torizo occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="surroundings">The room, Samus, controller and camera the population load sets the Torizo up in.</param>
    internal BombTorizoEntity(TorizoBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        EnemyPopulationSurroundings surroundings)
        : this(species, occupant, place, common,
            species.Movement.InitializeTorizo(place, TorizoVariant.Bomb, surroundings.Samus, surroundings.ControllerInput))
    {
    }

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunBombTorizoMainAi(Occupant, Place, State, frame);

    /// <summary>He is Crateria's Bomb Torizo.</summary>
    protected override TorizoVariant Variant => TorizoVariant.Bomb;

    /// <summary>The shot reaction of this Torizo's own that its definition names (#2558).</summary>
    protected override EnemyReaction OwnShot => TorizoReactions.Shot;

    /// <inheritdoc/>
    public override bool RunHurt(RoomEnemyFrame frame)
    {
        Species.Combat.ApplyBombTorizoHurt(Occupant, Place, State);
        return true;
    }

    /// <inheritdoc/>
    protected override EnemyShotReaction OwnShotReaction => field ??= TorizoCombat.BombTorizoShotReaction(this);

    /// <summary>Resolves a shot on Bomb Torizo before its impact: his hitboxes' own reactions.</summary>
    /// <param name="c">The shot meeting him.</param>
    /// <returns>Whether the resolution replaces the rest of the shot reaction.</returns>
    internal bool ResolveShotBeforeImpact(in EnemyShotContext c) => TorizoCombat.ResolveBombTorizoShotBeforeImpact(c, State);

    /// <inheritdoc/>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        Species.Combat.ReactToBombTorizoBomb(context, State);
        return true;
    }
}
