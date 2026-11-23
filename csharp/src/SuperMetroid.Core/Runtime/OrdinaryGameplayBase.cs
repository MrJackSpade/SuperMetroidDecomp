using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// An ordinary gameplay frame's base (#2498): the room's scene, on which the room's effects compose, and the HUD's layer,
/// drawn straight after it.
/// </summary>
/// <param name="Room">The room's layers and the actors, as the frame shows them before any effect blends them.</param>
/// <param name="Hud">The HUD the frame boundary latched, its own layer (#1466).</param>
public sealed partial record OrdinaryGameplayBase(GameplaySceneLayer Room, HudLayer Hud);
