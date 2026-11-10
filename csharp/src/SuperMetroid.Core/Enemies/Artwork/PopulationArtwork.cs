using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The artwork one room population's enemies draw from (#1599): the variant of each enemy it places, and of each enemy
/// those enemies' encounters spawn, and the variant of the pictures of each owner's spawned objects. A room that places no
/// enemies draws from the standard artwork.
/// </summary>
public sealed class PopulationArtwork
{
    /// <summary>The variant each enemy draws in, by enemy.</summary>
    internal readonly Dictionary<EnemyDefinitionId, EnemyArtworkVariant<SpriteSheet>> enemies;

    /// <summary>The variant each owner's spawned objects draw in, by owner.</summary>
    internal readonly Dictionary<SpawnedObjectOwner, EnemyArtworkVariant<SpawnedObjectSheet>> spawned;

    /// <summary>The artwork that draws each enemy in the variant <paramref name="enemies"/> names, and each owner's spawned objects in the variant <paramref name="spawned"/> names.</summary>
    /// <param name="enemies">The variant each enemy draws in, by enemy.</param>
    /// <param name="spawned">The variant each owner's spawned objects draw in, by owner.</param>
    public PopulationArtwork(IReadOnlyDictionary<EnemyDefinitionId, EnemyArtworkVariant<SpriteSheet>> enemies,
        IReadOnlyDictionary<SpawnedObjectOwner, EnemyArtworkVariant<SpawnedObjectSheet>> spawned)
    {
        this.enemies = new Dictionary<EnemyDefinitionId, EnemyArtworkVariant<SpriteSheet>>(Ensure.NotNull(enemies));
        this.spawned = new Dictionary<SpawnedObjectOwner, EnemyArtworkVariant<SpawnedObjectSheet>>(Ensure.NotNull(spawned));
    }

    /// <summary>The variant <paramref name="enemy"/> draws in.</summary>
    /// <exception cref="InvalidDataException">The population draws no variant of that enemy.</exception>
    public EnemyArtworkVariant<SpriteSheet> Enemy(EnemyDefinitionId enemy) =>
        enemies.TryGetValue(enemy, out EnemyArtworkVariant<SpriteSheet>? variant) ? variant
            : throw new InvalidDataException($"The room's population draws no variant of {enemy}.");

    /// <summary>The variant the objects <paramref name="owner"/> owns draw in.</summary>
    /// <exception cref="InvalidDataException">The population draws no spawned objects of that owner.</exception>
    public EnemyArtworkVariant<SpawnedObjectSheet> Spawned(SpawnedObjectOwner owner) =>
        spawned.TryGetValue(owner, out EnemyArtworkVariant<SpawnedObjectSheet>? variant) ? variant
            : throw new InvalidDataException($"The room's population draws no spawned objects of {owner}.");
}
