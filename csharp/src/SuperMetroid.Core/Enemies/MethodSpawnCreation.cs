namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A creation one named method makes (#2554): a species' own method, or one of the object that carries what a part joins,
/// so a copy of the game's state names the method and holds its target.
/// </summary>
/// <typeparam name="TSettings">The settings the creation takes.</typeparam>
/// <param name="create">The named method.</param>
internal sealed partial class MethodSpawnCreation<TSettings>(EnemySpawnCreation<TSettings> create) : SpeciesSpawnCreation<TSettings>
    where TSettings : EnemySpawnSettings
{
    /// <summary>The named method.</summary>
    private readonly EnemySpawnCreation<TSettings> _create = Ensure.NotNull(create);

    /// <inheritdoc/>
    internal override EnemyOccupant Create(RoomEnemyOccupant occupant, TSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        _create(occupant, settings, place, common);
}
