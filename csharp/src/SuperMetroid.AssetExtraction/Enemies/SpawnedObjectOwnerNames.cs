using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// How the enemy sheet manifest names each spawned-object sheet's owner (#1513): "Shared" for the room's shared sheet,
/// "MotherBrainTankGlass" for the sheet of her tank glass's pieces, and the spawner enemy's name for its own sheet.
/// </summary>
internal static partial class SpawnedObjectOwnerNames
{
    /// <summary>The name of the shared sheet's owner.</summary>
    private const string SharedName = "Shared";

    /// <summary>The name of the tank glass sheet's owner.</summary>
    private const string TankGlassName = "MotherBrainTankGlass";

    /// <summary>The manifest name of <paramref name="owner"/>.</summary>
    [InstalledKey]
    internal static string Name(SpawnedObjectOwner owner) => owner.IsMotherBrainTankGlass ? TankGlassName
        : owner.Spawner is { } spawner ? EnemyDefinitionIdKeys.Installed.Of(spawner) : SharedName;

    /// <summary>The owner manifest name <paramref name="name"/> names.</summary>
    /// <exception cref="InvalidDataException">The name names no owner.</exception>
    internal static SpawnedObjectOwner Parse(string name) => name switch
    {
        SharedName => SpawnedObjectOwner.Shared,
        TankGlassName => SpawnedObjectOwner.MotherBrainTankGlass,
        _ => EnemyDefinitionIdKeys.Installed.TryParse(name, out EnemyDefinitionId spawner)
            ? SpawnedObjectOwner.Of(spawner)
            : throw new InvalidDataException($"Enemy sheet manifest names unknown spawned-object owner {name}."),
    };
}
