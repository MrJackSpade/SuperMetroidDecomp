namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// What Kraid's collision and growth logic read where a head-animation cursor stands. The mouth reads look at the step
/// just behind the cursor: after a pose, its two shapes; after a cry, the outer read finds the pose before the cry's
/// inner shape and the inner read finds no pose; at an animation's start, the reads look into the animation authored before.
/// </summary>
/// <param name="Outer">The outer mouth read, tested by body hits.</param>
/// <param name="Inner">The inner mouth read, tested by mouth hits.</param>
/// <param name="Growth">Where growth restarts the head from this cursor.</param>
internal readonly record struct KraidHeadCursorReads(KraidMouthShapeRead Outer, KraidMouthShapeRead Inner, KraidHeadGrowthResume Growth);
