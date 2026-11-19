using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The Ceres encounter over the room's view of Ridley, Ceres Ridley and the station's escape.</summary>
/// <param name="encounter">The room's view of Ridley's encounters.</param>
/// <param name="ceresRidley">Ceres Ridley.</param>
/// <param name="escape">The station's escape, whose getaway the room main runs.</param>
internal sealed class CeresEncounter(RidleyEncounter encounter, CeresRidleyBehavior ceresRidley, CeresEscape escape) : ICeresEncounter
{
    /// <inheritdoc/>
    public CeresEscapeStatus CeresStatus { get => encounter.CeresStatus; set => encounter.CeresStatus = value; }

    /// <inheritdoc/>
    public bool RidleyPresent => encounter.RidleyPresent;

    /// <inheritdoc/>
    public int ExploredArenaMapRows => encounter.ExploredArenaMapRows;

    /// <inheritdoc/>
    public int ResolveCeresRidleyProjectileHits(SamusProjectileSystem projectiles, SamusBombProjectileSystem sharedProjectiles) =>
        ceresRidley.Hits.ResolveCeresRidleyProjectileHits(projectiles, sharedProjectiles);

    /// <inheritdoc/>
    public void StepCeresBabyWithEnemyPass() => ceresRidley.BabyScene.StepCeresBabyWithEnemyPass();

    /// <inheritdoc/>
    public void StepCeresBabyAfterActors() => ceresRidley.BabyScene.StepCeresBabyAfterActors();

    /// <inheritdoc/>
    public void PublishCeresBaby(SpriteFrameBuffer sprites, Position16<RoomPixels> camera) =>
        ceresRidley.BabyScene.PublishCeresBaby(sprites, camera);

    /// <inheritdoc/>
    public void RunCeresRidleyGetawayRoomMain(SamusState samus, RoomFloor floor, int frameCounter) =>
        escape.RunGetawayRoomMain(samus, floor, frameCounter);
}
