using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Verification.Audio;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that the game plays an installed clip as the player edited it in place (#1533).</summary>
internal static class InstalledAudioClipEditsVerification
{
    /// <summary>The edited song: Green Brinstar track 3.</summary>
    private const int EditedTrack = 3;

    /// <summary>
    /// Exports a small stock fixture into an installation's audio directory, overwrites one song's installed WAV in
    /// place with a tone of the same length, and checks that a fresh load plays the tone, not the extracted samples.
    /// </summary>
    internal static void VerifyInPlaceAudioClipEdit()
    {
        ExtractedAudioAssetCatalog extracted = RepositoryInstallation.Installation.LoadAudio();
        using var scratch = new TestScratchDirectory("audio-clip-edit");
        var installation = new GameInstallation(scratch.Root);
        Directory.CreateDirectory(installation.AudioDirectory);
        AudioClipExporter.Export(
            extracted,
            installation.AudioDirectory,
            [new AudioClipExporter.MusicKey(AudioBank.GreenBrinstar, EditedTrack)],
            [
                new AudioClipExporter.SoundEffectKey(AudioBank.GreenBrinstar, SoundEffectLibrary.Library1, checked((byte)SoundEffectCommands.Command(Library1Sound.UnchargedPowerBeam))),
            ]);

        InstalledClipAudioLibrary extractedClips = installation.LoadAudioClips();
        MusicClipEntry song = extractedClips.GetMusic(MusicDataSet.GreenBrinstar, new MusicTrack(EditedTrack));
        short[] tone = new short[checked((int)(song.Clip.Frames * PcmSampleFormat.StereoChannelCount))];
        for (int sample = 0; sample < tone.Length; sample++)
            tone[sample] = unchecked((short)(sample * 37 % 20000 - 10000));
        AssertTrue(!tone.AsSpan().SequenceEqual(extractedClips.ReadClip(song.Clip)), "the tone differs from the extracted song");
        _ = PcmWaveFile.WriteHashedStereoPcm16(
            Path.Combine(installation.AudioDirectory, song.Clip.Path), extractedClips.Manifest.SampleRate, tone);

        InstalledClipAudioLibrary edited = installation.LoadAudioClips();
        AssertSequenceEqual(
            tone,
            edited.ReadClip(edited.GetMusic(MusicDataSet.GreenBrinstar, new MusicTrack(EditedTrack)).Clip),
            "the session plays the installed song as edited in place");
        Console.WriteLine("Installed audio clip edit: a song's WAV edited in place is what the session plays.");
    }

    /// <summary>Runs the in-place clip edit check from the command dispatcher.</summary>
    [VerificationCommand("--audio-clip-edits")]
    internal static int AudioClipEditsCommand()
    {
        Suite(nameof(VerifyInPlaceAudioClipEdit), () => VerifyInPlaceAudioClipEdit());
        return 0;
    }
}
