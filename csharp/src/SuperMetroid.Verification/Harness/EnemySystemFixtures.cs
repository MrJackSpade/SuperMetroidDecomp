using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Areas.Tourian;

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
    /// artwork bound as the game binds it, and a room object list of its own for the blocks its enemies break, in a
    /// Crateria room unless <paramref name="area"/> is given, with a door passage of its own unless
    /// <paramref name="doorPassage"/> is given.
    /// </summary>
    /// <param name="session">The enemy state and installed artwork that outlive each room.</param>
    /// <param name="compatibility">The compatibility services the room's enemies consult.</param>
    /// <param name="definitions">The room's enemy definition source, or null for the compiled definitions.</param>
    /// <param name="services">The game-state services the room's AI calls, or null for verification services.</param>
    /// <param name="installedBosses">The bosses' installed artwork, or null when the check reads none of it.</param>
    /// <param name="breakableTerrain">The room objects its enemies break blocks into, or null for an empty list of its own.</param>
    /// <param name="area">The area the room is in.</param>
    /// <param name="doorPassage">Samus's passage through a door, or null for one of its own that never starts.</param>
    /// <param name="roomFx">The room's effect state, or null for an effect of its own with no liquid.</param>
    /// <param name="floor">The room's turning floor, or null for a room that shows none.</param>
    /// <param name="statueRoom">The Tourian statue room, or null for any other room.</param>
    /// <param name="shaftTurn">The elevator shaft's turn, or null for a room that is no turning shaft.</param>
    internal static RoomEnemySystem New(EnemySession session, RoomEnemyCompatibility compatibility,
        IRoomEnemyDefinitionSource? definitions = null, IRoomEnemyServices? services = null,
        BossSourceArtwork? installedBosses = null, EnemyBreakableTerrainObjects? breakableTerrain = null,
        AreaId area = AreaId.Crateria, DoorPassage? doorPassage = null, RoomFxOverlayState? roomFx = null,
        RoomFloor? floor = null, ITourianStatueRoom? statueRoom = null, CeresElevatorShaftRoomMainState? shaftTurn = null)
    {
        var system = new RoomEnemySystem(new RoomEnemySystemDependencies
        {
            Session = session,
            Compatibility = compatibility,
            Looks = new EncounterLooks(),
            Definitions = definitions ?? CompiledRoomEnemyDefinitions.Instance,
            Services = services ?? new VerificationRoomEnemyServices(null),
            Area = area,
            BossArtwork = new BossArtworkBinding(InstalledFixtures.Maps.CeresRidleyGetawayBoostDelays, InstalledFixtures.Maps.EscapeTypewriter,
                InstalledFixtures.Maps.MotherBrainRainbowBeamColours, InstalledFixtures.KraidEyes, InstalledFixtures.BossBodySheets),
            BreakableTerrain = breakableTerrain ?? new EnemyBreakableTerrainObjects(new RoomObjectPlaceList(new RoomObjectPlaces())),
            DoorPassage = doorPassage ?? new DoorPassage(),
            RoomFx = roomFx ?? new RoomFxOverlayState(SuperMetroid.Core.Audio.GameplayAudioRequests.Standalone()),
            Earthquake = new RoomEarthquake(),
            Scenery = new RoomEnemyScenery(floor, shaftTurn, statueRoom),
        });
        system.BossArtwork = installedBosses;
        return system;
    }

    /// <summary>
    /// A room's enemy system, as <see cref="New"/> builds it, in a room that can place any enemy: it shows the Ceres
    /// turning floor whose platform the elevator door cycles, turns as the elevator shaft its rotating steam vents ride, and
    /// holds the Tourian statue room whose statue the statue's decorations follow down (#2382, #2426), for a check that
    /// binds definitions it does not pick one by one (#2420).
    /// </summary>
    /// <param name="session">The enemy state and installed artwork that outlive each room.</param>
    /// <param name="compatibility">The compatibility services the room's enemies consult.</param>
    /// <param name="definitions">The room's enemy definition source, or null for the compiled definitions.</param>
    /// <param name="services">The game-state services the room's AI calls, or null for verification services.</param>
    /// <param name="installedBosses">The bosses' installed artwork, or null when the check reads none of it.</param>
    internal static RoomEnemySystem PlacingAnyEnemy(EnemySession session, RoomEnemyCompatibility compatibility,
        IRoomEnemyDefinitionSource? definitions = null, IRoomEnemyServices? services = null, BossSourceArtwork? installedBosses = null) =>
        New(session, compatibility, definitions, services, installedBosses,
            floor: RoomBackgroundsFixtures.CeresFloor(), statueRoom: new FixedTourianStatueRoom(WaterSurface: 216, StatueOffset: 0),
            shaftTurn: new CeresElevatorShaftRoomMainState(new RoomMainHandOvers().ShaftTurnToDebris));

    /// <summary>The Tourian statue scene of <paramref name="enemies"/>, which the check built for the statue room.</summary>
    /// <exception cref="InvalidOperationException">The check built the enemies for another room.</exception>
    internal static TourianEntranceStatueBehavior StatueScene(RoomEnemySystem enemies) => enemies.TourianEntranceStatueBehavior
        ?? throw new InvalidOperationException("The check built its enemies without the statue room, which alone holds the statue scene.");

    /// <summary>The Tourian statues' unlock effects of <paramref name="enemies"/>, which the check built for the statue room.</summary>
    /// <exception cref="InvalidOperationException">The check built the enemies for another room.</exception>
    internal static TourianUnlockBehavior UnlockEffects(RoomEnemySystem enemies) => enemies.TourianUnlockEffects
        ?? throw new InvalidOperationException("The check built its enemies without the statue room, which alone holds the unlock effects.");

    /// <summary>Initializes the statue scene's placement <paramref name="place"/> holds, as the room's enemy load does.</summary>
    /// <exception cref="InvalidOperationException">The place holds no statue scene placement.</exception>
    internal static void InitializeStatuePlacement(RoomEnemyPlace place) =>
        (place.Entity as TourianEntranceStatueEntity
            ?? throw new InvalidOperationException($"Enemy place {place.Ordinal} holds no Tourian statue scene placement.")).Initialize();
}
