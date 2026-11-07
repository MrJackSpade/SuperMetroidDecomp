namespace SuperMetroid.Core.Input;

/// <summary>
/// One of the seven configurable actions <see cref="ControllerBindings"/> binds to a button. Directional input and Start
/// are fixed controls, not actions.
/// </summary>
public enum ControllerAction
{
    /// <summary>Fires the selected weapon.</summary>
    Shoot,

    /// <summary>Jumps.</summary>
    Jump,

    /// <summary>Runs.</summary>
    Dash,

    /// <summary>Cycles the HUD's selected item.</summary>
    ItemSelect,

    /// <summary>Clears the HUD's selected item.</summary>
    ItemCancel,

    /// <summary>Aims diagonally up.</summary>
    AimUp,

    /// <summary>Aims diagonally down.</summary>
    AimDown,
}
