using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Verification access: the charge flare's pictures by the number the original counts them by, each component's pictures
/// numbered through its animation's phases in play order.
/// </summary>
internal static class ChargeFlarePictureNumbers
{
    /// <summary>The main flare's picture numbered <paramref name="number"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The main flare has no picture of that number.</exception>
    internal static MainFlarePicture MainFlare(int number) =>
        Numbered(ChargeFlareAnimationDefinitions.MainFlareSegments.PlayedBeats(ChargeFlareAnimationDefinitions.MainFlareAnimation.Animation), number, "main flare");

    /// <summary>The slow sparks' picture numbered <paramref name="number"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The slow sparks have no picture of that number.</exception>
    internal static SparkPicture SlowSparks(int number) =>
        Numbered(ChargeFlareAnimationDefinitions.SparkSegments.PlayedBeats(ChargeFlareAnimationDefinitions.SlowSparksAnimation.Animation), number, "slow sparks");

    /// <summary>The fast sparks' picture numbered <paramref name="number"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The fast sparks have no picture of that number.</exception>
    internal static SparkPicture FastSparks(int number) =>
        Numbered(ChargeFlareAnimationDefinitions.SparkSegments.PlayedBeats(ChargeFlareAnimationDefinitions.FastSparksAnimation.Animation), number, "fast sparks");

    /// <summary>The picture of <paramref name="beats"/> numbered <paramref name="number"/>.</summary>
    private static TPicture Numbered<TPicture>(IReadOnlyList<AnimationBeat<TPicture, NoAnimationCue>> beats, int number, string component)
        where TPicture : struct =>
        number >= 0 && number < beats.Count
            ? beats[number].ShownFrame ?? throw new InvalidOperationException($"Charge-flare {component} picture {number} shows no picture.")
            : throw new ArgumentOutOfRangeException(nameof(number), number, $"Charge-flare {component} has no picture {number}.");
}
