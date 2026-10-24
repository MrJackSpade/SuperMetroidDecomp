namespace SuperMetroid.Core.Assets;

/// <summary>
/// The identity of an animated piece of artwork whose picture a behaviour selects (#1406): a room's lava, a treadmill, a
/// statue, a corpse's rot or a boss's melt. Asset extraction pictures every frame of it, and the display shows the frame
/// the behaviour selected last.
/// </summary>
/// <param name="Name">The animation's name, which its frames' installed pictures are keyed by.</param>
public readonly record struct ArtworkAnimation(string Name)
{
    /// <summary>The animation's name.</summary>
    public override string ToString() => Name;
}
