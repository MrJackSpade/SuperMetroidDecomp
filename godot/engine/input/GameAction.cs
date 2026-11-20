namespace Engine.Input;

/// <summary>
/// Something the player can ask for, such as jumping, by name. Gameplay reads actions, never buttons; the player's
/// bindings say which button gives which action. The engine names the actions its own states and menus need, and a
/// game adds its own.
/// </summary>
/// <param name="Name">The action's name, which is also its name in the player's bindings.</param>
public readonly record struct GameAction(string Name)
{
    /// <summary>Move or aim left.</summary>
    public static GameAction Left { get; } = new("left");

    /// <summary>Move or aim right.</summary>
    public static GameAction Right { get; } = new("right");

    /// <summary>Aim up, or move up where that means something.</summary>
    public static GameAction Up { get; } = new("up");

    /// <summary>Crouch or aim down, or move down where that means something.</summary>
    public static GameAction Down { get; } = new("down");

    /// <summary>Jump.</summary>
    public static GameAction Jump { get; } = new("jump");

    /// <summary>Use the selected weapon.</summary>
    public static GameAction Attack { get; } = new("attack");

    /// <summary>Pause the game.</summary>
    public static GameAction Pause { get; } = new("pause");

    /// <summary>Accept, in a menu or a message.</summary>
    public static GameAction Confirm { get; } = new("confirm");

    /// <summary>Go back, in a menu or a message.</summary>
    public static GameAction Cancel { get; } = new("cancel");

    /// <summary>The actions the engine itself names.</summary>
    public static IReadOnlyList<GameAction> Standard { get; } = [Left, Right, Up, Down, Jump, Attack, Pause, Confirm, Cancel];

    /// <summary>The action's name.</summary>
    public override string ToString() => Name;
}
