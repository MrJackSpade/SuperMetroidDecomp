using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Authors a fixture slot's spawn settings from the cartridge's parameter words (#1472): the fixture names the species
/// whose packing reads them, and <see cref="EnemySpawnSettingsCodec"/> turns the words into that species' settings.
/// </summary>
internal static class RoomEnemySlotSpawnWords
{
    /// <summary>Sets the first parameter word of the slot's entry, as <paramref name="species"/> reads it, keeping the rest.</summary>
    internal static void SetSpawnParameter1(this RoomEnemySlot slot, EnemyDefinitionId species, int value) =>
        slot.SetSpawnWords(species, words => words with { Parameter1 = value });

    /// <summary>Sets the second parameter word of the slot's entry, as <paramref name="species"/> reads it, keeping the rest.</summary>
    internal static void SetSpawnParameter2(this RoomEnemySlot slot, EnemyDefinitionId species, int value) =>
        slot.SetSpawnWords(species, words => words with { Parameter2 = value });

    /// <summary>
    /// Rewrites the words of the slot's entry as <paramref name="species"/> reads them: the current settings' words when
    /// they are that species' kind, otherwise empty words, changed by <paramref name="change"/>.
    /// </summary>
    internal static void SetSpawnWords(this RoomEnemySlot slot, EnemyDefinitionId species, Func<PopulationWords, PopulationWords> change)
    {
        RoomEnemyPopulationRecord current = slot.Spawn.Population;
        Type kind = EnemySpawnSettingsCodec.Decode(species, default).GetType();
        PopulationWords words = !current.Equals(default(RoomEnemyPopulationRecord)) && current.SpawnSettings.GetType() == kind
            ? EnemySpawnSettingsCodec.Encode(current.SpawnSettings)
            : default;
        slot.SetSpawnSettings(EnemySpawnSettingsCodec.Decode(species, change(words)));
    }
}
