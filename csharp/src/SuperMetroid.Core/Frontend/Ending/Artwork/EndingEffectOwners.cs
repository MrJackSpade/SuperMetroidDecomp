using System.Collections.Immutable;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>What one of the ending's colour effects is authored for (#1544): the actors and the scenery that show it.</summary>
/// <param name="Actors">The colourings of the actors whose pictures show it.</param>
/// <param name="Scenery">The background pictures and floors that show it.</param>
internal sealed record EndingEffectOwners(ImmutableArray<EndingSpriteColouring> Actors, ImmutableArray<EndingScenery> Scenery);
