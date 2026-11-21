namespace SuperMetroid.Core.Audio;

/// <summary>
/// How a library accepts and starts a sound-effect request (#1305): its effect on the library's priority and low-health
/// mode, and which rejections it is exempt from. How its sound is layered was decided when the clips were rendered and is
/// read by nothing in the game (#1749).
/// </summary>
/// <param name="Priority">The library priority after the effect starts.</param>
/// <param name="LowHealthMode">The effect on the room library's low-health mode.</param>
/// <param name="IgnoresPriority">Whether the request is accepted while a high-priority sound plays.</param>
/// <param name="IgnoresLowHealthMode">Whether the request is accepted while low-health mode is active.</param>
[IndependentFlag("independent exemptions a clip carries: from the priority rule and from low-health mode")]
public sealed partial record SoundEffectClipPolicy(
    SoundEffectPriorityEffect Priority,
    LowHealthModeEffect LowHealthMode,
    bool IgnoresPriority,
    bool IgnoresLowHealthMode);
