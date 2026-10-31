using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The typed selection a single population parameter word names for a species (#1472), through
/// <see cref="EnemySpawnSettingsCodec"/>, for the checks that index the cartridge's selector tables by that word.
/// </summary>
internal static class SpawnWordDecodes
{
    /// <summary>The Tourian entrance statue part a first parameter word names.</summary>
    public static TourianEntranceStatueProgram TourianStatue(int parameter1) =>
        Settings<TourianEntranceStatueSpawnSettings>(EnemyDefinitionId.TourianStatue, parameter1).Statue;

    /// <summary>The Ceres door or wall a first parameter word names.</summary>
    public static CeresDoorVariant CeresDoor(int parameter1) =>
        Settings<CeresDoorSpawnSettings>(EnemyDefinitionId.CeresDoor, parameter1).Variant;

    /// <summary>The escape Etecoon role a first parameter word names.</summary>
    public static EscapeEtecoonRole EscapeEtecoonRole(int parameter1) =>
        Settings<EscapeEtecoonSpawnSettings>(EnemyDefinitionId.EtecoonEscape, parameter1).Role;

    /// <summary>The Ridley breakup part a first parameter word names.</summary>
    public static RidleyExplosionPart RidleyExplosionPart(int parameter1) =>
        Settings<RidleyExplosionSpawnSettings>(EnemyDefinitionId.RidleyExplosion, parameter1).Part;

    /// <summary>The first parameter word that names a Ridley breakup part.</summary>
    public static int RidleyExplosionParameter(RidleyExplosionPart part) =>
        EnemySpawnSettingsCodec.Encode(new RidleyExplosionSpawnSettings(part)).Parameter1;

    /// <summary>The settings a first parameter word gives <paramref name="species"/>.</summary>
    private static T Settings<T>(EnemyDefinitionId species, int parameter1) where T : EnemySpawnSettings =>
        (T)EnemySpawnSettingsCodec.Decode(species, PopulationWords.Parameters(parameter1, 0));
}
