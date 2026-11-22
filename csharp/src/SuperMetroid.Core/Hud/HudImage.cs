namespace SuperMetroid.Core.Hud;

/// <summary>
/// One of the gameplay HUD's pre-coloured installed images (#1466): the static top row, the template, an energy tank, a
/// numeral, an item icon in a shade, the AUTO indicator in one of its states or a minimap cell. The HUD's values select images and
/// place them; the import pictures each in every colouring the HUD shows in, as the frame its name names.
/// </summary>
public abstract partial record HudImage
{
    /// <summary>The installed frame that pictures the image.</summary>
    public abstract string FrameName { get; }
}
