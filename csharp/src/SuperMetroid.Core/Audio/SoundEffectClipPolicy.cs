namespace SuperMetroid.Core.Audio;

/// <summary>
/// How a library accepts and starts a sound-effect request (#1305): the voices it plays on, its
/// effect on the library's priority and low-health mode, and which rejections it is exempt from.
/// </summary>
/// <param name="Voices">Voices the effect plays on.</param>
/// <param name="Priority">The library priority after the effect starts.</param>
/// <param name="LowHealthMode">The effect on library 3's low-health mode.</param>
/// <param name="IgnoresPriority">Whether the request is accepted while a high-priority sound plays.</param>
/// <param name="IgnoresLowHealthMode">Whether the request is accepted while low-health mode is active.</param>
[IndependentFlag("independent exemptions a clip carries: from the priority rule and from low-health mode")]
public sealed record SoundEffectClipPolicy(
    int Voices,
    SoundEffectPriorityEffect Priority,
    LowHealthModeEffect LowHealthMode,
    bool IgnoresPriority,
    bool IgnoresLowHealthMode);
