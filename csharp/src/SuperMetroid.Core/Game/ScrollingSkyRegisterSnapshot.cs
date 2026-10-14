namespace SuperMetroid.Core.Game;

/// <summary>
/// The scrolling sky's register-shadow contribution as an accepted NMI uploaded it: the BG2VOFS
/// mirror <c>$88:AFB2</c> writes, and whether the sky selects the 32x64 BG2 map. The per-band
/// BG2HOFS values are an HDMA table read during display and are not part of this latch.
/// </summary>
/// <param name="HorizontalHdmaConfigured">Whether the sky FX installed the horizontal HDMA object and its 32x64 BG2 map.</param>
/// <param name="VerticalScroll">BG2VOFS mirror written from layer-one Y.</param>
public readonly record struct ScrollingSkyRegisterSnapshot(bool HorizontalHdmaConfigured, ushort VerticalScroll)
{
    /// <summary>Copies the sky's current register shadows, or null when the room has no scrolling sky.</summary>
    public static ScrollingSkyRegisterSnapshot? Capture(ScrollingSkyState? sky) =>
        sky is null ? null : new(sky.HorizontalHdmaConfigured, sky.VerticalScroll);
}
