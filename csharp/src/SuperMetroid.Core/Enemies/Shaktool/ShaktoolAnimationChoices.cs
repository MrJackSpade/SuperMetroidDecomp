namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Chooses Shaktool's head aim by direction.</summary>
internal static class ShaktoolAnimationChoices
{
    /// <summary>The head animation aiming along <paramref name="orientation"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="orientation"/> is not one of the eight directions.</exception>
    internal static ShaktoolSegment ForOrientation(CompassDirection orientation) =>
        ShaktoolAnimationDefinitions.OrientationAnimations.TryGetValue(orientation, out ShaktoolSegment segment)
            ? segment
            : throw new InvalidDataException(
                $"Shaktool orientation {orientation} is not one of the eight directions.");
}
