using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Assets;

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
    public int ResolveCeresRidleyProjectileHits(SamusProjectileSystem projectiles, SamusBombProjectileSystem sharedProjectiles) =>
        ceresRidley.Hits.ResolveCeresRidleyProjectileHits(projectiles, sharedProjectiles);

    /// <inheritdoc/>
    public void DrawCeresRidleyImmediateBabyAndDoor(SpriteFrameBuffer sprites, Position16<RoomPixels> camera) =>
        ceresRidley.BabyScene.DrawCeresRidleyImmediateBabyAndDoor(sprites, camera);

    /// <inheritdoc/>
    public void DrawCeresRidleyBabyAndDoorOverEnemies(SpriteFrameBuffer sprites, Position16<RoomPixels> camera) =>
        ceresRidley.BabyScene.DrawCeresRidleyBabyAndDoorOverEnemies(sprites, camera);

    /// <inheritdoc/>
    public void RunCeresRidleyGetawayRoomMain(SamusState? samus, int frameCounter) =>
        escape.RunGetawayRoomMain(samus, frameCounter);
}
