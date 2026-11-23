namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// How steeply the grapple rope runs, which picks the pictures it is drawn with. A rope's angle repeats its pictures
/// every half turn, and each half turn mirrors about level: a rope near upright is drawn vertical, one near level
/// horizontal, and one between diagonal (<see cref="GrappleRopeOrientations"/>).
/// </summary>
public enum GrappleRopeOrientation
{
    /// <summary>A rope near level.</summary>
    Horizontal,

    /// <summary>A rope between the vertical and horizontal ones.</summary>
    Diagonal,

    /// <summary>A rope near upright.</summary>
    Vertical,
}
