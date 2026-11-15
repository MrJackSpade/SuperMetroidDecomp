namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The two looks a collectible lying in the room alternates between (#1994): the look it shows first and the look it
/// changes to, four updates each. What each look shows is the kind's own: a tank's lit or dimmed block, an item's own
/// picture in one colouring or the other.
/// </summary>
public enum CollectibleLook
{
    /// <summary>The look the collectible shows first.</summary>
    Primary,
    /// <summary>The look the collectible changes to.</summary>
    Alternate,
}
