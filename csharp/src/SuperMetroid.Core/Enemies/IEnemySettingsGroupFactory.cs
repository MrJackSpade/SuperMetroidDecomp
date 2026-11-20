namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species whose head enemy, placed from an entry that tells it <typeparamref name="TSettings"/>, brings the parts
/// the room lists right after it (#2554), as <see cref="IEnemyGroupFactory"/> does for a head told nothing.
/// </summary>
/// <typeparam name="TSettings">The settings the head's entry tells it, which carry what its parts start from.</typeparam>
internal interface IEnemySettingsGroupFactory<TSettings> : IEnemySpeciesFactory where TSettings : EnemySpawnSettings
{
    /// <summary>How many places right after the head the room lists the group's parts in.</summary>
    [IntegerQuantity("parts the room lists after a group's head")]
    int PartCount { get; }

    /// <summary>
    /// Builds the group's head with its state from <paramref name="settings"/>, over its
    /// parts' places, and how each part is built as the population load reaches its place.
    /// </summary>
    /// <param name="places">The places of the head and of its parts.</param>
    /// <param name="common">The shared participation the entities compose.</param>
    /// <param name="settings">What the head's entry tells its species.</param>
    EnemyPlaced CreateGroup(EnemyGroupPlaces places, EnemyCommonParticipation common, TSettings settings);

    /// <inheritdoc/>
    void IEnemySpeciesFactory.EnrollIn(EnemySpeciesRegistry registry) => Ensure.NotNull(registry).Add(this);
}
