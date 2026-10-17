namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// The scrolling sky's register-shadow contribution as an accepted NMI uploaded it: the BG2
/// vertical scroll <c>$88:AFB2</c> writes, and whether the sky selects the 32x64 BG2 map. The
/// per-band horizontal scrolls are an HDMA table read during display and are not part of this latch.
/// </summary>
/// <param name="HorizontalHdmaConfigured">Whether the sky FX installed the horizontal HDMA object and its 32x64 BG2 map.</param>
/// <param name="Bg2Scroll">
/// The BG2 scroll the sky latches. Only <see cref="Position16{TSpace}.Y"/> is the sky's: the
/// layer-one Y it copies, kept with its sixteen-bit wrap. X is zero because the sky never writes
/// the uniform horizontal scroll; its bands come from the HDMA table instead.
/// </param>
public readonly record struct ScrollingSkyRegisterSnapshot(bool HorizontalHdmaConfigured, Position16<Bg2Pixels> Bg2Scroll)
{
    /// <summary>Copies the sky's current register shadows, or null when the room has no scrolling sky.</summary>
    public static ScrollingSkyRegisterSnapshot? Capture(ScrollingSkyState? sky) =>
        sky is null ? null : new(sky.HorizontalHdmaConfigured, new Position16<Bg2Pixels>(0, sky.VerticalScroll));
}
