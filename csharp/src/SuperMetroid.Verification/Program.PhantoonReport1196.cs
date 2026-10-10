using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyPhantoonIntroFlameSound()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = CreatePhantoonInstructionSystem(bus);
        var state = enemies.Phantoon!;
        typeof(RoomEnemySystem).GetMethod("BeginEnemySoundRequestFrame", flags)!.Invoke(enemies, null);
        state.Body.VariableE = 1;
        typeof(RoomEnemySystem).GetMethod("RunPhantoonStartingFlameSpawner", flags)!.Invoke(enemies, [state.Body, state]);
        typeof(RoomEnemySystem).GetMethod("CollectLegacyEnemyAudioRequests", flags)!.Invoke(enemies, null);
        AssertEqual(1, state.StartingFlamesSpawned, "intro producer actually spawns a starting flame");
        var audio = new CartridgeAudioState();
        var renderer = new CartridgeAudioRenderer(RepositoryInstallation.Installation.LoadAudio());
        renderer.RenderFrame(audio.AdvanceFrame(bus, renderer.ReadAcknowledgements()));
        renderer.RenderFrame([CartridgeAudioCommand.Upload(AudioUploadAddresses.GreenBrinstar)]);
        foreach (var request in enemies.SoundRequests)
            audio.QueueSound(request.SoundEffect, request.MaximumQueued);
        var writes = new List<CartridgeAudioCommand>();
        for (int frame = 0; frame < 8; frame++)
        {
            var commands = audio.AdvanceFrame(bus, renderer.ReadAcknowledgements());
            writes.AddRange(commands.Where(command => command.Kind == CartridgeAudioCommandKind.WritePort));
            renderer.RenderFrame(commands);
        }
        AssertEqual(1, writes.Count(command => command.Port == 3 && command.Value == 0x1d),
            "opening blue flame sends native sound 1D to library three");
        AssertEqual(0, writes.Count(command => command.Port == 2 && command.Value == 0x1d),
            "opening flame never sends materialization-library 1D");
        AssertEqual(6, enemies.SoundRequests.Single().MaximumQueued, "opening flame uses native Max6");
        Suite(nameof(VerifyPhantoonFlameSound), () => VerifyPhantoonFlameSound());
        Console.WriteLine("Phantoon intro flame: actual spawn sends library 3 / 1D / Max6 through the production audio queue.");
    }

    private static void VerifyPhantoonDeathWaveInitialization()
    {
        var enemies = CreatePhantoonInstructionSystem(new SlopeHeightNoReadBus());
        var boss = enemies.Phantoon!;
        boss.Bg2HorizontalScroll = 9;
        boss.Mouth!.VariableD = 3072;
        boss.Mouth.VariableF = 8;
        var wave = boss.Wave;
        wave.Begin(PhantoonWaveRomData.IntroMode);
        wave.Step(boss);
        wave.Step(boss);
        AssertTrue(wave.ScrollCycle.ToArray().Distinct().Count() > 1, "fixture retains a used introductory wave");
        boss.Eye!.Parameter1 = 0;
        wave.Step(boss);
        boss.Bg2HorizontalScroll = 13;
        boss.Body.Health = 0;
        typeof(RoomEnemySystem).GetMethod("ResolvePhantoonShotReaction", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(enemies, [boss.Body, boss, (ushort)0x0100, (ushort)100]);
        typeof(RoomEnemySystem).GetMethod("BeginPhantoonWavyMosaicDeath", BindingFlags.Static | BindingFlags.NonPublic)!
            .Invoke(null, [boss.Body, boss]);
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
        var pixels = SnesBgTilemapRenderer.Render4BppViewport(vram, cgram, 0x4800, 0, 13, 32, 256, 192,
            tilemapWidthInTiles: 32, tilemapHeightInTiles: 32, horizontalScrollByLine: wave.DisplayedScrolls);
        for (int line = 0; line < 192; line++)
            AssertEqual(new Rgba32(255, 0, 0, 255), pixels[line * 256 + 51],
                $"first death-wave BG2 stripe stays aligned on rendered scanline {line}");
        AssertTrue(wave.DisplayedScrolls!.All(scroll => scroll == 13), "every first-frame death scroll equals native BG2 X");
        boss.Mouth.VariableD = 3072;
        wave.Step(boss);
        AssertEqual((ushort)14, wave.Phase, "following HDMA pass resumes native phase advancement");
        AssertTrue(wave.ScrollCycle.ToArray().Any(scroll => scroll != 13), "following pass rebuilds the animated wave");
        Console.WriteLine("Phantoon death: retained intro data is initialized before setup-only death frame; all rendered rows align before wave advancement.");
    }
}
