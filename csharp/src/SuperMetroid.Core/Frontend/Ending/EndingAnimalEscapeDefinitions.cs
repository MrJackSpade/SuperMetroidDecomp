using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Definitions for the rescued animals' escape pod.</summary>
internal static class EndingAnimalEscapeDefinitions
{
    /// <summary>The colouring the pod draws in.</summary>
    internal const EndingSpriteColouring Colouring = EndingSpriteColouring.AnimalEscape;
    /// <summary>The program the pod runs.</summary>
    internal const CinematicSpriteSegment Animation = CinematicSpriteSegment.EndingAnimalEscape;
    /// <summary>Both pod coordinates start at 128.</summary>
    internal const int Origin = 128;
    /// <summary>The pod adds 1/512 of a pixel to its Y each actor call.</summary>
    internal static SignedSixteenSixteen YFractionVelocity { get; } = SignedSixteenSixteen.FromParts(0, 128);
    /// <summary>The pod advances one horizontal pixel per actor call.</summary>
    internal const int XVelocity = 1;
    /// <summary>The pod deletes itself when its center reaches 272.</summary>
    internal const int DeleteX = 272;
}
