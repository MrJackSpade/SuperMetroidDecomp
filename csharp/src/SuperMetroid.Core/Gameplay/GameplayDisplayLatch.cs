using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The scene one shown (or loading) frame shows (#1407), as the frame boundary latched it, already resolved (#2498): one
/// kind for each kind of frame. Before the first room the frame shows no room (<see cref="LatchBeforeFirstRoom"/>); a room
/// shows its ordinary layers (<see cref="OrdinaryRoomFrameLatch"/>) or a Ceres perspective floor in their place
/// (<see cref="PerspectiveFloorFrameLatch"/>). Gameplay latches it at the frame boundary, before the update's controller
/// latch, so a change an update makes shows from the next shown frame. An update without a frame boundary leaves the
/// previous latch in place. Arrays are owned copies; simulation cannot change them.
/// </summary>
/// <param name="Hud">The HUD's images this frame boundary latched, which the pause screen keeps (#1466).</param>
public abstract partial record GameplayDisplayLatch(ShownHud Hud);
