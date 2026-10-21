namespace SuperMetroid.Verification.Audio;

/// <summary>The frame and sound effect of one verify spc command ports match emulated driver case.</summary>
/// <param name="Frame">The frame.</param>
/// <param name="SoundEffect">The sound effect.</param>
internal readonly record struct SoundAcceptanceClockCase(int Frame, SoundEffectId SoundEffect);
