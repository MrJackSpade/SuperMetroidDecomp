using Godot;

namespace Engine.Input;

/// <summary>
/// The player's devices as an input source. It reads Godot's input map, where each action's name is bound to the
/// player's buttons, and remembers every press between two updates so that a quick tap is not lost.
/// </summary>
public partial class DeviceInputSource : Node, IInputSource
{
    /// <summary>The actions this source watches.</summary>
    private readonly List<GameAction> _actions = [];

    /// <summary>The actions that went down since they were last taken.</summary>
    private readonly HashSet<GameAction> _pressedSinceTaken = [];

    /// <summary>Sets the actions to watch. Each must be an action in Godot's input map.</summary>
    /// <exception cref="InvalidOperationException">An action is not in the input map.</exception>
    public void Watch(IEnumerable<GameAction> actions)
    {
        _actions.Clear();
        foreach (GameAction action in actions)
        {
            if (!InputMap.HasAction(action.Name))
                throw new InvalidOperationException($"The input map has no action named \"{action.Name}\".");
            _actions.Add(action);
        }
    }

    /// <inheritdoc/>
    public override void _Input(InputEvent @event)
    {
        foreach (GameAction action in _actions)
        {
            if (@event.IsActionPressed(action.Name))
                _pressedSinceTaken.Add(action);
        }
    }

    /// <inheritdoc/>
    public IReadOnlySet<GameAction> TakeActionsDown()
    {
        var down = new HashSet<GameAction>(_pressedSinceTaken);
        foreach (GameAction action in _actions)
        {
            if (Godot.Input.IsActionPressed(action.Name))
                down.Add(action);
        }

        _pressedSinceTaken.Clear();
        return down;
    }
}
