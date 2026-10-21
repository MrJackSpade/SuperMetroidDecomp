namespace SuperMetroid.Verification.Audio;

/// <summary>The frame and start of one start value.</summary>
/// <param name="Frame">The frame.</param>
/// <param name="Start">The start.</param>
internal readonly record struct StartValue(int Frame, AudioEvent.StartSoundEffect Start);
