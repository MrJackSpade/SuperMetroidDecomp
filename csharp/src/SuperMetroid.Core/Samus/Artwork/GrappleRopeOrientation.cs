namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// How steeply the grapple rope runs, which picks the pictures it is drawn with. A rope's angle repeats its pictures
/// every half turn, and each half turn mirrors about level: a rope within three sixty-fourths of a turn of upright is
/// drawn vertical, one within five of level horizontal, and one between diagonal.
/// </summary>
public enum GrappleRopeOrientation
{
    /// <summary>A rope within five sixty-fourths of a turn of level.</summary>
    Horizontal,

    /// <summary>A rope in the eight sixty-fourths of a turn between the vertical and horizontal ones.</summary>
    Diagonal,

    /// <summary>A rope within three sixty-fourths of a turn of upright.</summary>
    Vertical,
}
