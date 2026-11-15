namespace SuperMetroid.Core.Input;

/// <summary>Named combinations of <see cref="GameplayAction"/> and the action each configurable binding asks for.</summary>
public static class GameplayActions
{
    /// <summary>The four directions.</summary>
    public const GameplayAction Directions =
        GameplayAction.Right | GameplayAction.Left | GameplayAction.Down | GameplayAction.Up;

    /// <summary>The two opposed horizontal directions.</summary>
    public const GameplayAction HorizontalDirections = GameplayAction.Right | GameplayAction.Left;

    /// <summary>Tests whether at least one of <paramref name="actions"/> is present.</summary>
    public static bool HasAny(this GameplayAction input, GameplayAction actions) =>
        (input & actions) != GameplayAction.None;

    /// <summary>The gameplay action a configurable <paramref name="action"/> asks for.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="action"/> is not a defined action.</exception>
    public static GameplayAction Of(ControllerAction action) => action switch
    {
        ControllerAction.Shoot => GameplayAction.Shoot,
        ControllerAction.Jump => GameplayAction.Jump,
        ControllerAction.Dash => GameplayAction.Dash,
        ControllerAction.ItemSelect => GameplayAction.ItemSelect,
        ControllerAction.ItemCancel => GameplayAction.ItemCancel,
        ControllerAction.AimUp => GameplayAction.AimUp,
        ControllerAction.AimDown => GameplayAction.AimDown,
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "Undefined controller action."),
    };
}
