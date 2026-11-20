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
/// <param name="parts">His parts' places, which his body heads.</param>
internal sealed class KraidBodyEntity(KraidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, KraidParts parts)
    : KraidEntity(species, occupant, place, common)
{
    /// <inheritdoc/>
    internal override KraidEnemyState Encounter { get; } = species.Rise.InitializeKraidBody(place, parts);

    /// <inheritdoc/>
    internal override KraidPartState PartState => Encounter.BodyState;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunKraidMainAi(Occupant, Encounter, frame);

    /// <summary>
    /// The shared Power Bomb reaction; Kraid's body leaves the blast to the species, which has no reaction to it, and fails
    /// loudly if a blast reaches him.
    /// </summary>
    /// <exception cref="InvalidDataException">The body's definition leaves the blast to the species.</exception>
    public override EnemyPowerBombReaction PowerBombReaction => LeavesPowerBombToSpecies
        ? throw new InvalidDataException($"Enemy {Place.Occupant?.DefinitionId} has no response to its own Power Bomb reaction.")
        : base.PowerBombReaction;

    /// <summary>Only shots meet Kraid's body's frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities => EnemyCapabilities.ProjectileFrameHitboxes;
}
