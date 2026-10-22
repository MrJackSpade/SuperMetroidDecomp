using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Escape;

/// <summary>
/// Publishes the escape timer's installed label and digit frames as whole-frame draws. The
/// cartridge's <c>$80:9F6C-$80:9FD3</c> layout is extracted before gameplay.
/// </summary>
public static class EscapeTimerRenderer
{
    /// <summary>Draws "TIME mm:ss:cc" using the timer's current fixed-point position.</summary>
    public static void Draw(EscapeTimer timer, SpriteFrameBuffer sprites,
        EscapeTimerPresentation presentation, SpriteSheet sheet)
    {
        Ensure.NotNull(timer);
        Ensure.NotNull(sprites);
        Ensure.NotNull(presentation);
        presentation.Draw(timer, sprites, sheet);
    }
}
