namespace Engine.World;

/// <summary>Which edge of its tile a slope's solid part is measured from.</summary>
public enum SlopeAnchor
{
    /// <summary>From the tile's bottom: a floor.</summary>
    Bottom,

    /// <summary>From the tile's top: a ceiling.</summary>
    Top,
}
