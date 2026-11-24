using Engine.Actors;
using Engine.Core;
using Engine.Input;
using Engine.Ui;
using Engine.World;
using Game.Data;
using Godot;

namespace Game;

/// <summary>
/// The pause screen, laid over play. It has two pages. The map page shows the map of the area Samus is in, which
/// the directions scroll. The equipment page lists what she owns; confirming a line switches it on or off through
/// her inventory, and with her reserves on manual she can pour them into her energy there. The angle buttons turn
/// the pages and pause goes back to play.
/// </summary>
/// <remarks>
/// Its words are "pause_map", "pause_equipment", "equipment_" and a piece of equipment's name, "options_on",
/// "options_off" and "pause_use_reserves".
/// </remarks>
public partial class PauseScreen : GameMode
{
    private readonly PlayingMode _playing;
    private Label? _heading;
    private MapView? _map;
    private ChoiceList? _list;
    private bool _showsMap = true;
    private readonly List<string> _listed = [];

    /// <summary>The pause screen, laid over <paramref name="playing"/>.</summary>
    public PauseScreen(PlayingMode playing)
    {
        ArgumentNullException.ThrowIfNull(playing);
        _playing = playing;
    }

    /// <summary>The folder, within the assets tree, of the files of the areas' maps.</summary>
    public string AreaFolder { get; set; } = "areas";

    /// <summary>How many cells of the map show at once, across and down.</summary>
    public Vector2I MapWindow { get; set; } = new(30, 20);

    /// <summary>How much of her reserves pours into her energy each update she holds confirm on that line.</summary>
    public int ReservePourPerUpdate { get; set; } = 1;

    /// <inheritdoc/>
    public override GameMode? Keeps => _playing;

    private Player Samus => Session.Player;

    /// <inheritdoc/>
    protected override void Enter()
    {
        var layer = new CanvasLayer { Name = "Screen", Layer = 10 };
        AddChild(layer);
        var backdrop = new ColorRect { Name = "Backdrop", Color = new Color(0f, 0f, 0f, 0.9f) };
        backdrop.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        layer.AddChild(backdrop);
        _heading = new Label { Name = "Heading" };
        _heading.SetAnchorsPreset(Control.LayoutPreset.CenterTop);
        layer.AddChild(_heading);
        _map = new MapView { Name = "Map", Window = MapWindow, Position = new Vector2(8, 24) };
        layer.AddChild(_map);
        _list = new ChoiceList { Name = "Equipment" };
        _list.SetAnchorsPreset(Control.LayoutPreset.Center);
        layer.AddChild(_list);

        Room room = _playing.Room;
        if (room.Area.Length > 0)
        {
            _map.Centre = new Vector2I(
                room.MapCell.X + Samus.Position.X.Whole / room.MapCellSize.X,
                room.MapCell.Y + Samus.Position.Y.Whole / room.MapCellSize.Y);
            _map.Present(Area.Read(Session.Content.ReadData($"{AreaFolder}/{room.Area}.json")), Session.Progress, Session.Content);
        }

        ShowPage(map: room.Area.Length > 0);
    }

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        if (input.WasPressed(GameAction.Pause))
            return _playing;
        if (input.WasPressed(Actions.AngleUp) || input.WasPressed(Actions.AngleDown))
        {
            ShowPage(map: !_showsMap && _playing.Room.Area.Length > 0);
            return this;
        }

        if (_showsMap)
        {
            // The directions scroll the map a cell at a time.
            var move = new Vector2I(
                (input.WasPressed(GameAction.Right) ? 1 : 0) - (input.WasPressed(GameAction.Left) ? 1 : 0),
                (input.WasPressed(GameAction.Down) ? 1 : 0) - (input.WasPressed(GameAction.Up) ? 1 : 0));
            if (move != Vector2I.Zero)
            {
                _map!.Centre += move;
                _map.QueueRedraw();
            }

            return this;
        }

        ChoiceList list = _list!;
        string line = _listed[list.Chosen];
        if (line == Amounts.ReserveEnergy)
        {
            if (input.IsHeld(GameAction.Confirm))
                PourReserves();
            list.Step(input.WasPressed(GameAction.Confirm) ? GameInput.None : input);
            return this;
        }

        if (list.Step(input) == ChoiceList.Outcome.Confirmed)
        {
            Switch(line);
            ShowEquipment(list.Chosen);
        }

        return this;
    }

    private void ShowPage(bool map)
    {
        _showsMap = map;
        _map!.Visible = map;
        _list!.Visible = !map;
        _heading!.Text = Session.Text.Get(map ? "pause_map" : "pause_equipment");
        if (!map)
            ShowEquipment(0);
    }

    /// <summary>Lists what she owns, each with whether it is on, and her reserves when she has any to pour.</summary>
    private void ShowEquipment(int chosen)
    {
        Inventory inventory = Samus.Inventory;
        _listed.Clear();
        var lines = new List<string>();
        foreach (string equipment in Equipment.All)
        {
            // The hyper beam is not hers to switch.
            if (equipment == Equipment.HyperBeam || !inventory.Owns(equipment))
                continue;
            _listed.Add(equipment);
            lines.Add($"{Session.Text.Get($"equipment_{equipment}")}  {Session.Text.Get(inventory.IsOn(equipment) ? "options_on" : "options_off")}");
        }

        if (inventory.Capacity(Amounts.ReserveEnergy) > 0 && !inventory.IsOn(Equipment.ReserveTank))
        {
            _listed.Add(Amounts.ReserveEnergy);
            lines.Add($"{Session.Text.Get("pause_use_reserves")}  {inventory.Amount(Amounts.ReserveEnergy)}");
        }

        if (lines.Count == 0)
        {
            _listed.Add("");
            lines.Add("");
        }

        _list!.Present(lines, Math.Min(chosen, lines.Count - 1));
    }

    /// <summary>Switches a piece of equipment on or off. The spazer and the plasma beam do not go together: one on turns the other off.</summary>
    private void Switch(string equipment)
    {
        if (equipment.Length == 0)
            return;
        Inventory inventory = Samus.Inventory;
        bool on = !inventory.IsOn(equipment);
        inventory.Switch(equipment, on);
        if (!on)
            return;
        if (equipment == Equipment.Spazer && inventory.Owns(Equipment.PlasmaBeam))
            inventory.Switch(Equipment.PlasmaBeam, false);
        else if (equipment == Equipment.PlasmaBeam && inventory.Owns(Equipment.Spazer))
            inventory.Switch(Equipment.Spazer, false);
    }

    /// <summary>Pours a little of her reserves into her energy.</summary>
    private void PourReserves()
    {
        Inventory inventory = Samus.Inventory;
        int room = inventory.Capacity(Inventory.Energy) - inventory.Amount(Inventory.Energy);
        int pour = Math.Min(ReservePourPerUpdate, Math.Min(room, inventory.Amount(Amounts.ReserveEnergy)));
        if (pour <= 0)
            return;
        inventory.Lose(Amounts.ReserveEnergy, pour);
        inventory.Add(Inventory.Energy, pour);
        ShowEquipment(_list!.Chosen);
    }
}
