using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>
/// The shade the Zebes escape's typewriter text is pre-coloured in (#1512): the text in the two colours of the Mother
/// Brain room's tiles the game copies into the text's palette when the escape's typing starts. The import colours it
/// from that room's installed tile colours.
/// </summary>
public static class EscapeTypewriterShades
{
    /// <summary>The typewriter text in the room's colours.</summary>
    public static PictureShade Text { get; } = new("escape-typewriter-text", 0);
}
