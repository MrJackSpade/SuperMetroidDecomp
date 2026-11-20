using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Golden Torizo (#2146): the Torizo's shared participation, with Golden Torizo's fight and its own shot reaction.</summary>
/// <param name="species">The Torizo species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place Golden Torizo occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="surroundings">The room, Samus, controller and camera the population load sets the Torizo up in.</param>
internal sealed class GoldenTorizoEntity(TorizoBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    EnemyPopulationSurroundings surroundings)
    : TorizoEntity(species, occupant, place, common, surroundings, TorizoVariant.Golden)
{
    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunGoldenTorizoMainAi(Occupant, Place, State, frame);

    /// <summary>He is Norfair's Golden Torizo.</summary>
    protected override TorizoVariant Variant => TorizoVariant.Golden;

    /// <summary>The shot reaction of this Torizo's own that its definition names (#2558).</summary>
    protected override EnemyReaction OwnShot => TorizoReactions.GoldenShot;

    /// <inheritdoc/>
    public override bool RunHurt(RoomEnemyFrame frame)
    {
        Species.Combat.ApplyGoldenTorizoHurt(Occupant, Place, State);
        return true;
    }

    /// <inheritdoc/>
    protected override EnemyShotReaction OwnShotReaction => field ??= TorizoCombat.GoldenTorizoShotReaction(State);

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
