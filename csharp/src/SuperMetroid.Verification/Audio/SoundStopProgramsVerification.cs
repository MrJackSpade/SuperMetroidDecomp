using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;

namespace SuperMetroid.Verification.Audio;

/// <summary>
/// Confirms #2580's split of the requests the game makes into sounds and stops against the cartridge's own sound
/// programs: every stop's channels play only notes at zero volume, the Maridia elevatube's request among them, and every
/// sound's channels play at least one note above it. The installed clips hold each stop as a one-shot with no clip.
/// </summary>
internal static class SoundStopProgramsVerification
{
    /// <summary>A note packet's operand that holds its volume, read by the driver after the instrument.</summary>
    private const int NoteVolumeOperand = 1;

    /// <summary>
    /// Every request the game makes, decoded from each library's command: a stop plays only zero-volume notes and installs
    /// no clip, a sound plays a note above zero volume.
    /// </summary>
    internal static void VerifyStopsPlayOnlySilentNotes()
    {
        ExtractedAudioAssetCatalog catalog = InstalledFixtures.Audio;
        Dictionary<string, AudioSoundProgramMetadata> programs =
            catalog.SoundPrograms.ToDictionary(program => program.Id, StringComparer.Ordinal);
        HashSet<SoundEffectId> stops =
        [
            .. Enum.GetValues<SamusSoundStop>().Select(stop => (SoundEffectId)stop),
            .. Enum.GetValues<EnemySoundStop>().Select(stop => (SoundEffectId)stop),
            .. Enum.GetValues<RoomSoundStop>().Select(stop => (SoundEffectId)stop),
        ];
        var stopsFound = new HashSet<SoundEffectId>();
        int sounds = 0;
        foreach (AudioSoundLibraryMetadata library in catalog.SoundLibraries)
        {
            SoundEffectLibrary requests = SoundLibraryNumbers.Decode(library.Library);
            foreach (AudioSoundEffectMetadata effect in library.Effects)
            {
                if (!SoundEffectCommands.Names(requests, effect.Command))
                    continue;
                SoundEffectId request = SoundEffectCommands.Decode(requests, effect.Command);
                int[] volumes =
                [
                    .. effect.ChannelPrograms
                        .SelectMany(program => programs[program].Instructions)
                        .Where(instruction => instruction.Operation == AudioSoundOperation.PlayNote)
                        .Select(instruction => (int)instruction.Arguments[NoteVolumeOperand]),
                ];
                AssertTrue(volumes.Length > 0, $"{request} plays a note");
                if (stops.Contains(request))
                {
                    AssertTrue(volumes.All(volume => volume == 0), $"{request}, a stop, plays only zero-volume notes");
                    AssertTrue(InstalledFixtures.AudioClips.GetSoundEffect(request).Shape is OneShotSoundEffectClip { Clip: null },
                        $"{request}, a stop, installs a one-shot with no clip");
                    stopsFound.Add(request);
                }
                else
                {
                    AssertTrue(volumes.Any(volume => volume != 0), $"{request}, a sound, plays a note above zero volume");
                    sounds++;
                }
            }
        }
        AssertTrue(stopsFound.SetEquals(stops), "every stop is a request of its library");
        Console.WriteLine(
            $"  Sound stops: {stops.Count} stops play only zero-volume notes and install no clip; {sounds} sounds each play a note above it.");
    }
}
