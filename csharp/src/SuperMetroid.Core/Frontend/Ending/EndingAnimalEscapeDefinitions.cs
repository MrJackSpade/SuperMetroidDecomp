using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Definitions for the rescued animals' escape pod.</summary>
internal static class EndingAnimalEscapeDefinitions
{
    /// <summary>The colouring the pod draws in.</summary>
    internal const EndingSpriteColouring Colouring = EndingSpriteColouring.AnimalEscape;
    /// <summary>The animation the pod runs.</summary>
    internal const EndingSceneAnimation Animation = EndingSceneAnimation.AnimalEscape;
    /// <summary>Where both pod coordinates start.</summary>
    internal const int Origin = 128;
    /// <summary>How far the pod drifts down each update.</summary>
    internal static SignedSixteenSixteen YVelocity { get; } = SignedSixteenSixteen.FromPixelFraction(1, 512);
    /// <summary>How far the pod advances across each update.</summary>
    internal const int XVelocity = 1;
    /// <summary>The X, past the screen's right, at which the pod deletes itself.</summary>
    internal const int DeleteX = 272;
}
