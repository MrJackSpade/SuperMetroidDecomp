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
        RoomEnemyPlacement current = populated.Occupancy.LastSpawn;
        Position16<RoomPixels> position = RoomEnemyPopulationRecordCodec.PositionWordsOf(current.Placing);
        string kind = KindOf(EnemySpawnSettingsCodec.Decode(species, blank, position));
        PopulationWords words = KindOf(current.SpawnSettings) == kind
            ? EnemySpawnSettingsCodec.Encode(species, current.SpawnSettings)
            : blank;
        place.SetSpawnSettings(EnemySpawnSettingsCodec.Decode(species, change(words), position));
    }

    /// <summary>
    /// The kind of settings <paramref name="settings"/> is: the settings its species' parts share, where the parts' settings
    /// derive from shared settings, or its own type.
    /// </summary>
    private static string KindOf(EnemySpawnSettings settings) => settings switch
    {
        SuperMetroid.Core.Bosses.MotherBrain.RinkaSpawnSettings => nameof(SuperMetroid.Core.Bosses.MotherBrain.RinkaSpawnSettings),
        SuperMetroid.Core.Enemies.Crawlers.CrawlerStartSettings => nameof(SuperMetroid.Core.Enemies.Crawlers.CrawlerStartSettings),
        SuperMetroid.Core.Enemies.FuneNamihe.FuneNamiheSpawnSettings => nameof(SuperMetroid.Core.Enemies.FuneNamihe.FuneNamiheSpawnSettings),
        SuperMetroid.Core.Enemies.Hibashi.HibashiSpawnSettings => nameof(SuperMetroid.Core.Enemies.Hibashi.HibashiSpawnSettings),
        SuperMetroid.Core.Enemies.Magdollite.MagdolliteSpawnSettings => nameof(SuperMetroid.Core.Enemies.Magdollite.MagdolliteSpawnSettings),
        SuperMetroid.Core.Enemies.MorphBallEye.MorphBallEyeSpawnSettings => nameof(SuperMetroid.Core.Enemies.MorphBallEye.MorphBallEyeSpawnSettings),
        SuperMetroid.Core.Enemies.PipeBugs.NorfairPipeBugSpawnSettings => nameof(SuperMetroid.Core.Enemies.PipeBugs.NorfairPipeBugSpawnSettings),
        SuperMetroid.Core.Enemies.Powamp.PowampSpawnSettings => nameof(SuperMetroid.Core.Enemies.Powamp.PowampSpawnSettings),
        _ => Ensure.NotNull(settings.GetType().FullName),
    };
}
