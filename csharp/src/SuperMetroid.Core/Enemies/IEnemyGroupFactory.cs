namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species whose head enemy, placed by the room's population from an entry that tells it nothing, brings the
/// parts the room lists right after it (#2554): the population load hands it its parts' places as it places the head,
/// and builds each part through it, so every part holds its head as a typed reference from its setup.
/// </summary>
internal interface IEnemyGroupFactory : IEnemySpeciesFactory
{
    /// <summary>The definition of the group's head.</summary>
    EnemyDefinitionId HeadDefinition { get; }

    /// <summary>How many places right after the head the room lists the group's parts in.</summary>
    [IntegerQuantity("parts the room lists after a group's head")]
    int PartCount { get; }

    /// <summary>
    /// Builds the group's head with its state, over its parts' places,
    /// and how each part is built as the population load reaches its place.
    /// </summary>
    /// <param name="places">The places of the head and of its parts.</param>
    /// <param name="common">The shared participation the entities compose.</param>
    EnemyPlaced CreateGroup(EnemyGroupPlaces places, EnemyCommonParticipation common);

    /// <inheritdoc/>
    void IEnemySpeciesFactory.EnrollIn(EnemySpeciesRegistry registry) => Ensure.NotNull(registry).Add(this);
}
