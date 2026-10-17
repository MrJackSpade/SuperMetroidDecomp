using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

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
    public void DrawCeresRidleyImmediateBabyAndDoor(OamBuffer oam, Position16<RoomPixels> camera) =>
        ceresRidley.DrawCeresRidleyImmediateBabyAndDoor(oam, camera);

    /// <inheritdoc/>
    public void DrawCeresRidleyPostEnemyHook(OamBuffer oam, Position16<RoomPixels> camera) =>
        ceresRidley.DrawCeresRidleyPostEnemyHook(oam, camera);

    /// <inheritdoc/>
    public void RunCeresRidleyGetawayRoomMain(SamusState? samus, ushort nmiFrameCounter) =>
        ceresRidley.RunCeresRidleyGetawayRoomMain(samus, nmiFrameCounter);
}
