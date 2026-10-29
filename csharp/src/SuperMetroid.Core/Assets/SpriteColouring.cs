namespace SuperMetroid.Core.Assets;

/// <summary>
/// What colours a sprite draws in, named by what they are rather than by the sprite set that holds them (#1464): an
/// enemy's own loaded colours, a shared set such as the common colours a hit flash shows, the colours an encounter loads
/// for itself, or a frontend screen's colour group. The simulation's actors derive their colourings from this; each
/// load's <see cref="SpriteColourPlacement"/> places them in the sprite sets.
/// </summary>
public abstract record SpriteColouring
{
    /// <summary>Only this assembly's colourings derive from it.</summary>
    private protected SpriteColouring()
    {
    }
}
