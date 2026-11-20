using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Authors a fixture slot's spawn settings from the cartridge's parameter words (#1472): the fixture names the species
/// whose packing reads them, and <see cref="EnemySpawnSettingsCodec"/> turns the words into that species' settings.
/// </summary>
internal static class RoomEnemySlotSpawnWords
{
    /// <summary>Sets the first parameter word of the slot's entry, as <paramref name="species"/> reads it, keeping the rest.</summary>
    internal static void SetSpawnParameter1(this RoomEnemyPlace place, EnemyDefinitionId species, int value) =>
        place.SetSpawnWords(species, words => words with { Parameter1 = value });

    /// <summary>Sets the second parameter word of the slot's entry, as <paramref name="species"/> reads it, keeping the rest.</summary>
    internal static void SetSpawnParameter2(this RoomEnemyPlace place, EnemyDefinitionId species, int value) =>
        place.SetSpawnWords(species, words => words with { Parameter2 = value });

    /// <summary>
    /// Rewrites the words of the slot's entry as <paramref name="species"/> reads them: the current settings' words when
    /// they are that species' kind, otherwise the species' blank words, changed by <paramref name="change"/>. A species
    /// whose parts are placed with settings of their own (#2474) has one kind, the settings its parts' share.
    /// </summary>
    internal static void SetSpawnWords(this RoomEnemyPlace place, EnemyDefinitionId species, Func<PopulationWords, PopulationWords> change)
    {
        PopulationWords blank = EnemySpawnSettingsCodec.Blank(species);
        if (place.History is not PopulatedEnemyPlace populated)
        {
            place.PopulateFor(species, EnemySpawnSettingsCodec.Decode(species, change(blank), Position16<RoomPixels>.Zero));
            return;
        }
        RoomEnemyPlacement current = populated.LastSpawn;
        Position16<RoomPixels> position = RoomEnemyPopulationRecordCodec.PositionWordsOf(current.Placing);
        Type kind = KindOf(EnemySpawnSettingsCodec.Decode(species, blank, position).GetType());
        PopulationWords words = KindOf(current.SpawnSettings.GetType()) == kind
            ? EnemySpawnSettingsCodec.Encode(species, current.SpawnSettings)
            : blank;
        place.SetSpawnSettings(EnemySpawnSettingsCodec.Decode(species, change(words), position));
    }

    /// <summary>The kind of settings <paramref name="settings"/> is: its own type, or the settings its species' parts share.</summary>
    private static Type KindOf(Type settings) =>
        settings.BaseType is { } shared && shared != typeof(EnemySpawnSettings) && shared.IsAbstract ? shared : settings;
}
