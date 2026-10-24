using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonFlameSoundVerification;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonAnimationsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation;
namespace SuperMetroid.Verification.Bosses.Phantoon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Phantoon's opening flame sound and first death-wave display state.</summary>
internal static class PhantoonReport1196Verification
{
    /// <summary>Requires the first blue flame to queue sound $1D in library three with native Max6.</summary>
    internal static void VerifyPhantoonIntroFlameSound()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = CreatePhantoonSystem();
        var state = enemies.PhantoonBehavior.Combat.Phantoon;
        enemies.BeginEnemySoundRequestFrame();
        (state ?? throw new InvalidOperationException("Verification requires state.")).Parts[state.Body.SlotIndex].FunctionTimer = 1;
        enemies.PhantoonBehavior.Opening.RunPhantoonStartingFlameSpawner(state.Body, state);
        enemies.CollectLegacyEnemyAudioRequests();
        AssertEqual(1, state.StartingFlamesSpawned, "intro producer actually spawns a starting flame");
        var audio = new AudioQueues();
        var renderer = AudioSessionAccess.CreateSynthesizing(RepositoryInstallation.Installation.LoadAudio());
        renderer.RenderFrame(audio.AdvanceFrame(renderer.Acknowledgements));
        renderer.RenderFrame([new AudioRequest.ChangeMusicDataSet(MusicDataSet.GreenBrinstar)]);
        foreach (var request in enemies.Audio.SoundRequests)
            audio.QueueSound(request.SoundEffect, request.MaximumQueued);
        // The handshake rewrites a request each update until the driver echoes it ($82:8A55), so
        // the CPU writes it twice; the driver starts a sound only on a changed latch, so count the
        // sound starts the session publishes.
        var starts = new List<SoundEffectId>();
        for (int frame = 0; frame < 8; frame++)
        {
            IReadOnlyList<AudioEvent> events = renderer.Advance(audio.AdvanceFrame(renderer.Acknowledgements));
            starts.AddRange(events.OfType<AudioEvent.StartSoundEffect>().Select(audioEvent => audioEvent.SoundEffect));
        }
        AssertEqual(1, starts.Count(sound => sound == Library3Sound.PhantoonFlame),
            "opening blue flame starts native sound 1D in library three once");
        AssertEqual(0, starts.Count(sound => sound == Library2Sound.DachoraCry),
            "opening flame never starts materialization-library 1D");
        AssertEqual(6, enemies.Audio.SoundRequests.Single().MaximumQueued.Requests, "opening flame uses native Max6");
        Suite(nameof(VerifyPhantoonFlameSound), () => VerifyPhantoonFlameSound());
        Console.WriteLine("Phantoon intro flame: actual spawn sends library 3 / 1D / Max6 through the production audio queue.");
    }

    /// <summary>Checks that death-wave setup clears retained intro offsets before the displayed BG2 scanlines.</summary>
    internal static void VerifyPhantoonDeathWaveInitialization()
    {
        var enemies = CreatePhantoonSystem();
        var boss = enemies.PhantoonBehavior.Combat.Phantoon;
        (boss ?? throw new InvalidOperationException("Verification requires boss.")).Bg2Scroll = boss.Bg2Scroll.WithX(9);
        boss.Parts[boss.Mouth.SlotIndex].WaveAmplitude = 3072;
        boss.Parts[boss.Mouth.SlotIndex].WavyPhaseDelta = 8;
        var wave = boss.Wave;
        wave.Begin(PhantoonWaveRomData.IntroMode);
        wave.Step(boss);
        wave.Step(boss);
        AssertTrue(wave.ScrollCycle.CountDistinct() > 1, "fixture retains a used introductory wave");
        boss.Parts[boss.Eye.SlotIndex].WaveMode = 0;
        wave.Step(boss);
        boss.Bg2Scroll = boss.Bg2Scroll.WithX(13);
        boss.Body.Health = 0;
        enemies.PhantoonBehavior.Combat.ResolvePhantoonShotReaction(boss.Body, boss, 0x0100, 100);
        PhantoonDeath.BeginPhantoonWavyMosaicDeath(boss.Body, boss);
        wave.Step(boss);
        wave.LatchDisplay();
        AssertEqual(PhantoonWaveRomData.InitialPhase, wave.Phase, "first death HDMA call remains setup-only");

        // Any stale per-line X offset would split the body: every displayed line takes BG2's own X.
        AssertTrue((wave.DisplayedWave?.GameplayScrolls().ToArray() ?? throw new InvalidOperationException("Verification requires a latched Phantoon wave.")).All(scroll => scroll == 13), "every first-frame death scroll equals native BG2 X");
        boss.Parts[boss.Mouth.SlotIndex].WaveAmplitude = 3072;
        wave.Step(boss);
        AssertEqual(14, wave.Phase, "following HDMA pass resumes native phase advancement");
        AssertTrue(wave.ScrollCycle.Any(scroll => scroll != 13), "following pass rebuilds the animated wave");
        Console.WriteLine("Phantoon death: retained intro data is initialized before setup-only death frame; every displayed line aligns before wave advancement.");
    }

    /// <summary>Runs the actual opening flame spawn and audio-queue check.</summary>
    [VerificationCommand("--phantoon-intro-flame-sound")]
    internal static int PhantoonIntroFlameSoundCommand()
    {
        Suite(nameof(VerifyPhantoonIntroFlameSound), () => VerifyPhantoonIntroFlameSound());
        return 0;
    }

    /// <summary>Runs the death-wave initialization and BG2 alignment check.</summary>
    [VerificationCommand("--phantoon-death-wave-initialization")]
    internal static int PhantoonDeathWaveInitializationCommand()
    {
        Suite(nameof(VerifyPhantoonDeathWaveInitialization), () => VerifyPhantoonDeathWaveInitialization());
        return 0;
    }
}
