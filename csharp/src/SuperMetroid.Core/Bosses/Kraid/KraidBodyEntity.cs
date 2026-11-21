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
internal sealed class KraidBodyEntity(KraidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : KraidEntity(species, occupant, place, common, species.Parts.BodyReference)
{
    /// <inheritdoc/>
    protected override void SetUp() => Species.Rise.InitializeKraidBody(Place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunKraidMainAi(Occupant, Encounter, frame);

    /// <summary>Only shots meet Kraid's body's frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities => EnemyCapabilities.ProjectileFrameHitboxes;
}
