namespace SuperMetroid.Core.Frontend.Options;

/// <summary>
/// The options screen's fixed layout and timing: page geometry, fades and scrolling and row counts. The cursor's and
/// headings' anchors are installed (#1615).
/// </summary>
public static class GameOptionsDefinitions
{
    /// <summary>The fully lit brightness level the options fade-in reaches, which scales rendered colours.</summary>
    public const int MaximumBrightness = 15;
    /// <summary>How far, in pixels, the controller page scrolls down to show its Exit and Reset rows.</summary>
    public const int ControllerScrollLimit = 32;
    /// <summary>Pixels the controller page scrolls per update.</summary>
    public const int ControllerScrollPixelsPerFrame = 2;
    /// <summary>The queue limit options navigation and confirmation sounds are requested under.</summary>
    public static readonly SoundRequestPriority MaximumQueuedMenuSounds = SoundRequestPriority.Normal;

    /// <summary>Row counts and the controller page's navigation rows.</summary>
    public static class Rows
    {
        /// <summary>Rows of the primary page.</summary>
        public const int PrimaryCount = 5;
        /// <summary>Assignable action rows of the controller page, zero through six.</summary>
        public const int ControllerActionCount = 7;
        /// <summary>The controller page's Reset row, which restores the default buttons.</summary>
        public const int ControllerReset = 8;
        /// <summary>Rows of the controller page.</summary>
        public const int ControllerCount = 9;
        /// <summary>Rows of the special-settings page.</summary>
        public const int SpecialCount = 3;
    }
}
