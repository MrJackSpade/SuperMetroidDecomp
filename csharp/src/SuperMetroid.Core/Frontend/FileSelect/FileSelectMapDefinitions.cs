namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// The saved-game area and room maps' definitions (#1416): how many areas the world map selects, the entry and return
/// windows' geometry and timing, the room view's frame offset and the markers' draw palettes.
/// </summary>
public static class FileSelectMapDefinitions
{
    /// <summary>The six Zebes areas the world map selects; Ceres has no world-map entry.</summary>
    public const int AreaCount = 6;

    /// <summary>The room view's frame drawn this many pixels lower, aligning it with the visible map viewport.</summary>
    public const ushort RoomFrameVerticalScroll = 24;

    /// <summary>Updates the return window's area timer is shortened by.</summary>
    public const int ReturnWindowTimerReduction = 12;

    /// <summary>Pixels inside the viewport the return window starts.</summary>
    public const int ReturnWindowInset = 8;

    /// <summary>Updates the load prelude takes before the screen fades.</summary>
    public const int LoadPreludeFrames = 2;

    /// <summary>Black updates the load waits after the fade before gameplay setup begins.</summary>
    public const int LoadBlackFrames = 32;

    /// <summary>Updates the return setup takes before the window contracts.</summary>
    public const int ReturnSetupFrames = 6;

    /// <summary>Steps the entry fade divides its colour change into.</summary>
    public const int EntryPaletteDenominator = 15;

    /// <summary>The entry window's upper margin when it opens, in scanlines.</summary>
    public const int EntryWindowTop = 111;

    /// <summary>The entry window's inclusive left edge when it opens.</summary>
    public const int EntryWindowLeft = 127;

    /// <summary>The entry window's inclusive right edge when it opens.</summary>
    public const int EntryWindowRight = 129;

    /// <summary>Pixels each entry-window edge expands per update.</summary>
    public const int EntryWindowSpeed = 4;

    /// <summary>The draw palette of the station, boss and gunship markers and the load-station marker: sprite palette seven.</summary>
    public const int StationMarkerPalette = 7;

    /// <summary>The draw palette a defeated boss's marker takes: sprite palette six.</summary>
    public const int DefeatedBossPalette = 6;
}
