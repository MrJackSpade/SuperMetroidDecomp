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
    /// <remarks>
    /// Issues #625 and #951: this twelve-entry order is the controller-settings row order,
    /// indexed only by the dialog's selected row (0..11). It lists four directions, four
    /// Retroid face buttons, L1/R1, Start, and Select exactly once. The corresponding
    /// default button for each row is Map(key); this is one logical host policy with two
    /// views, not a cartridge table. Retain the explicit order because it is presentation
    /// and remapping policy, not a numerical sequence to generate.
    /// </remarks>
    public static readonly Keycode[] Keys =
    [
        Keycode.DpadLeft, Keycode.DpadRight, Keycode.DpadUp, Keycode.DpadDown,
        Keycode.ButtonA, Keycode.ButtonB, Keycode.ButtonX, Keycode.ButtonY,
        Keycode.ButtonL1, Keycode.ButtonR1, Keycode.ButtonStart, Keycode.ButtonSelect,
    ];

    /// <summary>Returns the Retroid Classic default SNES button for an Android key.</summary>
    /// <remarks>
    /// Issues #625 and #951: the twelve Keys cases are one-to-one with same-named SNES
    /// directions, A/B/X/Y, L/R, Start, and Select. Every other Keycode maps to None;
    /// Back and Mode are reserved by MainActivity for host controls. Settings and key
    /// events both pass this value as the fallback to AndroidControllerPreferences.Resolve,
    /// so a saved per-key override takes priority. Device testing established the face
    /// names; no ROM revision or native address applies. An arithmetic enum cast would
    /// depend on unrelated Android/SNES numeric identities and lose the explicit default
    /// domain, so retain this small authored selector.
    /// </remarks>
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
