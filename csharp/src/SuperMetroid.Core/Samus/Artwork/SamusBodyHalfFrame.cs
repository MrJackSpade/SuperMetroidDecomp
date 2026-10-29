using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The picture one half of Samus's body shows: a pose's top or bottom half at one of the pose's animation frames. It is
/// the identity by which the body's compositions are selected and drawn.
/// </summary>
/// <param name="Pose">The pose whose half is shown.</param>
/// <param name="Half">The body half.</param>
/// <param name="AnimationFrame">The pose's animation frame.</param>
public readonly record struct SamusBodyHalfFrame(SamusPoseId Pose, SamusBodyHalf Half, int AnimationFrame);
