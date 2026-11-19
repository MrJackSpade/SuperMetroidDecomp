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
    int ResolveCeresRidleyProjectileHits(SamusProjectileSystem projectiles, SamusBombProjectileSystem samusBombs);

    /// <summary>Runs Ceres Ridley's Baby's animation as the enemy pass ends, until his animation starts (#2573).</summary>
    void StepCeresBabyWithEnemyPass();

    /// <summary>Runs Ceres Ridley's Baby's animation after the actors' drawing steps, once his animation has started (#2573).</summary>
    void StepCeresBabyAfterActors();

    /// <summary>Shows Ceres Ridley's Baby where it is, once, at its picture's own depth among the actors (#2573).</summary>
    void PublishCeresBaby(SpriteFrameBuffer sprites, Position16<RoomPixels> camera);

    /// <summary>Shows the door of Ceres Ridley's room as its own overlay picture while it shows it during his fight (#2573).</summary>
    void PublishRidleyRoomDoorOverlay(SpriteFrameBuffer sprites, Position16<RoomPixels> camera);

    /// <summary>Runs the getaway room's main step over the arena's turning <paramref name="floor"/>.</summary>
    void RunCeresRidleyGetawayRoomMain(SamusState samus, RoomFloor floor, int frameCounter);
}
