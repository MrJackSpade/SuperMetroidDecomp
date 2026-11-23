using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>The menu choices a selected game's runtime starts with.</summary>
/// <param name="Language">The language message boxes and room names are written in.</param>
/// <param name="ControllerBindings">The controller layout chosen in the options menu.</param>
/// <param name="MoonwalkEnabled">Whether moonwalking is enabled.</param>
/// <param name="IconCancelEnabled">Whether the HUD selection cancels automatically.</param>
internal sealed partial record SelectedGameplayOptions(
    TextLanguage Language,
    ControllerBindings ControllerBindings,
    bool MoonwalkEnabled,
    bool IconCancelEnabled)
{
    /// <summary>
    /// The options a game starts with before the options menu first closes: English text, the default bindings, and
    /// moonwalking and icon cancelling off.
    /// </summary>
    internal static SelectedGameplayOptions Default { get; } = new(TextLanguage.English, ControllerBindings.Default, false, false);
}
