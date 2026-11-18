using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Definitions for the rescued animals' escape pod.</summary>
internal static class EndingAnimalEscapeDefinitions
{
    /// <summary>The colouring the pod draws in.</summary>
    internal const EndingSpriteColouring Colouring = EndingSpriteColouring.AnimalEscape;
    /// <summary>The animation the pod runs.</summary>
    internal const EndingSceneAnimation Animation = EndingSceneAnimation.AnimalEscape;
    /// <summary>Both pod coordinates start at 128.</summary>
    internal const int Origin = 128;
    /// <summary>How far the pod drifts down each update.</summary>
    internal static SignedSixteenSixteen YVelocity { get; } = SignedSixteenSixteen.FromPixelFraction(1, 512);
    /// <summary>The pod advances one horizontal pixel an update.</summary>
    internal const int XVelocity = 1;
    /// <summary>The pod deletes itself when its center reaches 272.</summary>
    internal const int DeleteX = 272;
}
