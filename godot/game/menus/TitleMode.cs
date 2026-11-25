using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.Ui;
using Engine.World;
using Godot;

namespace Game.Menus;

/// <summary>
/// The title. It opens with cards of words typed a letter at a time, each followed by a move across its backdrop
/// seen from close to, the last by the backdrop drawing back to its own size; then the title fades in over it and the
/// copyright under that, and it rests. A press during the opening cuts it short. At rest, confirm goes to the choice
/// of saved game; left alone for a while it shows its demos, a set of them at a time, and comes back from its
/// opening once a set has played through, or straight to its rest when a demo is cut short. The larva in its tube on
/// the backdrop pulses throughout.
/// </summary>
/// <remarks>
/// Its data names its "music" at rest, asked for when the opening is cut short; its "backdrop" colour; its "floor",
/// the picture behind the title, with how much of it there is to each pixel of the screen at rest ("floor_per_pixel"), the point of it that stays put
/// ("floor_centre") and how far it is moved at rest ("floor_scroll"); the "baby" on the floor (where, and its
/// "timeline"); its "pictures" at rest, each with where its corner goes; the copyright in the colours it is shown in at
/// rest once the opening has been cut short or a demo has come back ("restored_copyright", with where its corner
/// goes), the copyright at a rest come to by itself keeping the last look of its fade; how many updates it is left alone before it
/// shows a demo ("demo_after_updates"); its "demo_sets", each a list of the files of its demos; and its "opening":
/// its "music"; how long each letter of a card but the last shows before the next is typed ("letter_updates") and
/// how far the letters typed move as each further one is ("letter_scroll"); its "steps" in turn, each a "card" (the
/// updates before its first letter, "opening_updates"; how long its last shows, "hold_updates"; and its "letters",
/// each a picture and where its corner goes as it is typed, a space having no picture), a "pan" (how much of the
/// floor there is to a pixel, "floor_per_pixel"; where it starts, "x" and "y"; how far it moves each update,
/// "move_x" and "move_y"; and when it "ends": "x_below" or "y_at_least" the number "at") or a "zoom" (the same
/// start, and how much more of the floor there is to a pixel, "grows_by", on every so many updates, "every", until
/// the floor is its own size); the "logo" and the "copyright" as they fade in (each a place and a "timeline", whose
/// first look shows on the update each first shows) and
/// how long each is given ("logo_hold", "copyright_hold"); and how long the fades take when the opening is cut
/// short ("skip_fade_updates") and when the title is left ("leave_fade_updates").
/// </remarks>
public partial class TitleMode : GameMode
{
    private readonly Beginning _beginning;

    /// <summary>The title, from its opening, as the game starts or starts again.</summary>
    public TitleMode()
    {
    }

    /// <summary>The title, begun as <paramref name="beginning"/> says.</summary>
    public TitleMode(Beginning beginning) => _beginning = beginning;

    /// <summary>How the title begins.</summary>
    public enum Beginning
    {
        /// <summary>From its opening, as the game starts or starts again: the random numbers start over.</summary>
        Afresh,

        /// <summary>From its opening, once a set of demos has played through: the random numbers run on.</summary>
        AfterDemos,

        /// <summary>Straight at its rest, as a demo comes back cut short.</summary>
        AtRest,
    }

    /// <summary>The data file of what it shows.</summary>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "menus/title.json";

    /// <summary>What the title is doing.</summary>
    private enum Doing
    {
        /// <summary>Typing a card.</summary>
        Card,

        /// <summary>Moving across the floor.</summary>
        Pan,

        /// <summary>Drawing back from the floor.</summary>
        Zoom,

        /// <summary>The title is fading in.</summary>
        Logo,

        /// <summary>The copyright is fading in.</summary>
        Copyright,

        /// <summary>The skip has gone dark, and the title is put at its rest.</summary>
        Rebuilding,

        /// <summary>At rest.</summary>
        Resting,

        /// <summary>Going dark on the way to the choice of saved game.</summary>
        ToSavedGames,

        /// <summary>Going dark on the way to a demo.</summary>
        ToDemo,
    }

    private DataNode? _data;
    private TurningFloor? _floor;
    private CinematicSprite? _baby;
    private Node2D? _card;
    private CinematicSprite? _logo;
    private CinematicSprite? _copyright;
    private Node2D? _resting;
    private Sprite2D? _restoredCopyright;
    private ScreenFade? _fade;
    private Doing _doing;
    private bool _skipping;
    private int _step;
    private int _age;
    private int _typed;
    private int _hold;
    private double _perPixel;
    private Vector2 _pan;
    private int _idle;
    private bool _starting;

    /// <inheritdoc/>
    protected override void Enter()
    {
        _data = Session.Content.ReadData(DataFile);
        var layer = new CanvasLayer { Name = "Screen" };
        AddChild(layer);
        var backdrop = new ColorRect { Name = "Backdrop", Color = new Color(_data.Text("backdrop")) };
        backdrop.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        layer.AddChild(backdrop);
        _floor = new TurningFloor { Name = "Floor", Repeats = false, Picture = _data.Text("floor") };
        layer.AddChild(_floor);
        _floor.Setup(Session.Content, new Vector2I(256, 224));
        DataNode baby = _data.Get("baby");
        _baby = CinematicSprite.Of(Session.Content, FrameTimeline.Read(baby.Get("timeline")), PixelPosition.FromWhole(baby.Int("x"), baby.Int("y")));
        _baby.Name = "Baby";
        _floor.AddChild(_baby);
        _card = new Node2D { Name = "Card" };
        layer.AddChild(_card);
        DataNode opening = _data.Get("opening");
        _logo = Fading("Logo", opening.Get("logo"));
        layer.AddChild(_logo);
        _copyright = Fading("Copyright", opening.Get("copyright"));
        layer.AddChild(_copyright);
        _resting = new Node2D { Name = "Resting", Visible = false };
        layer.AddChild(_resting);
        foreach (DataNode picture in _data.List("pictures"))
        {
            _resting.AddChild(new Sprite2D
            {
                Texture = Session.Content.LoadTexture(picture.Text("picture")), Centered = false, Position = Place(picture),
                TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
            });
        }

        DataNode restored = _data.Get("restored_copyright");
        _restoredCopyright = new Sprite2D
        {
            Name = "RestoredCopyright", Texture = Session.Content.LoadTexture(restored.Text("picture")), Centered = false, Position = Place(restored),
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest, Visible = false,
        };
        layer.AddChild(_restoredCopyright);

        _fade = new ScreenFade { Name = "Fade", Layer = 20 };
        AddChild(_fade);
        if (_beginning == Beginning.AtRest)
        {
            Rest(restoredCopyright: true);
            _fade.FadeIn(opening.Int("skip_fade_updates"));
            return;
        }

        // The title begun afresh starts on an update of its own; the one it rebuilds after a set of demos asks for its
        // music as it is rebuilt.
        _starting = _beginning == Beginning.Afresh;
        if (!_starting)
            AskForMusic(opening);
        Begin(0);
    }

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        DataNode data = Data();
        DataNode opening = data.Get("opening");
        if (_starting)
        {
            // The cartridge's start, as the game starts or starts again, is an update by itself: it starts the random
            // numbers over from its power-on number and draws one, puts its queues of sounds and music back as at
            // power-on, puts its demos back to the first of the first set and asks for the opening's music. The
            // opening plays from the update after.
            _starting = false;
            Session.Random.Restart(Compatibility.CartridgeRandom.PowerOn);
            Session.Clock.Restart();
            Session.Random.Next();
            Compatibility.CartridgeSounds.Of(Session).Reset();
            Session.Service<DemoSchedule>().StartOver();
            AskForMusic(opening);
            return this;
        }

        // Every update of the old title draws a random number before anything else of it.
        Session.OpenUpdate();
        ScreenFade fade = _fade ?? throw NotShown();
        (_baby ?? throw NotShown()).Step();
        bool pressed = input.WasPressed(GameAction.Confirm) || input.WasPressed(GameAction.Pause) || input.WasPressed(GameAction.Cancel);
        switch (_doing)
        {
            case Doing.Card:
                StepCard(opening);
                break;
            case Doing.Pan:
                StepPan();
                break;
            case Doing.Zoom:
                StepZoom(opening);
                break;
            case Doing.Logo:
                (_logo ?? throw NotShown()).Step();
                if (--_hold <= 0)
                {
                    // The copyright shows its first look on the update its hold begins.
                    _doing = Doing.Copyright;
                    _hold = opening.Int("copyright_hold");
                    CinematicSprite copyright = _copyright ?? throw NotShown();
                    copyright.Visible = true;
                    copyright.Step();
                }

                break;
            case Doing.Copyright:
                (_logo ?? throw NotShown()).Step();
                (_copyright ?? throw NotShown()).Step();
                if (--_hold <= 0)
                    Rest(restoredCopyright: false);
                break;
            case Doing.Rebuilding:
                // The old game rebuilds the title at rest on the update after the skip's fade reaches black.
                Rest(restoredCopyright: true);
                fade.FadeIn(opening.Int("skip_fade_updates"));
                return this;
            case Doing.Resting:
                if (!fade.IsDone)
                {
                    fade.Step();
                    return this;
                }

                // Being left alone long enough wins over a press on the same update.
                IReadOnlyList<DataNode> sets = data.List("demo_sets");
                if (sets.Count > 0 && ++_idle >= data.Int("demo_after_updates"))
                    (_doing, pressed) = (Doing.ToDemo, false);
                else if (pressed)
                    _doing = Doing.ToSavedGames;
                else
                    return this;
                fade.FadeOut(opening.Int("leave_fade_updates"));
                return this;
            case Doing.ToSavedGames:
                fade.Step();
                return fade.IsDone ? new FileSelectMode() : this;
            case Doing.ToDemo:
                fade.Step();
                if (!fade.IsDone)
                    return this;
                string demo = Session.Service<DemoSchedule>().FromTitle(
                    [.. data.List("demo_sets").Select(set => (IReadOnlyList<string>)[.. set.Items().Select(file => file.AsText())])]);
                Session.BeginAgain();
                return AttractDemoMode.Of(Session.Content, demo);
            default:
                throw new InvalidOperationException($"The title has nothing to do called {_doing}.");
        }

        // A press while the cards, the moves or the drawing back are going on cuts the opening short, unless it is
        // already cut short; the step this update ends in decides.
        if (pressed && !_skipping && _doing is Doing.Card or Doing.Pan or Doing.Zoom)
        {
            _skipping = true;
            fade.FadeOut(opening.Int("skip_fade_updates"));
        }

        if (!_skipping)
            return this;

        // The opening goes on beneath the skip's fade, which steps after it, the first time on the update of the press.
        fade.Step();
        if (!fade.IsDone)
            return this;

        // Only a skip asks for the music at rest; the opening's own music runs on into it otherwise.
        Session.Sounds.RequestMusic(data.Text("music"));
        _skipping = false;
        _doing = Doing.Rebuilding;
        return this;
    }

    /// <summary>Asks for the opening's music, with the set it is in.</summary>
    private void AskForMusic(DataNode opening)
    {
        Compatibility.CartridgeSounds.Of(Session).AskMusicSet("title_sequence");
        Session.Sounds.RequestMusic(opening.Text("music"));
    }

    /// <summary>Begins a step of the opening; past the last of them the title fades in.</summary>
    private void Begin(int step)
    {
        DataNode opening = Data().Get("opening");
        IReadOnlyList<DataNode> steps = opening.List("steps");
        _step = step;
        _age = 0;
        ClearCard();
        TurningFloor floor = _floor ?? throw NotShown();
        if (step >= steps.Count)
        {
            // The title shows its first look on the update the drawing back ends.
            _doing = Doing.Logo;
            _hold = opening.Int("logo_hold");
            CinematicSprite logo = _logo ?? throw NotShown();
            logo.Visible = true;
            logo.Step();
            return;
        }

        DataNode now = steps[step];
        if (now.Has("card"))
        {
            // A card stands alone on the backdrop's colour.
            _doing = Doing.Card;
            _typed = 0;
            floor.Visible = false;
            return;
        }

        DataNode scene = now.Has("pan") ? now.Get("pan") : now.Get("zoom");
        _doing = now.Has("pan") ? Doing.Pan : Doing.Zoom;
        _perPixel = scene.Get("floor_per_pixel").AsPixels().ToDisplay();
        _pan = new Vector2(scene.Int("x"), scene.Int("y"));
        floor.Visible = true;
        TurnFloor();
    }

    /// <summary>Plays one update of a card: nothing at first, then its letters one after another, the last staying a while.</summary>
    private void StepCard(DataNode opening)
    {
        DataNode card = opening.List("steps")[_step].Get("card");
        IReadOnlyList<DataNode> letters = card.List("letters");
        int each = opening.Int("letter_updates");
        int since = _age++ - card.Int("opening_updates");
        if (since < 0)
            return;
        if (since >= (letters.Count - 1) * each + card.Int("hold_updates"))
        {
            Begin(_step + 1);
            return;
        }

        int typed = Math.Min(letters.Count, since / each + 1);
        if (typed == _typed)
            return;
        _typed = typed;
        ClearCard();
        Node2D shown = _card ?? throw NotShown();
        DataNode scroll = opening.Get("letter_scroll");

        // Each letter typed after a letter has moved it along once more.
        for (int letter = 0; letter < typed; letter++)
        {
            string picture = letters[letter].Text("picture");
            if (picture.Length == 0)
                continue;
            int moved = typed - 1 - letter;
            shown.AddChild(new Sprite2D
            {
                Texture = Session.Content.LoadTexture(picture), Centered = false, TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
                Position = new Vector2(letters[letter].Int("x") + scroll.Int("x") * moved, letters[letter].Int("y") + scroll.Int("y") * moved),
            });
        }
    }

    /// <summary>Plays one update of a move across the floor, which ends where its step says.</summary>
    private void StepPan()
    {
        DataNode pan = Data().Get("opening").List("steps")[_step].Get("pan");
        _pan += new Vector2((float)pan.Get("move_x").AsPixels().ToDisplay(), (float)pan.Get("move_y").AsPixels().ToDisplay());
        bool ended = pan.Text("ends") switch
        {
            "x_below" => Mathf.FloorToInt(_pan.X) < pan.Int("at"),
            "y_at_least" => Mathf.FloorToInt(_pan.Y) >= pan.Int("at"),
            _ => throw pan.Get("ends").Problem("must be \"x_below\" or \"y_at_least\""),
        };
        if (ended)
            Begin(_step + 1);
        else
            TurnFloor();
    }

    /// <summary>Plays one update of the floor drawing back: a little on every so many updates, and on the first such update that finds it at its own size the title begins to fade in.</summary>
    private void StepZoom(DataNode opening)
    {
        DataNode zoom = opening.List("steps")[_step].Get("zoom");
        if (_age++ % zoom.Int("every") != 0)
            return;
        if (_perPixel >= 1)
        {
            Begin(_step + 1);
            return;
        }

        _perPixel = Math.Min(1, _perPixel + zoom.Get("grows_by").AsPixels().ToDisplay());
        TurnFloor();
    }

    /// <summary>Lays the floor as the step under way has it: only whole pixels of a move count.</summary>
    private void TurnFloor()
    {
        (_floor ?? throw NotShown()).Turn(
            0, _perPixel, Place(Data().Get("floor_centre")), new Vector2(Mathf.FloorToInt(_pan.X), Mathf.FloorToInt(_pan.Y)));
    }

    /// <summary>
    /// Puts the title at its rest: the floor at its own size, and the title and the copyright over it. The
    /// old game rebuilds the title after a skip, or as a demo comes back, with the copyright's colours restored; come
    /// to rest by itself, the copyright keeps the last look of its fade, which lies over the restored colours.
    /// </summary>
    private void Rest(bool restoredCopyright)
    {
        DataNode data = Data();
        _doing = Doing.Resting;
        _idle = 0;
        ClearCard();
        TurningFloor floor = _floor ?? throw NotShown();
        floor.Visible = true;
        floor.Turn(0, data.Get("floor_per_pixel").AsPixels().ToDisplay(), Place(data.Get("floor_centre")), Place(data.Get("floor_scroll")));
        (_logo ?? throw NotShown()).Visible = false;
        (_copyright ?? throw NotShown()).Visible = !restoredCopyright;
        (_resting ?? throw NotShown()).Visible = true;
        (_restoredCopyright ?? throw NotShown()).Visible = restoredCopyright;
    }

    /// <summary>Takes the letters of a card away.</summary>
    private void ClearCard()
    {
        Node2D card = _card ?? throw NotShown();
        foreach (Node letter in card.GetChildren())
        {
            card.RemoveChild(letter);
            letter.QueueFree();
        }
    }

    /// <summary>A picture that fades in where the data puts it, unseen until its turn.</summary>
    private CinematicSprite Fading(string name, DataNode placed)
    {
        CinematicSprite sprite = CinematicSprite.Of(
            Session.Content, FrameTimeline.Read(placed.Get("timeline")), PixelPosition.FromWhole(placed.Int("x"), placed.Int("y")));
        sprite.Name = name;
        sprite.Visible = false;
        return sprite;
    }

    private static Vector2 Place(DataNode at) => new(at.Int("x"), at.Int("y"));

    private DataNode Data() => _data ?? throw NotShown();

    private static InvalidOperationException NotShown() => new("The title has not been shown.");
}
