using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>One channel of a never-ending sound effect, as the clip manifest stores it (#1305, #2560).</summary>
/// <param name="Clip">The rendered channel.</param>
/// <param name="LoopStart">The first frame of its loop, repeated to the clip's end.</param>
public sealed partial record InstalledLoopingSoundEffectLayer(AudioClipFile Clip, long LoopStart);
