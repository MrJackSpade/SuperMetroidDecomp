using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1392): the enemy presentation a room state's spawned objects draw through, over the installed
/// enemy artwork and sheets with that state's graphics set loaded, as the game loads it with the room.
/// </summary>
internal static class SpawnedObjectFixtures
{
    /// <summary>The first retail graphics set loading <paramref name="enemy"/>.</summary>
    internal static RoomEnemyGraphicsSetDefinition SetLoading(EnemyDefinitionId enemy) =>
        RoomEnemyGraphicsSetLists.All.First(set => RoomEnemyGraphicsSetLists.MembersOf(set).Records.ToArray().Any(record => record.DefinitionId == enemy));

    /// <summary>
    /// The first retail graphics set drawing <paramref name="enemy"/>: one loading it, or else one whose rooms place it
    /// without loading it, where it draws from the standard characters.
    /// </summary>
    internal static RoomEnemyGraphicsSetDefinition SetDrawing(EnemyDefinitionId enemy) =>
        RoomEnemyGraphicsSetLists.All.FirstOrDefault(set => RoomEnemyGraphicsSetLists.MembersOf(set).Records.ToArray().Any(record => record.DefinitionId == enemy))
        ?? RoomStateDefinitions.All.FirstOrDefault(state => state.EnemyPopulation.Records.ToArray().Any(record => record.DefinitionId == enemy))
            ?.EnemyGraphicsSet
        ?? throw new InvalidDataException($"No retail room draws {enemy}.");

    /// <summary>Whether a retail room draws <paramref name="enemy"/>: a graphics set loads it or a room places it.</summary>
    internal static bool Drawn(EnemyDefinitionId enemy) =>
        RoomEnemyGraphicsSetLists.All.Any(set => RoomEnemyGraphicsSetLists.MembersOf(set).Records.ToArray().Any(record => record.DefinitionId == enemy))
        || RoomStateDefinitions.All.Any(state => state.EnemyPopulation.Records.ToArray().Any(record => record.DefinitionId == enemy));

    /// <summary>
    /// Loads into <paramref name="enemies"/> a retail graphics set whose sheets hold projectile <paramref name="frame"/>:
    /// the first set loading an enemy that spawns it onto its own sheet, else onto its sets' shared sheet, or, for an
    /// object any room spawns, the first set; and returns the graphics the spawn draws with there: the spawner's own sheet
    /// in its loaded colours, or <paramref name="shared"/> for an object drawn from the shared sheet.
    /// </summary>
    /// <exception cref="InvalidOperationException">No enemy a set loads spawns the frame and no room spawns it anywhere.</exception>
    internal static SpriteGraphics LoadSetSpawning(RoomEnemySystem enemies, EnemyProjectileFrameId frame, SpriteGraphics shared)
    {
        ArgumentNullException.ThrowIfNull(enemies);
        if (SpawnedObjectFrameDefinitions.Common.Any(common => common.Objects.Projectiles.Contains(frame)))
        {
            // An object a common spawner spawns is held by the shared sheet of the sets whose rooms run that spawner (#1513).
            enemies.GraphicsSet.LoadGraphicsSet(RoomEnemyGraphicsSetLists.DrawnUnder.FirstOrDefault(set => EnemySheetComposer.CommonSpawnersOf(set, RoomEnemyGraphicsSetLists.Members)
                    .Any(common => common.Objects.Projectiles.Contains(frame)))
                ?? throw new InvalidOperationException($"No graphics set's rooms run a common spawner of projectile frame {frame}."));
            return shared;
        }
        if (SetLoadingSpawner(SpawnedObjectFrameDefinitions.ByEnemy, frame) is ({ } ownSet, { } owner))
        {
            enemies.GraphicsSet.LoadGraphicsSet(ownSet);
            return new SpriteGraphics(SpawnedObjectOwner.Of(owner), PictureShading.Standard);
        }
        if (SetLoadingSpawner(SpawnedObjectFrameDefinitions.SharedByEnemy, frame) is ({ } sharedSet, _))
        {
            enemies.GraphicsSet.LoadGraphicsSet(sharedSet);
            return shared;
        }
        throw new InvalidOperationException($"No retail graphics set spawns projectile frame {frame}.");
    }

    /// <summary>The first retail graphics set loading an enemy <paramref name="spawns"/> says spawns <paramref name="frame"/>, with that enemy.</summary>
    private static (RoomEnemyGraphicsSetDefinition? Set, EnemyDefinitionId? Spawner) SetLoadingSpawner(
        IReadOnlyDictionary<EnemyDefinitionId, SpawnedObjects> spawns, EnemyProjectileFrameId frame)
    {
        foreach (RoomEnemyGraphicsSetDefinition set in RoomEnemyGraphicsSetLists.All)
        {
            foreach (var record in RoomEnemyGraphicsSetLists.MembersOf(set).Records.ToArray())
            {
                if (spawns.TryGetValue(record.DefinitionId, out SpawnedObjects? spawned) && spawned.Projectiles.Contains(frame))
                    return (set, record.DefinitionId);
            }
        }
        return (null, null);
    }

    /// <summary>
    /// Loads into <paramref name="enemies"/> the graphics set a retail room drawing <paramref name="enemy"/> loads, as that
    /// room's load does, so its sheets draw it, and gives <paramref name="slot"/> the indexes it draws at there.
    /// </summary>
    internal static void LoadSetDrawing(RoomEnemySystem enemies, EnemyDefinitionId enemy, RoomEnemySlot? slot = null)
    {
        ArgumentNullException.ThrowIfNull(enemies);
        enemies.GraphicsSet.LoadGraphicsSet(SetDrawing(enemy));
        if (slot is null)
            return;
        slot.Shading = PictureShading.Standard;
    }

    /// <summary>
    /// Shows in <paramref name="enemies"/>' session the first look of each encounter gameplay shows the looks of that
    /// <paramref name="drawings"/> draws its frame in, for an encounter that never draws it as its room loads: the sheet holds
    /// that frame's pictures of only the looks it is drawn in (#1538), so a fixture draws it in one, as the encounter does.
    /// </summary>
    internal static void ShowDrawnLooks(RoomEnemySystem enemies, FrameDrawings drawings)
    {
        ArgumentNullException.ThrowIfNull(enemies);
        ArgumentNullException.ThrowIfNull(drawings);
        foreach (ComposedEncounter encounter in Enum.GetValues<ComposedEncounter>())
        {
            if (EncounterLookUploads.ShownAs(encounter) is EncounterArtwork shown && drawings.For(encounter) is { Looks: [int first and > 0, ..] })
                enemies.Room.Session.Looks.Show(new EncounterLook(shown, EncounterLookUploads.Looks(encounter)[first]));
        }
    }

    /// <summary>The installed enemy artwork with <paramref name="spritemaps"/> in place of its compositions.</summary>
    internal static SuperMetroid.AssetExtraction.Enemies.EnemyTileSourceArtwork InstalledTilesWith(SuperMetroid.AssetExtraction.Enemies.EnemySpritemapCatalog spritemaps)
    {
        SuperMetroid.AssetExtraction.Enemies.EnemyTileSourceArtwork catalog = RepositoryInstallation.Installation.LoadEnemyTiles().Enemies;
        catalog.Spritemaps = spritemaps;
        return catalog;
    }

    /// <summary>The presentation <paramref name="state"/>'s spawned objects draw through.</summary>
    internal static EnemyPresentation Presentation(RoomStateSettings state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var session = new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), InstalledFixtures.EnemyOverreads, InstalledFixtures.BossAndIntroOverreads, new SuperMetroid.Core.Compatibility.RainbowBeamCameraCheckpoint()) { FrameDisplays = InstalledFixtures.EnemyTiles.ExtendedFrames?.Displays, Sheets = InstalledFixtures.EnemySheets };
        var room = new EnemyRoomBinding(session, CompiledRoomEnemyDefinitions.Instance, services: null);
        var graphics = new EnemyGraphicsSet();
        graphics.LoadGraphicsSet(state.EnemyGraphicsSet);
        return new EnemyPresentation(room, graphics);
    }
}
