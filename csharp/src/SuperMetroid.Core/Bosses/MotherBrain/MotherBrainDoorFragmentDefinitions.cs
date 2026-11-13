using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Compiled physical definitions for Mother Brain's eight escape-door fragments.</summary>
internal static class MotherBrainDoorFragmentDefinitions
{
    /// <summary>Eight authored fragments are selected by the projectile's zero-based spawn parameter.</summary>
    internal const int Count = 8;

    /// <summary>Interleaved with the X velocity: the fragments' authored scatter speeds.</summary>
    internal static ImmutableArray<SignedEightEight> YVelocities { get; } =
    [
        SignedEightEight.FromPixels(-2),
        SignedEightEight.FromPixels(-1),
        SignedEightEight.FromPixels(-1),
        SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-128),
        SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-128),
        SignedEightEight.FromTwoHundredFiftySixthsOfPixel(128),
        SignedEightEight.FromPixels(-1),
        SignedEightEight.FromPixels(2),
    ];

    /// <summary>Every fragment flies right, five pixels an update.</summary>
    internal static SignedEightEight XVelocity { get; } = SignedEightEight.FromPixels(5);

    /// <summary>The first fragment's Y offset, 32 pixels above the door.</summary>
    internal const int FirstY = -32;

    /// <summary>Each following fragment starts this many pixels lower.</summary>
    internal const int YStep = 8;
}
