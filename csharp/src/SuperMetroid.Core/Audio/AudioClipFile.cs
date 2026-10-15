namespace SuperMetroid.Core.Audio;

/// <summary>
/// One rendered clip file (#1305): an uncompressed 16-bit stereo WAV at the native S-DSP rate,
/// its path relative to the audio root, its SHA-256 and its length in stereo frames.
/// </summary>
/// <param name="Path">Relative path beneath the audio root.</param>
/// <param name="Sha256">Uppercase hexadecimal SHA-256 of the whole file.</param>
/// <param name="Frames">Stereo frames in the clip.</param>
public sealed record AudioClipFile(string Path, string Sha256, long Frames);
