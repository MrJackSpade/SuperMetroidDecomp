using System.Text.Json;
using Engine.Input;
using Godot;

namespace Engine.Core;

/// <summary>
/// The player's settings: which buttons stand for which control, which control each action is on, the language, and
/// the game's own toggles. They are kept in one plain text file outside any save slot, and do not change during an
/// update. A control is a button of the game's own controller, by name. An action is on one control: its own, of the
/// action's name, unless the game puts it on another (<see cref="Controls"/>); the player's keys and controller's
/// buttons are bound to controls (<see cref="Bindings"/>). So what a game keeps of its controls with a save, which
/// control each of its actions is on, is apart from what belongs to the machine it is played on. A save carries the
/// controls, the language and the toggles as they stood when it was made (<see cref="SaveData.Controls"/>,
/// <see cref="SaveData.Toggles"/>, <see cref="SaveData.Language"/>), for a game that gives a chosen save's back to
/// the options.
/// </summary>
public sealed class Options
{
    /// <summary>The file the options were loaded from; empty when they were not loaded from one.</summary>
    private string _file = "";

    private static readonly JsonSerializerOptions Format = new() { WriteIndented = true };

    /// <summary>The language the game's words are shown in, by name.</summary>
    public string Language { get; set; } = "english";

    /// <summary>The game's own settings that are on or off, by name.</summary>
    public Dictionary<string, bool> Toggles { get; set; } = [];

    /// <summary>
    /// The control each action is on, by the action's name, for the actions a game puts on a control other than
    /// their own.
    /// </summary>
    public Dictionary<string, string> Controls { get; set; } = [];

    /// <summary>
    /// The buttons bound to each control, by the control's name. A keyboard key is written "key:" and its name, such
    /// as "key:Space"; a controller button "pad:" and its number; a controller's axis pushed one way "axis:", its
    /// number and "+" or "-", such as "axis:5+", which is how a trigger, a stick or a pad that reports itself as axes
    /// is bound. A controller's binding holds for whichever controller is used. A control's standard buttons are a
    /// key and a controller's button, so that either works; one the player has set has the one button they pressed.
    /// </summary>
    public Dictionary<string, List<string>> Bindings { get; set; } = [];

    /// <summary>The engine's bindings for the controls of its own actions, which a game or a player may replace.</summary>
    public static Dictionary<string, List<string>> StandardBindings => new()
    {
        [GameAction.Left.Name] = ["key:Left", "pad:13"],
        [GameAction.Right.Name] = ["key:Right", "pad:14"],
        [GameAction.Up.Name] = ["key:Up", "pad:11"],
        [GameAction.Down.Name] = ["key:Down", "pad:12"],
        [GameAction.Jump.Name] = ["key:X", "pad:0"],
        [GameAction.Attack.Name] = ["key:Z", "pad:2"],
        [GameAction.Pause.Name] = ["key:Enter", "pad:6"],
        [GameAction.Confirm.Name] = ["key:X", "pad:0"],
        [GameAction.Cancel.Name] = ["key:Z", "pad:1"],
    };

    /// <summary>Whether a toggle is on. One that has never been set is off.</summary>
    public bool IsOn(string toggle) => Toggles.GetValueOrDefault(toggle);

    /// <summary>The control an action is on: the one the game put it on, or its own, of the action's name.</summary>
    public string ControlOf(string action) => Controls.GetValueOrDefault(action, action);

    /// <summary>The buttons that give an action now: those bound to the control it is on.</summary>
    public IReadOnlyList<string> ButtonsOf(string action) => Bindings.GetValueOrDefault(ControlOf(action)) ?? [];

    /// <summary>Reads the options kept in <paramref name="file"/>, or the standard ones when there is no such file yet.</summary>
    /// <exception cref="InvalidDataException">The file cannot be read as options.</exception>
    public static Options Load(string file)
    {
        if (!File.Exists(file))
            return new Options { Bindings = StandardBindings, _file = file };
        try
        {
            Options options = JsonSerializer.Deserialize<Options>(File.ReadAllText(file))
                ?? throw new InvalidDataException($"The options file {file} holds nothing that can be read.");
            foreach ((string control, List<string> buttons) in StandardBindings)
                options.Bindings.TryAdd(control, buttons);
            options._file = file;
            return options;
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"The options file {file} cannot be read: {exception.Message}", exception);
        }
    }

    /// <summary>Binds each control in <paramref name="bindings"/> that has no binding yet, as a game does for its own controls.</summary>
    public void AddMissingBindings(IReadOnlyDictionary<string, List<string>> bindings)
    {
        ArgumentNullException.ThrowIfNull(bindings);
        foreach ((string control, List<string> buttons) in bindings)
            Bindings.TryAdd(control, buttons);
    }

    /// <summary>Puts each action in <paramref name="controls"/> that is on no control of the game's yet on the one given for it.</summary>
    public void AddMissingControls(IReadOnlyDictionary<string, string> controls)
    {
        ArgumentNullException.ThrowIfNull(controls);
        foreach ((string action, string control) in controls)
            Controls.TryAdd(action, control);
    }

    /// <summary>Writes the options to the file they were loaded from.</summary>
    /// <exception cref="InvalidOperationException">They were not loaded from a file.</exception>
    public void Save()
    {
        if (_file.Length == 0)
            throw new InvalidOperationException("These options were not loaded from a file, so there is nowhere to save them.");
        Save(_file);
    }

    /// <summary>Writes the options to <paramref name="file"/>.</summary>
    public void Save(string file)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(file)) ?? ".");
        File.WriteAllText(file, JsonSerializer.Serialize(this, Format));
    }

    /// <summary>
    /// Puts into Godot's input map, for each action in <paramref name="actions"/>, the buttons bound to the control
    /// it is on, in place of what the map held for it. An action whose control has no button is in the map with
    /// nothing bound.
    /// </summary>
    /// <exception cref="InvalidDataException">A binding is not written in a way that can be read.</exception>
    public void ApplyBindings(IEnumerable<GameAction> actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        foreach (GameAction action in actions)
        {
            if (!InputMap.HasAction(action.Name))
                InputMap.AddAction(action.Name);
            InputMap.ActionEraseEvents(action.Name);
            foreach (string button in ButtonsOf(action.Name))
                InputMap.ActionAddEvent(action.Name, ReadButton(action.Name, button));
        }
    }

    /// <summary>How far an axis must be pushed, of its whole reach, to count as pressed.</summary>
    public const float AxisPressed = 0.5f;

    /// <summary>
    /// The button a press is, as a binding writes it: a key going down, a controller's button going down, or a
    /// controller's axis pushed at least <see cref="AxisPressed"/> of its reach one way. Null for anything else.
    /// </summary>
    public static string? ButtonOf(InputEvent @event) => @event switch
    {
        InputEventKey { Pressed: true, Echo: false } key => $"key:{OS.GetKeycodeString(key.Keycode)}",
        InputEventJoypadButton { Pressed: true } button => $"pad:{(int)button.ButtonIndex}",
        InputEventJoypadMotion motion when Math.Abs(motion.AxisValue) >= AxisPressed => AxisButton((int)motion.Axis, motion.AxisValue > 0),
        _ => null,
    };

    /// <summary>The binding of an axis pushed one way.</summary>
    public static string AxisButton(int axis, bool positive) => $"axis:{axis}{(positive ? "+" : "-")}";

    private static bool IsKey(string button) => button.StartsWith("key:", StringComparison.Ordinal);

    /// <summary>
    /// Puts an action on a control. Among the actions of <paramref name="keptApart"/>, when the action is one of
    /// them, no two are on one control: the one that was on this control takes the one this action leaves, so the two
    /// swap.
    /// </summary>
    public void PutOn(string action, string control, IReadOnlyCollection<string> keptApart)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(control);
        ArgumentNullException.ThrowIfNull(keptApart);
        string left = ControlOf(action);
        if (left == control)
            return;
        Controls[action] = control;
        if (!keptApart.Contains(action))
            return;
        foreach (string other in keptApart)
        {
            if (other != action && ControlOf(other) == control)
                Controls[other] = left;
        }
    }

    /// <summary>
    /// Leaves each of <paramref name="controls"/> one button: a controller's, when the player is on one and the
    /// control has one, a key otherwise, and failing that the first it has. A control's standard buttons name one
    /// for the keyboard and one for a controller, so that either works before anything is chosen; a control the
    /// player sets has one, and this settles which, by the device they are using.
    /// </summary>
    public void KeepOneButton(IEnumerable<string> controls, bool onController)
    {
        ArgumentNullException.ThrowIfNull(controls);
        foreach (string control in controls)
        {
            if (!Bindings.TryGetValue(control, out List<string>? held) || held.Count <= 1)
                continue;
            Bindings[control] = [held.FirstOrDefault(button => IsKey(button) != onController) ?? held[0]];
        }
    }

    /// <summary>
    /// Puts back the buttons each of <paramref name="controls"/> has until the player chooses otherwise: the engine's
    /// own for the controls of its actions and <paramref name="gameStandard"/> for the game's.
    /// </summary>
    /// <exception cref="InvalidOperationException">A control has no standard buttons.</exception>
    public void RestoreBindings(IEnumerable<string> controls, IReadOnlyDictionary<string, List<string>> gameStandard)
    {
        ArgumentNullException.ThrowIfNull(controls);
        ArgumentNullException.ThrowIfNull(gameStandard);
        Dictionary<string, List<string>> engineStandard = StandardBindings;
        foreach (string control in controls)
        {
            List<string> standard = gameStandard.GetValueOrDefault(control) ?? engineStandard.GetValueOrDefault(control)
                ?? throw new InvalidOperationException($"The control \"{control}\" has no standard buttons to be put back.");
            Bindings[control] = [.. standard];
        }
    }

    /// <summary>A binding's text as the Godot input event it stands for.</summary>
    private static InputEvent ReadButton(string action, string button)
    {
        string[] parts = button.Split(':', 2);
        if (parts.Length == 2 && parts[0] == "key")
        {
            Key key = OS.FindKeycodeFromString(parts[1]);
            if (key == Key.None)
                throw new InvalidDataException($"The binding \"{button}\" for \"{action}\" names no key.");
            return new InputEventKey { Keycode = key };
        }

        // A controller's binding is for any controller: the one in use is whichever the player picks up.
        const int anyController = -1;
        if (parts.Length == 2 && parts[0] == "pad" && int.TryParse(parts[1], out int number) && number >= 0)
            return new InputEventJoypadButton { ButtonIndex = (JoyButton)number, Device = anyController };
        if (parts.Length == 2 && parts[0] == "axis" && parts[1].Length >= 2 && parts[1][^1] is '+' or '-'
            && int.TryParse(parts[1][..^1], out int axis) && axis >= 0)
        {
            return new InputEventJoypadMotion { Axis = (JoyAxis)axis, AxisValue = parts[1][^1] == '+' ? 1 : -1, Device = anyController };
        }

        throw new InvalidDataException(
            $"The binding \"{button}\" for \"{action}\" is not \"key:\" and a key's name, \"pad:\" and a button's number, or \"axis:\", an axis's number and \"+\" or \"-\".");
    }
}
