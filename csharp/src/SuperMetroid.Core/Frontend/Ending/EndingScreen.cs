using SuperMetroid.Core.Rooms.Backgrounds;
using System.Collections.Immutable;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's screen (#1358, #1467): its colours, which of the ending's pre-assembled pictures its scenes have loaded,
/// the post-credits text each text page shows, and how far the floor's loads have come since the last full clear.
/// </summary>
internal sealed class EndingScreen
{
    /// <summary>The escape or explosion scene whose floor is loaded, or null before one is.</summary>
    internal EndingFloorSceneId? FloorScene { get; private set; }

    /// <summary>Whether the flyaway's floor is loaded, which it is once the explosion's hold ends (#1467, #1513).</summary>
    internal bool FlyawayFloorLoaded { get; private set; }

    /// <summary>Whether the reward icon's floor is loaded, which it is once Samus has fallen for its reveal (#1467, #1513).</summary>
    internal bool RewardIconLoaded { get; private set; }

    /// <summary>What the ending's colours show: each load, fade, flash and palette-effect record its scenes have shown (#1512).</summary>
    internal EndingColours Colours { get; } = new();

    /// <summary>Whether the explosion's four maps and their characters are loaded, which the explosion displays show.</summary>
    internal bool ExplosionMapsLoaded { get; private set; }

    /// <summary>Whether the post-credits art is loaded: the waiting backdrop and the wavy Samus the post-credits scenes show.</summary>
    internal bool PostCreditsArtLoaded { get; private set; }

    /// <summary>How many of the post-shot's subtitle and logo parts are shown.</summary>
    internal int PostShotPartsShown { get; private set; }

    /// <summary>The text the result and copyright page shows, as of its last showing.</summary>
    internal ImmutableArray<WrittenPicture> ResultPageText { get; private set; } = [];

    /// <summary>The text the final page's upper screen shows, or null before the white flash first shows it.</summary>
    internal ImmutableArray<WrittenPicture>? FinalPageText { get; private set; }

    /// <summary>The text the final page's lower screen shows, a copy of the staging page at the end of the white flash.</summary>
    internal ImmutableArray<WrittenPicture> FinalPageLowerText { get; private set; } = [];

    /// <summary>Clears every load and every page, as a full display clear does.</summary>
    public void Clear()
    {
        FloorScene = null;
        FlyawayFloorLoaded = false;
        RewardIconLoaded = false;
        ExplosionMapsLoaded = false;
        PostCreditsArtLoaded = false;
        PostShotPartsShown = 0;
        ResultPageText = [];
        FinalPageText = null;
        FinalPageLowerText = [];
    }

    /// <summary>Loads the explosion's maps and characters.</summary>
    internal void LoadExplosionMaps() => ExplosionMapsLoaded = true;

    /// <summary>Loads the post-credits art, whose reward and shooting characters replace the floor's.</summary>
    internal void LoadPostCreditsArt() => PostCreditsArtLoaded = true;

    /// <summary>Shows the post-shot's next subtitle or logo part.</summary>
    /// <exception cref="InvalidOperationException">Every part is already shown.</exception>
    internal void ShowPostShotPart()
    {
        if (PostShotPartsShown >= EndingPostShotParts.Count)
            throw new InvalidOperationException("The post-shot has shown every part.");
        PostShotPartsShown++;
    }

    /// <summary>Whether the post-shot has shown the logo's map, so the logo plane shows the logo.</summary>
    internal bool LogoShown => PostShotPartsShown == EndingPostShotParts.Count;

    /// <summary>Shows <paramref name="text"/> on the text page <paramref name="page"/>.</summary>
    internal void ShowText(EndingTextScreen page, ImmutableArray<WrittenPicture> text)
    {
        switch (page)
        {
            case EndingTextScreen.ResultPanel:
                ResultPageText = text;
                break;
            case EndingTextScreen.FinalText:
                FinalPageText = text;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(page), page, "Undefined ending text page.");
        }
    }

    /// <summary>Shows <paramref name="text"/> on the final page's lower screen.</summary>
    internal void ShowFinalLowerText(ImmutableArray<WrittenPicture> text) => FinalPageLowerText = text;

    /// <summary>
    /// The shot whose turning floor is loaded (#1467): the reward icon once it is loaded; nothing once the post-credits
    /// art has replaced the floor; the flyaway once it is loaded; else the scene's.
    /// </summary>
    internal EndingFloorShot FloorShot =>
        RewardIconLoaded ? EndingFloorShot.RewardIcon
        : PostCreditsArtLoaded ? EndingFloorShot.None
        : FlyawayFloorLoaded ? EndingFloorShot.Flyaway
        : FloorScene is { } scene ? EndingScreenFloor.SceneShot(scene)
        : EndingFloorShot.None;

    /// <summary>Loads the escape or explosion scene <paramref name="scene"/>'s floor.</summary>
    public void LoadFloor(EndingFloorSceneId scene) => FloorScene = scene;

    /// <summary>Loads the flyaway's floor.</summary>
    /// <exception cref="InvalidOperationException">The flyaway's floor is already loaded.</exception>
    internal void LoadFlyawayFloor() => FlyawayFloorLoaded = !FlyawayFloorLoaded
        ? true : throw new InvalidOperationException("The flyaway's floor is loaded once per clear.");

    /// <summary>Loads the reward icon's floor.</summary>
    /// <exception cref="InvalidOperationException">The reward icon's floor is already loaded.</exception>
    internal void LoadRewardIcon() => RewardIconLoaded = !RewardIconLoaded
        ? true : throw new InvalidOperationException("The reward icon's floor is loaded once per clear.");
}