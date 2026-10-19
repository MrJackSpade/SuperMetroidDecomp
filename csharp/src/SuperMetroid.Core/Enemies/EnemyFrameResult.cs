using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Gunship;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What one enemy pass produced for the runtime to apply after EnemyMain. Each value is applied at the point the native
/// code performs it; see <see cref="EnemyFramePhase"/>.
/// </summary>
/// <param name="Elevator">The elevator lifecycle event, if any.</param>
/// <param name="ElevatorClearedProjectileData">The elevator called <c>ResetProjectileData</c> ($A3:9548); the bomb owner completes it.</param>
/// <param name="DraygonDiscardedSamusPoseRequests">Draygon grabbed or released Samus ($90:E23B/$90:E2DE), discarding alpha's pose requests.</param>
/// <param name="CeresEscapeStarted">Ceres Ridley started the escape ($A6:C117) on this EnemyMain call.</param>
/// <param name="LayerOneXOverride">The Shitroid's direct layer-one X write ($A9:EFE6) in room pixels, if any.</param>
/// <param name="DeleteLiquidHdmaObjects">Mother Brain's tube landed ($A9:8C0C), deleting HDMA objects 0-3.</param>
/// <param name="Gunship">The gunship lifecycle event, if any.</param>
/// <param name="KraidLayer1XBlockReset">Kraid's dead-room function wrote PreviousLayer1XBlock ($A7:C85E).</param>
/// <param name="RoomEffects">The hardcoded PLMs and room writes enemy AI requested.</param>
public readonly record struct EnemyFrameResult(
    ElevatorFrameEvent Elevator,
    bool ElevatorClearedProjectileData,
    bool DraygonDiscardedSamusPoseRequests,
    bool CeresEscapeStarted,
    int? LayerOneXOverride,
    bool DeleteLiquidHdmaObjects,
    GunshipFrameEvent Gunship,
    bool KraidLayer1XBlockReset,
    EnemyRoomEffectRequests RoomEffects);
