namespace SuperMetroid.Core.Frontend.Options;

/// <summary>
/// The options screen's fixed layout and timing: page geometry, fades and scrolling, row counts, cursor anchors and
/// heading anchors. <see cref="GameOptionsLayout"/> derives each row's
/// position from them.
/// </summary>
public static class GameOptionsDefinitions
{
    /// <summary>The fully lit brightness level the options fade-in reaches, which scales rendered colours.</summary>
    public const int MaximumBrightness = 15;
    /// <summary>How far, in pixels, the controller page scrolls down to show its Exit and Reset rows.</summary>
    public const int ControllerScrollLimit = 32;
    /// <summary>Pixels the controller page scrolls per update.</summary>
    public const int ControllerScrollPixelsPerFrame = 2;
    /// <summary>Cells in each options-page row.</summary>
    public const int MenuTilemapWidth = 32;
    /// <summary>Rows in each options page, including rows outside the visible screen.</summary>
    public const int MenuTilemapHeight = 32;
    /// <summary>The queue limit options navigation and confirmation sounds are requested under.</summary>
    public static readonly SoundQueueLimit MaximumQueuedMenuSounds = SoundQueueLimit.Max6;
    /// <summary>Two tilemap rows for each of the two language labels.</summary>
    public const int LanguagePaletteRegionCount = 4;

    /// <summary>Row counts and the controller page's navigation rows.</summary>
    public static class Rows
    {
        /// <summary>Rows of the primary page.</summary>
        public const int PrimaryCount = 5;
        /// <summary>Assignable action rows of the controller page, zero through six.</summary>
        public const int ControllerActionCount = 7;
        /// <summary>The controller page's Exit row; reaching it scrolls the page down.</summary>
        public const int ControllerExit = 7;
        /// <summary>The controller page's Reset row, which restores the default buttons.</summary>
        public const int ControllerReset = 8;
        /// <summary>Rows of the controller page.</summary>
        public const int ControllerCount = 9;
        /// <summary>Rows of the special-settings page.</summary>
        public const int SpecialCount = 3;
    }

    /// <summary>The cursor missile's anchors, in screen pixels.</summary>
    public static class Cursors
    {
        /// <summary>The primary page's cursor column.</summary>
        public const ushort PrimaryX = 24;
        /// <summary>The controller page's cursor column.</summary>
        public const ushort ControllerX = 40;
        /// <summary>The cursor's column while a page phase shows no cursor (including scrolling): off screen.</summary>
        public const ushort HiddenX = 384;
        /// <summary>The hidden cursor's row.</summary>
        public const ushort HiddenY = 16;
        /// <summary>The special page's cursor column.</summary>
        public const ushort SpecialX = 16;
        /// <summary>The primary page's first row.</summary>
        public const int PrimaryFirstY = 56;
        /// <summary>Pixels between primary rows.</summary>
        public const int PrimaryRowSpacing = 32;
        /// <summary>How far the primary page's rows from the third on sit above the even spacing: the authored gap.</summary>
        public const int PrimaryLowerRowsRaise = 8;
        /// <summary>The first primary row the raise applies to.</summary>
        public const int PrimaryFirstRaisedRow = 2;
        /// <summary>The controller page's first row.</summary>
        public const int ControllerFirstY = 48;
        /// <summary>Pixels between controller rows; Exit and Reset sit higher by the page's scroll.</summary>
        public const int ControllerRowSpacing = 24;
        /// <summary>The special page's first row.</summary>
        public const int SpecialFirstY = 64;
        /// <summary>Pixels between special rows.</summary>
        public const int SpecialRowSpacing = 48;
    }

    /// <summary>The page headings' anchors, in screen pixels.</summary>
    public static class Headings
    {
        /// <summary>The row every heading's top edge is at.</summary>
        public const ushort HeadingY = 16;
        /// <summary>The primary page heading's column.</summary>
        public const ushort PrimaryX = 124;
        /// <summary>The controller page heading's column.</summary>
        public const ushort ControllerX = 132;
        /// <summary>The special page heading's column.</summary>
        public const ushort SpecialX = 128;
    }
}
