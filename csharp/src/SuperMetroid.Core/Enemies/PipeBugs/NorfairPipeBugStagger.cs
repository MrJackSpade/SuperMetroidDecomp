namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>How a Norfair formation member moves to its place after rising (#2071, #2473).</summary>
/// <param name="Direction">The way it moves from where its rise ended: on up past Samus, or back down.</param>
/// <param name="Distance">How far, in pixels, it moves that way.</param>
internal sealed partial record NorfairPipeBugStagger(VerticalDirection Direction, int Distance);
