using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The whole picture of Samus's body one update draws (#1513, #1585): her pose at one of the pose's picture frames. The
/// body sheet holds each as one complete figure, which the import composed at installation; nothing the body showed
/// before chooses what it shows now.
/// </summary>
/// <param name="Pose">The pose the body shows.</param>
/// <param name="PictureFrame">The pose's picture frame the body shows.</param>
public readonly record struct SamusBodyFrame(SamusPoseId Pose, SamusAnimationFrame PictureFrame)
{
    /// <summary>
    /// The picture's name on the body sheet: its pose and its frame's name in the pose's animation, the segment and the frame
    /// within it, as the installation names it (#2048).
    /// </summary>
    public string Name => $"{Pose}.{SamusAnimationDelays.AnimationForPose(Pose).NameOf(PictureFrame)}";
}
