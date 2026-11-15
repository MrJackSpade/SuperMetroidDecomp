using SuperMetroid.Core.Input;
using SuperMetroid.Diagnostics.Input;

namespace SuperMetroid.TestSupport.SamusNativeCodecs;

/// <summary>
/// Converts gameplay actions to and from the native joypad word for native traces, recordings and comparisons (#1925).
/// The native game stores the buttons held and compares them with the save's bindings; these fixtures and traces use the
/// bindings a new save installs, so an action's word is the word of its default button.
/// </summary>
internal static class GameplayActionWordCodec
{
    /// <summary>The actions a native joypad word asks for under the default bindings.</summary>
    /// <param name="word">The native joypad word.</param>
    /// <param name="sourceContext">Where the word came from, named in an error.</param>
    internal static GameplayAction FromJoypadWord(ushort word, string sourceContext) =>
        ControllerBindings.Default.ToActions(ControllerWordCodec.FromJoypadWord(word, sourceContext));

    /// <summary>The native joypad word that holds <paramref name="actions"/> under the default bindings.</summary>
    internal static ushort ToJoypadWord(GameplayAction actions) => ControllerWordCodec.ToJoypadWord(ToButtons(actions));

    /// <summary>The default buttons that ask for <paramref name="actions"/>.</summary>
    internal static ControllerButton ToButtons(GameplayAction actions)
    {
        ControllerBindings bindings = ControllerBindings.Default;
        ControllerButton buttons =
            FixedControl(actions, GameplayAction.Up, ControllerButton.Up) |
            FixedControl(actions, GameplayAction.Down, ControllerButton.Down) |
            FixedControl(actions, GameplayAction.Left, ControllerButton.Left) |
            FixedControl(actions, GameplayAction.Right, ControllerButton.Right) |
            FixedControl(actions, GameplayAction.Start, ControllerButton.Start);
        foreach (ControllerAction action in Enum.GetValues<ControllerAction>())
        {
            if (actions.HasAny(GameplayActions.Of(action)))
                buttons |= bindings[action];
        }
        return buttons;
    }

    /// <summary>The fixed control's button when <paramref name="action"/> is held, otherwise none.</summary>
    private static ControllerButton FixedControl(GameplayAction actions, GameplayAction action, ControllerButton button) =>
        actions.HasAny(action) ? button : ControllerButton.None;
}
