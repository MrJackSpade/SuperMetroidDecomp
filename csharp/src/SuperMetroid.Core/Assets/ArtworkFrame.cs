namespace SuperMetroid.Core.Assets;

/// <summary>
/// One frame of an animated piece of artwork (#1406): the picture a behaviour selects to show. Asset extraction pictures
/// each frame under this identity, and the display looks the selected frame's pictures up by it.
/// </summary>
/// <param name="Animation">The animation the frame belongs to.</param>
/// <param name="Name">The frame's name within its animation.</param>
public readonly record struct ArtworkFrame(ArtworkAnimation Animation, string Name)
{
    /// <summary>The artwork identity the frame's installed pictures are keyed by.</summary>
    public BgArtworkIdentity Artwork => new(ToString());

    /// <summary>The frame's full name: its animation's, then its own.</summary>
    public override string ToString() => $"{Animation.Name} {Name}";
}
