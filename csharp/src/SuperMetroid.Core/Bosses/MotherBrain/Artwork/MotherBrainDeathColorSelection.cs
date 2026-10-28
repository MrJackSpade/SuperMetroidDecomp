using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Mother Brain's selected death colours: the body, leg and corpse fades and the exploded door.</summary>
/// <param name="BodyFade">Sixteen-stage body and brain palette progression toward black.</param>
/// <param name="LegFade">Separate sixteen-stage back-leg palette progression.</param>
/// <param name="CorpseFade">Eight-stage detached-head progression toward the drained palette.</param>
/// <param name="ExplodedDoor">Edited escape-door colors, or null when the canonical image suffices.</param>
internal sealed record MotherBrainDeathColorSelection(MotherBrainDeathFade BodyFade, MotherBrainDeathFade LegFade,
    MotherBrainDeathFade CorpseFade, RgbColor[]? ExplodedDoor);
