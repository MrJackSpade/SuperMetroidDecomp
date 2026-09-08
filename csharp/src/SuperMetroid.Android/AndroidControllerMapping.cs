using Android.Views;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Android;

/// <summary>
/// Retroid Classic's Nintendo-layout key mapping, separate from cartridge bindings.
/// Player testing confirmed this device reports face-button names, not Xbox positions:
/// A is accept/jump, B is back/dash, X is fire, and Y is item cancel.
/// </summary>
internal static class AndroidControllerMapping
{
    /// <summary>Remappable gameplay keys; Back and Mode remain reserved for host tools.</summary>
    public static readonly Keycode[] Keys =
    [
        Keycode.DpadLeft, Keycode.DpadRight, Keycode.DpadUp, Keycode.DpadDown,
        Keycode.ButtonA, Keycode.ButtonB, Keycode.ButtonX, Keycode.ButtonY,
        Keycode.ButtonL1, Keycode.ButtonR1, Keycode.ButtonStart, Keycode.ButtonSelect,
    ];

    public static SnesButton Map(Keycode code) => code switch
    {
        Keycode.DpadLeft => SnesButton.Left,
        Keycode.DpadRight => SnesButton.Right,
        Keycode.DpadUp => SnesButton.Up,
        Keycode.DpadDown => SnesButton.Down,
        Keycode.ButtonA => SnesButton.A,
        Keycode.ButtonB => SnesButton.B,
        Keycode.ButtonX => SnesButton.X,
        Keycode.ButtonY => SnesButton.Y,
        Keycode.ButtonL1 => SnesButton.L,
        Keycode.ButtonR1 => SnesButton.R,
        Keycode.ButtonStart => SnesButton.Start,
        Keycode.ButtonSelect => SnesButton.Select,
        _ => SnesButton.None,
    };
}
