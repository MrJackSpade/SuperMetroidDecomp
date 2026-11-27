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
/// between pages and the flight to the colony. Its data names its pictures ("first_card", "portrait", "final_line"),
/// the "font" its pages are typed in and their "typing_sound", the timelines of what it shows ("eye", "eye_closing",
/// "caret", "caret_blinking", "next_page_arrow", "next_page_arrow_delayed"), where things are ("places", among them
/// "words", the corner of a page's first letter), its "pages", and what each scene needs. A page gives the name of
/// its words ("text"), the "scene" it turns to if it turns to one, and whether, in the "subtitled_languages", a
/// second press is waited for before it may be turned ("awaits_second_press"). The words of a page are its lines,
/// with a line feed for every row down from one line to the next.
/// </remarks>
public partial class Intro : Cinematic
{
    /// <summary>The data file of what it shows.</summary>
    public string DataFile { get; set; } = "cinematics/intro.json";

    private enum Phase
    {
        Initial,
        WaitingForCardMusic,
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

    /// <summary>How many updates a fade of the whole screen takes, and how many steps it goes in.</summary>
    private const int FadeUpdates = 30, FadeLevels = 15;

    /// <summary>How many turns of the music's queue a scene's piece of music waits once at its front.</summary>
    private const int SceneMusicWait = 14;

    /// <summary>How long the card is held before each of its two pieces of music, and after the second.</summary>
    private const int FirstLineUpdates = 60, CaptivityUpdates = 200, PeaceUpdates = 240;

    /// <summary>How long a crossfade lasts, how many steps it goes in, and how many updates each step lasts.</summary>
    private const int CrossfadeUpdates = 128, CrossfadeLevels = 32, CrossfadeStepUpdates = 4;

    /// <summary>How many updates pass between one letter and the next, and how long the last page is held.</summary>
    private const int LetterUpdates = 5, FinalPageUpdates = 128;

    /// <summary>How long the page must wait, once the second press has come, before it may be turned.</summary>
    private const int SecondPressHoldUpdates = 60;

    private DataNode? _data;
    private TimelineSet _timelines = TimelineSet.None;
    private Phase _phase;
    private int _wait;
    private int _page;
    private string _scene = "";
    private int _crossfade;

    private ScreenFade? _fade;
    private GradedView? _sceneRoot;
    private Node2D? _narration;
    private Sprite2D? _card;
    private PixelText? _words;

    /// <summary>The fonts of a typed letter's glow, brightest first, and how many updates each shows for.</summary>
    private readonly List<PixelFont> _glowFonts = [];
    private int _glowStepUpdates;

    /// <summary>The letters still glowing, each laid over its place in the page's words, with how many updates it has glowed.</summary>
    private readonly List<(PixelText Letter, int Age)> _glowing = [];
    private PixelPosition _wordsAt;
    private int _cell;
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

        // The colour behind the scenes lies on a layer of its own beneath them. A room shown in a scene draws its
        // tiles and what lies behind it at depths below everything else of its layer, and a colour laid in the same
        // layer would hide them.
        var behind = new CanvasLayer { Name = "Backdrop", Layer = 0 };
        AddChild(behind);
        var black = new ColorRect { Name = "Black", Color = new Color(_data.Text("backdrop")) };
        black.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        behind.AddChild(black);
        var scenes = new CanvasLayer { Name = "Scenes", Layer = 1 };
        AddChild(scenes);
        _sceneRoot = new GradedView { Name = "Scene", Modulate = new Color(1, 1, 1, 0) };
        scenes.AddChild(_sceneRoot);

        var pages = new CanvasLayer { Name = "Pages", Layer = 2 };
        AddChild(pages);
        _card = new Sprite2D { Name = "Card", Centered = false, Texture = Session.Content.LoadTexture(_data.Text("first_card")) };
        pages.AddChild(_card);
        _narration = new Node2D { Name = "Narration", Visible = false };
        pages.AddChild(_narration);

        // Her portrait lies under everything else of a page, her eye over it.
        _narration.AddChild(Picture("Portrait", "portrait", "portrait"));
        _eye = Sprite("eye", "eye");
        _narration.AddChild(_eye);
        _narration.AddChild(Picture("FinalLine", "final_line", "final_line"));
        PixelFont font = PixelFont.Read(Session.Content, _data.Text("font"));
        DataNode wordsAt = _data.Get("places").Get("words");
        (_wordsAt, _cell) = (PixelPosition.FromWhole(wordsAt.Int("x"), wordsAt.Int("y")), font.Cell.X);
        _words = new PixelText { Name = "Words", Font = font, Position = new Vector2(wordsAt.Int("x"), wordsAt.Int("y")), VisibleCharacters = 0 };
        _narration.AddChild(_words);
        _glowFonts.Clear();
        _glowFonts.AddRange(_data.List("glow_fonts").Select(glow => PixelFont.Read(Session.Content, glow.AsText())));
        _glowStepUpdates = _data.Int("glow_step_updates");
        _caret = CinematicSprite.Of(Session.Content, _timelines.Get("caret"), _wordsAt);
        _caret.Name = "Caret";
        _narration.AddChild(_caret);

        _fade = new ScreenFade { Name = "Fade", Layer = 5, Levels = FadeLevels };
        AddChild(_fade);
        _fade.FadeOut(1);
        _fade.Step();
        _phase = Phase.Initial;

        // No page is under way until the first is started.
        _pageFinished = true;
    }

    /// <inheritdoc/>
    protected override bool Play(int elapsed)
    {
        // Every update of the old introduction draws a random number before anything else of it.
        Session.OpenUpdate();
        ScreenFade fade = _fade ?? throw NotEntered();
        switch (_phase)
        {
            case Phase.Initial:
                Session.Sounds.RequestMusic("");
                Compatibility.CartridgeSounds.Of(Session).AskMusicSet("last_metroid_in_captivity");
                _phase = Phase.WaitingForCardMusic;
                break;

            case Phase.WaitingForCardMusic:
                // The card comes up once the music asked for has all been carried out.
                if (Session.Sounds.HasQueuedMusic)
                    break;
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
                Compatibility.CartridgeSounds.Of(Session).AskMusicSet("galaxy_at_peace");
                Session.Sounds.RequestMusic(Music.GalaxyAtPeace, SceneMusicWait);
                _phase = Phase.WaitingForPeaceMusic;
                break;

            case Phase.WaitingForPeaceMusic:
                // The card is held from when the music asked for has all been carried out.
                if (Session.Sounds.HasQueuedMusic)
                    break;
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
                Compatibility.CartridgeSounds.Of(Session).AskMusicSet("intro");
                Session.Sounds.RequestMusic(Music.Introduction, SceneMusicWait);
                _phase = Phase.WaitingForPageMusic;
                break;

            case Phase.WaitingForPageMusic:
                // The first page comes up once the music asked for has all been carried out.
                if (Session.Sounds.HasQueuedMusic)
                    break;
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
                // The page is typed below, and what follows its last letter is settled after it.
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

                // A flashback's lights come on once it has come in.
                foreach (Engine.World.ColourEffect lights in _room?.GetChildren().OfType<Engine.World.ColourEffect>() ?? [])
                    lights.Triggered("start");
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

        // The pages' eye and caret run from the update the first page is set up, while the screen is still black.
        if (_phase is >= Phase.WaitingForPageMusic and <= Phase.FadingOut)
            StepNarration();

        // A page typed out on this update waits from this update to be turned, and the last, held, fades from it.
        if (_phase == Phase.PageText && _pageFinished && !_awaitingSecondSubtitle)
        {
            if (_page == LastPage)
            {
                fade.FadeOut(FadeUpdates);
                _phase = Phase.FadingOut;
            }
            else
            {
                _phase = Phase.PageAwaitingInput;
            }
        }

        if (_scene.Length > 0)
            StepScene(_phase == Phase.CrossfadingToScene);
        return _phase == Phase.Finished;
    }

    /// <inheritdoc/>
    protected override GameMode Next()
    {
        // The new game is loaded on the update after the opening's last: whatever the opening did with Samus is
        // forgotten there, and the game begins with her as she starts.
        return Menus.GameLoad.OfNewGame();
    }

    /// <summary>Starts a page: its words begin to be typed from the top.</summary>
    private void StartPage(int page, bool restoresCaret)
    {
        DataNode entry = Data().List("pages")[page];
        _page = page;
        _text = Session.Text.Get(entry.Text("text"));
        (_typed, _letterWait, _pageFinished, _finalHold, _awaitingSecondSubtitle, _subtitleHold) = (0, 2, false, 0, false, 0);
        ClearGlows();
        PixelText words = _words ?? throw NotEntered();
        words.Text = _text;
        words.VisibleCharacters = 0;
        _arrow?.QueueFree();
        _arrow = null;
        CinematicSprite caret = _caret ?? throw NotEntered();
        if (restoresCaret || page == LastPage)
        {
            caret.Visible = true;
            caret.Place = _wordsAt;
            caret.Play(_timelines.Get("caret"));
        }

        // The last page takes the place of everything before it, and her eye closes on it.
        if (page == LastPage)
            (_eye ?? throw NotEntered()).Play(_timelines.Get("eye_closing"));
    }

    /// <summary>Plays one update of the page: her eye, the caret, the next letter when it is due, and the glow of the letters lately typed.</summary>
    private void StepNarration()
    {
        TypeNarration();
        StepGlows();
    }

    /// <summary>
    /// A typed letter glows before it settles: it shows each of its glow's colourings for a while, brightest first,
    /// from the update it is typed on, and then the colour the page's words are in.
    /// </summary>
    private void StepGlows()
    {
        for (int index = _glowing.Count - 1; index >= 0; index--)
        {
            (PixelText letter, int age) = _glowing[index];
            int step = age / _glowStepUpdates;
            if (step >= _glowFonts.Count)
            {
                letter.QueueFree();
                _glowing.RemoveAt(index);
                continue;
            }

            letter.Font = _glowFonts[step];
            _glowing[index] = (letter, age + 1);
        }
    }

    /// <summary>Takes away every letter's glow, as a page's words go.</summary>
    private void ClearGlows()
    {
        foreach ((PixelText letter, _) in _glowing)
            letter.QueueFree();
        _glowing.Clear();
    }

    /// <summary>Her eye, the caret, and the next letter when it is due.</summary>
    private void TypeNarration()
    {
        (_eye ?? throw NotEntered()).Step();
        CinematicSprite caret = _caret ?? throw NotEntered();
        caret.Step();
        _arrow?.Step();
        if (_awaitingSecondSubtitle && Input.AnyPressed)
        {
            // The second press has come: the page is held a moment before it may be turned, its arrow gone meanwhile.
            _awaitingSecondSubtitle = false;
            _subtitleHold = SecondPressHoldUpdates;
            _arrow?.Play(_timelines.Get("next_page_arrow_delayed"));
            return;
        }

        if (_pageFinished)
            return;

        // Each letter is shown for its time before the next comes, the last as well: what follows the last waits for it.
        if (_letterWait > 0 && --_letterWait > 0)
            return;
        if (_typed >= _text.Length)
        {
            FinishTyping(caret);
            return;
        }

        _letterWait = LetterUpdates;
        char letter = _text[_typed];
        if (!char.IsWhiteSpace(letter) && _glowFonts.Count > 0)
        {
            // The letter is laid over its place in the page's words, in its glow's colourings, until it settles.
            PixelPosition cell = CellOf(_typed);
            var glowing = new PixelText { Name = "Glow", Font = _glowFonts[0], Text = letter.ToString(), Position = new Vector2(cell.X.Whole, cell.Y.Whole) };
            (_narration ?? throw NotEntered()).AddChild(glowing);
            _glowing.Add((glowing, 0));
        }

        _typed++;

        // Going on to the next line takes no time of its own.
        while (_typed < _text.Length && char.IsControl(_text[_typed]))
            _typed++;
        (_words ?? throw NotEntered()).VisibleCharacters = _typed;
        caret.Place = _typed < _text.Length ? CellOf(_typed) : PixelPosition.FromWhole(_wordsAt.X.Whole, CellOf(_typed - 1).Y.Whole + 2 * _cell);
        caret.ShowPlace();
        _click = !_click;
        if (!char.IsWhiteSpace(letter) && _click)
            Session.Sounds.Request(Data().Text("typing_sound"), channel: "text");
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
            DataNode page = Data().List("pages")[_page];
            _awaitingSecondSubtitle = page.Has("awaits_second_press") && page.Flag("awaits_second_press");
        }
    }

    /// <summary>Turns from a page to what it leads to: a scene, or the last page.</summary>
    private void TurnPage()
    {
        _arrow?.QueueFree();
        _arrow = null;
        DataNode page = Data().List("pages")[_page];
        string scene = page.Has("scene") ? page.Text("scene") : "";
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
        PixelText words = _words ?? throw NotEntered();
        (_text, _typed, words.Text, words.VisibleCharacters) = ("", 0, "", 0);
        ClearGlows();
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

        return PixelPosition.FromWhole(_wordsAt.X.Whole + column * _cell, _wordsAt.Y.Whole + row * _cell);
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

    /// <summary>One of the opening's still pictures, with its corner at the place the data gives a name.</summary>
    private Sprite2D Picture(string name, string picture, string place)
    {
        DataNode at = Data().Get("places").Get(place);
        return new Sprite2D
        {
            Name = name, Centered = false, Position = new Vector2(at.Int("x"), at.Int("y")), Texture = Session.Content.LoadTexture(Data().Text(picture)),
        };
    }

    private DataNode Data() => _data ?? throw NotEntered();

    private static InvalidOperationException NotEntered() => new("The opening has not begun.");
}
