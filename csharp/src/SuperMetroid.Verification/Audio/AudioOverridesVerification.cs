using SuperMetroid.Core.Frontend;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Verification.Audio;
using SuperMetroid.Verification.Harness;

/// <summary>Checks persistent audio overrides and their contribution to installed content identity.</summary>
internal static class AudioOverridesVerification
{
    /// <summary>The replaced song: Green Brinstar track 3.</summary>
    private const int ReplacedTrack = 3;

    /// <summary>Frames in the synthetic replacement tone.</summary>
    private const int ToneFrames = 2000;

    /// <summary>The tone's loop start.</summary>
    private const long ToneLoop = 100;

    /// <summary>
    /// Checks clip-level overrides (#1305 Q3) on a small exported stock fixture: initialization
    /// outside the replaceable content, a song replaced and played, untouched sounds, survival of a
    /// stock repair, rejected edits that leave the override in place, and loud failures for corrupt
    /// stock or a missing replacement.
    /// </summary>
    internal static void VerifyPersistentAudioOverrides()
    {
        ExtractedAudioAssetCatalog extracted = RepositoryInstallation.Installation.LoadAudio();
        using var rootScratch = new TestScratchDirectory("audio-override");
        var installation = new GameInstallation(rootScratch.Root);
        void ExportStock()
        {
            Directory.CreateDirectory(installation.AudioDirectory);
            AudioClipExporter.Export(
                extracted,
                installation.AudioDirectory,
                [new AudioClipExporter.MusicKey(AudioBank.GreenBrinstar, ReplacedTrack)],
                [
                    new AudioClipExporter.SoundEffectKey(AudioBank.GreenBrinstar, SoundEffectLibrary.Library1, checked((byte)SoundEffectLibrary1Sounds.PowerBeam.Value)),
                    new AudioClipExporter.SoundEffectKey(AudioBank.GreenBrinstar, SoundEffectLibrary.Library3, checked((byte)SoundEffectLibrary3Sounds.SpeedBoosterEcho.Value)),
                ]);
        }
        ExportStock();

        ClipAudioCatalog stock = installation.LoadAudioClips();
        AssertEqual(stock.ContentIdentity, installation.LoadAudioClips().ContentIdentity, "stock clip identity is stable across reload");
        string selectedDirectory = AudioAssetOverrideInstaller.Initialize(installation);
        AssertEqual(
            Path.GetFullPath(installation.AudioOverrideDirectory),
            Path.GetFullPath(selectedDirectory),
            "audio override initializes outside replaceable game content");
        AssertThrows<IOException>(
            () => AudioAssetOverrideInstaller.Initialize(installation),
            "audio override initialization refuses to overwrite user content");
        AssertEqual(
            stock.GetMusic(AudioBank.GreenBrinstar, ReplacedTrack).Clip,
            installation.LoadAudioClips().GetMusic(AudioBank.GreenBrinstar, ReplacedTrack).Clip,
            "an empty override selects the stock clips");
        AssertEqual(stock.ContentIdentity, installation.LoadAudioClips().ContentIdentity, "an empty override keeps the stock identity");

        short[] tone = new short[ToneFrames * 2];
        for (int sample = 0; sample < tone.Length; sample++)
            tone[sample] = unchecked((short)(sample * 37 % 20000 - 10000));
        string tonePath = Path.Combine(rootScratch.Root, "tone.wav");
        PcmWaveFile.WriteStereoPcm16(tonePath, ClipSampleRate(stock), tone);
        string songId = ClipAudioOverrides.MusicId(AudioBank.GreenBrinstar, ReplacedTrack);
        AudioAssetOverrideInstaller.Replace(installation, songId, tonePath, ToneLoop);

        ClipAudioCatalog selected = installation.LoadAudioClips();
        MusicClipEntry song = selected.GetMusic(AudioBank.GreenBrinstar, ReplacedTrack);
        AssertEqual(ToneFrames, song.Clip.Frames, "the replacement's length selects the song");
        AssertEqual<long?>(ToneLoop, song.LoopStart, "the replacement's loop selects the song");
        AssertSequenceEqual(tone, selected.ReadClip(song.Clip), "the session plays the replacement's samples");
        AssertTrue(selected.ContentIdentity != stock.ContentIdentity, "a replacement changes the selected audio identity");
        AssertSequenceEqual(
            stock.GetSoundEffect(AudioBank.GreenBrinstar, SoundEffectLibrary.Library1, SoundEffectLibrary1Sounds.PowerBeam.Value).Layers.Select(layer => layer.Clip),
            selected.GetSoundEffect(AudioBank.GreenBrinstar, SoundEffectLibrary.Library1, SoundEffectLibrary1Sounds.PowerBeam.Value).Layers.Select(layer => layer.Clip),
            "unreplaced sounds stay stock");

        // A stock repair replaces only game/audio; the persistent override remains selected.
        Directory.Delete(installation.AudioDirectory, recursive: true);
        ExportStock();
        AssertEqual(selected.ContentIdentity, installation.LoadAudioClips().ContentIdentity, "stock repair preserves the selected audio");

        AssertThrows<InvalidDataException>(
            () => AudioAssetOverrideInstaller.Replace(installation, "music-ff-track-99", tonePath, null),
            "an unknown song is rejected");
        AssertThrows<InvalidDataException>(
            () => AudioAssetOverrideInstaller.Replace(installation,
                ClipAudioOverrides.SoundEffectId(SoundEffectLibrary.Library3, SoundEffectLibrary3Sounds.SpeedBoosterEcho.Value), tonePath, null),
            "a never-ending sound's replacement needs a loop");
        string monoPath = Path.Combine(rootScratch.Root, "mono.wav");
        PcmWaveFile.WriteMonoPcm16(monoPath, ClipSampleRate(stock), tone);
        AssertThrows<InvalidDataException>(
            () => AudioAssetOverrideInstaller.Replace(installation, songId, monoPath, null),
            "a mono replacement is rejected");
        AssertEqual(selected.ContentIdentity, installation.LoadAudioClips().ContentIdentity, "rejected edits leave the override in place");

        string stockManifestPath = Path.Combine(installation.AudioDirectory, ClipAudioCatalog.ManifestFileName);
        File.AppendAllText(stockManifestPath, "corrupt");
        AssertThrows<System.Text.Json.JsonException>(
            () => installation.LoadAudioClips(),
            "a valid override cannot conceal corrupt stock");
        Directory.Delete(installation.AudioDirectory, recursive: true);
        ExportStock();

        File.Delete(Path.Combine(installation.AudioOverrideDirectory, "clips", songId + ".wav"));
        AssertThrows<FileNotFoundException>(
            () => installation.LoadAudioClips(),
            "an invalid override never silently falls back to stock");

        Suite(nameof(VerifyGameContentIdentityComposition), () => VerifyGameContentIdentityComposition());

        Console.WriteLine(
            "Persistent audio overrides: isolated initialization, clip replacement, stock repair, " +
            "rejected edits and loud corruption failures pass.");
    }

    /// <summary>The clips' sample rate.</summary>
    private static int ClipSampleRate(ClipAudioCatalog clips) => clips.Manifest.SampleRate;

    /// <summary>Checks component digests and build identity affect compatibility warnings independently.</summary>
    internal static void VerifyGameContentIdentityComposition()
    {
        string audio = new('A', 64);
        string maps = new('B', 64);
        string projectiles = new('C', 64);
        Guid definitions = Guid.Parse("89abcdef-0123-4567-89ab-cdef01234567");
        GameContentIdentity baseline = GameContentIdentity.Create(audio, maps, projectiles, definitions);
        AssertEqual(
            baseline,
            GameContentIdentity.Create(audio, maps, projectiles, definitions),
            "aggregate installation identity is deterministic");
        AssertTrue(
            baseline.CompositeSha256 != GameContentIdentity.Create(new string('D', 64), maps, projectiles, definitions).CompositeSha256,
            "audio identity invalidates aggregate installation identity");
        AssertTrue(
            baseline.CompositeSha256 != GameContentIdentity.Create(audio, new string('D', 64), projectiles, definitions).CompositeSha256,
            "map identity invalidates aggregate installation identity");
        AssertTrue(
            baseline.CompositeSha256 != GameContentIdentity.Create(audio, maps, new string('D', 64), definitions).CompositeSha256,
            "projectile identity invalidates aggregate installation identity");
        AssertTrue(
            baseline.CompositeSha256 != GameContentIdentity.Create(audio, maps, projectiles, Guid.Empty).CompositeSha256,
            "compiled-definition build invalidates aggregate installation identity");
        AssertThrows<ArgumentException>(
            () => GameContentIdentity.Create("not-a-sha", maps, projectiles, definitions),
            "aggregate installation identity rejects malformed component digest");

        GameContentIdentitySnapshot recorded = baseline.ToSnapshot();
        AssertEqual(0, baseline.GetCompatibilityWarnings(recorded, "test artifact").Count,
            "matching recording identity produces no compatibility warning");
        AssertTrue(
            baseline.GetCompatibilityWarnings(null, "test artifact").Single().Contains("Legacy", StringComparison.Ordinal),
            "legacy recording explains unavailable installed-content comparison");
        AssertTrue(
            baseline.GetCompatibilityWarnings(recorded with
            {
                AudioContentSha256 = Convert.FromHexString(new string('D', 64)),
            }, "test artifact").Single().Contains("audio", StringComparison.Ordinal),
            "audio drift receives a component-specific warning");
        AssertTrue(
            baseline.GetCompatibilityWarnings(recorded with
            {
                CompiledDefinitionsBuildId = Guid.Empty,
            }, "test artifact").Single().Contains("Compiled gameplay definitions", StringComparison.Ordinal),
            "compiled-definition drift receives a specific warning");
        AssertTrue(
            baseline.GetCompatibilityWarnings(recorded with
            {
                CompositeSha256 = Convert.FromHexString(new string('D', 64)),
            }, "test artifact").Single().Contains("Aggregate", StringComparison.Ordinal),
            "unexplained aggregate drift cannot pass component comparison");
    }

    /// <summary>Runs persistent audio override checks from the command dispatcher.</summary>
    [VerificationCommand("--audio-overrides")]
    internal static int AudioOverridesCommand()
    {
        Suite(nameof(VerifyPersistentAudioOverrides), () => VerifyPersistentAudioOverrides());
        return 0;
    }
}
