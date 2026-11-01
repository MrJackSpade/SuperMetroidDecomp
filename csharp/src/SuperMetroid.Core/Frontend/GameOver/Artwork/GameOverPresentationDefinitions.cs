namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>The game-over screen's cursor frames and the names its pieces are pictured under.</summary>
public static class GameOverPresentationDefinitions
{
    /// <summary>Four frames in the looping missile-cursor animation.</summary>
    public const int CursorFrameCount = 4;
    /// <summary>Schema sprite identity of the egg composition, drawn at the Baby anchor.</summary>
    public const string EggFrame = "Egg";
    /// <summary>The menus' picture of the whole game-over page: its title, objective, prompt and both answers (#1467).</summary>
    public const string PageFrame = "GameOver.Page";

    /// <summary>Maps a compiled Baby animation frame to its authored sprite composition identity.</summary>
    internal static readonly IReadOnlyDictionary<GameOverBabyFrame, string> BabyFrameNames =
        new Dictionary<GameOverBabyFrame, string>
        {
            [GameOverBabyFrame.Closed] = "Baby.Closed",
            [GameOverBabyFrame.Middle] = "Baby.Middle",
            [GameOverBabyFrame.Open] = "Baby.Open",
        };

    /// <summary>Returns the authored composition identity for a missile-cursor frame.</summary>
    internal static readonly IReadOnlyDictionary<int, string> CursorFrameNames =
        new Dictionary<int, string>
        {
            [0] = "Cursor.0",
            [1] = "Cursor.1",
            [2] = "Cursor.2",
            [3] = "Cursor.3",
        };

    /// <summary>Maps a compiled Baby palette phase to its authored color-array identity.</summary>
    internal static readonly IReadOnlyDictionary<GameOverBabyPalette, string> BabyPaletteNamesByPhase =
        new Dictionary<GameOverBabyPalette, string>
        {
            [GameOverBabyPalette.Idle] = "Baby.Idle",
            [GameOverBabyPalette.ClosedCry] = "Baby.ClosedCry",
            [GameOverBabyPalette.MiddleCry] = "Baby.MiddleCry",
            [GameOverBabyPalette.OpenCry] = "Baby.OpenCry",
        };
}
