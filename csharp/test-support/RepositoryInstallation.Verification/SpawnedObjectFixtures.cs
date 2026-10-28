using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1392): the enemy presentation a room state's spawned objects draw through, over the installed
/// enemy artwork and sheets with that state's graphics set loaded, as the game loads it with the room.
/// </summary>
internal static class SpawnedObjectFixtures
{
    /// <summary>The first retail graphics set loading <paramref name="enemy"/>.</summary>
    internal static RoomEnemyGraphicsSetDefinition SetLoading(EnemyDefinitionId enemy) =>
        RoomEnemyGraphicsSetDefinitions.All.First(set => set.Records.ToArray().Any(record => record.DefinitionId == enemy));

    /// <summary>
    /// The first retail graphics set drawing <paramref name="enemy"/>: one loading it, or else one whose rooms place it
    /// without loading it, where it draws from the standard characters.
    /// </summary>
    internal static RoomEnemyGraphicsSetDefinition SetDrawing(EnemyDefinitionId enemy) =>
        RoomEnemyGraphicsSetDefinitions.All.FirstOrDefault(set => set.Records.ToArray().Any(record => record.DefinitionId == enemy))
        ?? RoomStateDefinitions.All.FirstOrDefault(state => state.EnemyPopulation.Records.ToArray().Any(record => record.DefinitionId == enemy))
            ?.EnemyGraphicsSet
        ?? throw new InvalidDataException($"No retail room draws {enemy}.");

    /// <summary>Whether a retail room draws <paramref name="enemy"/>: a graphics set loads it or a room places it.</summary>
    internal static bool Drawn(EnemyDefinitionId enemy) =>
        RoomEnemyGraphicsSetDefinitions.All.Any(set => set.Records.ToArray().Any(record => record.DefinitionId == enemy))
        || RoomStateDefinitions.All.Any(state => state.EnemyPopulation.Records.ToArray().Any(record => record.DefinitionId == enemy));

    /// <summary>
    /// Loads into <paramref name="enemies"/> a retail graphics set whose character-zero sheet holds projectile
    /// <paramref name="frame"/>: the first set loading an enemy that spawns it, or, for an object any room spawns, the
    /// first set.
    /// </summary>
    /// <exception cref="InvalidOperationException">No enemy a set loads spawns the frame and no room spawns it anywhere.</exception>
    internal static void LoadSetSpawning(RoomEnemySystem enemies, EnemyProjectileFrameId frame)
    {
        ArgumentNullException.ThrowIfNull(enemies);
        RoomEnemyGraphicsSetDefinition set = SpawnedObjectFrameDefinitions.Anywhere.Projectiles.Contains(frame)
            ? RoomEnemyGraphicsSetDefinitions.All[0]
            : RoomEnemyGraphicsSetDefinitions.All.FirstOrDefault(candidate => candidate.Records.ToArray().Any(record =>
                SpawnedObjectFrameDefinitions.ByEnemy.TryGetValue(record.DefinitionId, out SpawnedObjects? spawned) && spawned.Projectiles.Contains(frame)))
            ?? throw new InvalidOperationException($"No retail graphics set spawns projectile frame {frame}.");
        enemies.GraphicsSet.LoadGraphicsSet(CompiledRoomEnemyDefinitions.Instance, set, new SpriteColourSets());
    }

    /// <summary>
    /// Loads into <paramref name="enemies"/> the graphics set a retail room drawing <paramref name="enemy"/> loads, as that
    /// room's load does, so its sheets draw it, and gives <paramref name="slot"/> the indexes it draws at there.
    /// </summary>
    internal static void LoadSetDrawing(RoomEnemySystem enemies, EnemyDefinitionId enemy, RoomEnemySlot? slot = null)
    {
        ArgumentNullException.ThrowIfNull(enemies);
        enemies.GraphicsSet.LoadGraphicsSet(CompiledRoomEnemyDefinitions.Instance, SetDrawing(enemy), new SpriteColourSets());
        if (slot is null)
            return;
        SpriteGraphics indexes = enemies.GraphicsSet.DrawIndexes(enemy);
        slot.TileIndexBase = indexes.TileBase;
        slot.PaletteRow = indexes.PaletteRow;
    }

    /// <summary>The installed enemy artwork with <paramref name="spritemaps"/> in place of its compositions.</summary>
    internal static SuperMetroid.Core.Enemies.Artwork.EnemyTileArtworkCatalog InstalledTilesWith(SuperMetroid.Core.Enemies.Artwork.EnemySpritemapCatalog spritemaps)
    {
        SuperMetroid.Core.Enemies.Artwork.EnemyTileArtworkCatalog catalog = RepositoryInstallation.Installation.LoadEnemyTiles().Enemies;
        catalog.Spritemaps = spritemaps;
        return catalog;
    }

    /// <summary>The presentation <paramref name="state"/>'s spawned objects draw through.</summary>
    internal static EnemyPresentation Presentation(RoomStateSettings state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var session = new EnemySession { TileArtwork = InstalledFixtures.EnemyTiles, Sheets = InstalledFixtures.EnemySheets };
        var room = new EnemyRoomBinding(session, CompiledRoomEnemyDefinitions.Instance, services: null);
        var graphics = new EnemyGraphicsSet(room);
        graphics.LoadGraphicsSet(CompiledRoomEnemyDefinitions.Instance, state.EnemyGraphicsSet, new SpriteColourSets());
        return new EnemyPresentation(room, graphics);
    }
}
