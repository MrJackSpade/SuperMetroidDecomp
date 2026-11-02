namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Who owns a Sidehopper palette's colors: the reviewed stock paint, calculated shades of independent paints, or the supplied colors.</summary>
internal enum SidehopperPaletteOwnership
{
    /// <summary>Every color is the reviewed stock paint.</summary>
    Stock,

    /// <summary>Independent paints are kept and the dependent shades calculate from them.</summary>
    Calculated,

    /// <summary>Every supplied color is kept as supplied.</summary>
    Supplied,
}
