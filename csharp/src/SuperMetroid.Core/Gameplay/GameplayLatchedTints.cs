using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The tints the accepted frame's glows blend with, each published by its own effect. Effects choose them during the
/// update, so a tint reaches the screen one frame after it is chosen, even though the same effect's window is read
/// live during display.
/// </summary>
/// <param name="PowerBomb">The power-bomb explosion's colour.</param>
/// <param name="SuitPickup">The suit pickup light beam's colour.</param>
public readonly partial record struct GameplayLatchedTints(
    RgbColor PowerBomb,
    RgbColor SuitPickup);
