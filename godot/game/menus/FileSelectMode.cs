using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.Ui;
using Godot;

namespace Game.Menus;

/// <summary>
/// The choice of saved game. Its first page shows the three saved games, each with the energy and play time it holds
/// or that it holds nothing, a helmet beside each, and under them copying, clearing and leaving; a missile points at
/// the row chosen. Choosing a saved game turns its helmet to face out and goes on to the options, from where the game
/// starts. Copying asks which saved game, then over which, then whether; clearing asks which, then whether; each says
/// when it is done. Leaving goes back to the title.
/// </summary>
/// <remarks>
/// Its data names its "backdrop" colour and "background" picture; under "pages" a whole picture for each page
/// ("main_with_data", "main_empty", "copy_source", "copy_destination", "copy_confirm", "copy_completed",
/// "clear_selection", "clear_confirm", "clear_completed"); under "borders" the heading of the main page, of copying
/// and of clearing ("main", "copy", "clear"), each a "picture" and the "x" and "y" of its corner; the pictures laid
/// over a page for a saved game: "energy", "no_data", "time_colon", "tank_filled", "tank_empty", the ten "digits"
/// and a letter for each saved game ("letters"); where a saved game's are laid on the main page ("main_slots") and
/// on the others ("data_slots"), each with "energy", "health", "no_data", "time" and the "cursor" beside it, and on
/// the main page its "helmet"; under "places" the cursor beside the other rows ("cursor_copy", "cursor_clear",
/// "cursor_exit", "cursor_data_exit", "cursor_yes", "cursor_no"), the letters a question names saved games with
/// ("letter_copy_destination_source", "letter_copy_confirm_source", "letter_copy_confirm_destination",
/// "letter_clear_confirm_source") and the arrow of each copy ("arrow_" and the two saved games' numbers, as
/// "arrow_1_2"); under "arrows" the timeline of each copy's arrow by the same numbers; how wide a digit is
/// ("glyph_width"), how many digits along the colon of a time stands ("colon_column"), the most a part of a time
/// shows ("largest_time_part"), the energy a tank holds ("energy_per_tank"), a tank's "tank_size", the column and
/// row of the first from the word energy ("tank_column", "tank_row") and how many make a row ("tanks_per_row");
/// the updates it stays black, nothing moving, as it is come to before it fades in ("entry_black_updates"); the
/// levels of brightness its fades go through ("fade_levels") and the updates each level lasts ("fade_updates_per_level");
/// the sounds of the cursor moving, of a saved game chosen and of an answer given ("move_sound", "select_sound",
/// "confirm_sound"); and the timelines "cursor", "helmet_away", "helmet_turn" (which ends by staying on its last
/// picture) and the arrows'.
/// </remarks>
public partial class FileSelectMode : GameMode
{
    /// <summary>The data file of what it shows.</summary>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "menus/file_select.json";

    /// <summary>How many saved games its pages are drawn for.</summary>
    private const int SavedGames = 3;

    /// <summary>The rows of the main page under the saved games, and the row under them on the other pages.</summary>
    private const int CopyRow = SavedGames, ClearRow = SavedGames + 1, ExitRow = SavedGames + 2, DataExitRow = SavedGames;

    /// <summary>What the screen is asking.</summary>
    private enum Asking
    {
        /// <summary>Which saved game to play, or what else to do.</summary>
        Main,

        /// <summary>Nothing: the chosen saved game's helmet is turning.</summary>
        HelmetTurns,

        /// <summary>Which saved game to copy.</summary>
        CopyFrom,

        /// <summary>Which saved game to copy it over.</summary>
        CopyTo,

        /// <summary>Whether to copy.</summary>
        CopyConfirm,

        /// <summary>Nothing: the copy is done.</summary>
        CopyDone,

        /// <summary>Which saved game to clear.</summary>
        ClearWhich,

        /// <summary>Whether to clear it.</summary>
        ClearConfirm,

        /// <summary>Nothing: it is cleared.</summary>
        ClearDone,
    }

    private readonly List<CinematicSprite> _helmets = [];
    private DataNode? _data;
    private TimelineSet? _timelines;
    private Sprite2D? _page;
    private Sprite2D? _border;
    private Node2D? _fields;
    private CinematicSprite? _cursor;
    private CinematicSprite? _arrow;
    private FrameTimeline? _arrowPlaying;
    private int _arrowUpdates;
    private int _arrowTurns;
    private ScreenFade? _fade;
    private Func<GameMode?>? _afterFade;
    private Asking _asking;
    private int _row;
    private int _dataRow;
    private int _from;
    private int _to;
    private bool _yes = true;
    private int _turnLeft;
    private int _blackLeft;
    private int _levels;
    private int _updatesPerLevel;
    private int _lit;
    private int _fadeUpdates;
    private bool _brightening;
    private ClosingSoundWork _closing = ClosingSoundWork.Usual;

    /// <inheritdoc/>
    protected override void Enter()
    {
        if (Session.Saves.Slots != SavedGames)
            throw new InvalidOperationException($"The choice of saved game is drawn for {SavedGames} saved games, and the game keeps {Session.Saves.Slots}.");
        _data = Session.Content.ReadData(DataFile);
        _timelines = TimelineSet.ReadFrom(_data);
        var layer = new CanvasLayer { Name = "Screen" };
        AddChild(layer);
        var backdrop = new ColorRect { Name = "Backdrop", Color = new Color(_data.Text("backdrop")) };
        backdrop.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        layer.AddChild(backdrop);
        layer.AddChild(Picture("Background", _data.Text("background"), PixelPosition.Origin));
        _page = Picture("Page", _data.Get("pages").Text("main_empty"), PixelPosition.Origin);
        layer.AddChild(_page);
        _fields = new Node2D { Name = "Fields" };
        layer.AddChild(_fields);
        _border = new Sprite2D { Name = "Border", Centered = false, TextureFilter = CanvasItem.TextureFilterEnum.Nearest };
        layer.AddChild(_border);
        for (int game = 0; game < SavedGames; game++)
        {
            CinematicSprite helmet = CinematicSprite.Of(Session.Content, _timelines.Get("helmet_away"), At(_data.List("main_slots")[game].Get("helmet")));
            helmet.Name = $"Helmet{game + 1}";
            _helmets.Add(helmet);
            layer.AddChild(helmet);
        }

        _arrow = CinematicSprite.Of(Session.Content, _timelines.Get(_data.Get("arrows").Text("1_2")), Place("arrow_1_2"));
        _arrow.Name = "Arrow";
        _arrow.Visible = false;
        layer.AddChild(_arrow);
        _cursor = CinematicSprite.Of(Session.Content, _timelines.Get("cursor"), PixelPosition.Origin);
        _cursor.Name = "Cursor";
        layer.AddChild(_cursor);
        _fade = new ScreenFade { Name = "Fade", Layer = 20 };
        AddChild(_fade);
        _row = Math.Clamp(Session.Slot, 1, SavedGames) - 1;
        ShowMain();
        (_levels, _updatesPerLevel, _lit) = (_data.Int("fade_levels"), _data.Int("fade_updates_per_level"), 0);
        _fade.Cover(_levels, _levels);
        BeginFade(brightening: true);
        _blackLeft = _data.Int("entry_black_updates");
    }

    /// <inheritdoc/>
    public override bool GivesMusicItsTurn => !EntryWaitsOutDisplayFrame;

    /// <inheritdoc/>
    public override ClosingSoundWork SoundWorkAtClose => _closing;

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        // Every update of the old choice of saved game draws a random number first and gives the sounds their turns,
        // but for one that finishes a wait for a frame of the display.
        if (!EntryWaitsOutDisplayFrame)
            Session.OpenUpdate();
        _closing = EntryWaitsOutDisplayFrame ? ClosingSoundWork.None : ClosingSoundWork.Usual;

        // As the old game's screen is made, it stays black a few updates, nothing moving, before it fades in.
        if (_blackLeft > 0)
        {
            _blackLeft--;
            return this;
        }

        (_cursor ?? throw NotShown()).Step();
        foreach (CinematicSprite shown in _helmets)
            shown.Step();
        if (_afterFade is { } then)
        {
            StepFade();
            if (!FadeDone)
                return this;
            _afterFade = null;
            if (then() is { } next)
                return next;
            BeginFade(brightening: true);
            return this;
        }

        if (!FadeDone)
        {
            StepFade();
            return this;
        }

        switch (_asking)
        {
            case Asking.Main:
                StepMain(input);
                break;
            case Asking.HelmetTurns:
                // The turn ends once its last picture has shown its while; confirming again cuts it short. The helmet
                // keeps the picture it has through the fade.
                if (--_turnLeft <= 0 || input.WasPressed(GameAction.Confirm))
                    FadeOutTo(static () => new OptionsMode());
                break;
            case Asking.CopyFrom:
            case Asking.ClearWhich:
                StepWhich(input);
                break;
            case Asking.CopyTo:
                StepCopyTo(input);
                break;
            case Asking.CopyConfirm:
                (_arrow ?? throw NotShown()).Step();
                _arrowUpdates++;
                StepConfirm(input);
                break;
            case Asking.ClearConfirm:
                StepConfirm(input);
                break;
            case Asking.CopyDone:
            case Asking.ClearDone:
                // Any button newly pressed leaves the page.
                if (input.AnyPressed)
                {
                    MoveSound();
                    _row = Math.Clamp(Session.Slot, 1, SavedGames) - 1;
                    FadeOutTo(BackToMain);
                }

                break;
            default:
                throw new InvalidOperationException($"The choice of saved game has no question {_asking}.");
        }

        return this;
    }

    /// <summary>
    /// Whether the update finishes a wait for a frame of the display: in the old game the second of the black updates
    /// (after the screen is blanked) and the last (after it is set up).
    /// </summary>
    private bool EntryWaitsOutDisplayFrame => _blackLeft == 1 || _blackLeft == ScreenData().Int("entry_black_updates") - 1;

    /// <summary>Moves on the first page: the cursor up and down its rows, and what confirming and cancelling do.</summary>
    private void StepMain(GameInput input)
    {
        // With nothing saved there is nothing to copy or clear, and the cursor passes those rows by.
        int[] rows = AnySaved ? [0, 1, 2, CopyRow, ClearRow, ExitRow] : [0, 1, 2, ExitRow];
        if (input.WasPressed(GameAction.Up) || input.WasPressed(GameAction.Down))
        {
            int at = Array.IndexOf(rows, _row);
            int step = input.WasPressed(GameAction.Up) ? rows.Length - 1 : 1;
            _row = at < 0 ? ExitRow : rows[(at + step) % rows.Length];
            MoveSound();
            PlaceCursor();
        }

        if (input.WasPressed(GameAction.Cancel))
        {
            // The old game asks for the cursor's sound twice as cancel leaves.
            MoveSound();
            MoveSound();
            FadeOutTo(static () => new TitleMode());
            return;
        }

        if (!input.WasPressed(GameAction.Confirm))
            return;
        if (_row < SavedGames)
        {
            Session.Slot = _row + 1;
            Sound("select_sound");
            FrameTimeline turn = Timelines().Get("helmet_turn");
            _helmets[_row].Play(turn);
            _turnLeft = turn.Steps.Sum(step => step.Updates);
            _asking = Asking.HelmetTurns;
            return;
        }

        MoveSound();
        switch (_row)
        {
            case CopyRow:
                FadeOutTo(() => BeginData(Asking.CopyFrom));
                break;
            case ClearRow:
                FadeOutTo(() => BeginData(Asking.ClearWhich));
                break;
            default:
                FadeOutTo(static () => new TitleMode());
                break;
        }
    }

    /// <summary>Moves on a page that asks which saved game: only ones that hold something can be pointed at, and the way out.</summary>
    private void StepWhich(GameInput input)
    {
        bool up = input.WasPressed(GameAction.Up);
        if (up || input.WasPressed(GameAction.Down))
        {
            // Up wins when both are pressed.
            for (int row = _dataRow + (up ? -1 : 1); row >= 0 && row <= DataExitRow; row += up ? -1 : 1)
            {
                if (row != DataExitRow && !Session.Saves.Has(row + 1))
                    continue;
                _dataRow = row;
                MoveSound();
                PlaceCursor();
                break;
            }
        }

        bool confirmed = input.WasPressed(GameAction.Confirm);
        if (input.WasPressed(GameAction.Cancel) || (confirmed && _dataRow == DataExitRow))
        {
            MoveSound();
            FadeOutTo(BackToMain);
            return;
        }

        if (!confirmed)
            return;
        MoveSound();
        _from = _dataRow;
        if (_asking == Asking.CopyFrom)
        {
            _dataRow = CopyTargets[0];
            Show(Asking.CopyTo);
        }
        else
        {
            _yes = true;
            Show(Asking.ClearConfirm);
        }
    }

    /// <summary>The rows the cursor can point at when it asks what to copy over: the other two saved games, and the way out.</summary>
    private int[] CopyTargets => [.. Enumerable.Range(0, SavedGames).Where(game => game != _from), DataExitRow];

    /// <summary>Moves on the page that asks which saved game to copy over.</summary>
    private void StepCopyTo(GameInput input)
    {
        int[] rows = CopyTargets;
        int at = Array.IndexOf(rows, _dataRow);
        // The cursor stops at the first row and at the last.
        int next = input.WasPressed(GameAction.Up) ? Math.Max(0, at - 1) : input.WasPressed(GameAction.Down) ? Math.Min(rows.Length - 1, at + 1) : at;
        if (next != at)
        {
            _dataRow = rows[next];
            MoveSound();
            PlaceCursor();
        }

        if (input.WasPressed(GameAction.Cancel))
        {
            MoveSound();
            _dataRow = _from;
            Show(Asking.CopyFrom);
            return;
        }

        if (!input.WasPressed(GameAction.Confirm))
            return;
        MoveSound();
        if (_dataRow == DataExitRow)
        {
            FadeOutTo(BackToMain);
            return;
        }

        _to = _dataRow;
        _yes = true;
        Show(Asking.CopyConfirm);
    }

    /// <summary>Moves on a page that asks whether: up and down change the answer, cancelling is no.</summary>
    private void StepConfirm(GameInput input)
    {
        if (input.WasPressed(GameAction.Up) || input.WasPressed(GameAction.Down))
        {
            _yes = !_yes;
            MoveSound();
            PlaceCursor();
            return;
        }

        bool declined;
        if (input.WasPressed(GameAction.Cancel))
        {
            MoveSound();
            declined = true;
        }
        else if (input.WasPressed(GameAction.Confirm))
        {
            Sound("confirm_sound");
            declined = !_yes;
        }
        else
        {
            return;
        }

        bool copying = _asking == Asking.CopyConfirm;
        if (declined)
        {
            _dataRow = copying ? _to : _from;
            Show(copying ? Asking.CopyTo : Asking.ClearWhich);
            return;
        }

        if (copying)
            Session.Saves.Copy(_from + 1, _to + 1);
        else
            Session.Saves.Clear(_from + 1);
        Show(copying ? Asking.CopyDone : Asking.ClearDone);
    }

    /// <summary>Whether any saved game holds anything.</summary>
    private bool AnySaved => Enumerable.Range(1, SavedGames).Any(Session.Saves.Has);

    /// <summary>Fades the screen out, and then does <paramref name="then"/>: the mode to go to, or none to stay and fade back in.</summary>
    private void FadeOutTo(Func<GameMode?> then)
    {
        _afterFade = then;
        BeginFade(brightening: false);
    }

    /// <summary>Starts the screen brightening to full, or darkening to black, from the level it is at.</summary>
    private void BeginFade(bool brightening) => (_brightening, _fadeUpdates) = (brightening, 0);

    /// <summary>Whether the fade begun last has reached full brightness, or black.</summary>
    private bool FadeDone => _lit == (_brightening ? _levels : 0);

    /// <summary>Plays one update of the fade: as in the old game, it moves a level every so many updates, the first on the last of them.</summary>
    private void StepFade()
    {
        if (++_fadeUpdates % _updatesPerLevel != 0)
            return;
        _lit += _brightening ? 1 : -1;
        (_fade ?? throw NotShown()).Cover(_levels - _lit, _levels);
    }

    /// <summary>Shows the first page, and stays.</summary>
    private GameMode? BackToMain()
    {
        ShowMain();
        return null;
    }

    /// <summary>Shows the first page, with every helmet looking away.</summary>
    private void ShowMain()
    {
        foreach (CinematicSprite helmet in _helmets)
            helmet.Play(Timelines().Get("helmet_away"));
        if (!AnySaved && _row is CopyRow or ClearRow)
            _row = ExitRow;
        Show(Asking.Main);
    }

    /// <summary>Shows the first page of copying or of clearing, pointing at the first saved game that holds something, and stays.</summary>
    private GameMode? BeginData(Asking asking)
    {
        _dataRow = Enumerable.Range(0, SavedGames).Cast<int?>().FirstOrDefault(game => Session.Saves.Has(game!.Value + 1)) ?? DataExitRow;
        _from = 0;
        _yes = true;
        Show(asking);
        return null;
    }

    /// <summary>Shows the page of <paramref name="asking"/>: its picture, its heading, what each saved game holds, and the letters it names saved games with.</summary>
    private void Show(Asking asking)
    {
        if (_asking == Asking.CopyConfirm)
            CountArrowTurns();
        _asking = asking;
        DataNode data = ScreenData();
        bool main = asking is Asking.Main or Asking.HelmetTurns;
        (string page, string border) = asking switch
        {
            Asking.Main or Asking.HelmetTurns => (AnySaved ? "main_with_data" : "main_empty", "main"),
            Asking.CopyFrom => ("copy_source", "copy"),
            Asking.CopyTo => ("copy_destination", "copy"),
            Asking.CopyConfirm => ("copy_confirm", "copy"),
            Asking.CopyDone => ("copy_completed", "copy"),
            Asking.ClearWhich => ("clear_selection", "clear"),
            Asking.ClearConfirm => ("clear_confirm", "clear"),
            Asking.ClearDone => ("clear_completed", "clear"),
            _ => throw new InvalidOperationException($"The choice of saved game has no question {asking}."),
        };
        (_page ?? throw NotShown()).Texture = Session.Content.LoadTexture(data.Get("pages").Text(page));
        DataNode heading = data.Get("borders").Get(border);
        Sprite2D shown = _border ?? throw NotShown();
        shown.Texture = Session.Content.LoadTexture(heading.Text("picture"));
        shown.Position = new Vector2(heading.Int("x"), heading.Int("y"));

        Node2D fields = _fields ?? throw NotShown();
        foreach (Node old in fields.GetChildren())
        {
            fields.RemoveChild(old);
            old.QueueFree();
        }

        IReadOnlyList<DataNode> slots = data.List(main ? "main_slots" : "data_slots");
        for (int game = 0; game < SavedGames; game++)
            ShowSavedGame(fields, slots[game], game + 1);
        IReadOnlyList<DataNode> letters = data.List("letters");
        switch (asking)
        {
            case Asking.CopyTo:
                fields.AddChild(Picture("From", letters[_from].AsText(), Place("letter_copy_destination_source")));
                break;
            case Asking.CopyConfirm or Asking.CopyDone:
                fields.AddChild(Picture("From", letters[_from].AsText(), Place("letter_copy_confirm_source")));
                fields.AddChild(Picture("To", letters[_to].AsText(), Place("letter_copy_confirm_destination")));
                break;
            case Asking.ClearConfirm or Asking.ClearDone:
                fields.AddChild(Picture("From", letters[_from].AsText(), Place("letter_clear_confirm_source")));
                break;
        }

        foreach (CinematicSprite helmet in _helmets)
            helmet.Visible = main;
        CinematicSprite arrow = _arrow ?? throw NotShown();
        arrow.Visible = asking == Asking.CopyConfirm;
        if (arrow.Visible)
        {
            string route = $"{_from + 1}_{_to + 1}";
            _arrowPlaying = ArrowCarryingOn(Timelines().Get(data.Get("arrows").Text(route)));
            arrow.Play(_arrowPlaying);
            arrow.Place = Place("arrow_" + route);
            arrow.Step();
            _arrowUpdates = 1;
        }

        (_cursor ?? throw NotShown()).Visible = asking is not (Asking.CopyDone or Asking.ClearDone);
        PlaceCursor();
    }

    /// <summary>Lays over the page what the saved game numbered <paramref name="slot"/> holds: its energy, its tanks and its play time, or that it holds nothing.</summary>
    private void ShowSavedGame(Node2D fields, DataNode layout, int slot)
    {
        DataNode data = ScreenData();
        if (!Session.Saves.Has(slot))
        {
            fields.AddChild(Picture($"NoData{slot}", data.Text("no_data"), At(layout.Get("no_data"))));
            return;
        }

        SaveData save = Session.Saves.Read(slot);
        IReadOnlyList<DataNode> digits = data.List("digits");
        int glyph = data.Int("glyph_width");
        void Digits(string name, int number, PixelPosition at)
        {
            fields.AddChild(Picture($"{name}Tens{slot}", digits[number / 10].AsText(), at));
            fields.AddChild(Picture($"{name}Ones{slot}", digits[number % 10].AsText(), at + PixelOffset.FromWhole(glyph, 0)));
        }

        PixelPosition energyAt = At(layout.Get("energy"));
        fields.AddChild(Picture($"Energy{slot}", data.Text("energy"), energyAt));
        int perTank = data.Int("energy_per_tank");
        int energy = save.Amounts.GetValueOrDefault(Data.Amounts.Energy);
        Digits("Energy", energy % perTank, At(layout.Get("health")));

        // Tanks fill rows from the left: the first row under the word, each later row a tank above the last.
        int tanks = save.Capacities.GetValueOrDefault(Data.Amounts.Energy) / perTank;
        int size = data.Int("tank_size"), perRow = data.Int("tanks_per_row");
        for (int tank = 0; tank < tanks; tank++)
        {
            int column = data.Int("tank_column") + tank % perRow, row = data.Int("tank_row") - tank / perRow;
            fields.AddChild(Picture(
                $"Tank{slot}_{tank}", data.Text(tank < energy / perTank ? "tank_filled" : "tank_empty"), energyAt + PixelOffset.FromWhole(column * size, row * size)));
        }

        long minutes = save.PlayUpdates / 3600;
        int most = data.Int("largest_time_part"), colon = data.Int("colon_column");
        PixelPosition timeAt = At(layout.Get("time"));
        Digits("Hours", (int)Math.Min(minutes / 60, most), timeAt);
        fields.AddChild(Picture($"Colon{slot}", data.Text("time_colon"), timeAt + PixelOffset.FromWhole(colon * glyph, 0)));
        Digits("Minutes", (int)(minutes % 60), timeAt + PixelOffset.FromWhole((colon + 1) * glyph, 0));
    }

    /// <summary>Puts the cursor beside the row it points at.</summary>
    private void PlaceCursor()
    {
        DataNode data = ScreenData();
        CinematicSprite cursor = _cursor ?? throw NotShown();
        cursor.Place = _asking switch
        {
            Asking.Main or Asking.HelmetTurns => _row switch
            {
                < SavedGames => At(data.List("main_slots")[_row].Get("cursor")),
                CopyRow => Place("cursor_copy"),
                ClearRow => Place("cursor_clear"),
                _ => Place("cursor_exit"),
            },
            Asking.CopyConfirm or Asking.ClearConfirm => Place(_yes ? "cursor_yes" : "cursor_no"),
            _ => _dataRow < SavedGames ? At(data.List("data_slots")[_dataRow].Get("cursor")) : Place("cursor_data_exit"),
        };
        cursor.ShowPlace();
    }

    /// <summary>
    /// The copy arrow's <paramref name="timeline"/>, begun on the colours the arrows last stopped on: as on the
    /// cartridge, each copy asked holds them its first while and then turns them on from there, not from the first.
    /// </summary>
    /// <remarks>
    /// The timeline's first step holds the first colours, and the steps it loops through turn them once each; the
    /// arrows of every copy turn through the same colours.
    /// </remarks>
    private FrameTimeline ArrowCarryingOn(FrameTimeline timeline)
    {
        int loop = timeline.LoopsTo ?? throw new InvalidOperationException("The copy arrow's timeline turns its colours round a loop.");
        TimelineStep[] turns = [.. timeline.Steps.Skip(loop)];
        TimelineStep shown = _arrowTurns == 0 ? timeline.Steps[0] : turns[(_arrowTurns - 1) % turns.Length];
        TimelineStep[] steps =
        [
            shown with { Updates = timeline.Steps[0].Updates },
            .. Enumerable.Range(0, turns.Length).Select(turn => turns[(_arrowTurns + turn) % turns.Length]),
        ];
        return new FrameTimeline(steps, 1);
    }

    /// <summary>Counts the turns the arrow's colours took while the copy was asked, to carry them on to the next.</summary>
    private void CountArrowTurns()
    {
        FrameTimeline playing = _arrowPlaying ?? throw NotShown();
        int step = 0;
        // A step begins on the update after the one before it has shown its while; the first on the update the arrow shows.
        for (int begins = playing.Steps[0].Updates + 1; begins <= _arrowUpdates; begins += playing.Steps[step].Updates)
        {
            step = step + 1 < playing.Steps.Count ? step + 1 : playing.LoopsTo!.Value;
            _arrowTurns++;
        }
    }

    /// <summary>A picture drawn from its corner at <paramref name="at"/>.</summary>
    private Sprite2D Picture(string name, string file, PixelPosition at) => new()
    {
        Name = name,
        Centered = false,
        Texture = Session.Content.LoadTexture(file),
        TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
        Position = new Vector2(at.X.Whole, at.Y.Whole),
    };

    private void MoveSound() => Sound("move_sound");

    private void Sound(string name) => Session.Sounds.Request(ScreenData().Text(name), channel: "menu");

    private PixelPosition Place(string name) => At(ScreenData().Get("places").Get(name));

    private static PixelPosition At(DataNode at) => PixelPosition.FromWhole(at.Int("x"), at.Int("y"));

    private DataNode ScreenData() => _data ?? throw NotShown();

    private TimelineSet Timelines() => _timelines ?? throw NotShown();

    private static InvalidOperationException NotShown() => new("The choice of saved game has not been shown.");
}
