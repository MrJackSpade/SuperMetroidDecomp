using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Editable RGB5 image schema for Mother Brain's body disappearance and detached-head gray transition.</summary>
public sealed partial record MotherBrainDeathColorDocument
{
    /// <summary>Schema revision; loading requires <see cref="MotherBrainDeathColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>
    /// The body fade's sixteen stages in the order they show, each holding its fourteen body/brain colors (from $AD:EA0A's
    /// first segments) and its fourteen back-leg colors (the second segments); each RGB5 channel is 0..31, with stock stages
    /// fading to black.
    /// </summary>
    public required MotherBrainBodyColoursDocument[] BodyFade { get; init; }
    /// <summary>Eight ordered fifteen-color detached-head images corresponding to $AD:F119, ending in the stock drained appearance rather than the body fade's black.</summary>
    public required PaletteRgb5[][] CorpseFade { get; init; }
}
