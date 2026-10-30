using System.Collections.Immutable;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's screen (#1358, #1467): its palette, which of the ending's pre-assembled pictures its scenes have loaded,
/// the post-credits text each text page shows, and how far the floor's loads have come since the last full clear.
/// </summary>
internal sealed class EndingScreen
{
    /// <summary>The escape or explosion scene whose floor is loaded, or null before one is.</summary>
    internal EndingFloorSceneId? FloorScene { get; private set; }

    /// <summary>How many of the flyaway floor's loads are made, in order; it shows once all are (#1467).</summary>
    internal int FlyawayChunksLoaded { get; private set; }

    /// <summary>How many of the reward icon floor's loads are made, in order; it shows once all are (#1467).</summary>
    internal int RewardChunksLoaded { get; private set; }

    /// <summary>The ending's colours, including partial fades.</summary>
    public ScreenPalette Palette { get; } = new();

    /// <summary>Whether the explosion's four maps and their characters are loaded, which the explosion displays show.</summary>
    internal bool ExplosionMapsLoaded { get; private set; }

    /// <summary>Whether the post-credits art is loaded: the waiting backdrop and the wavy Samus the post-credits scenes show.</summary>
    internal bool PostCreditsArtLoaded { get; private set; }

    /// <summary>How many of the post-shot's subtitle and logo parts are shown.</summary>
    internal int PostShotPartsShown { get; private set; }

    /// <summary>The text the result and copyright page shows, as of its last showing.</summary>
    internal ImmutableArray<ShownEndingText> ResultPageText { get; private set; } = [];

    /// <summary>The text the final page's upper screen shows, or null before the white flash first shows it.</summary>
    internal ImmutableArray<ShownEndingText>? FinalPageText { get; private set; }

    /// <summary>The text the final page's lower screen shows, a copy of the staging page at the end of the white flash.</summary>
    internal ImmutableArray<ShownEndingText> FinalPageLowerText { get; private set; } = [];

    /// <summary>Clears every load and every page, as a full VRAM clear does.</summary>
    public void Clear()
    {
        FloorScene = null;
        FlyawayChunksLoaded = 0;
        RewardChunksLoaded = 0;
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

    /// <summary>How many of the logo's character parts the post-shot has shown.</summary>
    internal int LogoChunksShown => Math.Clamp(PostShotPartsShown - EndingPostShotParts.FirstLogoPart, 0, EndingPostShotParts.LogoParts);

    /// <summary>Shows <paramref name="text"/> on the text page <paramref name="page"/>.</summary>
    internal void ShowText(EndingTextTilemap page, ImmutableArray<ShownEndingText> text)
    {
        switch (page)
        {
            case EndingTextTilemap.ResultPanel:
                ResultPageText = text;
                break;
            case EndingTextTilemap.FinalText:
                FinalPageText = text;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(page), page, "Undefined ending text page.");
        }
    }

    /// <summary>Shows <paramref name="text"/> on the final page's lower screen.</summary>
    internal void ShowFinalLowerText(ImmutableArray<ShownEndingText> text) => FinalPageLowerText = text;

    /// <summary>
    /// The shot whose turning floor is loaded (#1467): the reward icon once all its loads are made; nothing once the
    /// post-credits art has replaced the floor's characters; the flyaway once all its loads are made; else the scene's.
    /// While a floor is still loading, the floor keeps showing the shot loaded before it.
    /// </summary>
    internal EndingFloorShot FloorShot =>
        RewardChunksLoaded == EndingRewardJumpDefinitions.IconChunkCount ? EndingFloorShot.RewardIcon
        : PostCreditsArtLoaded ? EndingFloorShot.None
        : FlyawayChunksLoaded == EndingFloorDefinitions.FlyawayChunkCount ? EndingFloorShot.Flyaway
        : FloorScene is { } scene ? EndingScreenFloor.SceneShot(scene)
        : EndingFloorShot.None;

    /// <summary>Loads the escape or explosion scene <paramref name="scene"/>'s floor.</summary>
    public void LoadFloor(EndingFloorSceneId scene) => FloorScene = scene;

    /// <summary>Makes the flyaway floor's load <paramref name="index"/>, the next in order.</summary>
    /// <exception cref="InvalidOperationException">The load is not the next.</exception>
    internal void LoadFlyawayChunk(int index) => FlyawayChunksLoaded = Next(index, FlyawayChunksLoaded, EndingFloorDefinitions.FlyawayChunkCount, "flyaway");

    /// <summary>Makes the reward icon floor's load <paramref name="index"/>, the next in order.</summary>
    /// <exception cref="InvalidOperationException">The load is not the next.</exception>
    internal void LoadRewardChunk(int index) => RewardChunksLoaded = Next(index, RewardChunksLoaded, EndingRewardJumpDefinitions.IconChunkCount, "reward icon");

    /// <summary>The loads made once load <paramref name="index"/> follows <paramref name="loaded"/> of <paramref name="count"/>.</summary>
    private static int Next(int index, int loaded, int count, string floor) => index == loaded && index < count
        ? loaded + 1
        : throw new InvalidOperationException($"The {floor} floor's load {index} does not follow its {loaded} loads.");
}