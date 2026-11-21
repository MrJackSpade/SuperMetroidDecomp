using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.Powamp;

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
        PopulationWords words = SameKind(EnemySpawnSettingsCodec.Decode(species, blank, position), current.SpawnSettings)
            ? EnemySpawnSettingsCodec.Encode(species, current.SpawnSettings)
            : blank;
        place.SetSpawnSettings(EnemySpawnSettingsCodec.Decode(species, change(words), position));
    }

    /// <summary>
    /// Whether two settings are of one kind: the settings a species' parts share (#2474), or otherwise the same settings type.
    /// </summary>
    private static bool SameKind(EnemySpawnSettings left, EnemySpawnSettings right) => (left, right) switch
    {
        (RinkaSpawnSettings, RinkaSpawnSettings) or (FuneNamiheSpawnSettings, FuneNamiheSpawnSettings)
            or (HibashiSpawnSettings, HibashiSpawnSettings) or (MagdolliteSpawnSettings, MagdolliteSpawnSettings)
            or (MorphBallEyeSpawnSettings, MorphBallEyeSpawnSettings) or (NorfairPipeBugSpawnSettings, NorfairPipeBugSpawnSettings)
            or (PowampSpawnSettings, PowampSpawnSettings) => true,
        _ => left.GetType() == right.GetType(),
    };
}
