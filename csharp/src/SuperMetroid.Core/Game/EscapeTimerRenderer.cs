using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Emits the escape timer's installed label and digit spritemaps into OAM. The
/// cartridge's <c>$80:9F6C-$80:9FD3</c> layout is extracted before gameplay.
/// </summary>
public static class EscapeTimerRenderer
{
    /// <summary>Draws "TIME mm:ss:cc" using the timer's current fixed-point position.</summary>
    public static void Draw(EscapeTimer timer, OamBuffer oam,
        EscapeTimerPresentation presentation)
    {
        ArgumentNullException.ThrowIfNull(timer);
        ArgumentNullException.ThrowIfNull(oam);
        ArgumentNullException.ThrowIfNull(presentation);
        presentation.Draw(timer, oam);
    }
}
