using Engine.Actors;
using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.Ui;
using Engine.World;
using Game.Data;
using Godot;

namespace Game;

/// <summary>
/// The pause screen, laid over play. It has two pages. The map page shows the map of the area Samus is in, which
/// the directions scroll. The equipment page shows what she owns, each thing lit when it is switched on, with a
/// selector on one of them: up and down move it within its side of the page, left and right take it to the other
/// side, and confirm switches the thing on or off. Her reserves are there too: whether they refill by themselves,
/// and, when they do not, a line on which holding confirm pours them into her energy. The angle buttons turn the
/// pages and pause goes back to play.
/// </summary>
/// <remarks>
/// Its data gives its "backdrop" colour; the picture behind everything for each area ("backdrops"); the pictures
/// of its "frame"; its three "buttons" ("map", "exit", "samus"), each "lit" or "dim"; where the map is and how many
/// cells of it show ("map": "x", "y", "columns", "rows", and "indicator", the timeline of the sign of where Samus
/// is, its pictures placed from the corner of her cell), with the "area_folder" of the areas' maps; its "sounds"
/// ("move", "switch", "page", "pour", and "pour_every", the updates from one pouring sound to the next); and its
/// "equipment" page: the "base" picture, the "hyper" beam's picture, the "controls" (each the "equipment" it
/// switches, the "side" of the page it is on, the "group" it is in ("beams", "suits" or "boots", each a column the
/// selector moves along, in the order given), its picture switched "on" and "off" with where that goes, and its
/// "selector" picture and place), the suit the selector goes to from the beams when it is sent right
/// ("beams_right_enters"), the "wireframes" of her suit by whether she wears the Varia suit and the high
/// jump boots ("plain", "hi_jump", "varia", "varia_hi_jump"), and her "reserve": the pictures shown with none
/// ("absent", "arrow_none"), and with some the "tank", the "arrow", the "bar" (a little tank for each she has:
/// the pictures of a "full" one, of one by how many steps full it is ("fills", from none), and of the "end_cap"
/// after the last; their "places"; the energy a tank holds ("energy_per_tank") and a step is ("energy_per_step");
/// and that a tank fewer than "flicker_below_steps" steps full shows its next step on and off, "flicker_updates"
/// each way), the mode ("auto", "manual"), three "digits"
/// (each a place and ten pictures) and the selectors of its two lines ("mode_selector", "transfer_selector").
/// Every picture is given with where its corner goes.
/// </remarks>
public partial class PauseScreen : GameMode
{
    /// <summary>The data file of what it shows.</summary>
    public string DataFile { get; set; } = "pause/pause.json";

    /// <summary>How much of her reserves pours into her energy each update she holds confirm on that line.</summary>
    public int ReservePourPerUpdate { get; set; } = 1;

    /// <summary>One thing the selector can rest on: a piece of equipment, or one of the two lines of her reserves.</summary>
    private sealed record Line(string Equipment, string Side, string Group, DataNode? Data, DataNode Selector);

    private const string ReserveMode = "reserve_mode", ReserveTransfer = "reserve_transfer";

    private readonly PlayingMode _playing;
    private readonly List<Line> _controls = [];
    private DataNode? _data;
    private CanvasLayer? _layer;
    private Node2D? _page;
    private MapView? _map;
    private CinematicSprite? _indicator;
    private Vector2I _here;
    private int _mapCellSize;
    private bool _showsMap = true;
    private int _chosen;
    private int _pouring;
    private readonly Dictionary<string, List<(string Equipment, int Control)>> _columns = new(StringComparer.Ordinal);
    private bool _beamsClosed;
    private int _updates;
    private int _flickerEvery;

    /// <summary>The pause screen, laid over <paramref name="playing"/>.</summary>
    public PauseScreen(PlayingMode playing)
    {
        ArgumentNullException.ThrowIfNull(playing);
        _playing = playing;
    }

    /// <inheritdoc/>
    public override GameMode? Keeps => _playing;

    private Player Samus => Session.Player;

    /// <inheritdoc/>
    protected override void Enter()
    {
        _data = Session.Content.ReadData(DataFile);
        _layer = new CanvasLayer { Name = "Screen", Layer = 10 };
        AddChild(_layer);
        var backdrop = new ColorRect { Name = "Backdrop", Color = new Color(_data.Text("backdrop")) };
        backdrop.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _layer.AddChild(backdrop);
        Room room = _playing.Room;
        ShowPage(map: room.Area.Length > 0);
    }

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        if (input.WasPressed(GameAction.Pause))
            return _playing;
        if (input.WasPressed(Actions.AngleUp) || input.WasPressed(Actions.AngleDown))
        {
            Sound("page");
            ShowPage(map: !_showsMap && _playing.Room.Area.Length > 0);
            return this;
        }

        if (_showsMap)
        {
            // The directions scroll the map a cell at a time.
            var move = new Vector2I(
                (input.WasPressed(GameAction.Right) ? 1 : 0) - (input.WasPressed(GameAction.Left) ? 1 : 0),
                (input.WasPressed(GameAction.Down) ? 1 : 0) - (input.WasPressed(GameAction.Up) ? 1 : 0));
            if (move != Vector2I.Zero && _map is not null)
            {
                _map.Centre += move;
                _map.QueueRedraw();
                PlaceIndicator();
            }

            _indicator?.Step();
            return this;
        }

        StepEquipment(input);
        return this;
    }

    /// <summary>Plays one update of the equipment page: the selector moves, and confirm switches or pours.</summary>
    private void StepEquipment(GameInput input)
    {
        // A tank of her reserves that is nearly empty flickers, and the page is shown again each time it changes.
        _updates++;
        if (_flickerEvery > 0 && _updates % _flickerEvery == 0)
            ShowEquipment();
        if (_controls.Count == 0)
            return;
        Line chosen = _controls[_chosen];
        if (MoveSelector(input, chosen))
        {
            ShowEquipment();
            return;
        }

        if (chosen.Equipment == ReserveTransfer)
        {
            if (input.IsHeld(GameAction.Confirm))
                PourReserves();
            else
                _pouring = 0;
            return;
        }

        if (!input.WasPressed(GameAction.Confirm))
            return;
        Switch(chosen.Equipment);
        Sound("switch");
        ShowEquipment();
    }

    /// <summary>
    /// Moves the selector as the directions pressed send it, and says whether it moved. It goes up and down its own
    /// column, passing what she has not got; off the end of a column, or sent across, it goes where the page has that
    /// column lead: from the beams up to her reserves and right to the suits or, failing them, the boots; from the
    /// suits down to the boots and left to her reserves or the beams; from the boots up to the suits and left to the
    /// last beam or her reserves; from her reserves down to the beams and right to the suits or the boots.
    /// </summary>
    private bool MoveSelector(GameInput input, Line chosen)
    {
        bool left = input.WasPressed(GameAction.Left), right = input.WasPressed(GameAction.Right);
        bool up = input.WasPressed(GameAction.Up), down = input.WasPressed(GameAction.Down);
        Inventory inventory = Samus.Inventory;
        int before = _chosen;
        if (chosen.Equipment is ReserveMode or ReserveTransfer)
        {
            if (right)
            {
                if (down || !SelectFrom("suits", 0, 1))
                    SelectFrom("boots", 0, 1);
            }
            else if (up)
            {
                if (chosen.Equipment != ReserveMode)
                    Select(_controls.FindIndex(line => line.Equipment == ReserveMode));
            }
            else if (down)
            {
                int transfer = _controls.FindIndex(line => line.Equipment == ReserveTransfer);
                if (chosen.Equipment == ReserveTransfer || transfer < 0)
                {
                    SelectFrom("beams", 0, 1);
                }
                else if (inventory.Amount(Amounts.ReserveEnergy) == 0)
                {
                    // With nothing to pour, the selector only passes the pouring line on its way to the beams.
                    _chosen = transfer;
                    SelectFrom("beams", 0, 1);
                }
                else
                {
                    Select(transfer);
                }
            }

            return _chosen != before;
        }

        string group = chosen.Group;
        int place = _columns[group].FindIndex(entry => entry.Equipment == chosen.Equipment);
        switch (group)
        {
            case "beams":
                if (right)
                {
                    int entry = up ? 0 : _columns["suits"].FindIndex(suit => suit.Equipment == Data().Get("equipment").Text("beams_right_enters"));
                    if (!SelectFrom("suits", entry, 1) && !up)
                        SelectFrom("boots", 0, 1);
                }
                else if (down)
                {
                    SelectFrom(group, place + 1, 1);
                }
                else if (up && !SelectFrom(group, place - 1, -1))
                {
                    SelectReserves();
                }

                break;
            case "suits":
                if (left)
                {
                    if (down || !SelectReserves())
                        SelectFrom("beams", 0, 1);
                }
                else if (up)
                {
                    SelectFrom(group, place - 1, -1);
                }
                else if (down && !SelectFrom(group, place + 1, 1))
                {
                    SelectFrom("boots", 0, 1);
                }

                break;
            case "boots":
                if (left)
                {
                    if (up || !SelectFrom("beams", _columns["beams"].Count - 1, -1))
                        SelectReserves();
                }
                else if (down)
                {
                    SelectFrom(group, place + 1, 1);
                }
                else if (up && !SelectFrom(group, place - 1, -1))
                {
                    SelectFrom("suits", _columns["suits"].Count - 1, -1);
                }

                break;
            default:
                throw new InvalidOperationException($"The pause screen's equipment has no column \"{group}\".");
        }

        return _chosen != before;
    }

    /// <summary>
    /// Puts the selector on the first thing she has in a column, looking from a place in it up or down, and says
    /// whether there was one. The beams are closed while she has the hyper beam. Looking down the suits past one she
    /// has not got never finds the last of them, as the old game's search does not.
    /// </summary>
    private bool SelectFrom(string group, int start, int step)
    {
        List<(string Equipment, int Control)> column = _columns[group];
        if (group == "beams" && _beamsClosed)
            return false;
        for (int place = start; place >= 0 && place < column.Count; place += step)
        {
            if (column[place].Control < 0)
            {
                if (group == "suits" && step > 0 && place + step >= column.Count - 1)
                    return false;
                continue;
            }

            Select(column[place].Control);
            return true;
        }

        return false;
    }

    /// <summary>Puts the selector on the first line of her reserves, if she has any, and says whether she has.</summary>
    private bool SelectReserves()
    {
        if (Samus.Inventory.Capacity(Amounts.ReserveEnergy) == 0)
            return false;
        Select(_controls.FindIndex(line => line.Equipment == ReserveMode));
        return true;
    }

    /// <summary>Puts the selector on a line, with the sound of it moving.</summary>
    private void Select(int control)
    {
        _chosen = control;
        Sound("move");
    }

    /// <summary>Shows one of the two pages.</summary>
    private void ShowPage(bool map)
    {
        DataNode data = Data();
        CanvasLayer layer = _layer ?? throw NotShown();
        _showsMap = map;
        _page?.QueueFree();
        _map = null;
        _indicator = null;
        _page = new Node2D { Name = "Page" };
        layer.AddChild(_page);
        Room room = _playing.Room;
        if (room.Area.Length > 0)
            Show(data.Get("backdrops").Get(room.Area));
        foreach (DataNode picture in data.List("frame"))
            Show(picture);

        // The button of the page that is not showing is lit, and the way out always is.
        foreach ((string name, DataNode button) in data.Get("buttons").Entries())
        {
            bool lit = name == "exit" || name == (map ? "samus" : "map");
            Show(button, button.Text(lit ? "lit" : "dim"));
        }

        if (!map)
        {
            _chosen = 0;
            ShowEquipment();
            return;
        }

        DataNode at = data.Get("map");
        _map = new MapView { Name = "Map", Window = new Vector2I(at.Int("columns"), at.Int("rows")), Position = new Vector2(at.Int("x"), at.Int("y")) };
        _page.AddChild(_map);
        _here = new Vector2I(
            room.MapCell.X + Samus.Position.X.Whole / room.MapCellSize.X,
            room.MapCell.Y + Samus.Position.Y.Whole / room.MapCellSize.Y);
        _map.Centre = _here;
        Area area = Area.Read(Session.Content.ReadData($"{data.Text("area_folder")}/{room.Area}.json"));
        _mapCellSize = area.TileSize;
        _map.Present(area, Session.Progress, Session.Content);
        _indicator = CinematicSprite.Of(Session.Content, FrameTimeline.Read(at.Get("indicator")), PixelPosition.Origin);
        _indicator.Name = "Here";
        _page.AddChild(_indicator);
        PlaceIndicator();
    }

    /// <summary>Puts the sign of where Samus is over her cell of the map, and hides it while that cell is out of the window.</summary>
    private void PlaceIndicator()
    {
        if (_map is not { } map || _indicator is not { } indicator)
            return;
        Vector2I cell = _here - (map.Centre - map.Window / 2);
        indicator.Visible = map.IsInWindow(_here);
        indicator.Place = PixelPosition.FromWhole((int)map.Position.X + cell.X * _mapCellSize, (int)map.Position.Y + cell.Y * _mapCellSize);
        indicator.ShowPlace();
    }

    /// <summary>Shows the equipment page as her inventory now stands, with the selector where it is.</summary>
    private void ShowEquipment()
    {
        DataNode page = Data().Get("equipment");
        Node2D shown = _page ?? throw NotShown();
        if (shown.GetNodeOrNull<Node2D>("Equipment") is { } old)
        {
            shown.RemoveChild(old);
            old.QueueFree();
        }

        var equipment = new Node2D { Name = "Equipment" };
        shown.AddChild(equipment);
        Inventory inventory = Samus.Inventory;
        Show(page.Get("base"), parent: equipment);
        _controls.Clear();

        // Her reserves: with none, their place says so; with some, how they refill and how much they hold.
        DataNode reserve = page.Get("reserve");
        int held = inventory.Amount(Amounts.ReserveEnergy);
        if (inventory.Capacity(Amounts.ReserveEnergy) == 0)
        {
            Show(reserve.Get("absent"), parent: equipment);
            Show(reserve.Get("arrow_none"), parent: equipment);
        }
        else
        {
            bool auto = inventory.IsOn(Equipment.ReserveTank);
            Show(reserve.Get(auto ? "auto" : "manual"), parent: equipment);
            Show(reserve.Get("tank"), parent: equipment);
            Show(reserve.Get("arrow"), parent: equipment);
            IReadOnlyList<DataNode> digits = reserve.List("digits");
            int divisor = 1;
            for (int place = 1; place < digits.Count; place++)
                divisor *= 10;
            foreach (DataNode digit in digits)
            {
                Show(digit, digit.List("pictures")[held / divisor % 10].AsText(), equipment);
                divisor /= 10;
            }

            // The bar: the full tanks, the one part filled, the empty ones, and the cap after the last.
            DataNode bar = reserve.Get("bar");
            IReadOnlyList<DataNode> places = bar.List("places");
            IReadOnlyList<DataNode> fills = bar.List("fills");
            int perTank = bar.Int("energy_per_tank"), perStep = bar.Int("energy_per_step");
            int tank = 0;
            for (; tank < held / perTank; tank++)
                Show(places[tank], bar.Text("full"), equipment);
            int rest = held % perTank;
            _flickerEvery = 0;
            if (rest != 0)
            {
                int steps = rest / perStep;
                if (steps < bar.Int("flicker_below_steps") && rest % perStep != 0)
                {
                    _flickerEvery = bar.Int("flicker_updates");
                    if (_updates / _flickerEvery % 2 == 0)
                        steps++;
                }

                Show(places[tank++], fills[steps].AsText(), equipment);
            }

            for (; tank < inventory.Capacity(Amounts.ReserveEnergy) / perTank; tank++)
                Show(places[tank], fills[0].AsText(), equipment);
            Show(places[tank], bar.Text("end_cap"), equipment);

            _controls.Add(new Line(ReserveMode, "left", "", null, reserve.Get("mode_selector")));
            if (!auto)
                _controls.Add(new Line(ReserveTransfer, "left", "", null, reserve.Get("transfer_selector")));
        }

        // The hyper beam stands in the place of the beams, which cannot then be switched.
        bool hyper = inventory.IsOn(Equipment.HyperBeam);
        if (hyper)
            Show(page.Get("hyper"), parent: equipment);
        _beamsClosed = hyper;
        _columns.Clear();
        foreach (DataNode control in page.List("controls"))
        {
            string name = control.Text("equipment"), group = control.Text("group");
            if (!_columns.TryGetValue(group, out List<(string Equipment, int Control)>? column))
                _columns[group] = column = [];
            if (!inventory.Owns(name) || (hyper && control.Text("side") == page.Text("hyper_side") && control.Flag("beam")))
            {
                column.Add((name, -1));
                continue;
            }

            Show(control, control.Text(inventory.IsOn(name) ? "on" : "off"), equipment);
            column.Add((name, _controls.Count));
            _controls.Add(new Line(name, control.Text("side"), group, control, control.Get("selector")));
        }

        string wireframe = (inventory.IsOn(Equipment.VariaSuit), inventory.IsOn(Equipment.HiJumpBoots)) switch
        {
            (false, false) => "plain",
            (false, true) => "hi_jump",
            (true, false) => "varia",
            (true, true) => "varia_hi_jump",
        };
        Show(page.Get("wireframes").Get(wireframe), parent: equipment);
        if (_controls.Count == 0)
            return;
        _chosen = Math.Min(_chosen, _controls.Count - 1);
        Show(_controls[_chosen].Selector, parent: equipment);
    }

    /// <summary>
    /// Switches a piece of equipment on or off, or her reserves between refilling by themselves and not. The spazer
    /// and the plasma beam do not go together: one on turns the other off.
    /// </summary>
    private void Switch(string equipment)
    {
        Inventory inventory = Samus.Inventory;
        if (equipment == ReserveMode)
        {
            inventory.Switch(Equipment.ReserveTank, !inventory.IsOn(Equipment.ReserveTank));
            return;
        }

        bool on = !inventory.IsOn(equipment);
        inventory.Switch(equipment, on);
        if (!on)
            return;
        if (equipment == Equipment.Spazer && inventory.Owns(Equipment.PlasmaBeam))
            inventory.Switch(Equipment.PlasmaBeam, false);
        else if (equipment == Equipment.PlasmaBeam && inventory.Owns(Equipment.Spazer))
            inventory.Switch(Equipment.Spazer, false);
    }

    /// <summary>Pours a little of her reserves into her energy, sounding every so often as it goes.</summary>
    private void PourReserves()
    {
        Inventory inventory = Samus.Inventory;
        int room = inventory.Capacity(Inventory.Energy) - inventory.Amount(Inventory.Energy);
        int pour = Math.Min(ReservePourPerUpdate, Math.Min(room, inventory.Amount(Amounts.ReserveEnergy)));
        if (pour <= 0)
        {
            _pouring = 0;
            return;
        }

        inventory.Lose(Amounts.ReserveEnergy, pour);
        inventory.Add(Inventory.Energy, pour);
        if (_pouring++ % Data().Get("sounds").Int("pour_every") == 0)
            Sound("pour");
        ShowEquipment();
    }

    /// <summary>Shows a picture the data places, or another picture at that place; an empty picture shows nothing.</summary>
    private void Show(DataNode placed, string? picture = null, Node2D? parent = null)
    {
        picture ??= placed.Text("picture");
        if (picture.Length == 0)
            return;
        (parent ?? _page ?? throw NotShown()).AddChild(new Sprite2D
        {
            Texture = Session.Content.LoadTexture(picture), Centered = false, Position = new Vector2(placed.Int("x"), placed.Int("y")),
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
        });
    }

    private void Sound(string name) => Session.Sounds.Request(Data().Get("sounds").Text(name), channel: "menu");

    private DataNode Data() => _data ?? throw NotShown();

    private static InvalidOperationException NotShown() => new("The pause screen has not been shown.");
}
