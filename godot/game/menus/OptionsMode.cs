using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.Ui;
using Godot;

namespace Game.Menus;

/// <summary>
/// The options, come to from the choice of saved game. Its first page starts the game, changes the language of the
/// game's words, and leads to the page of buttons and the page of special settings; a missile points at the row
/// chosen. The page of buttons lists each action with the button it is bound to: confirming one waits for a button
/// and binds the action to it. The page of special settings shows each of the game's toggles, and confirming one
/// changes it. Cancelling on the first page goes back to the choice of saved game. Leaving saves them.
/// </summary>
/// <remarks>
/// Its data names its "backdrop" colour and "background" picture; under "headings" the heading of each page
/// ("primary", "controller", "special"), each a "picture" and the "x" and "y" of its corner; the picture of its
/// first page ("primary") and, under "special", of the page of special settings in each language; under
/// "languages", for each language, what is laid over the first page while it is the one chosen; under "toggles",
/// for each language and each toggle, what is laid over the special page while it is "on" and while it is "off";
/// the toggles of the special page in the order of its rows ("special_rows"); under "places" the cursor beside
/// each row ("cursor_start_game", "cursor_english_text", "cursor_japanese_text", "cursor_controller_settings",
/// "cursor_special_settings", "cursor_special_" and a toggle's name, "cursor_special_exit") and where the list of
/// "buttons" starts; the sounds of the cursor moving and of a row confirmed ("move_sound", "select_sound"); the updates
/// it holds black as it is come to before it fades in ("entry_black_updates"); the levels of brightness its fades go
/// through, a level an update ("fade_levels"); and the timeline "cursor". The words of its list of buttons are "options_action_" and an action's name (with {button}),
/// "options_press_a_button" and "options_exit".
/// </remarks>
public partial class OptionsMode : GameMode
{
    /// <summary>The rows of the first page, from the top.</summary>
    private static readonly string[] PrimaryRows = ["start_game", "english_text", "japanese_text", "controller_settings", "special_settings"];

    /// <summary>The pages of the options.</summary>
    private enum Page
    {
        /// <summary>The first page.</summary>
        Primary,

        /// <summary>The list of each action's button.</summary>
        Buttons,

        /// <summary>The special settings.</summary>
        Special,
    }

    private readonly List<string> _specialRows = [];
    private DataNode? _data;
    private Sprite2D? _pagePicture;
    private Sprite2D? _heading;
    private Node2D? _laidOver;
    private ChoiceList? _list;
    private CinematicSprite? _cursor;
    private ScreenFade? _fade;
    private Func<GameMode?>? _afterFade;
    private Page _page;
    private int _row;
    private string? _binding;
    private string? _pressed;
    private int _blackUpdates;
    private int _levels;
    private int _lit;
    private bool _brightening;
    private bool _starting;
    private bool _cursorHidden;

    /// <summary>The data file of what it shows.</summary>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "menus/options.json";

    /// <summary>The languages the game's words come in, the first being the one its first language row names.</summary>
    public string[] Languages { get; set; } = ["english", "japanese"];

    private IReadOnlyList<string> Actions => [.. GameAction.Standard.Select(action => action.Name), .. Session.Definition.Actions];

    /// <inheritdoc/>
    protected override void Enter()
    {
        _data = Session.Content.ReadData(DataFile);
        _specialRows.AddRange(_data.List("special_rows").Select(row => row.AsText()));
        var layer = new CanvasLayer { Name = "Screen", Layer = 10 };
        AddChild(layer);
        var backdrop = new ColorRect { Name = "Backdrop", Color = new Color(_data.Text("backdrop")) };
        backdrop.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        layer.AddChild(backdrop);
        layer.AddChild(Picture("Background", _data.Text("background"), 0, 0));
        _pagePicture = new Sprite2D { Name = "Page", Centered = false, TextureFilter = CanvasItem.TextureFilterEnum.Nearest };
        layer.AddChild(_pagePicture);
        _laidOver = new Node2D { Name = "LaidOver" };
        layer.AddChild(_laidOver);
        _heading = new Sprite2D { Name = "Heading", Centered = false, TextureFilter = CanvasItem.TextureFilterEnum.Nearest };
        layer.AddChild(_heading);
        DataNode buttons = _data.Get("places").Get("buttons");
        _list = new ChoiceList
        {
            Name = "Buttons", Font = Session.Font, Mark = "", UnchosenTint = new Color(0.5f, 0.5f, 0.5f), Position = new Vector2(buttons.Int("x"), buttons.Int("y")),
        };
        layer.AddChild(_list);
        _cursor = CinematicSprite.Of(Session.Content, TimelineSet.ReadFrom(_data).Get("cursor"), PixelPosition.Origin);
        _cursor.Name = "Cursor";
        _cursor.Visible = false;
        layer.AddChild(_cursor);
        _fade = new ScreenFade { Name = "Fade", Layer = 20 };
        AddChild(_fade);
        Show(Page.Primary);
        (_levels, _lit) = (_data.Int("fade_levels"), 0);
        _fade.Cover(_levels, _levels);
        _brightening = true;
    }

    /// <inheritdoc/>
    public override void _Input(InputEvent @event)
    {
        // While an action waits to be bound, the next button pressed is its button: a key, a controller's button,
        // or a controller's axis pushed, which is how a trigger and some pads' shoulder buttons reach the game. An
        // axis that was already past its mark as the wait began is one that rests there, and is not a press.
        if (_binding is null || _pressed is not null)
            return;
        if (Options.ButtonOf(@event) is { } button && !_restingAxes.Contains(button))
            _pressed = button;
    }

    /// <summary>The axes that read as pushed as the wait for a button began, as a binding would write each.</summary>
    private readonly HashSet<string> _restingAxes = [];

    /// <summary>Begins the wait for the button an action is to be bound to.</summary>
    private void AwaitButtonFor(string action)
    {
        (_binding, _pressed) = (action, null);
        _restingAxes.Clear();
        foreach (int controller in Godot.Input.GetConnectedJoypads())
        {
            for (int axis = 0; axis < (int)JoyAxis.Max; axis++)
            {
                float pushed = Godot.Input.GetJoyAxis(controller, (JoyAxis)axis);
                if (Math.Abs(pushed) >= Options.AxisPressed)
                    _restingAxes.Add(Options.AxisButton(axis, pushed > 0));
            }
        }
    }

    /// <summary>
    /// The actions of play, which no two of keep one button: binding one of them to a button another has swaps the
    /// two, as the cartridge's page of buttons does with its seven.
    /// </summary>
    private IReadOnlyCollection<string> PlayActions => [GameAction.Jump.Name, GameAction.Attack.Name, .. Session.Definition.Actions];

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        // Every update of the old options draws a random number before anything else of it.
        Session.OpenUpdate();
        GameMode next = Play(input);
        (_cursor ?? throw NotShown()).Visible = CursorShows;
        return next;
    }

    /// <summary>
    /// Whether the cursor shows as the update ends: as in the old game, not while the screen holds black as it is come
    /// to, while a page turns, or as a game starts.
    /// </summary>
    private bool CursorShows => _page != Page.Buttons && _blackUpdates >= ScreenData().Int("entry_black_updates") && !_starting && !_cursorHidden;

    /// <summary>Plays one update of the options, and says the mode to go to.</summary>
    private GameMode Play(GameInput input)
    {
        CinematicSprite cursor = _cursor ?? throw NotShown();

        // As the old game's options are come to, they hold black a few updates before fading in, the cursor still on
        // the first of them.
        if (_blackUpdates < ScreenData().Int("entry_black_updates"))
        {
            if (_blackUpdates++ > 0)
                cursor.Step();
            return this;
        }

        cursor.Step();
        if (_starting)
        {
            Session.Options.Save();
            if (SaveMapMode.Of(Session) is { } map)
                return map;

            // A game saved where the map marks nothing (at Ceres) is loaded with no map shown; with no game saved a
            // new one begins.
            return Session.Saves.Has(Session.Slot) ? GameLoad.OfSlot(appears: false) : new LoadingMode();
        }

        if (_afterFade is { } then)
        {
            StepFade();
            if (!FadeDone)
                return this;
            _afterFade = null;
            if (then() is { } next)
                return next;
            _brightening = true;
            return this;
        }

        if (!FadeDone)
        {
            StepFade();

            // A page turned shows its cursor again once it has faded in.
            _cursorHidden &= !FadeDone;
            return this;
        }

        switch (_page)
        {
            case Page.Primary:
                StepPrimary(input);
                break;
            case Page.Buttons:
                StepButtons(input);
                break;
            case Page.Special:
                StepSpecial(input);
                break;
            default:
                throw new InvalidOperationException($"The options have no page {_page}.");
        }

        return this;
    }

    /// <summary>Moves on the first page.</summary>
    private void StepPrimary(GameInput input)
    {
        if (MovesRow(input, PrimaryRows.Length))
            PlaceCursor();
        if (input.WasPressed(GameAction.Cancel))
        {
            // The cursor stays on going back to the choice of saved game, as in the old game.
            FadeOutTo(Leave, cursorShows: true);
            return;
        }

        if (!input.WasPressed(GameAction.Confirm))
            return;
        Sound("select_sound");
        switch (PrimaryRows[_row])
        {
            case "start_game":
                // As in the old game, a saved game starts on the next update with no fade; a new game fades out first and
                // starts on the update after the screen is black. A saved game shows where it was saved before it
                // starts; anything else starts at once.
                if (Session.Saves.Has(Session.Slot))
                    _starting = true;
                else
                    FadeOutTo(Start);
                break;
            case "english_text" or "japanese_text":
                // Either row changes the language to the other one, and the cursor goes back to the top.
                Options options = Session.Options;
                options.Language = Languages[(Array.IndexOf(Languages, options.Language) + 1) % Languages.Length];
                Show(Page.Primary);
                break;
            case "controller_settings":
                FadeOutTo(() => Turn(Page.Buttons));
                break;
            default:
                FadeOutTo(() => Turn(Page.Special));
                break;
        }
    }

    /// <summary>Moves on the list of buttons, and binds an action to the next button pressed when one is asked for.</summary>
    private void StepButtons(GameInput input)
    {
        ChoiceList list = _list ?? throw NotShown();
        if (_binding is { } action)
        {
            if (_pressed is not { } button)
                return;
            Session.Options.Bind(action, button, PlayActions);
            Session.Options.ApplyBindings([]);
            (_binding, _pressed) = (null, null);
            ShowButtons(list.Chosen);
            return;
        }

        switch (list.Step(input))
        {
            case ChoiceList.Outcome.Moved:
                Sound("move_sound");
                break;
            case ChoiceList.Outcome.Confirmed:
                Sound("select_sound");
                if (list.Chosen < Actions.Count)
                {
                    AwaitButtonFor(Actions[list.Chosen]);
                    ShowButtons(list.Chosen);
                }
                else
                {
                    FadeOutTo(() => Turn(Page.Primary));
                }

                break;
            case ChoiceList.Outcome.Cancelled:
                Sound("select_sound");
                FadeOutTo(() => Turn(Page.Primary));
                break;
        }
    }

    /// <summary>Moves on the page of special settings.</summary>
    private void StepSpecial(GameInput input)
    {
        if (MovesRow(input, _specialRows.Count + 1))
            PlaceCursor();
        if (input.WasPressed(GameAction.Cancel))
        {
            Sound("select_sound");
            FadeOutTo(() => Turn(Page.Primary));
            return;
        }

        if (!input.WasPressed(GameAction.Confirm) && !input.WasPressed(GameAction.Left) && !input.WasPressed(GameAction.Right))
            return;
        Sound("select_sound");
        if (_row >= _specialRows.Count)
        {
            FadeOutTo(() => Turn(Page.Primary));
            return;
        }

        string toggle = _specialRows[_row];
        Session.Options.Toggles[toggle] = !Session.Options.IsOn(toggle);
        int row = _row;
        Show(Page.Special);
        _row = row;
        PlaceCursor();
    }

    /// <summary>Moves the cursor up or down a page of <paramref name="rows"/> rows, round from one end to the other, and says whether it moved.</summary>
    private bool MovesRow(GameInput input, int rows)
    {
        // Up wins when both are pressed.
        int step = input.WasPressed(GameAction.Up) ? rows - 1 : input.WasPressed(GameAction.Down) ? 1 : 0;
        if (step == 0)
            return false;
        _row = (_row + step) % rows;
        Sound("move_sound");
        return true;
    }

    /// <summary>
    /// Fades the screen out, and then does <paramref name="then"/>: the mode to go to (itself to stay black), or none to
    /// stay and fade back in. The cursor is hidden from now until the screen has faded back in, unless
    /// <paramref name="cursorShows"/>.
    /// </summary>
    private void FadeOutTo(Func<GameMode?> then, bool cursorShows = false)
    {
        _afterFade = then;
        _brightening = false;
        _cursorHidden = !cursorShows;
    }

    /// <summary>Whether the fade begun last has reached full brightness, or black.</summary>
    private bool FadeDone => _lit == (_brightening ? _levels : 0);

    /// <summary>Plays one update of the fade: as in the old game, a level of brightness an update.</summary>
    private void StepFade()
    {
        _lit += _brightening ? 1 : -1;
        (_fade ?? throw NotShown()).Cover(_levels - _lit, _levels);
    }

    /// <summary>Starts the game on the next update, staying black until then.</summary>
    private GameMode Start()
    {
        _starting = true;
        return this;
    }

    /// <summary>Shows <paramref name="page"/> from its top, and stays.</summary>
    private GameMode? Turn(Page page)
    {
        Show(page);
        return null;
    }

    private GameMode Leave()
    {
        Session.Options.Save();

        // The old game makes the choice of saved game anew, its cursor's roll begun again on the saved game chosen.
        return new FileSelectMode();
    }

    /// <summary>Shows <paramref name="page"/> with its cursor at the top: its picture, its heading, and what is laid over it for how things are set.</summary>
    private void Show(Page page)
    {
        _page = page;
        _row = 0;
        DataNode data = ScreenData();
        Options options = Session.Options;
        string language = options.Language;
        Node2D laid = _laidOver ?? throw NotShown();
        foreach (Node old in laid.GetChildren())
        {
            laid.RemoveChild(old);
            old.QueueFree();
        }

        Sprite2D picture = _pagePicture ?? throw NotShown();
        picture.Visible = page != Page.Buttons;
        ChoiceList list = _list ?? throw NotShown();
        list.Visible = page == Page.Buttons;
        string heading;
        switch (page)
        {
            case Page.Primary:
                heading = "primary";
                picture.Texture = Session.Content.LoadTexture(data.Text("primary"));
                laid.AddChild(Laid("Language", data.Get("languages").Get(language)));
                break;
            case Page.Special:
                heading = "special";
                picture.Texture = Session.Content.LoadTexture(data.Get("special").Text(language));
                foreach (string toggle in _specialRows)
                    laid.AddChild(Laid(toggle, data.Get("toggles").Get(language).Get(toggle).Get(options.IsOn(toggle) ? "on" : "off")));
                break;
            case Page.Buttons:
                heading = "controller";
                ShowButtons(0);
                break;
            default:
                throw new InvalidOperationException($"The options have no page {page}.");
        }

        DataNode shown = data.Get("headings").Get(heading);
        Sprite2D top = _heading ?? throw NotShown();
        top.Texture = Session.Content.LoadTexture(shown.Text("picture"));
        top.Position = new Vector2(shown.Int("x"), shown.Int("y"));
        PlaceCursor();
    }

    /// <summary>Lists each action with its button, and the way out.</summary>
    private void ShowButtons(int chosen)
    {
        Options options = Session.Options;
        GameText text = Session.Text;
        var lines = new List<string>();
        foreach (string action in Actions)
        {
            string button = action == _binding
                ? text.Get("options_press_a_button")
                : InFont(string.Join(", ", options.Bindings.GetValueOrDefault(action) ?? []));
            lines.Add(text.Get($"options_action_{action}").Replace("{button}", button, StringComparison.Ordinal));
        }

        lines.Add(text.Get("options_exit"));
        (_list ?? throw NotShown()).Present(lines, chosen);
    }

    /// <summary>A name as the game's font can show it: in capitals, with a space for each character the font has not.</summary>
    private string InFont(string name)
    {
        PixelFont font = Session.Font ?? throw new InvalidOperationException("The game has no font to show its options in.");
        return string.Concat(name.ToUpperInvariant().Select(character => font.Has(character) ? character : ' '));
    }

    /// <summary>Puts the cursor beside the row it points at.</summary>
    private void PlaceCursor()
    {
        if (_page == Page.Buttons)
            return;
        string place = _page == Page.Primary
            ? "cursor_" + PrimaryRows[_row]
            : _row < _specialRows.Count ? "cursor_special_" + _specialRows[_row] : "cursor_special_exit";
        DataNode at = ScreenData().Get("places").Get(place);
        CinematicSprite cursor = _cursor ?? throw NotShown();
        cursor.Place = PixelPosition.FromWhole(at.Int("x"), at.Int("y"));
        cursor.ShowPlace();
    }

    /// <summary>A picture the data places by its corner.</summary>
    private Sprite2D Laid(string name, DataNode placed) => Picture(name, placed.Text("picture"), placed.Int("x"), placed.Int("y"));

    private Sprite2D Picture(string name, string file, int x, int y) => new()
    {
        Name = name,
        Centered = false,
        Texture = Session.Content.LoadTexture(file),
        TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
        Position = new Vector2(x, y),
    };

    private void Sound(string name) => Session.Sounds.Request(ScreenData().Text(name), channel: "menu");

    private DataNode ScreenData() => _data ?? throw NotShown();

    private static InvalidOperationException NotShown() => new("The options have not been shown.");
}
