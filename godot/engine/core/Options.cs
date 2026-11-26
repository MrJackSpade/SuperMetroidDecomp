using System.Text.Json;
using Engine.Input;
using Godot;

namespace Engine.Core;

/// <summary>
/// The player's settings: which buttons give which action, the language, and the game's own toggles. They are kept
/// in one plain text file outside any save slot, and do not change during an update.
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
    /// The buttons bound to each action, by the action's name. A keyboard key is written "key:" and its name, such as
    /// "key:Space"; a controller button "pad:" and its number; a controller's axis pushed one way "axis:", its number
    /// and "+" or "-", such as "axis:5+", which is how a trigger, a stick or a pad that reports itself as axes is
    /// bound. A controller's binding holds for whichever controller is used.
    /// </summary>
    public Dictionary<string, List<string>> Bindings { get; set; } = [];

    /// <summary>The engine's bindings for its own actions, which a game or a player may replace.</summary>
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
            foreach ((string action, List<string> buttons) in StandardBindings)
                options.Bindings.TryAdd(action, buttons);
            options._file = file;
            return options;
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"The options file {file} cannot be read: {exception.Message}", exception);
        }
    }

    /// <summary>Binds each action in <paramref name="bindings"/> that has no binding yet, as a game does for its own actions.</summary>
    public void AddMissingBindings(IReadOnlyDictionary<string, List<string>> bindings)
    {
        ArgumentNullException.ThrowIfNull(bindings);
        foreach ((string action, List<string> buttons) in bindings)
            Bindings.TryAdd(action, buttons);
    }

    /// <summary>Writes the options to <paramref name="file"/>.</summary>
    /// <summary>Writes the options to the file they were loaded from.</summary>
    /// <exception cref="InvalidOperationException">They were not loaded from a file.</exception>
    public void Save()
    {
        if (_file.Length == 0)
            throw new InvalidOperationException("These options were not loaded from a file, so there is nowhere to save them.");
        Save(_file);
    }

    public void Save(string file)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(file)) ?? ".");
        File.WriteAllText(file, JsonSerializer.Serialize(this, Format));
    }

    /// <summary>
    /// Puts the bindings into Godot's input map, replacing what it held for each action bound here, and makes sure
    /// every action in <paramref name="actions"/> exists there even if nothing is bound to it.
    /// </summary>
    /// <exception cref="InvalidDataException">A binding is not written in a way that can be read.</exception>
    public void ApplyBindings(IEnumerable<GameAction> actions)
    {
        foreach (GameAction action in actions)
        {
            if (!InputMap.HasAction(action.Name))
                InputMap.AddAction(action.Name);
        }

        foreach ((string action, List<string> buttons) in Bindings)
        {
            if (!InputMap.HasAction(action))
                InputMap.AddAction(action);
            InputMap.ActionEraseEvents(action);
            foreach (string button in buttons)
                InputMap.ActionAddEvent(action, ReadButton(action, button));
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
    /// Binds an action to a button the player has pressed. The button takes the place of the action's binding of its
    /// own kind (a key for a key; a controller's button or axis for either of those) and the other kind is left as it
    /// is, so binding on a controller does not unbind the keyboard. Among the actions of
    /// <paramref name="keptApart"/>, when the action is one of them, no two keep one button: the one that had this
    /// button is given the one this action gives up, so the two swap.
    /// </summary>
    public void Bind(string action, string button, IReadOnlyCollection<string> keptApart)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(button);
        ArgumentNullException.ThrowIfNull(keptApart);
        bool key = IsKey(button);
        List<string> held = Bindings.GetValueOrDefault(action) ?? [];
        string? givenUp = held.FirstOrDefault(other => IsKey(other) == key);
        if (givenUp == button)
            return;
        Bindings[action] = [.. held.Where(other => IsKey(other) != key), button];
        if (!keptApart.Contains(action))
            return;
        foreach (string other in keptApart)
        {
            if (other == action || !Bindings.TryGetValue(other, out List<string>? theirs) || !theirs.Contains(button))
                continue;
            Bindings[other] = [.. theirs.Select(one => one == button ? givenUp : one).OfType<string>()];
        }
    }

    /// <summary>
    /// Puts back the buttons each of <paramref name="actions"/> has until the player chooses otherwise: the engine's
    /// own for its actions and <paramref name="gameStandard"/> for the game's.
    /// </summary>
    /// <exception cref="InvalidOperationException">An action has no standard buttons.</exception>
    public void RestoreBindings(IEnumerable<string> actions, IReadOnlyDictionary<string, List<string>> gameStandard)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ArgumentNullException.ThrowIfNull(gameStandard);
        Dictionary<string, List<string>> engineStandard = StandardBindings;
        foreach (string action in actions)
        {
            List<string> standard = engineStandard.GetValueOrDefault(action) ?? gameStandard.GetValueOrDefault(action)
                ?? throw new InvalidOperationException($"The action \"{action}\" has no standard buttons to be put back.");
            Bindings[action] = [.. standard];
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
