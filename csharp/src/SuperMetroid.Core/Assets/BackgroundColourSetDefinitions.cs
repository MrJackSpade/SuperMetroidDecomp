namespace SuperMetroid.Core.Assets;

/// <summary>The roles gameplay's background sets play beyond the images that draw in them (#1464).</summary>
public static class BackgroundColourSetDefinitions
{
    /// <summary>The set whose colour <see cref="BackdropColour"/> is the backdrop, shown where nothing is drawn.</summary>
    public const BackgroundColourSet Backdrop = BackgroundColourSet.Hud;

    /// <summary>The backdrop's colour within <see cref="Backdrop"/>: its first.</summary>
    public const int BackdropColour = 0;
}
