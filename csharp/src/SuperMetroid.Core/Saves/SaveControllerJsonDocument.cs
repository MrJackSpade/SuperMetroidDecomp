using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Saves;

/// <summary>The seven configurable actions, each represented by one SNES controller bit.</summary>
/// <param name="Shoot">Button assigned to firing beams and weapons.</param>
/// <param name="Jump">Button assigned to jumping.</param>
/// <param name="Dash">Button assigned to running and speed-boost input.</param>
/// <param name="ItemSelect">Button assigned to cycling the HUD item selection.</param>
/// <param name="ItemCancel">Button assigned to returning the HUD selection to the beam.</param>
/// <param name="AimUp">Button assigned to upward diagonal aiming.</param>
/// <param name="AimDown">Button assigned to downward diagonal aiming.</param>
public sealed record SaveControllerJsonDocument(
    ControllerButton Shoot,
    ControllerButton Jump,
    ControllerButton Dash,
    ControllerButton ItemSelect,
    ControllerButton ItemCancel,
    ControllerButton AimUp,
    ControllerButton AimDown);
