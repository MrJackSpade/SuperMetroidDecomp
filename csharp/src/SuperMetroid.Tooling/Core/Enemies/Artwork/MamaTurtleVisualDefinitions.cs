
namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Editable Mama/Baby Turtle OAM compositions selected by the compiled tatori
/// programs. Movement, shell callbacks, durations and collision remain in code.
/// </summary>
internal static class MamaTurtleVisualDefinitions
{
    /// <summary>Tatori instruction and OAM bank $A2.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>Twenty-nine selected Spritemap_BabyTurtle/MamaTurtle records at $A2:94D9..983F.</summary>
    internal const int FrameCount = 29;
}
