using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The whole picture of Samus's body one update draws (#1513, #1585): her pose at one of the pose's picture frames. The
/// body sheet holds each as one complete figure, which the import composed at installation; nothing the body showed
/// before chooses what it shows now.
/// </summary>
/// <param name="Pose">The pose the body shows.</param>
/// <param name="PictureFrame">The pose's picture frame the body shows.</param>
public readonly record struct SamusBodyFrame(SamusPoseId Pose, int PictureFrame)
{
    /// <summary>The picture's name on the body sheet: its pose and picture frame.</summary>
    public string Name => $"{Pose}.{PictureFrame.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
}
