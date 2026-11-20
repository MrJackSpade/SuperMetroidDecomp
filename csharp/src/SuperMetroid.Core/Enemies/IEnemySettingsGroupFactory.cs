namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species owner whose head enemy, placed from an entry that tells it <typeparamref name="TSettings"/>, brings the parts
/// the room lists right after it (#2554), as <see cref="IEnemyGroupFactory"/> does for a head told nothing.
/// </summary>
/// <typeparam name="TSettings">The settings the head's entry tells it, which carry what its parts start from.</typeparam>
internal interface IEnemySettingsGroupFactory<TSettings> : IEnemySpeciesFactory where TSettings : EnemySpawnSettings
{
    /// <summary>How many places right after the head the room lists the group's parts in.</summary>
    [IntegerQuantity("parts the room lists after a group's head")]
    int PartCount { get; }

    /// <summary>
    /// Builds the head in <paramref name="head"/> with its state from <paramref name="settings"/>, over the places
    /// <paramref name="parts"/> of its parts, and how each part is built as the population load reaches its place.
    /// </summary>
    /// <param name="head">The head's place, already holding its identity and entry.</param>
    /// <param name="parts">The places the room lists the group's parts in, in listing order, before any part is placed.</param>
    /// <param name="common">The shared participation the entities compose.</param>
    /// <param name="setup">How the head comes to be set up.</param>
    /// <param name="settings">What the head's entry tells its species.</param>
    EnemyPlaced CreateGroup(RoomEnemyPlace head, IReadOnlyList<RoomEnemyPlace> parts, EnemyCommonParticipation common,
        EnemySetup setup, TSettings settings);

    /// <inheritdoc/>
    void IEnemySpeciesFactory.RegisterIn(EnemySpeciesRegistry registry) => Ensure.NotNull(registry).Add(this);
}
