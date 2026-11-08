using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The tints the accepted frame's effects blend with, each published by its own effect. Effects choose them during the
/// main loop, so a tint reaches the screen one frame after it is chosen, even though the same effect's window is read
/// live during display.
/// </summary>
/// <param name="Fireflea">The shade Fireflea darkness subtracts, or null when the room's effect is not Fireflea darkness.</param>
/// <param name="PowerBomb">The power-bomb explosion's colour.</param>
/// <param name="SuitPickup">The suit pickup light beam's colour.</param>
/// <param name="RainbowBeam">Mother Brain's rainbow beam's colour, or null when no Mother Brain exists.</param>
public readonly record struct GameplayLatchedTints(
    RgbColor? Fireflea,
    RgbColor PowerBomb,
    RgbColor SuitPickup,
    RgbColor? RainbowBeam);
