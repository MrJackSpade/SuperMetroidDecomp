using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>The menu choices a selected game's runtime starts with.</summary>
/// <param name="JapaneseText">Whether message boxes and room names use the Japanese text set.</param>
/// <param name="ControllerBindings">The controller layout chosen in the options menu.</param>
/// <param name="MoonwalkEnabled">Whether moonwalking is enabled.</param>
/// <param name="IconCancelEnabled">Whether the HUD selection cancels automatically.</param>
internal sealed record SelectedGameplayOptions(
    bool JapaneseText,
    ControllerBindings ControllerBindings,
    bool MoonwalkEnabled,
    bool IconCancelEnabled);
