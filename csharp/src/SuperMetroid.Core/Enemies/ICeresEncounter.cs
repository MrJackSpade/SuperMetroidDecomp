using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Enemies;

/// <summary>The Ceres encounter's status and the operations the runtime runs at its fixed phase points.</summary>
internal interface ICeresEncounter
{
    /// <summary>How far Ceres's escape has progressed.</summary>
    CeresEscapeStatus CeresStatus { get; set; }

    /// <summary>Whether a Ridley encounter is loaded.</summary>
    bool RidleyPresent { get; }

    /// <summary>The rows of the room's map, from its top, that the loaded Ridley encounter marks explored: none for Ceres Ridley's.</summary>
    int ExploredArenaMapRows { get; }

    /// <summary>Resolves Samus's projectiles against Ceres Ridley before the enemy pass; returns the hits.</summary>
    int ResolveCeresRidleyProjectileHits(SamusProjectileSystem projectiles, SamusBombProjectileSystem sharedProjectiles);

    /// <summary>Draws the Baby and door sprite during the enemy pass, before his animation starts.</summary>
    void DrawCeresRidleyImmediateBabyAndDoor(SpriteFrameBuffer sprites, Position16<RoomPixels> camera);

    /// <summary>Draws the Baby and door sprite over every enemy, once his animation has started.</summary>
    void DrawCeresRidleyBabyAndDoorOverEnemies(SpriteFrameBuffer sprites, Position16<RoomPixels> camera);

    /// <summary>Runs the getaway room's main step over the arena's turning <paramref name="floor"/>.</summary>
    void RunCeresRidleyGetawayRoomMain(SamusState samus, RoomFloor floor, int frameCounter);
}
