using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Editable RGB5 image schema for Mother Brain's body disappearance, detached-head gray transition, and escape-door explosion.</summary>
public sealed record MotherBrainDeathColorDocument
{
    /// <summary>Schema revision; loading requires version one from <see cref="MotherBrainDeathColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Sixteen ordered fourteen-color body/brain images from $AD:EA0A's first segments; each RGB5 channel is 0..31, with stock stages fading to black.</summary>
    public required PaletteRgb5[][] BodyFade { get; init; }
    /// <summary>Sixteen ordered fourteen-color back-leg images from $AD:EA0A's second segments, separate from body colors but selected by the same fade-stage index.</summary>
    public required PaletteRgb5[][] LegFade { get; init; }
    /// <summary>Eight ordered fifteen-color detached-head images corresponding to $AD:F119, ending in the stock drained appearance rather than the body fade's black.</summary>
    public required PaletteRgb5[][] CorpseFade { get; init; }
    /// <summary>Fourteen nonnull RGB5 colors corresponding to $A9:9534, replacing sprite-palette-one colors 1..14 for the exploded escape door.</summary>
    public required PaletteRgb5[] ExplodedDoor { get; init; }
}
