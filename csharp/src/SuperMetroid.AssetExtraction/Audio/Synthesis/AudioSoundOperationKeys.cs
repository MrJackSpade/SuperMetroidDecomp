using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// The installed key of each <see cref="AudioSoundOperation"/> (#2548), the operation name the installed audio manifest
/// spells a sound-effect instruction by, written out so renaming a member never changes it.
/// </summary>
internal static class AudioSoundOperationKeys
{
    /// <summary>Each AudioSoundOperation member's installed key.</summary>
    internal static InstalledKeys<AudioSoundOperation> Installed { get; } = new(new Dictionary<AudioSoundOperation, string>
    {
        [AudioSoundOperation.SetAdsr] = "setAdsr",
        [AudioSoundOperation.PitchSlideLegato] = "pitchSlideLegato",
        [AudioSoundOperation.PitchSlide] = "pitchSlide",
        [AudioSoundOperation.End] = "end",
        [AudioSoundOperation.BeginRepeat] = "beginRepeat",
        [AudioSoundOperation.RepeatForever] = "repeatForever",
        [AudioSoundOperation.EndRepeat] = "endRepeat",
        [AudioSoundOperation.EnableNoise] = "enableNoise",
        [AudioSoundOperation.PlayNote] = "playNote",
    });
}
