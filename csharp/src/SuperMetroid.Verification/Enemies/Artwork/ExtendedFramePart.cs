using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>One piece of an extended enemy frame the draw path places in more than one piece (#1513).</summary>
/// <param name="Frame">The extended frame an enemy shows.</param>
/// <param name="Component">The piece's place among the frame's components.</param>
/// <param name="Components">How many pieces the draw path places for the frame, each as its own sheet frame.</param>
internal readonly record struct ExtendedFramePart(EnemyFrameId Frame, int Component, int Components);
