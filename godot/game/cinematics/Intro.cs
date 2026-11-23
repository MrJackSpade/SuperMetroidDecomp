using Engine.Content;
using Engine.Core;
using Engine.Shared;
using Engine.Ui;
using Game.Data;
using Godot;

namespace Game.Cinematics;

/// <summary>
/// The opening. A card comes up, "the last Metroid is in captivity, the galaxy is at peace". Then Samus tells what
/// has happened, a page at a time, typed out beside her watching eye; between pages what she tells is shown: her
/// fight with Mother Brain, the hatching of the larva, its delivery to the scientists and their study of it. After
/// the last page her gunship is seen flying to the space colony, and play begins there.
/// </summary>
/// <remarks>
/// It is written across several files: this one is its order of events and its pages; the others are the scenes
/// between pages and the flight to the colony. Its data names its pictures ("first_card", "final_line"), the
/// timelines of what it shows, where those start ("places"), its "pages" (each the name of its words, and of the
/// second wordings shown with them in the "subtitled_languages"), and what each scene needs.
/// </remarks>
public partial class Intro : Cinematic
{
    /// <summary>The data file of what it shows.</summary>
    public string DataFile { get; set; } = "cinematics/intro.json";

    private enum Phase
    {
        Initial,
        FadingInCard,
        CaptivityLine,
        PeaceLine,
        WaitingForPeaceMusic,
        HoldingCard,
        FadingOutCard,
        SettingUpFirstPage,
        WaitingForPageMusic,
        FadingInFirstPage,
        PageText,
        PageAwaitingInput,
        CrossfadingToScene,
        Scene,
        CrossfadingToPage,
        FadingOut,
        Flight,
        Finished,
    }

    /// <summary>The scenes shown between pages, in the order of the pages that lead to them; the last two pages lead to none.</summary>
    private static readonly string[] SceneAfterPage = ["mother_brain", "discovery", "delivery", "examination", "", ""];

    /// <summary>How many updates a fade of the whole screen takes, and how many steps it goes in.</summary>
    private const int FadeUpdates = 30, FadeLevels = 15;

    /// <summary>How long music takes to come in once asked for.</summary>
    private const int MusicUpdates = 14;

    /// <summary>How long the card is held before each of its two pieces of music, and after the second.</summary>
    private const int FirstLineUpdates = 60, CaptivityUpdates = 200, PeaceUpdates = 240;

    /// <summary>How long a crossfade lasts, how many steps it goes in, and how many updates each step lasts.</summary>
    private const int CrossfadeUpdates = 128, CrossfadeLevels = 32, CrossfadeStepUpdates = 4;

    /// <summary>How many updates pass between one letter and the next, and how long the last page is held.</summary>
    private const int LetterUpdates = 5, FinalPageUpdates = 128;

    /// <summary>The size of a letter's cell, and where the first row of a page's words is.</summary>
    private const int Cell = 8, FirstRowY = 24;

    private DataNode? _data;
    private TimelineSet _timelines = TimelineSet.None;
    private Phase _phase;
    private int _wait;
    private int _page;
    private string _scene = "";
    private int _crossfade;

    private ScreenFade? _fade;
    private Node2D? _sceneRoot;
    private Node2D? _narration;
    private Sprite2D? _card;
    private Label? _words;
    private Label? _subtitle;
    private CinematicSprite? _eye;
    private CinematicSprite? _caret;
    private CinematicSprite? _arrow;

    private string _text = "";
    private int _typed;
    private int _letterWait;
    private bool _click;
    private bool _pageFinished;
    private int _finalHold;
    private bool _awaitingSecondSubtitle;
    private int _subtitleHold;

    /// <inheritdoc/>
    protected override bool CanBeSkipped => false;

    /// <inheritdoc/>
    protected override void Enter()
    {
        _data = Session.Content.ReadData(DataFile);
        _timelines = TimelineSet.ReadFrom(_data);

        var scenes = new CanvasLayer { Name = "Scenes", Layer = 1 };
        AddChild(scenes);
        var black = new ColorRect { Name = "Black", Color = Colors.Black };
        black.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        scenes.AddChild(black);
        _sceneRoot = new Node2D { Name = "Scene", Modulate = new Color(1, 1, 1, 0) };
        scenes.AddChild(_sceneRoot);

        var pages = new CanvasLayer { Name = "Pages", Layer = 2 };
        AddChild(pages);
        _card = new Sprite2D { Name = "Card", Centered = false, Texture = Session.Content.LoadTexture(_data.Text("first_card")) };
        pages.AddChild(_card);
        _narration = new Node2D { Name = "Narration", Visible = false };
        pages.AddChild(_narration);
        _narration.AddChild(new Sprite2D
        {
            Name = "FinalLine", Centered = false, Position = new Vector2(0, 23 * Cell),
            Texture = Session.Content.LoadTexture(_data.Text("final_line")),
        });
        _eye = Sprite("eye", "eye");
        _narration.AddChild(_eye);
        _words = new Label { Name = "Words", Position = new Vector2(0, FirstRowY), VisibleCharacters = 0 };
        _narration.AddChild(_words);
        _subtitle = new Label { Name = "Subtitle", Visible = false };
        DataNode subtitleAt = _data.Get("places").Get("subtitle");
        _subtitle.Position = new Vector2(subtitleAt.Int("x"), subtitleAt.Int("y"));
        _narration.AddChild(_subtitle);
        _caret = CinematicSprite.Of(Session.Content, _timelines.Get("caret"), PixelPosition.FromWhole(Cell, FirstRowY));
        _caret.Name = "Caret";
        _narration.AddChild(_caret);

        _fade = new ScreenFade { Name = "Fade", Layer = 5, Levels = FadeLevels };
        AddChild(_fade);
        _fade.FadeOut(1);
        _fade.Step();
        _phase = Phase.Initial;
    }

    /// <inheritdoc/>
    protected override bool Play(int elapsed)
    {
        ScreenFade fade = _fade ?? throw NotEntered();
        switch (_phase)
        {
            case Phase.Initial:
                Session.Sounds.RequestMusic("");
                fade.FadeIn(FadeUpdates);
                _phase = Phase.FadingInCard;
                break;

            case Phase.FadingInCard:
                fade.Step();
                if (fade.IsDone)
                    (_phase, _wait) = (Phase.CaptivityLine, FirstLineUpdates);
                break;

            case Phase.CaptivityLine:
                if (--_wait > 0)
                    break;
                Session.Sounds.RequestMusic(Music.LastMetroidInCaptivity);
                (_phase, _wait) = (Phase.PeaceLine, CaptivityUpdates);
                break;

            case Phase.PeaceLine:
                if (--_wait > 0)
                    break;
                Session.Sounds.RequestMusic("");
                (_phase, _wait) = (Phase.WaitingForPeaceMusic, MusicUpdates);
                break;

            case Phase.WaitingForPeaceMusic:
                if (--_wait > 0)
                    break;
                Session.Sounds.RequestMusic(Music.GalaxyAtPeace);
                (_phase, _wait) = (Phase.HoldingCard, PeaceUpdates);
                break;

            case Phase.HoldingCard:
                if (--_wait > 0)
                    break;
                fade.FadeOut(FadeUpdates);
                _phase = Phase.FadingOutCard;
                break;

            case Phase.FadingOutCard:
                fade.Step();
                if (fade.IsDone)
                    _phase = Phase.SettingUpFirstPage;
                break;

            case Phase.SettingUpFirstPage:
                (_card ?? throw NotEntered()).Visible = false;
                (_narration ?? throw NotEntered()).Visible = true;
                Session.Sounds.RequestMusic("");
                (_phase, _wait) = (Phase.WaitingForPageMusic, MusicUpdates);
                break;

            case Phase.WaitingForPageMusic:
                if (--_wait > 0)
                    break;
                Session.Sounds.RequestMusic(Music.Introduction);
                StartPage(0, restoresCaret: false);
                fade.FadeIn(FadeUpdates);
                _phase = Phase.FadingInFirstPage;
                break;

            case Phase.FadingInFirstPage:
                fade.Step();
                if (fade.IsDone)
                    _phase = Phase.PageText;
                break;

            case Phase.PageText:
                // A page that has been typed out waits to be turned; the last is held and then fades.
                if (!_pageFinished || _awaitingSecondSubtitle)
                    break;
                if (_page == LastPage)
                {
                    fade.FadeOut(FadeUpdates);
                    _phase = Phase.FadingOut;
                }
                else
                {
                    _phase = Phase.PageAwaitingInput;
                }

                break;

            case Phase.PageAwaitingInput:
                if (_subtitleHold > 0)
                    _subtitleHold--;
                else if (Input.AnyPressed)
                    TurnPage();
                break;

            case Phase.CrossfadingToScene:
                if (!StepCrossfade(toScene: true))
                    break;
                ClearWords();
                _phase = Phase.Scene;
                break;

            case Phase.Scene:
                if (!SceneWantsNextPage())
                    break;
                StartPage(_page + 1, restoresCaret: true);
                (_phase, _crossfade) = (Phase.CrossfadingToPage, 0);
                break;

            case Phase.CrossfadingToPage:
                if (!StepCrossfade(toScene: false))
                    break;
                EndScene();
                _phase = Phase.PageText;
                break;

            case Phase.FadingOut:
                fade.Step();
                if (!fade.IsDone)
                    break;
                (_narration ?? throw NotEntered()).Visible = false;
                BeginFlight();
                _phase = Phase.Flight;
                break;

            case Phase.Flight:
                if (StepFlight())
                    _phase = Phase.Finished;
                break;

            case Phase.Finished:
                break;

            default:
                throw new InvalidOperationException($"The opening has no phase {_phase}.");
        }

        if (_phase is >= Phase.FadingInFirstPage and <= Phase.FadingOut)
            StepNarration();
        if (_scene.Length > 0)
            StepScene(_phase == Phase.CrossfadingToScene);
        return _phase == Phase.Finished;
    }

    /// <inheritdoc/>
    protected override GameMode Next()
    {
        // Whatever the opening did with Samus is forgotten: the game begins with her as she starts.
        Session.BeginAgain();
        return new PlayingMode();
    }

    /// <summary>Starts a page: its words begin to be typed from the top.</summary>
    private void StartPage(int page, bool restoresCaret)
    {
        DataNode entry = Data().List("pages")[page];
        _page = page;
        _text = Session.Text.Get(entry.Text("text"));
        (_typed, _letterWait, _pageFinished, _finalHold, _awaitingSecondSubtitle, _subtitleHold) = (0, 2, false, 0, false, 0);
        Label words = _words ?? throw NotEntered();
        words.Text = _text;
        words.VisibleCharacters = 0;
        _arrow?.QueueFree();
        _arrow = null;
        Label subtitle = _subtitle ?? throw NotEntered();
        subtitle.Visible = IsSubtitled && entry.Has("subtitle");
        if (subtitle.Visible)
            subtitle.Text = Session.Text.Get(entry.Text("subtitle"));
        CinematicSprite caret = _caret ?? throw NotEntered();
        if (restoresCaret || page == LastPage)
        {
            caret.Visible = true;
            caret.Place = PixelPosition.FromWhole(Cell, FirstRowY);
            caret.Play(_timelines.Get("caret"));
        }

        // The last page takes the place of everything before it, and her eye closes on it.
        if (page == LastPage)
            (_eye ?? throw NotEntered()).Play(_timelines.Get("eye_closing"));
    }

    /// <summary>Plays one update of the page: her eye, the caret, and the next letter when it is due.</summary>
    private void StepNarration()
    {
        (_eye ?? throw NotEntered()).Step();
        CinematicSprite caret = _caret ?? throw NotEntered();
        caret.Step();
        _arrow?.Step();
        if (_awaitingSecondSubtitle && Input.AnyPressed)
        {
            // The second wording comes up, and is held a moment before the page may be turned.
            _awaitingSecondSubtitle = false;
            (_subtitle ?? throw NotEntered()).Text = Session.Text.Get(Data().List("pages")[_page].Text("second_subtitle"));
            _subtitleHold = 60;
            return;
        }

        if (_pageFinished)
            return;
        if (_typed >= _text.Length)
        {
            FinishTyping(caret);
            return;
        }

        if (--_letterWait > 0)
            return;
        _letterWait = LetterUpdates;
        char letter = _text[_typed];
        _typed++;

        // Going on to the next line takes no time of its own.
        while (_typed < _text.Length && char.IsControl(_text[_typed]))
            _typed++;
        (_words ?? throw NotEntered()).VisibleCharacters = _typed;
        caret.Place = _typed < _text.Length ? CellOf(_typed) : PixelPosition.FromWhole(Cell, CellOf(_typed - 1).Y.Whole + 2 * Cell);
        caret.ShowPlace();
        _click = !_click;
        if (!char.IsWhiteSpace(letter) && _click)
            Session.Sounds.Request(Sounds.Typing, channel: "text");
    }

    /// <summary>What follows the last letter of a page: the last page is held a while; any other shows it may be turned.</summary>
    private void FinishTyping(CinematicSprite caret)
    {
        if (_page == LastPage)
        {
            if (_finalHold == 0)
                caret.Play(_timelines.Get("caret_blinking"));
            if (++_finalHold <= FinalPageUpdates)
                return;
            _pageFinished = true;
            return;
        }

        _pageFinished = true;
        caret.Play(_timelines.Get("caret_blinking"));
        if (IsSubtitled)
        {
            DataNode at = Data().Get("places").Get("next_page_arrow");
            _arrow = CinematicSprite.Of(Session.Content, _timelines.Get("next_page_arrow"), PixelPosition.FromWhole(at.Int("x"), at.Int("y")));
            (_narration ?? throw NotEntered()).AddChild(_arrow);
            _awaitingSecondSubtitle = Data().List("pages")[_page].Has("second_subtitle");
        }
    }

    /// <summary>Turns from a page to what it leads to: a scene, or the last page.</summary>
    private void TurnPage()
    {
        _arrow?.QueueFree();
        _arrow = null;
        string scene = SceneAfterPage[_page];
        if (scene.Length == 0)
        {
            ClearWords();
            StartPage(LastPage, restoresCaret: true);
            _phase = Phase.PageText;
            return;
        }

        (_caret ?? throw NotEntered()).Visible = false;
        BeginScene(scene);
        (_phase, _crossfade) = (Phase.CrossfadingToScene, 0);
    }

    /// <summary>
    /// Plays one update of a crossfade between the page and the scene, and returns whether it has ended. One gives
    /// way to the other a step at a time.
    /// </summary>
    private bool StepCrossfade(bool toScene)
    {
        _crossfade++;
        float scene = (float)(_crossfade / CrossfadeStepUpdates) / CrossfadeLevels;
        if (!toScene)
            scene = 1 - scene;
        (_sceneRoot ?? throw NotEntered()).Modulate = new Color(1, 1, 1, scene);
        (_narration ?? throw NotEntered()).Modulate = new Color(1, 1, 1, 1 - scene);
        return _crossfade >= CrossfadeUpdates;
    }

    private void ClearWords()
    {
        Label words = _words ?? throw NotEntered();
        (_text, _typed, words.Text, words.VisibleCharacters) = ("", 0, "", 0);
        (_subtitle ?? throw NotEntered()).Visible = false;
    }

    /// <summary>Where the cell of the letter at an index of the page's words is on the screen.</summary>
    private PixelPosition CellOf(int index)
    {
        int column = 0, row = 0;
        for (int at = 0; at < index; at++)
        {
            if (_text[at] == (char)10)
                (column, row) = (0, row + 1);
            else if (!char.IsControl(_text[at]))
                column++;
        }

        return PixelPosition.FromWhole(column * Cell, FirstRowY + row * Cell);
    }

    private int LastPage => Data().List("pages").Count - 1;

    private bool IsSubtitled =>
        Data().Has("subtitled_languages") && Data().List("subtitled_languages").Any(language => language.AsText() == Session.Options.Language);

    /// <summary>A sprite playing one of the opening's timelines at the place the data gives a name.</summary>
    private CinematicSprite Sprite(string timeline, string place)
    {
        DataNode at = Data().Get("places").Get(place);
        CinematicSprite sprite = CinematicSprite.Of(Session.Content, _timelines.Get(timeline), PixelPosition.FromWhole(at.Int("x"), at.Int("y")));
        sprite.Name = place;
        return sprite;
    }

    private DataNode Data() => _data ?? throw NotEntered();

    private static InvalidOperationException NotEntered() => new("The opening has not begun.");
}
