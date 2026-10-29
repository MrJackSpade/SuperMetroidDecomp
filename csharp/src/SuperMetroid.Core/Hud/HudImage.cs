namespace SuperMetroid.Core.Hud;

/// <summary>
/// One of the gameplay HUD's pre-coloured installed images (#1466): the static top row, the template, an energy tank, a
/// numeral, an item icon in a shade, a cell of the AUTO indicator or a minimap cell. The HUD's values select images and
/// place them; the import pictures each under every palette the HUD shows in, as the frame its name names.
/// </summary>
public abstract record HudImage
{
    /// <summary>The installed frame that pictures the image.</summary>
    public abstract string FrameName { get; }
}
