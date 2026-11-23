using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// A room's Fireflea darkness as an accepted frame shows it (#2053, #2498): the darkness and the shade it subtracts, one
/// value. The darkness chooses its shade during the update, so a shade reaches the screen one frame after it is chosen.
/// </summary>
/// <param name="Shade">The shade the darkness subtracts from the background and the backdrop.</param>
public readonly partial record struct LatchedFirefleaDarkness(RgbColor Shade);
