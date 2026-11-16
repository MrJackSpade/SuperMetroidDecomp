using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Golden Torizo (#2146): the Torizo's shared participation, with Golden Torizo's fight and its own shot reaction.</summary>
/// <param name="species">The Torizo species owner.</param>
/// <param name="place">The place Golden Torizo occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class GoldenTorizoEntity(TorizoBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : TorizoEntity(species, place, common)
{
    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunGoldenTorizoMainAi(Place, frame);

    /// <summary>Golden Torizo's own shot reaction, which its definition leaves to it.</summary>
    public override EnemyReaction? SpeciesShot => TorizoReactions.GoldenShot;

    /// <summary>The Torizo shot reactions, which a normal bomb triggers as it does the common one, and Golden Torizo's own.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names, or null.</param>
    public override bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        base.OwnsPrivateShotReaction(reaction) || reaction == TorizoReactions.GoldenShot;
}
