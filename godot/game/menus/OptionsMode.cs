using Engine.Core;
using Engine.Input;
using Engine.Ui;
using Godot;

namespace Game.Menus;

/// <summary>
/// The options screen. Its lines are the language, each of the game's toggles, and each action with the button it
/// is bound to. Confirming the language or a toggle changes it; confirming an action waits for a button and binds
/// the action to it. Leaving saves them.
/// </summary>
/// <remarks>
/// Its words are "options_language" (with {language}), "options_toggle_" and a toggle's name (with {state}),
/// "options_on", "options_off", "options_action_" and an action's name (with {button}), "options_press_a_button"
/// and "options_exit".
/// </remarks>
public partial class OptionsMode : GameMode
{
    private readonly GameMode _returnsTo;
    private ChoiceList? _list;
    private string? _binding;
    private string? _pressed;

    /// <summary>The options, laid over <paramref name="returnsTo"/>.</summary>
    public OptionsMode(GameMode returnsTo)
    {
        ArgumentNullException.ThrowIfNull(returnsTo);
        _returnsTo = returnsTo;
    }

    /// <summary>The languages the game's words come in.</summary>
    public string[] Languages { get; set; } = ["english", "japanese"];

    /// <inheritdoc/>
    public override GameMode? Keeps => _returnsTo;

    private IReadOnlyList<string> Actions => [.. GameAction.Standard.Select(action => action.Name), .. Session.Definition.Actions];

    /// <inheritdoc/>
    protected override void Enter()
    {
        var layer = new CanvasLayer { Name = "Screen", Layer = 10 };
        AddChild(layer);
        var backdrop = new ColorRect { Name = "Backdrop", Color = Colors.Black };
        backdrop.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        layer.AddChild(backdrop);
        _list = new ChoiceList { Name = "List" };
        _list.SetAnchorsPreset(Control.LayoutPreset.Center);
        layer.AddChild(_list);
        ShowLines(0);
    }

    /// <inheritdoc/>
    public override void _Input(InputEvent @event)
    {
        // While an action waits to be bound, the next button pressed is its button.
        if (_binding is null || _pressed is not null)
            return;
        if (@event is InputEventKey { Pressed: true, Echo: false } key)
            _pressed = $"key:{OS.GetKeycodeString(key.Keycode)}";
        else if (@event is InputEventJoypadButton { Pressed: true } button)
            _pressed = $"pad:{(int)button.ButtonIndex}";
    }

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        ChoiceList list = _list ?? throw new InvalidOperationException("The options have not been shown.");
        if (_binding is { } action)
        {
            if (_pressed is not { } button)
                return this;
            Session.Options.Bindings[action] = [button];
            Session.Options.ApplyBindings([]);
            (_binding, _pressed) = (null, null);
            ShowLines(list.Chosen);
            return this;
        }

        switch (list.Step(input))
        {
            case ChoiceList.Outcome.Confirmed:
                return Confirm(list.Chosen);
            case ChoiceList.Outcome.Cancelled:
                return Leave();
            default:
                return this;
        }
    }

    private GameMode Confirm(int chosen)
    {
        Options options = Session.Options;
        int toggles = Data.Toggles.All.Count;
        if (chosen == 0)
        {
            int at = Array.IndexOf(Languages, options.Language);
            options.Language = Languages[(at + 1) % Languages.Length];
        }
        else if (chosen <= toggles)
        {
            string toggle = Data.Toggles.All[chosen - 1];
            options.Toggles[toggle] = !options.IsOn(toggle);
        }
        else if (chosen <= toggles + Actions.Count)
        {
            (_binding, _pressed) = (Actions[chosen - toggles - 1], null);
        }
        else
        {
            return Leave();
        }

        ShowLines(chosen);
        return this;
    }

    private GameMode Leave()
    {
        Session.Options.Save();
        return _returnsTo;
    }

    private void ShowLines(int chosen)
    {
        Options options = Session.Options;
        Engine.Shared.GameText text = Session.Text;
        var lines = new List<string> { text.Get("options_language").Replace("{language}", options.Language, StringComparison.Ordinal) };
        foreach (string toggle in Data.Toggles.All)
        {
            lines.Add(text.Get($"options_toggle_{toggle}")
                .Replace("{state}", text.Get(options.IsOn(toggle) ? "options_on" : "options_off"), StringComparison.Ordinal));
        }

        foreach (string action in Actions)
        {
            string button = action == _binding
                ? text.Get("options_press_a_button")
                : string.Join(", ", options.Bindings.GetValueOrDefault(action) ?? []);
            lines.Add(text.Get($"options_action_{action}").Replace("{button}", button, StringComparison.Ordinal));
        }

        lines.Add(text.Get("options_exit"));
        _list!.Present(lines, chosen);
    }
}
