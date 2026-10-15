namespace SuperMetroid.Core.Audio;

/// <summary>
/// A cut clip fading out over one key-off release (#1305): the clip, its loop, its frame zero and
/// how many release frames remain.
/// </summary>
/// <param name="Clip">The clip file.</param>
/// <param name="LoopStart">Its loop start, or null when it does not loop.</param>
/// <param name="Start">Absolute frame of its frame zero.</param>
/// <param name="Remaining">Release frames left; the gain is <c>Remaining / ReleaseFrames</c>.</param>
internal sealed record ReleasingClip(AudioClipFile Clip, long? LoopStart, long Start, int Remaining);
