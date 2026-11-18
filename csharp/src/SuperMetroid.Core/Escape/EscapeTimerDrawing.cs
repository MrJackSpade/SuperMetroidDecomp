using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Escape;

/// <summary>
/// Publishes the escape timer's installed label and digit frames as whole-frame draws. The
/// timer's layout is extracted before gameplay.
/// </summary>
public static class EscapeTimerDrawing
{
    /// <summary>Draws "TIME mm:ss:cc" where the timer stands now, to a fraction of a pixel.</summary>
    public static void Draw(EscapeTimer timer, SpriteFrameBuffer sprites,
        EscapeTimerPresentation presentation, SpriteSheet sheet)
    {
        Ensure.NotNull(timer);
        Ensure.NotNull(sprites);
        Ensure.NotNull(presentation);
        presentation.Draw(timer, sprites, sheet);
    }
}
