namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// One user replacement clip (#1305 Q3): a finished 16-bit stereo WAV at the native rate that
/// replaces a song or a sound effect by its stable identity, in every bank context for a sound.
/// </summary>
/// <param name="Id">
/// The replaced clip: <c>music-{MusicDataSet}-track-{nn}</c> for a song, or <c>sfx-{Library}Sound.{Name}</c> for a
/// sound effect (for example <c>music-GreenBrinstar-track-05</c>, <c>sfx-Library3Sound.SpeedBooster</c>).
/// </param>
/// <param name="File">The replacement WAV, relative to the override directory.</param>
/// <param name="LoopStart">The frame the replacement loops back to, or null when it plays once. A looping sound needs one; a one-shot sound must have none.</param>
public sealed record AudioClipOverride(string Id, string File, long? LoopStart);
