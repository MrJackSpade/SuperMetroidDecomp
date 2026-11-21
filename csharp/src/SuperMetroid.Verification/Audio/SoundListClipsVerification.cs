using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;

namespace SuperMetroid.Verification.Audio;

/// <summary>
/// Confirms #2580's merges in the installed clips: each different sound has one name, so no two members of the sound
/// lists install the same sound: the same clip, or the same looping layers. The export writes identical PCM once, so two
/// names for one sound would share its files.
/// </summary>
internal static class SoundListClipsVerification
{
    /// <summary>Every member of the three sound lists installs clips no other member installs.</summary>
    internal static void VerifyEachSoundHasItsOwnClip()
    {
        ClipAudioLibrary clips = InstalledFixtures.AudioClips;
        SoundEffectId[] sounds =
        [
            .. Enum.GetValues<SamusSound>().Select(sound => (SoundEffectId)sound),
            .. Enum.GetValues<EnemySound>().Select(sound => (SoundEffectId)sound),
            .. Enum.GetValues<RoomSound>().Select(sound => (SoundEffectId)sound),
        ];
        // A sound is its clip, or the set of its looping layers: two sounds may share one layer, never the whole sound.
        var owners = new Dictionary<string, SoundEffectId>(StringComparer.Ordinal);
        var withoutClip = new List<SoundEffectId>();
        foreach (SoundEffectId sound in sounds)
        {
            string? audio = clips.GetSoundEffect(sound).Shape switch
            {
                OneShotSoundEffectClip { Clip: AudioClipFile clip } => clip.Path,
                OneShotSoundEffectClip => null,
                LoopingSoundEffectClip looping => string.Join(" + ", looping.Layers.Select(layer => $"{layer.Clip.Path} from {layer.LoopStart}")),
                var shape => throw new InvalidOperationException($"{sound} has an unknown shape {shape}."),
            };
            if (audio is null)
            {
                withoutClip.Add(sound);
                continue;
            }
            if (owners.TryGetValue(audio, out SoundEffectId owner))
                AssertTrue(false, $"{sound} installs {audio}, as {owner} does: one sound under two names");
            owners[audio] = sound;
        }
        Console.WriteLine(
            $"  Sound list clips: {sounds.Length} sounds install {owners.Count} different sounds, none under two names; " +
            $"installed without a clip: {(withoutClip.Count == 0 ? "none" : string.Join(", ", withoutClip))}.");
    }
}
