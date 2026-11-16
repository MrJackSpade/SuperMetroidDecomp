using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The warning Ceres's self-destruct readies and types once the getaway ends (#2228): its phase, its readying, its text
/// delay, its typewriter, its alarm's step and what it has written over the arena's foreground.
/// </summary>
public sealed class CeresSelfDestructWarning
{
    /// <summary>Which step of the warning runs next; the getaway's end starts it.</summary>
    internal CeresSelfDestructPhase Phase { get; set; }
    /// <summary>Where the warning stands in its current stage of readying, advanced each update (#1406, #1538).</summary>
    internal CeresEscapeReadying Readying { get; set; }
    /// <summary>Gets the warning phase or glyph delay countdown, including the initial 128-frame wait.</summary>
    public int TextDelayTimer { get; internal set; }
    /// <summary>Gets the installed warning typewriter state, created after the initial delay when presentation data is available.</summary>
    public EscapeTypewriterState? Typewriter { get; internal set; }
    /// <summary>Gets the step the warning's sixteen-step alarm shows, zero through fifteen, returning to zero after fifteen.</summary>
    public int AlarmStep { get; internal set; }
    /// <summary>
    /// What the warning shows over the arena's foreground (#1555): its EMERGENCY title, the Japanese subtitle and the glyphs
    /// its typewriter has typed, in the order shown, each at its foreground cell; nothing before it shows any.
    /// </summary>
    public RoomWrittenOverlay Overlay { get; private set; } = RoomWrittenOverlay.Empty;

    /// <summary>Shows <paramref name="picture"/> as the next part of <see cref="Overlay"/>.</summary>
    internal void Show(WrittenPicture picture) => Overlay = Overlay.With(picture);
}
