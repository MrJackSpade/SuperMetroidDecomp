namespace SuperMetroid.Core.Assets;

/// <summary>
/// What colours a sprite draws in, named by what they are rather than by the sprite palette row that holds them (#1464):
/// an enemy's own loaded colours, a shared set such as the common colours a hit flash shows, or the colours an encounter
/// loads for itself. The simulation's actors derive their colourings from this; one resolver per loaded graphics set
/// places each in a row until colour sets are keyed by owner.
/// </summary>
public abstract record SpriteColouring
{
    /// <summary>Only this assembly's colourings derive from it.</summary>
    private protected SpriteColouring()
    {
    }
}
