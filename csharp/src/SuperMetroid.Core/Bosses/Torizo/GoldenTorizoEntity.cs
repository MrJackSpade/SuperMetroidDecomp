using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Golden Torizo (#2146): the Torizo's shared participation, with Golden Torizo's fight and its own shot reaction.</summary>
/// <param name="species">The Torizo species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place Golden Torizo occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class GoldenTorizoEntity(TorizoBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : TorizoEntity(species, occupant, place, common)
{
    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunGoldenTorizoMainAi(Occupant, Place, frame);

    /// <summary>He is Norfair's Golden Torizo.</summary>
    protected override TorizoVariant Variant => TorizoVariant.Golden;

    /// <summary>Golden Torizo's own shot reaction, which its definition leaves to it.</summary>
    public override EnemyReaction? SpeciesShot => TorizoReactions.GoldenShot;

    /// <inheritdoc/>
    public override bool RunHurt(RoomEnemyFrame frame)
    {
        Species.Combat.ApplyGoldenTorizoHurt(Occupant, Place);
        return true;
    }

    /// <inheritdoc/>
    protected override EnemyShotReaction OwnShotReaction => Species.Combat.GoldenTorizoShotReaction;

    /// <inheritdoc/>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        Species.Combat.ReactToGoldenTorizoBomb(context);
        return true;
    }

    /// <summary>The Torizo shot reactions, which a normal bomb triggers as it does the common one, and Golden Torizo's own.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names, or null.</param>
    public override bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        base.OwnsPrivateShotReaction(reaction) || reaction == TorizoReactions.GoldenShot;
}
