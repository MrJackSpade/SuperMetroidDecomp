using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The Ceres encounter over Ridley's encounter state and Ceres Ridley.</summary>
/// <param name="encounter">Ridley's encounter state.</param>
/// <param name="ceresRidley">Ceres Ridley.</param>
internal sealed class CeresEncounter(RidleyEncounter encounter, CeresRidleyBehavior ceresRidley) : ICeresEncounter
{
    /// <inheritdoc/>
    public CeresEscapeStatus CeresStatus { get => encounter.CeresStatus; set => encounter.CeresStatus = value; }

    /// <inheritdoc/>
    public bool RidleyPresent => encounter.Ridley is not null;

    /// <inheritdoc/>
    public int ResolveCeresRidleyProjectileHits(SamusProjectileSystem projectiles, SamusBombProjectileSystem sharedProjectiles) =>
        ceresRidley.ResolveCeresRidleyProjectileHits(projectiles, sharedProjectiles);

    /// <inheritdoc/>
    public void DrawCeresRidleyImmediateBabyAndDoor(SpriteFrameBuffer sprites, SpriteArtwork objects, Position16<RoomPixels> camera) =>
        ceresRidley.DrawCeresRidleyImmediateBabyAndDoor(sprites, objects, camera);

    /// <inheritdoc/>
    public void DrawCeresRidleyPostEnemyHook(SpriteFrameBuffer sprites, SpriteArtwork objects, Position16<RoomPixels> camera) =>
        ceresRidley.DrawCeresRidleyPostEnemyHook(sprites, objects, camera);

    /// <inheritdoc/>
    public void RunCeresRidleyGetawayRoomMain(SamusState? samus, int nmiFrameCounter) =>
        ceresRidley.RunCeresRidleyGetawayRoomMain(samus, nmiFrameCounter);
}
