using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's body (#2146): his rise and fight. His body is drawn by his fight rather than by a typed animation, and only
/// shots meet his frames' hitboxes.
/// </summary>
/// <param name="species">The Kraid species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="encounter">What the enemy's setup built, which its base holds (#2554).</param>
internal sealed partial class KraidBodyEntity(KraidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, KraidEnemyState encounter)
    : KraidEntity(species, occupant, place, common, encounter)
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Kraid species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the body occupies.</param>
    /// <param name="common">The shared participation.</param>
    internal KraidBodyEntity(KraidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
        : this(species, occupant, place, common, species.BeginKraid(place))
    {
    }

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunKraidMainAi(Occupant, Encounter, frame);

    /// <summary>Only shots meet Kraid's body's frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities => EnemyCapabilities.ProjectileFrameHitboxes;
}
