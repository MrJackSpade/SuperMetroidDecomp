using Engine.Actors;
using Engine.Content;
using Engine.Core;
using Engine.Ui;
using Engine.World;
using Game.Data;
using Godot;

namespace Game;

/// <summary>
/// The strip across the top of the screen during play: Samus's energy tanks and energy, her missiles, super missiles
/// and power bombs, which of her selectable things is selected, whether her reserves refill by themselves, the map
/// around where she is, and, during an escape, the countdown. It shows; it changes none of them.
/// </summary>
/// <remarks>
/// Its data names its pictures and places. "template" is its background. "digits" and "ammo_digits" are ten pictures
/// each, nought to nine. "tank_full" and "tank_empty" are an energy tank, and "tank_places" where each of the
/// fourteen goes. "energy_at" is where the two digits of her energy go. "counters" gives, for "missiles",
/// "super_missiles" and "power_bombs", where its digits go ("x", "y") and how many "digits". "icons" gives, for each
/// of those three, "grapple_beam" and "xray_scope", where it goes and its "picture" and "selected_picture".
/// "auto_reserve" gives where the reserves' sign goes and its "full" and "empty" pictures, and "cleared", shown
/// when she has reserves that do not refill by themselves. "minimap_at" is the top-left of the map, whose areas'
/// files are in "area_folder"; "minimap_backing" is the picture of a cell of it that shows nothing, which lies under
/// every cell, and is all that shows of the map in a room whose map is hidden. "timer" gives the countdown: its "label" and the ten "digits", and where each lies from the
/// countdown's place ("label_at", "minutes_at", "seconds_at", "hundredths_at": the first of each pair of digits,
/// the second "digit_spacing" to its right). Its child "Canvas" draws them; its child "Minimap" is the map.
/// </remarks>
public partial class Hud : GameHud, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>The sound of the selection changing, by name.</summary>
    [Export, DataValue("select_sound")]
    public string SelectSound { get; set; } = "";

    /// <summary>The folder, within the assets tree, of the files of the areas' maps: one a file, named for its area.</summary>
    [Export, DataValue("area_folder")]
    public string AreaFolder { get; set; } = "areas";

    private sealed record Counter(string Amount, Vector2I At, int Digits);

    private sealed record Icon(string Selects, Vector2I At, string Picture, string SelectedPicture);

    /// <summary>How much energy a tank holds, how many tanks are shown, and how wide a digit is.</summary>
    private const int EnergyPerTank = 100, MostTanks = 14, DigitWidth = 8;

    /// <summary>How many cells of the map are shown, across and down, and how many updates its mark of where Samus is shows and hides for.</summary>
    private static readonly Vector2I MinimapCells = new(5, 3);

    private const int BlinkUpdates = 8;

    private readonly List<string> _digits = [];
    private readonly List<string> _ammoDigits = [];
    private readonly List<Vector2I> _tankPlaces = [];
    private readonly List<Counter> _counters = [];
    private readonly List<Icon> _icons = [];
    private readonly List<string> _timerDigits = [];
    private readonly Dictionary<string, Area> _areas = new(StringComparer.Ordinal);
    private string _template = "";

    /// <summary>How tall the HUD's strip at the top of the screen is, in pixels: its background's height.</summary>
    public int StripHeight => _template.Length == 0 ? 0 : _content?.LoadTexture(_template).GetHeight() ?? 0;
    private string _tankFull = "";
    private string _tankEmpty = "";
    private Vector2I _energyAt;
    private Vector2I _reserveAt;
    private string _reserveFull = "";
    private string _reserveEmpty = "";
    private string _reserveCleared = "";
    private Vector2I _minimapAt;
    private string _minimapBacking = "";
    private string _timerLabel = "";
    private Vector2I _timerLabelAt, _minutesAt, _secondsAt, _hundredthsAt;
    private int _timerSpacing = DigitWidth;
    private string _lastSelected = "";
    private ContentLibrary? _content;

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
        _template = data.Text("template");
        Fill(_digits, data, "digits");
        Fill(_ammoDigits, data, "ammo_digits");
        _tankFull = data.Text("tank_full");
        _tankEmpty = data.Text("tank_empty");
        _tankPlaces.Clear();
        _tankPlaces.AddRange(data.List("tank_places").Select(Place));
        if (_tankPlaces.Count != MostTanks)
            throw data.Problem($"must place {MostTanks} energy tanks");
        _energyAt = Place(data.Get("energy_at"));
        _counters.Clear();
        foreach ((string amount, DataNode counter) in data.Get("counters").Entries())
            _counters.Add(new Counter(amount, Place(counter), counter.Int("digits")));
        _icons.Clear();
        foreach ((string selects, DataNode icon) in data.Get("icons").Entries())
            _icons.Add(new Icon(selects, Place(icon), icon.Text("picture"), icon.Text("selected_picture")));
        DataNode reserve = data.Get("auto_reserve");
        (_reserveAt, _reserveFull, _reserveEmpty, _reserveCleared) = (Place(reserve), reserve.Text("full"), reserve.Text("empty"), reserve.Text("cleared"));
        (_minimapAt, _minimapBacking) = (Place(data.Get("minimap_at")), data.Text("minimap_backing"));
        DataNode timer = data.Get("timer");
        Fill(_timerDigits, timer, "digits");
        (_timerLabel, _timerLabelAt, _timerSpacing) = (timer.Text("label"), Place(timer.Get("label_at")), timer.Int("digit_spacing"));
        (_minutesAt, _secondsAt, _hundredthsAt) = (Place(timer.Get("minutes_at")), Place(timer.Get("seconds_at")), Place(timer.Get("hundredths_at")));

        static void Fill(List<string> pictures, DataNode from, string name)
        {
            pictures.Clear();
            pictures.AddRange(from.List(name).Select(picture => picture.AsText()));
            if (pictures.Count != 10)
                throw from.Problem($"must list ten pictures as its \"{name}\"");
        }

        static Vector2I Place(DataNode at) => new(at.Int("x"), at.Int("y"));
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <inheritdoc/>
    public override void Present(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        _content = session.Content;
        DataValues.Fill(this, session.Content);
        _lastSelected = "";
        if (GetNodeOrNull<MapView>("Minimap") is { } minimap)
        {
            minimap.Window = MinimapCells;
            minimap.Position = _minimapAt;
        }
    }

    /// <inheritdoc/>
    public override void Refresh(GameSession session, Room room)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(room);
        ContentLibrary content = _content ?? throw new InvalidOperationException("The HUD has not been presented.");
        Player samus = room.Player;
        Inventory inventory = samus.Inventory;
        var pictures = new List<(Texture2D, Vector2I, Color)>();
        var marks = new List<(Rect2I, Color)>();
        void Draw(string picture, Vector2I at) => pictures.Add((content.LoadTexture(picture), at, Colors.White));

        Draw(_template, Vector2I.Zero);

        // Her selectable things: each she has shows, and the one selected shows lit.
        string selected = (samus as Samus)?.Selected ?? "";
        foreach (Icon icon in _icons)
        {
            bool has = Amounts.All.Contains(icon.Selects) ? inventory.Capacity(icon.Selects) > 0 : inventory.Owns(icon.Selects);
            if (has)
                Draw(icon.Selects == selected ? icon.SelectedPicture : icon.Picture, icon.At);
        }

        if (selected != _lastSelected)
        {
            // The change sounds, except while she spins, clings to her beam or leaps from a wall.
            if (_lastSelected.Length + selected.Length > 0 && SelectSound.Length > 0
                && samus.State is not (SpinJumpingState or GrapplingState or WallJumpingState))
            {
                session.Sounds.Request(SelectSound, channel: "hud");
            }

            _lastSelected = selected;
        }

        // Her energy: a tank for each hundred she can hold beyond the first, and what is left over as two digits.
        int energy = inventory.Amount(Inventory.Energy);
        int tanks = Math.Min(inventory.Capacity(Inventory.Energy) / EnergyPerTank, MostTanks);
        for (int tank = 0; tank < tanks; tank++)
            Draw(tank < energy / EnergyPerTank ? _tankFull : _tankEmpty, _tankPlaces[tank]);
        DrawNumber(energy % EnergyPerTank, _energyAt, 2, _digits, DigitWidth);

        foreach (Counter counter in _counters)
        {
            if (inventory.Capacity(counter.Amount) > 0)
                DrawNumber(inventory.Amount(counter.Amount), counter.At, counter.Digits, _ammoDigits, DigitWidth);
        }

        // Reserves that refill by themselves show their sign, full or empty; reserves that do not show its place cleared.
        if (inventory.IsOn(Equipment.ReserveTank))
            Draw(inventory.Amount(Amounts.ReserveEnergy) > 0 ? _reserveFull : _reserveEmpty, _reserveAt);
        else if (inventory.Capacity(Amounts.ReserveEnergy) > 0)
            Draw(_reserveCleared, _reserveAt);

        // Under the map lies a cell that shows nothing, in every place of it.
        Texture2D backing = content.LoadTexture(_minimapBacking);
        for (int row = 0; row < MinimapCells.Y; row++)
        {
            for (int column = 0; column < MinimapCells.X; column++)
                pictures.Add((backing, _minimapAt + new Vector2I(column * backing.GetWidth(), row * backing.GetHeight()), Colors.White));
        }

        ShowMinimap(session, room);

        Escape escape = session.Service<Escape>();
        if (escape.IsShown)
        {
            (int x, int y) = escape.ScreenPlace;
            var at = new Vector2I(x, y);
            Draw(_timerLabel, at + _timerLabelAt);
            DrawNumber(escape.Minutes, at + _minutesAt, 2, _timerDigits, _timerSpacing);
            DrawNumber(escape.Seconds, at + _secondsAt, 2, _timerDigits, _timerSpacing);
            DrawNumber(escape.Hundredths, at + _hundredthsAt, 2, _timerDigits, _timerSpacing);
        }

        GetNode<HudCanvas>("Canvas").Show(pictures, marks);
        ShowWhiteOut(escape.Whitened);

        void DrawNumber(int value, Vector2I at, int digits, List<string> numerals, int width)
        {
            int divisor = 1;
            for (int place = 1; place < digits; place++)
                divisor *= 10;
            for (int digit = 0; digit < digits; digit++)
            {
                Draw(numerals[Math.Max(0, value) / divisor % 10], at + new Vector2I(digit * width, 0));
                divisor /= 10;
            }
        }
    }

    private ScreenFade? _whiteOut;

    /// <summary>
    /// Shows how far time running out has whitened the screen, in parts of <see cref="Escape.WhiteOutParts"/>: all of
    /// it, this included. It is shown here, over play, and not by the time up, because it stays as it stood when
    /// something takes play from beneath the time up.
    /// </summary>
    private void ShowWhiteOut(int parts)
    {
        if (_whiteOut is null)
        {
            if (parts == 0)
                return;
            _whiteOut = new ScreenFade { Name = "WhiteOut", Colour = Colors.White, Layer = Layer + 1 };
            AddChild(_whiteOut);
        }

        _whiteOut.Cover(parts, Escape.WhiteOutParts);
    }

    /// <summary>Shows the map around the cell Samus is in, that cell shown as hers every other while.</summary>
    private void ShowMinimap(GameSession session, Room room)
    {
        if (GetNodeOrNull<MapView>("Minimap") is not { } minimap)
            return;
        // A room whose map is hidden, as a boss's may be, explores none of it, and its every cell shows blank: the
        // backing beneath, which is the cartridge's own picture of a hidden cell.
        minimap.Visible = room.MapShown && room.Area.Length > 0;
        if (!minimap.Visible)
            return;
        minimap.Reveals = Assists.TesterAssists.Of(session).Now(session).MapReveal;
        if (!_areas.TryGetValue(room.Area, out Area? area))
            _areas[room.Area] = area = Area.Read(session.Content.ReadData($"{AreaFolder}/{room.Area}.json"));
        Player samus = room.Player;
        minimap.Centre = new Vector2I(
            room.MapCell.X + samus.Position.X.Whole / room.MapCellSize.X,
            room.MapCell.Y + samus.Position.Y.Whole / room.MapCellSize.Y);

        // In the lower floor piece of a sloped hallway the cell above is explored with hers, as the cartridge has it
        // as it reads the map around her: the hallway's upper corner is no place she can stand in.
        Vector2I here = minimap.Centre;
        if (here.Y > 0 && session.Progress.IsExplored(room.Area, here.X, here.Y)
            && area.Cells.FirstOrDefault(cell => cell.Place == here) is { ExploresAbove: true })
        {
            session.Progress.MarkExplored(room.Area, here.X, here.Y - 1);
        }
        // The mark blinks by the short count, which a door's scroll starts over.
        minimap.Here = session.Clock.ShortUpdates / BlinkUpdates % 2 == 0 ? minimap.Centre : null;
        minimap.Present(area, session.Progress, session.Content);
    }
}
