namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Where Mother Brain's body shows on BG2 (#1356): the BG2 scroll her posture moves inversely once she takes BG2 over,
/// and whether her death flicker hides it this frame.
/// </summary>
/// <param name="Scroll">The BG2 scroll placing her body, or null before she takes BG2 over.</param>
/// <param name="Hidden">Whether her death flicker hides BG2.</param>
public sealed record MotherBrainBodyPlacement(Position16<Bg2Pixels>? Scroll, bool Hidden);
