using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Sparks;
using SuperMetroid.Core.Enemies.WorkRobots;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The typed selection a single population parameter word names for a species (#1472), through
/// <see cref="EnemySpawnSettingsCodec"/>, for the checks that index the cartridge's selector tables by that word.
/// </summary>
internal static class SpawnWordDecodes
{
    /// <summary>The Tourian entrance statue part a first parameter word names.</summary>
    public static TourianEntranceStatueSegment TourianStatue(int parameter1) =>
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

    /// <summary>The start a first parameter word gives a Wrecked Ship Spark.</summary>
    /// <exception cref="InvalidDataException">The word selects past the three authored starts.</exception>
    public static SparkMovementDefinition SparkStart(int parameter1) =>
        Settings<SparkSpawnSettings>(EnemyDefinitionId.Spark, parameter1).Start;

    /// <summary>The unpowered pose a first parameter word gives a Work Robot.</summary>
    /// <exception cref="InvalidDataException">The word selects the pose past the three authored ones.</exception>
    public static WorkRobotSegment WorkRobotPose(int parameter1) =>
        Settings<WorkRobotSpawnSettings>(EnemyDefinitionId.RobotNoPower, parameter1).Pose;

    /// <summary>The settings a first parameter word gives <paramref name="species"/>.</summary>
    private static T Settings<T>(EnemyDefinitionId species, int parameter1) where T : EnemySpawnSettings =>
        (T)EnemySpawnSettingsCodec.Decode(species, PopulationWords.Parameters(parameter1, 0));
}
