namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Where Mother Brain's body shows on her body layer (#1356): the layer placement her posture moves inversely once her
/// body layer is shown, and whether her death flicker hides it this frame.
/// </summary>
/// <param name="LayerPlacement">The layer position shown at the screen's top-left, or null before her body layer is placed.</param>
/// <param name="Hidden">Whether her death flicker hides her body layer.</param>
public sealed record MotherBrainBodyPlacement(Position16<BackgroundPixels>? LayerPlacement, bool Hidden);
