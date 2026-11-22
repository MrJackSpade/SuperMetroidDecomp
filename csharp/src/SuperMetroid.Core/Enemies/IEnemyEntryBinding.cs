namespace SuperMetroid.Core.Enemies;

/// <summary>
/// How the population entries of a definition bind to the creation that builds their enemies (#2554): the creation a
/// species registered for the definition, or the one a part already placed offered it. The binding is the shared creation
/// object itself, so a copy of the game's state holds it by reference rather than as a closure over it.
/// </summary>
internal interface IEnemyEntryBinding
{
    /// <summary>Binds <paramref name="placement"/>, checking once that its settings are the type the creation takes.</summary>
    /// <param name="placement">The population entry.</param>
    /// <returns>The bound entry, which builds its enemy with no further check.</returns>
    /// <exception cref="InvalidDataException">The entry carries other settings than the creation takes.</exception>
    PopulationEnemySpawn Bind(RoomEnemyPlacement placement);

    /// <summary>The rejection of an entry whose settings are not the <typeparamref name="TSettings"/> its creation takes.</summary>
    /// <typeparam name="TSettings">The settings the creation takes.</typeparam>
    /// <param name="placement">The entry.</param>
    internal static InvalidDataException Mismatch<TSettings>(RoomEnemyPlacement placement) where TSettings : EnemySpawnSettings =>
        new($"Enemy {Ensure.NotNull(placement).DefinitionId}'s population entry carries {placement.SpawnSettings}, not {typeof(TSettings).Name}.");
}
