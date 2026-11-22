namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species' creation of the enemies its bound entries place wherever they are set up (#2554), held as one object every
/// entry bound to it shares, so a copy of the game's state holds it once and each entry reaches it by reference: the
/// enemies it builds reach those entries again, which a creation each entry held as its own delegate would reach before
/// the copy held the delegate. It binds its definitions' entries itself.
/// </summary>
/// <typeparam name="TSettings">The settings the creation takes.</typeparam>
internal abstract partial class SpeciesSpawnCreation<TSettings> : IEnemyEntryBinding where TSettings : EnemySpawnSettings
{
    /// <summary>Builds <paramref name="occupant"/>'s enemy from <paramref name="settings"/> in <paramref name="place"/>.</summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and population entry.</param>
    /// <param name="settings">The settings the occupant's population entry gives its species.</param>
    /// <param name="place">The place, already holding the occupant and its live values.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    internal abstract EnemyOccupant Create(RoomEnemyOccupant occupant, TSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common);

    /// <inheritdoc/>
    public PopulationEnemySpawn Bind(RoomEnemyPlacement placement) =>
        Ensure.NotNull(placement).SpawnSettings is TSettings settings
            ? new OrdinaryEnemySpawn<TSettings>(placement, settings, this)
            : throw IEnemyEntryBinding.Mismatch<TSettings>(placement);
}
