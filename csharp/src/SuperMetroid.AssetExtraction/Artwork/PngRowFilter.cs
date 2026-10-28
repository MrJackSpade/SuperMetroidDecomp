namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>PNG filter method zero's mutually exclusive scanline predictors.</summary>
internal enum PngRowFilter
{
    /// <summary>Uses zero as the predictor for every byte.</summary>
    None,
    /// <summary>Predicts from the reconstructed byte to the left.</summary>
    Sub,
    /// <summary>Predicts from the reconstructed byte in the previous row.</summary>
    Up,
    /// <summary>Predicts the integer average of the left and upper bytes.</summary>
    Average,
    /// <summary>Chooses the nearest of left, upper, and upper-left bytes by the Paeth rule.</summary>
    Paeth,
}
