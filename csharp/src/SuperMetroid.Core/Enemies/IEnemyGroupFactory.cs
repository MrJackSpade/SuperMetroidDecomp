namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species owner whose head enemy, placed by the room's population from an entry that tells it nothing, brings the
/// parts the room lists right after it (#2554): the population load hands it its parts' places as it places the head,
/// and builds each part through it, so every part holds its head as a typed reference from its setup.
/// </summary>
internal interface IEnemyGroupFactory : IEnemySpeciesFactory
{
    /// <summary>The definition of the group's head.</summary>
    EnemyDefinitionId Head { get; }

    /// <summary>How many places right after the head the room lists the group's parts in.</summary>
    [IntegerQuantity("parts the room lists after a group's head")]
    int PartCount { get; }

    /// <summary>
    /// Builds the head in <paramref name="head"/> with its state, over the places <paramref name="parts"/> of its parts,
    /// and how each part is built as the population load reaches its place.
    /// </summary>
    /// <param name="head">The head's place, already holding its identity and entry.</param>
    /// <param name="parts">The places the room lists the group's parts in, in listing order, before any part is placed.</param>
    /// <param name="common">The shared participation the entities compose.</param>
    /// <param name="setup">How the head comes to be set up.</param>
    EnemyPlaced CreateGroup(RoomEnemyPlace head, IReadOnlyList<RoomEnemyPlace> parts, EnemyCommonParticipation common, EnemySetup setup);

    /// <inheritdoc/>
    void IEnemySpeciesFactory.RegisterIn(EnemySpeciesRegistry registry) => Ensure.NotNull(registry).Add(this);
}
