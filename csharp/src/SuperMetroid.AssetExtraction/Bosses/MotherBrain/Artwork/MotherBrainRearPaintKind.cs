namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Where a Mother Brain palette phase's rear legs take their colours from.</summary>
internal enum MotherBrainRearPaintKind
{
    /// <summary>The phase's retained rear colours, a rainbow phase's rear subset, or half-intensity body paint.</summary>
    Derived,
    /// <summary>The full stock rear-leg palette.</summary>
    Stock,
    /// <summary>The stock rear subset at half intensity, as the drained body shows it.</summary>
    Drained,
    /// <summary>The stock rear-leg subset.</summary>
    NormalSubset,
}
