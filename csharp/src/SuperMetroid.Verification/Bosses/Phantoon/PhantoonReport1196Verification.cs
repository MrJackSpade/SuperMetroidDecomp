using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonFlameSoundVerification;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonAnimationsVerification;
using static SuperMetroid.Verification.Rooms.LevelData.SharedSlopeHeightsVerification;
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
        var state = enemies.PhantoonBehavior.Phantoon;
        enemies.BeginEnemySoundRequestFrame();
        (state ?? throw new InvalidOperationException("Verification requires state.")).Parts[state.Body.SlotIndex].FunctionTimer = 1;
        enemies.PhantoonBehavior.RunPhantoonStartingFlameSpawner(state.Body, state);
        enemies.CollectLegacyEnemyAudioRequests();
        AssertEqual(1, state.StartingFlamesSpawned, "intro producer actually spawns a starting flame");
        var audio = new CartridgeAudioState();
        var renderer = CartridgeAudioSessionAccess.CreateSynthesizing(RepositoryInstallation.Installation.LoadAudio());
        renderer.RenderFrame(audio.AdvanceFrame(renderer.Acknowledgements));
        renderer.RenderFrame([CartridgeAudioCommand.Upload(AudioBank.GreenBrinstar)]);
        foreach (var request in enemies.Audio.SoundRequests)
            audio.QueueSound(request.SoundEffect, request.MaximumQueued);
        // The handshake rewrites a request each update until the driver echoes it ($82:8A55), so
        // the CPU writes it twice; the driver starts a sound only on a changed latch, so count the
        // sound starts the session publishes.
        var starts = new List<SoundEffectId>();
        for (int frame = 0; frame < 8; frame++)
        {
            IReadOnlyList<AudioEvent> events = renderer.Advance(audio.AdvanceFrame(renderer.Acknowledgements));
            starts.AddRange(events.Where(audioEvent => audioEvent.Kind == AudioEventKind.StartSoundEffect).Select(audioEvent => audioEvent.SoundEffect));
        }
        AssertEqual(1, starts.Count(sound => sound == new SoundEffectId(SoundEffectLibrary.Library3, 0x1d)),
            "opening blue flame starts native sound 1D in library three once");
        AssertEqual(0, starts.Count(sound => sound == new SoundEffectId(SoundEffectLibrary.Library2, 0x1d)),
            "opening flame never starts materialization-library 1D");
        AssertEqual(6, enemies.Audio.SoundRequests.Single().MaximumQueued, "opening flame uses native Max6");
        Suite(nameof(VerifyPhantoonFlameSound), () => VerifyPhantoonFlameSound());
        Console.WriteLine("Phantoon intro flame: actual spawn sends library 3 / 1D / Max6 through the production audio queue.");
    }

    /// <summary>Checks that death-wave setup clears retained intro offsets before rendered BG2 scanlines.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    internal static void VerifyPhantoonDeathWaveInitialization()
    {
        var enemies = CreatePhantoonSystem();
        var boss = enemies.PhantoonBehavior.Phantoon;
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
        enemies.PhantoonBehavior.ResolvePhantoonShotReaction(boss.Body, boss, (ushort)0x0100, (ushort)100);
        PhantoonBehavior.BeginPhantoonWavyMosaicDeath(boss.Body, boss);
        wave.Step(boss);
        wave.LatchDisplay();
        AssertEqual(PhantoonWaveRomData.InitialPhase, wave.Phase, "first death HDMA call remains setup-only");

        // A vertical BG2 stripe makes any stale per-line X offset visibly split the
        // body. Render through the same scanline sampler used by live gameplay.
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        cgram.SetColor(1, new Bgr555(31, 0, 0));
        var tile = new byte[32];
        for (int row = 0; row < 8; row++) tile[row * 2] = 255;
        vram.LoadBytes(32, tile);
        var map = new ushort[1024];
        for (int row = 0; row < 32; row++) map[row * 32 + 8] = 1;
        vram.ExecuteWordTransfer(map, 0x4800, 1);
        var pixels = SnesBgTilemapRenderer.Render4BppViewport(vram, cgram, new BgVramAddresses { TilemapBaseWord = 0x4800, CharacterBaseWord = 0 }, new BgViewport { Scroll = new Position16<BgPlanePixels>(13, 32), Size = new Extent16<ScreenPixels>(256, 192), TilemapSize = new Extent16<TilemapCells>(32, 32), HorizontalScrollByLine = wave.DisplayedScrolls });
        for (int line = 0; line < 192; line++)
            AssertEqual(new Rgba32(255, 0, 0, 255), pixels[line * 256 + 51],
                $"first death-wave BG2 stripe stays aligned on rendered scanline {line}");
        AssertTrue((wave.DisplayedScrolls ?? throw new InvalidOperationException("Verification requires wave.DisplayedScrolls.")).All(scroll => scroll == 13), "every first-frame death scroll equals native BG2 X");
        boss.Parts[boss.Mouth.SlotIndex].WaveAmplitude = 3072;
        wave.Step(boss);
        AssertEqual((ushort)14, wave.Phase, "following HDMA pass resumes native phase advancement");
        AssertTrue(wave.ScrollCycle.Any(scroll => scroll != 13), "following pass rebuilds the animated wave");
        Console.WriteLine("Phantoon death: retained intro data is initialized before setup-only death frame; all rendered rows align before wave advancement.");
    }

    /// <summary>Runs the actual opening flame spawn and audio-queue check.</summary>
    [VerificationCommand("--phantoon-intro-flame-sound")]
    internal static int PhantoonIntroFlameSoundCommand()
    {
        Suite(nameof(VerifyPhantoonIntroFlameSound), () => VerifyPhantoonIntroFlameSound());
        return 0;
    }

    /// <summary>Runs the death-wave initialization and BG2 stripe check.</summary>
    [VerificationCommand("--phantoon-death-wave-initialization")]
    internal static int PhantoonDeathWaveInitializationCommand()
    {
        Suite(nameof(VerifyPhantoonDeathWaveInitialization), () => VerifyPhantoonDeathWaveInitialization());
        return 0;
    }
}
