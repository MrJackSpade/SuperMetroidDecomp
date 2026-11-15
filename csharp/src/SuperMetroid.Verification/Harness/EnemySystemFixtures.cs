using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// The room enemy systems checks build outside the running game (#1992). The game builds each with its room's enemy
/// definitions, services, boss artwork and room objects; a check supplies those it sets itself, and this fixture the rest.
/// </summary>
internal static class EnemySystemFixtures
{
    /// <summary>
    /// A room's enemy system: the compiled enemy definitions unless <paramref name="definitions"/> is given, services that
    /// fail at the exact dependency a check did not set unless <paramref name="services"/> is given, the bosses' installed
    /// artwork bound as the game binds it, and a room object list of its own for the blocks its enemies break.
    /// </summary>
    /// <param name="session">The enemy state and installed artwork that outlive each room.</param>
    /// <param name="compatibility">The compatibility services the room's enemies consult.</param>
    /// <param name="definitions">The room's enemy definition source, or null for the compiled definitions.</param>
    /// <param name="services">The game-state services the room's AI calls, or null for verification services.</param>
    /// <param name="installedBosses">The bosses' installed artwork, or null when the check reads none of it.</param>
    /// <param name="breakableTerrain">The room objects its enemies break blocks into, or null for an empty list of its own.</param>
    internal static RoomEnemySystem New(EnemySession session, RoomEnemyCompatibility compatibility,
        IRoomEnemyDefinitionSource? definitions = null, IRoomEnemyServices? services = null,
        BossSourceArtwork? installedBosses = null, EnemyBreakableTerrainObjects? breakableTerrain = null)
    {
        var system = new RoomEnemySystem(session, compatibility, definitions ?? CompiledRoomEnemyDefinitions.Instance,
            services ?? new VerificationRoomEnemyServices(null), new BossArtworkBinding(),
            breakableTerrain ?? new EnemyBreakableTerrainObjects(new RoomObjectPlaceList(new RoomObjectPlaces())));
        system.BossArtwork = installedBosses;
        return system;
    }
}
