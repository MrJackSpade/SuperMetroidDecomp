using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Where an ending reward actor starts and the colouring it draws in.</summary>
/// <param name="Position">The starting screen position.</param>
/// <param name="Colouring">The colouring the actor's recolourable parts draw in (#1512).</param>
internal readonly partial record struct EndingRewardStart(Offset16<ScreenPixels> Position, EndingSpriteColouring Colouring);
