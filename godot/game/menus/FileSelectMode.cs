using Engine.Core;
using Engine.Input;
using Engine.Ui;
using Godot;

namespace Game.Menus;

/// <summary>
/// The choice of saved game. Each slot shows the energy and play time it holds, or that it holds nothing; choosing
/// one starts or continues its game. A slot can be copied over another or cleared, each after being asked for, and
/// the last line goes back to the title.
/// </summary>
/// <remarks>
/// Its words are "file_select_slot" (with {slot}, {energy}, {hours} and {minutes}), "file_select_empty" (with
/// {slot}), "file_select_copy", "file_select_clear", "file_select_options", "file_select_exit",
/// "file_select_copy_from", "file_select_copy_to" and "file_select_clear_which".
/// </remarks>
public partial class FileSelectMode : GameMode
{
    /// <summary>The music of the screen, by name.</summary>
    [Export]
    public string Music { get; set; } = Data.Music.TitleScreen;

    /// <summary>The sound of the mark moving, by name.</summary>
    [Export]
    public string MoveSound { get; set; } = "";

    private enum Asking
    {
        WhichGame,
        CopyFrom,
        CopyTo,
        ClearWhich,
    }

    private ChoiceList? _list;
    private PixelText? _heading;
    private Asking _asking;
    private int _copyFrom;

    private int Slots => Session.Saves.Slots;

    /// <inheritdoc/>
    protected override void Enter()
    {
        var layer = new CanvasLayer { Name = "Screen" };
        AddChild(layer);
        var column = new VBoxContainer { Name = "Column" };
        column.SetAnchorsPreset(Control.LayoutPreset.Center);
        layer.AddChild(column);
        _heading = new PixelText { Name = "Heading", Font = Session.Font };
        column.AddChild(_heading);
        _list = new ChoiceList { Name = "List", Font = Session.Font, Mark = "", UnchosenTint = new Color(0.5f, 0.5f, 0.5f) };
        column.AddChild(_list);
        Session.Sounds.RequestMusic(Music);
        Ask(Asking.WhichGame, 0);
    }

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        ChoiceList list = _list ?? throw new InvalidOperationException("The choice of saved game has not been shown.");
        switch (list.Step(input))
        {
            case ChoiceList.Outcome.Moved:
                if (MoveSound.Length > 0)
                    Session.Sounds.Request(MoveSound, channel: "menu");
                return this;

            case ChoiceList.Outcome.Cancelled:
                if (_asking == Asking.WhichGame)
                    return new TitleMode();
                Ask(Asking.WhichGame, 0);
                return this;

            case ChoiceList.Outcome.Confirmed:
                return Confirm(list.Chosen);

            default:
                return this;
        }
    }

    private GameMode Confirm(int chosen)
    {
        int slot = chosen + 1;
        switch (_asking)
        {
            case Asking.WhichGame:
                if (chosen < Slots)
                {
                    Session.Slot = slot;
                    return new LoadingMode();
                }

                switch (chosen - Slots)
                {
                    case 0:
                        Ask(Asking.CopyFrom, 0);
                        return this;
                    case 1:
                        Ask(Asking.ClearWhich, 0);
                        return this;
                    case 2:
                        return new OptionsMode(this);
                    default:
                        return new TitleMode();
                }

            case Asking.CopyFrom:
                if (chosen >= Slots || !Session.Saves.Has(slot))
                {
                    Ask(Asking.WhichGame, 0);
                    return this;
                }

                _copyFrom = slot;
                Ask(Asking.CopyTo, 0);
                return this;

            case Asking.CopyTo:
                if (chosen < Slots && slot != _copyFrom)
                    Session.Saves.Copy(_copyFrom, slot);
                Ask(Asking.WhichGame, 0);
                return this;

            case Asking.ClearWhich:
                if (chosen < Slots && Session.Saves.Has(slot))
                    Session.Saves.Clear(slot);
                Ask(Asking.WhichGame, 0);
                return this;

            default:
                throw new InvalidOperationException($"The choice of saved game has no question {_asking}.");
        }
    }

    /// <summary>Shows the slots under a heading, with the lines that go with the question.</summary>
    private void Ask(Asking asking, int chosen)
    {
        _asking = asking;
        var lines = new List<string>();
        for (int slot = 1; slot <= Slots; slot++)
            lines.Add(LineOf(slot));
        if (asking == Asking.WhichGame)
        {
            lines.Add(Session.Text.Get("file_select_copy"));
            lines.Add(Session.Text.Get("file_select_clear"));
            lines.Add(Session.Text.Get("file_select_options"));
        }

        lines.Add(Session.Text.Get("file_select_exit"));
        _list!.Present(lines, chosen);
        _heading!.Text = asking switch
        {
            Asking.WhichGame => "",
            Asking.CopyFrom => Session.Text.Get("file_select_copy_from"),
            Asking.CopyTo => Session.Text.Get("file_select_copy_to"),
            Asking.ClearWhich => Session.Text.Get("file_select_clear_which"),
            _ => throw new InvalidOperationException($"The choice of saved game has no question {asking}."),
        };
    }

    /// <summary>What a slot's line says: what it holds, or that it holds nothing.</summary>
    private string LineOf(int slot)
    {
        string number = slot.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (!Session.Saves.Has(slot))
            return Session.Text.Get("file_select_empty").Replace("{slot}", number, StringComparison.Ordinal);
        SaveData save = Session.Saves.Read(slot);
        long minutes = save.PlayUpdates / 3600;
        return Session.Text.Get("file_select_slot")
            .Replace("{slot}", number, StringComparison.Ordinal)
            .Replace("{energy}", save.Amounts.GetValueOrDefault(Data.Amounts.Energy).ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{hours}", (minutes / 60).ToString("00", System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{minutes}", (minutes % 60).ToString("00", System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }
}
