namespace SuperMetroid.Core.Input;

/// <summary>
/// What the player asks of the game during gameplay: the four directions, Start and the seven actions the controller
/// bindings assign to buttons. Each action is one independent bit, and a set of held actions is one flags value.
/// <see cref="ControllerBindings.ToActions"/> reads the player's buttons as actions once per accepted update; gameplay
/// tests actions, never buttons, so a remapped controller plays exactly as the default one does. The values are
/// arbitrary: nothing reads an action's number.
/// </summary>
[Flags]
public enum GameplayAction
{
    /// <summary>No action is held.</summary>
    None = 0,

    /// <summary>Up.</summary>
    Up = 1 << 0,

    /// <summary>Down.</summary>
    Down = 1 << 1,

    /// <summary>Left.</summary>
    Left = 1 << 2,

    /// <summary>Right.</summary>
    Right = 1 << 3,

    /// <summary>Start: pauses the game.</summary>
    Start = 1 << 4,

    /// <summary>Fires the selected weapon; held, it charges the beam.</summary>
    Shoot = 1 << 5,

    /// <summary>Jumps.</summary>
    Jump = 1 << 6,

    /// <summary>Runs; with the X-ray scope selected, holding it opens the scope.</summary>
    Dash = 1 << 7,

    /// <summary>Cycles the HUD's selected item.</summary>
    ItemSelect = 1 << 8,

    /// <summary>Clears the HUD's selected item.</summary>
    ItemCancel = 1 << 9,

    /// <summary>Aims diagonally up.</summary>
    AimUp = 1 << 10,

    /// <summary>Aims diagonally down.</summary>
    AimDown = 1 << 11,
}
