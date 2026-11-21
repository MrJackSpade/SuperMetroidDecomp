namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>The names the game-over screen's pieces are pictured under.</summary>
public static partial class GameOverPresentationDefinitions
{
    /// <summary>Schema sprite identity of the egg composition, drawn at the Baby anchor.</summary>
    public const string EggFrame = "Egg";
    /// <summary>The menus' picture of the whole game-over page: its title, objective, prompt and both answers (#1467).</summary>
    public const string PageFrame = "GameOver.Page";

    /// <summary>Maps a Baby animation frame to its authored sprite composition identity.</summary>
    internal static readonly IdentityTable<GameOverBabyFrame, string> BabyFrameNames =
        new(new Dictionary<GameOverBabyFrame, string>
        {
            [GameOverBabyFrame.Closed] = "Baby.Closed",
            [GameOverBabyFrame.Middle] = "Baby.Middle",
            [GameOverBabyFrame.Open] = "Baby.Open",
        });
}
