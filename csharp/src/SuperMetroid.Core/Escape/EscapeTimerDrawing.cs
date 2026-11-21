using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Escape;

/// <summary>
/// Publishes the escape timer's installed label and digit frames as whole-frame draws. The
/// timer's layout is extracted before gameplay.
/// </summary>
public static partial class EscapeTimerDrawing
{
    /// <summary>Draws "TIME mm:ss:cc" at the whole-pixel place where the timer stands now.</summary>
    public static void Draw(EscapeTimer timer, SpriteFrameBuffer sprites,
        EscapeTimerPresentation presentation, SpriteSheet sheet)
    {
        Ensure.NotNull(timer);
        Ensure.NotNull(sprites);
        Ensure.NotNull(presentation);
        presentation.Draw(timer, sprites, sheet);
    }
}
