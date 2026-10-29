using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Hud;

/// <summary>
/// The gameplay message box's definitions (#1416): its tilemap layout and window placement, its waits and display
/// durations, the two colours it borrows while open, and the button glyphs its panels patch in.
/// </summary>
public static class GameplayMessageBoxDefinitions
{
    /// <summary>Message tilemap dimensions, selection rows, and window placement.</summary>
    public static class Layout
    {
        /// <summary>Width of the message tilemap in tile cells.</summary>
        public const int TilemapWidth = 32;
        /// <summary>Number of rows occupied by the upper and lower borders together.</summary>
        public const int BorderRows = 2;
        /// <summary>Minimum supported message-box height in tile rows.</summary>
        public const int MinimumRows = 3;
        /// <summary>Maximum supported message-box height in tile rows.</summary>
        public const int MaximumRows = 6;
        /// <summary>Width and height of one tile in pixels.</summary>
        public const int TilePixels = 8;
        /// <summary>The message row the save confirmation's YES/NO selection occupies, counted from the upper border.</summary>
        public const int SaveSelectionRow = 4;
        /// <summary>Cells in one save-selection row.</summary>
        public const int SaveSelectionRowWords = 32;
        /// <summary>Vertical screen coordinate around which the message window expands.</summary>
        public const int WindowCenterY = 124;
    }

    /// <summary>Authored waits, window-animation steps, and minimum display durations.</summary>
    public static class Timing
    {
        /// <summary>The gunship's saving sound waits 160 updates.</summary>
        public const int GunshipSavingSoundFrames = 160;
        /// <summary>
        /// Updates before the box opens: preparing the screen 2, clearing the box 1, writing the message 1, setting up
        /// the active box and its window 2, and two updates of music and sound.
        /// </summary>
        public const int PreOpenLagFrames = 8;
        /// <summary>Updates before the gunship's second box opens, without screen setup or clearing.</summary>
        public const int ReopenLagFrames = 5;
        /// <summary>Clearing the box waits one update.</summary>
        public const int ClearTilemapLagFrames = 1;
        /// <summary>Updates restoring the screen after the box closes: clearing 1, restoring 2, music and sound 2.</summary>
        public const int RestoreLagFrames = 5;
        /// <summary>The save selector waits two updates per controller read.</summary>
        public const int SaveSelectionReadLagFrames = 2;
        /// <summary>Maximum half-height reached by the expanding message window.</summary>
        public const int MaximumRadiusPixels = 24;
        /// <summary>Per-step change in message-window radius.</summary>
        public const int RadiusStepPixels = 2;
        /// <summary>Minimum updates an item-acquisition message remains displayed.</summary>
        public const int ItemMinimumDisplayFrames = 360;
        /// <summary>Minimum updates a station message remains displayed.</summary>
        public const int StationMinimumDisplayFrames = 10;
    }

    /// <summary>The two colours the message box replaces while it is open.</summary>
    public static class Palette
    {
        /// <summary>The background set whose colours the message box's text shows.</summary>
        public const int TextSet = 1;
        /// <summary>The colour of the text set the light message colour replaces.</summary>
        public const int TemporaryLightColour = 9;
        /// <summary>The light colour installed for the message box.</summary>
        public static RgbColor TemporaryLightColor => RgbColor.FromWord(0x0bb1);
        /// <summary>The colour of the text set the dark message colour replaces.</summary>
        public const int TemporaryDarkColour = 10;
        /// <summary>The dark colour installed for the message box.</summary>
        public static RgbColor TemporaryDarkColor => new(31, 0, 0);
    }

    /// <summary>The button glyphs a panel patches in for the configured shoot or run binding, and where they go.</summary>
    public static class Buttons
    {
        /// <summary>The A button's glyph.</summary>
        internal static BackgroundTile A { get; } = new(0x0e0, 2, highPriority: true);
        /// <summary>The B button's glyph.</summary>
        internal static BackgroundTile B { get; } = new(0x0e1, 7, highPriority: true);
        /// <summary>The X button's glyph.</summary>
        internal static BackgroundTile X { get; } = new(0x0f7, 3, highPriority: true);
        /// <summary>The Y button's glyph.</summary>
        internal static BackgroundTile Y { get; } = new(0x0f8, 6, highPriority: true);
        /// <summary>The Select button's glyph.</summary>
        internal static BackgroundTile Select { get; } = new(0x0d0, 6, highPriority: true);
        /// <summary>The L button's glyph.</summary>
        internal static BackgroundTile L { get; } = new(0x0eb, 6, highPriority: true);
        /// <summary>The R button's glyph.</summary>
        internal static BackgroundTile R { get; } = new(0x0f1, 6, highPriority: true);
        /// <summary>The blank glyph drawn when no supported button is bound.</summary>
        public static BackgroundTile UnknownGlyph { get; } = new(0x04e, 2, highPriority: true);

        /// <summary>The byte offset of the glyph in the missile, super missile and bomb panels.</summary>
        internal const ushort AmmunitionPanelGlyphOffset = 0x12a;
        /// <summary>The byte offset of the glyph in the power bomb, grapple and X-ray panels.</summary>
        internal const ushort SelectedItemPanelGlyphOffset = 0x12c;
        /// <summary>The byte offset of the glyph in the speed booster panel.</summary>
        internal const ushort SpeedBoosterPanelGlyphOffset = 0x120;
    }
}
