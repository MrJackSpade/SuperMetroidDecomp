using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The codec between a typed <see cref="RoomEnemyPopulationRecord"/> and the words of a native sixteen-byte population
/// entry (#1472), for the checks that compare compiled populations with the cartridge and the fixtures that seed native
/// records. The definition pointer is the caller's: this type takes and gives the resolved identity.
/// </summary>
[EncodingOwner("room enemy population entry words")]
internal static class RoomEnemyPopulationRecordCodec
{
    /// <summary>The typed entry the words of a native population entry describe.</summary>
    /// <exception cref="InvalidDataException">A word holds a setting or property the entry cannot carry.</exception>
    public static RoomEnemyPopulationRecord Decode(
        EnemyDefinitionId enemy, int x, int y, int initialization, int properties, int extraProperties,
        int parameter1, int parameter2)
    {
        bool extraIsSettings = EnemySpawnSettingsCodec.TakesExtraPropertySettings(enemy);
        var words = new PopulationWords(initialization, EnemyPropertyWordCodec.SpeciesBitsOf(properties),
            extraIsSettings ? extraProperties : 0, parameter1, parameter2);
        EnemySpawnSettings settings = EnemySpawnSettingsCodec.Decode(enemy, words);
        if (words.SpeciesPropertyBits != 0 && EnemySpawnSettingsCodec.Encode(settings).SpeciesPropertyBits == 0)
            throw new InvalidDataException($"Enemy {enemy}'s property word ${properties:X4} holds species bits its settings do not take.");
        return new RoomEnemyPopulationRecord(
            enemy,
            new Position16<RoomPixels>(checked((ushort)x), checked((ushort)y)),
            EnemyPropertyWordCodec.EngineFlagsOf(properties),
            extraIsSettings ? EnemyExtraProperties.None : EnemyPropertyWordCodec.DecodeExtra(extraProperties),
            settings);
    }

    /// <summary>The words of a native population entry for a typed entry, after the definition pointer.</summary>
    public static PopulationEntryWords Encode(RoomEnemyPopulationRecord record)
    {
        PopulationWords settings = EnemySpawnSettingsCodec.Encode(record.SpawnSettings);
        return new PopulationEntryWords(
            record.Position.X, record.Position.Y, settings.Initialization,
            EnemyPropertyWordCodec.Encode(record.Properties) | settings.SpeciesPropertyBits,
            EnemyPropertyWordCodec.Encode(record.ExtraProperties) | settings.ExtraProperties,
            settings.Parameter1, settings.Parameter2);
    }
}
