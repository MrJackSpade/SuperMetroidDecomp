namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// What Kraid's collision and growth logic read where his head animation stands: the mouth shapes of the step just
/// passed (<see cref="Compatibility.KraidMouthTrailsHead"/>), and where growth resumes the roar by the step shown next.
/// </summary>
/// <param name="Outer">The outer mouth read, tested by body hits.</param>
/// <param name="Inner">The inner mouth read, tested by mouth hits.</param>
/// <param name="Growth">Where growth restarts the head from this place.</param>
internal readonly record struct KraidHeadCursorReads(KraidMouthShapeRead Outer, KraidMouthShapeRead Inner, KraidHeadGrowthResume Growth);
