namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>
/// The sizes of Samus's colour sets as the import reads, stores and pre-colours them (#1405, #1512). Which inks of her
/// sheets each colouring recolours is the import's too.
/// </summary>
public static class SamusColourSets
{
    /// <summary>
    /// The colours in each of Samus's colour sets: one colouring of her suit or her beams, and every palette her effects
    /// show.
    /// </summary>
    public const int ColoursPerSet = 16;

    /// <summary>The beam colours the Hyper Beam's own cycle shows.</summary>
    public const int HyperBeamColours = 8;

    /// <summary>The frames of the Hyper Beam's own colour cycle: red, yellow, green, blue and magenta, each followed by its blend into the next.</summary>
    public const int HyperBeamCycleFrames = 10;
}
