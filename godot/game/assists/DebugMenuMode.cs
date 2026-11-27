using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.Ui;
using Game.Data;
using Godot;

namespace Game.Assists;

/// <summary>
/// The tester's menu, laid over the pause screen, which item select opens: a row for each of the tester's settings
/// (<see cref="AssistSettings"/>) and a row to leave by, on the options' background with the options' cursor and
/// sounds, in the game's font. Up and down move between the rows; left, right and confirm change the row's setting;
/// cancel, pause or item select leaves, as confirm does on the last row. Leaving keeps the settings, and gives Samus
/// the full inventory at once when that was switched on here. It is the Godot game's own: nothing of it is the
/// cartridge's.
/// </summary>
public partial class DebugMenuMode : GameMode
{
    /// <summary>One row: its name, what it now shows, and what a step one way or the other does to it.</summary>
    private sealed record Row(string Name, Func<AssistSettings, string> Shows, Action<AssistSettings, int> Changes);

    private const int HeadingX = 56, HeadingY = 16;
    private const int NameX = 56, ValueX = 184, FirstRowY = 40, RowStep = 16;

    /// <summary>Where the cursor lies beside a row: as far left of the row's value, and as far below its top, as the options' page of buttons has it.</summary>
    private const int CursorX = 40, CursorBelow = 8;

    /// <summary>How many minutes one step of the ending's time is, and the most it can be set to.</summary>
    private const int EndingStep = 60, EndingMost = 99 * 60;

    /// <summary>The data file of the options, whose background, cursor and sounds this screen uses.</summary>
    private const string OptionsFile = "menus/options.json";

    private static readonly Row[] Rows =
    [
        Toggle("SKIP OPENING", settings => settings.SkipOpening, (settings, on) => settings.SkipOpening = on),
        Toggle("INVINCIBILITY", settings => settings.Invincible, (settings, on) => settings.Invincible = on),
        Toggle("INFINITE AMMO", settings => settings.AmmoNeverRunsOut, (settings, on) => settings.AmmoNeverRunsOut = on),
        Toggle("ALL EQUIPMENT", settings => settings.GrantsAllEquipment, (settings, on) => settings.GrantsAllEquipment = on),
        Toggle("UNLOCK TOURIAN", settings => settings.TourianOpen, (settings, on) => settings.TourianOpen = on),
        Toggle("RESET BOSSES", settings => settings.ResetsBossesOnLoad, (settings, on) => settings.ResetsBossesOnLoad = on),
        Toggle("HOLD COUNTDOWN", settings => settings.CountdownHolds, (settings, on) => settings.CountdownHolds = on),
        new("ENDING TIME", settings => settings.EndingMinutes is { } minutes ? $"{minutes / 60}.{minutes % 60:00}" : "OFF", StepEndingTime),
        new("REVEAL MAP",
            settings => settings.MapReveal switch { MapReveal.AsPlayed => "OFF", MapReveal.Charted => "PUBLIC", _ => "SECRET" },
            (settings, by) => settings.MapReveal = (MapReveal)(((int)settings.MapReveal + by + 3) % 3)),
    ];

    private readonly GameMode _over;
    private readonly List<ChoiceList> _values = [];
    private DataNode? _data;
    private CinematicSprite? _cursor;
    private int _row;
    private bool _grantedAtEntry;

    /// <summary>The tester's menu, laid over <paramref name="over"/>, which it hands back to.</summary>
    public DebugMenuMode(GameMode over)
    {
        ArgumentNullException.ThrowIfNull(over);
        _over = over;
    }

    /// <inheritdoc/>
    public override GameMode? Keeps => _over;

    /// <summary>A row that is on or off: any step changes it to the other.</summary>
    private static Row Toggle(string name, Func<AssistSettings, bool> isOn, Action<AssistSettings, bool> set) =>
        new(name, settings => isOn(settings) ? "ON" : "OFF", (settings, _) => set(settings, !isOn(settings)));

    /// <summary>The ending's time one step on or back: off, then from nothing up by the hour, and round again.</summary>
    private static void StepEndingTime(AssistSettings settings, int by)
    {
        int steps = EndingMost / EndingStep + 2;
        int at = settings.EndingMinutes is { } minutes ? minutes / EndingStep + 1 : 0;
        at = (at + by + steps) % steps;
        settings.EndingMinutes = at == 0 ? null : (at - 1) * EndingStep;
    }

    /// <inheritdoc/>
    protected override void Enter()
    {
        _data = Session.Content.ReadData(OptionsFile);
        _grantedAtEntry = TesterAssists.Of(Session).Settings.GrantsAllEquipment;

        // Over the pause screen and its fade, and covering both.
        var layer = new CanvasLayer { Name = "Screen", Layer = 30 };
        AddChild(layer);
        var backdrop = new ColorRect { Name = "Backdrop", Color = new Color(_data.Text("backdrop")) };
        backdrop.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        layer.AddChild(backdrop);
        layer.AddChild(new Sprite2D
        {
            Name = "Background", Centered = false, Texture = Session.Content.LoadTexture(_data.Text("background")),
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
        });
        layer.AddChild(Line("Heading", "DEBUG", HeadingX, HeadingY));
        for (int row = 0; row <= Rows.Length; row++)
        {
            int y = FirstRowY + row * RowStep;
            layer.AddChild(Line($"Name{row + 1}", row < Rows.Length ? Rows[row].Name : "EXIT", NameX, y));
            if (row == Rows.Length)
                continue;
            ChoiceList value = Line($"Value{row + 1}", "", ValueX, y);
            layer.AddChild(value);
            _values.Add(value);
        }

        _cursor = CinematicSprite.Of(Session.Content, TimelineSet.ReadFrom(_data).Get("cursor"), PixelPosition.Origin);
        _cursor.Name = "Cursor";
        layer.AddChild(_cursor);
        ShowValues();
        PlaceCursor();
    }

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        // What is held is noted for Samus, who plays none of these updates, as on the pause screen.
        Session.Player.NoteHeld(input);
        (_cursor ?? throw NotShown()).Step();
        if (input.WasPressed(GameAction.Cancel) || input.WasPressed(GameAction.Pause) || input.WasPressed(Actions.ItemSelect))
            return Leave();

        // Up wins when both are pressed, as in the options.
        int rows = Rows.Length + 1;
        int move = input.WasPressed(GameAction.Up) ? rows - 1 : input.WasPressed(GameAction.Down) ? 1 : 0;
        if (move != 0)
        {
            _row = (_row + move) % rows;
            Sound("move_sound");
            PlaceCursor();
            return this;
        }

        bool confirm = input.WasPressed(GameAction.Confirm);
        int by = confirm || input.WasPressed(GameAction.Right) ? 1 : input.WasPressed(GameAction.Left) ? -1 : 0;
        if (by == 0)
            return this;
        if (_row == Rows.Length)
            return confirm ? Leave() : this;
        Rows[_row].Changes(TesterAssists.Of(Session).Settings, by);
        Sound("select_sound");
        ShowValues();
        return this;
    }

    /// <summary>Keeps the settings, gives the full inventory if it was switched on here, and hands back to what the menu was laid over.</summary>
    private GameMode Leave()
    {
        TesterAssists assists = TesterAssists.Of(Session);
        assists.Save();
        if (assists.Settings.GrantsAllEquipment && !_grantedAtEntry)
            TesterAssists.GrantEquipment(Session);
        Sound("select_sound");
        return _over;
    }

    /// <summary>Shows each row's setting as it now stands.</summary>
    private void ShowValues()
    {
        AssistSettings settings = TesterAssists.Of(Session).Settings;
        for (int row = 0; row < Rows.Length; row++)
            _values[row].Present([InFont(Rows[row].Shows(settings))]);
    }

    /// <summary>Puts the cursor beside the row it points at.</summary>
    private void PlaceCursor()
    {
        CinematicSprite cursor = _cursor ?? throw NotShown();
        cursor.Place = PixelPosition.FromWhole(CursorX, FirstRowY + _row * RowStep + CursorBelow);
        cursor.ShowPlace();
    }

    /// <summary>One line in the game's font, with its corner at a place on the screen.</summary>
    private ChoiceList Line(string name, string words, int x, int y)
    {
        var line = new ChoiceList { Name = name, Font = Session.Font, Mark = "", Position = new Vector2(x, y) };
        if (words.Length > 0)
            line.Present([InFont(words)]);
        return line;
    }

    /// <summary>Words as the game's font can show them: in capitals, with a space for each character the font has not.</summary>
    private string InFont(string words)
    {
        PixelFont font = Session.Font ?? throw new InvalidOperationException("The game has no font to show the tester's menu in.");
        return string.Concat(words.ToUpperInvariant().Select(character => font.Has(character) ? character : ' '));
    }

    private void Sound(string name) => Session.Sounds.Request((_data ?? throw NotShown()).Text(name), channel: "menu");

    private static InvalidOperationException NotShown() => new("The tester's menu has not been shown.");
}
