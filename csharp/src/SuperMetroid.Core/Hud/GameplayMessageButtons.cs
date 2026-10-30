using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Core.Hud.GameplayMessageBoxDefinitions;

namespace SuperMetroid.Core.Hud;

/// <summary>Chooses the button a message panel's glyph shows for the action it names (#1416, #1466).</summary>
public static class GameplayMessageButtons
{
    /// <summary>
    /// The button the glyph shows for <paramref name="binding"/>: the first of <see cref="Buttons.GlyphOrder"/> the binding
    /// holds, so the first matching button wins even for a malformed multibit binding, or
    /// <see cref="ControllerButton.None"/> when it holds none of them.
    /// </summary>
    public static ControllerButton Shown(ControllerButton binding)
    {
        foreach (ControllerButton button in Buttons.GlyphOrder)
        {
            if ((binding & button) != ControllerButton.None)
                return button;
        }
        return ControllerButton.None;
    }

    /// <summary>The binding whose button <paramref name="messageId"/>'s panel names, or <see cref="GameplayMessagePanelButtonBinding.None"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="messageId"/> is no message the table lists.</exception>
    public static GameplayMessagePanelButtonBinding PanelBinding(GameplayMessageId messageId) =>
        Buttons.PanelBindings.TryGetValue(messageId, out GameplayMessagePanelButtonBinding binding) ? binding
            : throw new ArgumentOutOfRangeException(nameof(messageId), messageId, "The panel binding table does not list this message.");
}
