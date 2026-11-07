using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// What the gameplay message box shows (#1466): the message, the button an item panel names for its action, and the
/// save choice a save confirmation highlights. The installed message box images of a room's tileset picture each form
/// under its <see cref="FrameName"/>, and the room's message box sheet draws the image of the form shown by the form
/// (#1536), so the form lives with the message identity and the sheet, below the message box that shows it.
/// </summary>
/// <param name="Message">The message shown.</param>
/// <param name="Button">
/// The button the panel's glyph shows (one of the message box's glyph buttons), <see cref="ControllerButton.None"/>
/// when its action is bound to no button the glyph can show, or null for a message that names no button.
/// </param>
/// <param name="ChoiceYes">Whether a save confirmation highlights YES, or null for a message that offers no choice.</param>
public readonly record struct ShownMessage(GameplayMessageId Message, ControllerButton? Button, bool? ChoiceYes)
{
    /// <summary>The name of the installed image of this form.</summary>
    public string FrameName => $"{Message}{ButtonName}{ChoiceName}";

    /// <summary>The button part of <see cref="FrameName"/>.</summary>
    private string ButtonName => Button is not { } button ? ""
        : button == ControllerButton.None ? ".Unbound"
        : $".{button}";

    /// <summary>The choice part of <see cref="FrameName"/>.</summary>
    private string ChoiceName => ChoiceYes switch
    {
        null => "",
        true => ".Yes",
        false => ".No",
    };
}
