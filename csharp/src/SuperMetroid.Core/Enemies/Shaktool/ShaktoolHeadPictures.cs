using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Chooses the picture Shaktool's head shows from the way it aims (#2284).</summary>
internal static partial class ShaktoolHeadPictures
{
    /// <summary>The picture of the head aiming along <paramref name="orientation"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="orientation"/> is not one of the eight directions.</exception>
    internal static EnemyFrameId Aiming(CompassDirection orientation) =>
        ShaktoolHeadPictureDefinitions.AimedPictures[orientation];
}
