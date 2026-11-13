using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Bosses;

namespace SuperMetroid.Core.World;

/// <summary>
/// What one enemy pass produced for the runtime to apply after it. Each value is applied at the point the game
/// performs it; see <see cref="EnemyFramePhase"/>.
/// </summary>
/// <param name="Elevator">The elevator lifecycle event, if any.</param>
/// <param name="ElevatorClearedProjectileData">The elevator cleared Samus's projectiles; the bomb owner completes it.</param>
/// <param name="DraygonDiscardedSamusPoseRequests">Draygon grabbed or released Samus, discarding the input phase's pose requests.</param>
/// <param name="CeresEscapeStarted">Ceres Ridley started the escape on this enemy pass.</param>
/// <param name="CameraXOverride">The Shitroid's direct camera X write in room pixels, if any.</param>
/// <param name="StopLiquidMotion">Mother Brain's tube landed, stopping the liquid surface's motion from the next frame.</param>
/// <param name="Gunship">The gunship lifecycle event, if any.</param>
/// <param name="RoomEffects">The hardcoded room objects and room writes enemy AI requested.</param>
[IndependentFlag("independent events one enemy frame reports to the runtime")]
public readonly record struct EnemyFrameResult(
    ElevatorFrameEvent Elevator,
    bool ElevatorClearedProjectileData,
    bool DraygonDiscardedSamusPoseRequests,
    bool CeresEscapeStarted,
    int? CameraXOverride,
    bool StopLiquidMotion,
    GunshipFrameEvent Gunship,
    EnemyRoomEffectRequests RoomEffects);
