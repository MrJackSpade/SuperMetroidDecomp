namespace SuperMetroid.Core.Audio;

/// <summary>
/// One rendered clip file (#1305): an uncompressed stereo WAV of signed samples from -32768 through 32767 at the clips' 32,040-Hz rate,
/// its path relative to the audio root and its length in stereo frames.
/// </summary>
/// <param name="Path">Relative path beneath the audio root.</param>
/// <param name="Frames">Stereo frames in the clip.</param>
public sealed record AudioClipFile(string Path, long Frames);
