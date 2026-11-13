using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending's installed artwork: its text, its sprite sheets and its backdrops.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own palette FX for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingArtwork
{
    /// <summary>What every ending scene draws on: brightness, the phase timer, the turning-floor view, palettes, the shared sprites and stars, and the artwork they share.</summary>
    private readonly EndingStage _stage;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal EndingArtwork(EndingStage stage)
    {
        _stage = stage;
    }

    /// <summary>Scrolling staff-list owner active during the credits phase.</summary>
    internal CreditsObjectState? CurrentCredits { get; set; }

    /// <summary>Whether the staff handoff has shown the credits' colours, which it does once.</summary>
    private bool creditsColoursShown;

    /// <summary>Binds the installed sprite sheets when the game is bound to its installed content.</summary>
    internal void BindSpriteSheets(InstalledSpriteSheets? value) => Sheets = value;

    /// <summary>The installed sheet <paramref name="sheet"/>.</summary>
    internal SpriteSheet SpriteSheet(SpriteSheetId sheet) => Sheets.Get(sheet);

    /// <summary>The installed sprite sheets the ending's actors and backgrounds draw from (#1392).</summary>
    [AllowNull]
    internal InstalledSpriteSheets Sheets
    {
        get => field ?? throw new InvalidOperationException("Ending sprites require installed sprite sheets.");
        private set;
    }

    /// <summary>Binds the installed ending text when the game is bound to its installed content.</summary>
    public void BindEndingText(EndingTypewriterText? value)
    {
        EndingText = value;
        PostCreditsText.BindText(value);
    }


    /// <summary>
    /// Shows the credits' colours once, at the staff handoff; the post-credits scenes draw from the installed reward
    /// sheets, which hold the suitless and suited reward art both.
    /// </summary>
    internal void LoadCreditsAndPostCreditsAssets()
    {
        if (creditsColoursShown)
            return;

        _stage.LoadColours(EndingColourLoad.Credits);

        creditsColoursShown = true;
    }

    /// <summary>
    /// Binds the installed scene backdrops when the game is bound to its installed content (#1544). The running scene keeps
    /// the backdrop it started with; the next scene starts with its backdrop from the newly bound ones. The backdrops of the
    /// space behind the exploding planet (#1583, #1637) take effect from the next record shown, without resetting a live fade.
    /// </summary>
    internal void BindBackdrops(EndingBackdrops? value)
    {
        _stage.BindBackdrops(value);
        SpaceBackdrops = value?.Space;
    }

    /// <summary>Final logo actor created after the white flash.</summary>
    internal EndingLogo? CurrentEndingLogo { get; set; }

    /// <summary>The installed backdrops of the space behind the exploding planet, one for each of its palette-effect records.</summary>
    internal EndingSpaceBackdrops? SpaceBackdrops { get; private set; }

    /// <summary>The ending text's installed content.</summary>
    internal EndingTypewriterText? EndingText { get; private set; }

    /// <summary>The post-credits text scene (#1555).</summary>
    internal EndingPostCreditsText PostCreditsText { get; } = new();
}
