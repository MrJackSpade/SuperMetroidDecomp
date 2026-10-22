namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Which of the three rope character sets the grapple's angle selects. Each half turn of 32 sectors is reflected about
/// its midpoint, and the sectors from its ends take three, eight and five sector widths.
/// </summary>
public enum GrappleRopeOrientation
{
    /// <summary>The five sectors either side of the half turn's midpoint.</summary>
    Horizontal,

    /// <summary>The eight sectors between the vertical and horizontal ones.</summary>
    Diagonal,

    /// <summary>The three sectors at each end of the half turn.</summary>
    Vertical,
}
