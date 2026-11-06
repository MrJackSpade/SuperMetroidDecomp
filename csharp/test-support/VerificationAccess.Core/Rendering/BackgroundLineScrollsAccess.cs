namespace SuperMetroid.Core.Rendering;

/// <summary>Verification access to <see cref="BackgroundLineScrolls"/> facts production does not use.</summary>
internal static class BackgroundLineScrollsAccess
{
    /// <summary>The gameplay lines a room effect's line scrolls cover.</summary>
    extension(BackgroundLineScrolls)
    {
        /// <summary>The number of gameplay lines below the HUD a room effect moves.</summary>
        internal static int GameplayLines => ScreenGeometry.ScreenHeightPixels - ScreenGeometry.GameplayHudHeightPixels;
    }
}
