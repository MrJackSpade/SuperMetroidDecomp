using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Immutable constructor data of one of the ending's typed-text actors (#2195).</summary>
/// <param name="Position">Origin in signed screen pixels.</param>
/// <param name="Colouring">The colouring the text's letters draw in (#1512).</param>
/// <param name="Text">The text the actor types.</param>
internal readonly record struct EndingTextDefinition(
    Offset16<ScreenPixels> Position,
    EndingSpriteColouring Colouring,
    EndingTypedText Text);
