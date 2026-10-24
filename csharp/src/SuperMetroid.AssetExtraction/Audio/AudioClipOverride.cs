namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// One user replacement clip (#1305 Q3): a finished 16-bit stereo WAV at the native rate that
/// replaces a song or a sound effect by its stable identity, in every bank context for a sound.
/// </summary>
/// <param name="Id">
/// The replaced clip: <c>music-{data index:x2}-track-{nn}</c> for a song, as the extracted
/// catalog names tracks, or <c>sfx-{library}-{command:x2}</c> for a sound effect.
/// </param>
/// <param name="File">The replacement WAV, relative to the override directory.</param>
/// <param name="LoopStart">The frame the replacement loops back to, or null when it plays once. A looping sound needs one; a one-shot sound must have none.</param>
public sealed record AudioClipOverride(string Id, string File, long? LoopStart);
