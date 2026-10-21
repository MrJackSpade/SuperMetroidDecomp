
namespace SuperMetroid.Core.Audio;

/// <summary>A replacement audio clip and the file it was read from.</summary>
/// <param name="Clip">The clip.</param>
/// <param name="Path">The path.</param>
internal readonly record struct ClipReplacement(AudioClipFile Clip, string Path);
