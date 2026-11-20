using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Golden Torizo (#2146): the Torizo's shared participation, with Golden Torizo's fight and its own shot reaction.</summary>
/// <param name="species">The Torizo species owner.</param>
/// <param name="place">The place Golden Torizo occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="setup">The population load that places the Torizo.</param>
internal sealed class GoldenTorizoEntity(TorizoBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup)
    : TorizoEntity(species, occupant, place, common, TorizoVariant.Golden, setup)
{
    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunGoldenTorizoMainAi(Place, State, frame);

    /// <summary>Golden Torizo's own shot reaction, which its definition leaves to it.</summary>
    public override EnemyReaction? SpeciesShot => TorizoReactions.GoldenShot;

    /// <inheritdoc/>
    public override bool RunHurt(RoomEnemyFrame frame)
    {
        Species.Combat.ApplyGoldenTorizoHurt(Place, State);
        return true;
    }

    /// <inheritdoc/>
    protected override EnemyShotReaction OwnShotReaction => field ??= TorizoCombat.GoldenTorizoShotReactionOf(State);

    /// <inheritdoc/>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        Species.Combat.ReactToGoldenTorizoBomb(context, State);
        return true;
    }

    /// <summary>The Torizo shot reactions, which a normal bomb triggers as it does the common one, and Golden Torizo's own.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names, or null.</param>
    public override bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        base.OwnsPrivateShotReaction(reaction) || reaction == TorizoReactions.GoldenShot;
}
