using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The composition one half of the cartridge's split Samus body shows: a pose's top or bottom half at one of the pose's
/// animation frames. The import composes each whole body picture from the halves its frame shows (#1585).
/// </summary>
/// <param name="Pose">The pose whose half is shown.</param>
/// <param name="Half">The body half.</param>
/// <param name="AnimationFrame">The pose's animation frame.</param>
public readonly record struct SamusBodyHalfFrame(SamusPoseId Pose, SamusBodyHalf Half, int AnimationFrame);
