using SuperMetroid.Core.Areas.Ceres;
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
/// <param name="doors">The room's Ceres doors, the door of Ridley's room among them.</param>
internal sealed class CeresEncounter(RidleyEncounter encounter, CeresRidleyBehavior ceresRidley, CeresEscape escape, CeresDoorBehavior doors) : ICeresEncounter
{
    /// <inheritdoc/>
    public CeresEscapeStatus CeresStatus { get => encounter.CeresStatus; set => encounter.CeresStatus = value; }

    /// <inheritdoc/>
    public int ResolveCeresRidleyProjectileHits(SamusProjectileSystem projectiles, SamusBombProjectileSystem samusBombs) =>
        ceresRidley.Hits.ResolveCeresRidleyProjectileHits(projectiles, samusBombs);

    /// <inheritdoc/>
    public void StepCeresBabyWithEnemyPass() => ceresRidley.BabyScene.StepCeresBabyWithEnemyPass();

    /// <inheritdoc/>
    public void StepCeresBabyAfterActors() => ceresRidley.BabyScene.StepCeresBabyAfterActors();

    /// <inheritdoc/>
    public void PublishCeresBaby(SpriteFrameBuffer sprites, Position16<RoomPixels> camera) =>
        ceresRidley.BabyScene.PublishCeresBaby(sprites, camera);

    /// <inheritdoc/>
    /// <remarks>
    /// The overlay is a picture of Ceres Ridley's fight, installed with his room's pictures: a visit to his room after he is
    /// gone, where the door hides itself as it would for the overlay, shows no door there.
    /// </remarks>
    public void PublishRidleyRoomDoorOverlay(SpriteFrameBuffer sprites, Position16<RoomPixels> camera)
    {
        if (encounter.CeresRidley is not null)
            doors.PublishRidleyRoomDoorOverlay(sprites, camera);
    }

    /// <inheritdoc/>
    public void RunCeresRidleyGetawayRoomMain(SamusState samus, RoomFloor floor, int frameCounter) =>
        escape.RunGetawayRoomMain(samus, floor, frameCounter);
}
