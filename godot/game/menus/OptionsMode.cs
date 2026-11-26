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
/// chosen. The page of buttons lists each action of play with what it is bound to: with the mark on an action's row,
/// the next button pressed becomes that action's, and the action that had it takes the one given up; its last two
/// rows leave the page and put the standard buttons back. The page of special settings shows each of the game's toggles, and confirming one
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
/// "cursor_special_settings", "cursor_special_" and a toggle's name, "cursor_special_exit"); the picture of its
/// page of buttons in each language ("controller"), which has that page's heading and the words of its rows, with
/// the cursor's place beside each of its rows from the top ("button_rows": an action a row, then the way out, then
/// the row that puts the standard buttons back) and where on each action's row the button bound to it is named
/// ("button_names"); how far that page scrolls up to show its last two rows and how far an update ("buttons_scroll",
/// "buttons_scroll_step"); the sounds of the cursor moving and of a row confirmed ("move_sound", "select_sound"); the
/// updates it holds black as it is come to before it fades in ("entry_black_updates"); the levels of brightness its
/// fades go through, a level an update ("fade_levels"); and the timeline "cursor". A bound button is named in the
/// game's font: the cartridge has pictures only of its own controller's buttons. An action has one button on that
/// page: the one pressed for it, or, of its standard key and controller's button, the one of the device the player
/// is on.
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
    private CinematicSprite? _cursor;
    private ScreenFade? _fade;
    private Func<GameMode?>? _afterFade;
    private Page _page;
    private int _row;
    private string? _pressed;

    /// <summary>
    /// How far the page of buttons is scrolled up, in pixels, and how far it is on its way to being: all the way while
    /// the cursor is on its last two rows, which lie below the screen otherwise. The cursor is not shown while it moves.
    /// </summary>
    private int _buttonsScroll, _buttonsScrollTo;

    /// <summary>Where the heading's top lies while the page is not scrolled.</summary>
    private int _headingY;
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

    /// <summary>
    /// The actions of play, in the order of the page's rows: the shot, the jump, and the game's own. No two of them
    /// keep one button: binding one to a button another has swaps the two, as the cartridge's page does with its seven.
    /// </summary>
    private IReadOnlyList<string> PlayActions => [GameAction.Attack.Name, GameAction.Jump.Name, .. Session.Definition.Actions];

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
        // On the page of buttons whatever is pressed is taken as it comes from the device: a key, a controller's
        // button, or a controller's axis pushed, which is how a trigger and some pads' shoulder buttons reach the
        // game. An axis that was past its mark as the page was shown is one that rests there, and is no press.
        if (_page == Page.Buttons && Options.ButtonOf(@event) is { } button && !_restingAxes.Contains(button))
            _pressed = button;

        // Which device the player is on is whichever they last pressed a key or a button of.
        if (@event is InputEventKey { Pressed: true, Echo: false })
            _onController = false;
        else if (@event is InputEventJoypadButton { Pressed: true })
            _onController = true;
    }

    /// <summary>Whether the last key or button pressed in the options was a controller's.</summary>
    private bool _onController;

    /// <summary>The axes that read as pushed as the page of buttons was shown, as a binding would write each.</summary>
    private readonly HashSet<string> _restingAxes = [];

    /// <summary>Takes note of the axes that rest past their mark, and forgets whatever was pressed before the page was shown.</summary>
    private void BeginTakingButtons()
    {
        _pressed = null;
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
    /// The buttons no action of play can be given: those that move about and the one that is Start, which the
    /// cartridge's page keeps as they are.
    /// </summary>
    private HashSet<string> KeptButtons() =>
    [
        .. new[] { GameAction.Up, GameAction.Down, GameAction.Left, GameAction.Right, GameAction.Pause }
            .SelectMany(action => Session.Options.Bindings.GetValueOrDefault(action.Name) ?? []),
    ];

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
    private bool CursorShows => (_page != Page.Buttons || _buttonsScroll == _buttonsScrollTo) && _blackUpdates >= ScreenData().Int("entry_black_updates") && !_starting && !_cursorHidden;

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

        if (!Confirms(input))
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

    /// <summary>
    /// Plays the page of buttons as the cartridge's: up and down move the mark; any other button newly pressed is
    /// heard; on an action's row it becomes that action's button, and on the last two rows the confirm button or
    /// Start leaves the page or puts the standard buttons back.
    /// </summary>
    private void StepButtons(GameInput input)
    {
        string? pressed = _pressed;
        _pressed = null;

        // On its way from one end to the other the page moves a step an update and takes nothing asked of it.
        if (_buttonsScroll != _buttonsScrollTo)
        {
            int step = ScreenData().Int("buttons_scroll_step");
            _buttonsScroll += _buttonsScrollTo > _buttonsScroll ? step : -step;
            ShowButtonsScroll();
            return;
        }

        if (MovesRow(input, PlayActions.Count + 2))
        {
            // The last two rows lie below the screen until the page is scrolled up to them.
            _buttonsScrollTo = _row >= PlayActions.Count ? ScreenData().Int("buttons_scroll") : 0;
            PlaceCursor();
            return;
        }

        if (pressed is null)
            return;
        Sound("select_sound");
        if (_row < PlayActions.Count)
        {
            if (KeptButtons().Contains(pressed))
                return;
            Session.Options.Bind(PlayActions[_row], pressed, PlayActions);
            Session.Options.ApplyBindings([]);
            ShowButtons();
            return;
        }

        if (!input.WasPressed(GameAction.Confirm) && !input.WasPressed(GameAction.Pause))
            return;
        if (_row == PlayActions.Count)
        {
            FadeOutTo(() => Turn(Page.Primary));
            return;
        }

        // The standard buttons are put back for the device the row was confirmed on.
        Session.Options.RestoreBindings(PlayActions, Session.Definition.StandardBindings);
        Session.Options.KeepOneButton(PlayActions, _onController);
        Session.Options.ApplyBindings([]);
        ShowButtons();
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

        if (!Confirms(input) && !input.WasPressed(GameAction.Left) && !input.WasPressed(GameAction.Right))
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

    /// <summary>Whether a choice is confirmed: by confirm or by the pause button, the cartridge's A or Start.</summary>
    private static bool Confirms(GameInput input) => input.WasPressed(GameAction.Confirm) || input.WasPressed(GameAction.Pause);

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
        (_buttonsScroll, _buttonsScrollTo) = (0, 0);
        picture.Position = Vector2.Zero;
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
                picture.Texture = Session.Content.LoadTexture(data.Get("controller").Text(language));

                // An action set here has one button. One that still has its standard two, a key and a controller's,
                // keeps the one of the device the player came to the page on.
                options.KeepOneButton(PlayActions, _onController);
                options.ApplyBindings([]);
                BeginTakingButtons();
                ShowButtons();
                break;
            default:
                throw new InvalidOperationException($"The options have no page {page}.");
        }

        DataNode shown = data.Get("headings").Get(heading);
        Sprite2D top = _heading ?? throw NotShown();
        top.Texture = Session.Content.LoadTexture(shown.Text("picture"));
        _headingY = shown.Int("y");
        top.Position = new Vector2(shown.Int("x"), _headingY);
        PlaceCursor();
    }

    /// <summary>Puts the page of buttons, its heading and the names on it where its scroll has them.</summary>
    private void ShowButtonsScroll()
    {
        (_pagePicture ?? throw NotShown()).Position = new Vector2(0, -_buttonsScroll);
        Sprite2D top = _heading ?? throw NotShown();
        top.Position = new Vector2(top.Position.X, _headingY - _buttonsScroll);
        if ((_laidOver ?? throw NotShown()).GetNodeOrNull<Node2D>(ButtonNamesNode) is { } names)
            names.Position = new Vector2(0, -_buttonsScroll);
    }

    /// <summary>The name of the node the names of the bound buttons are under.</summary>
    private const string ButtonNamesNode = "ButtonNames";

    /// <summary>Names, on each action's row of the page of buttons, the button bound to it, in place of whatever was named there.</summary>
    private void ShowButtons()
    {
        Node2D laid = _laidOver ?? throw NotShown();
        if (laid.GetNodeOrNull(ButtonNamesNode) is { } old)
        {
            laid.RemoveChild(old);
            old.QueueFree();
        }

        IReadOnlyList<DataNode> places = ScreenData().List("button_names");
        if (places.Count != PlayActions.Count)
            throw new InvalidOperationException($"The options' data places the names of {places.Count} buttons, and play has {PlayActions.Count} actions to bind.");
        var names = new Node2D { Name = ButtonNamesNode, Position = new Vector2(0, -_buttonsScroll) };
        laid.AddChild(names);
        Options options = Session.Options;
        for (int row = 0; row < PlayActions.Count; row++)
        {
            // One line in the game's font, where the cartridge's page names its own controller's button.
            var name = new ChoiceList { Name = $"Button{row + 1}", Font = Session.Font, Mark = "", Position = new Vector2(places[row].Int("x"), places[row].Int("y")) };
            names.AddChild(name);
            name.Present([InFont(string.Join(", ", options.Bindings.GetValueOrDefault(PlayActions[row]) ?? []))]);
        }
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
        DataNode at;
        if (_page == Page.Buttons)
        {
            at = ScreenData().List("button_rows")[_row];
        }
        else
        {
            string place = _page == Page.Primary
                ? "cursor_" + PrimaryRows[_row]
                : _row < _specialRows.Count ? "cursor_special_" + _specialRows[_row] : "cursor_special_exit";
            at = ScreenData().Get("places").Get(place);
        }

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
