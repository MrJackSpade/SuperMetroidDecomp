using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rooms;

internal static class ColosseumSandFadeAudit
{
    internal static int Run(string installationRoot)
    {
        var installation = new GameInstallation(installationRoot);
        var bus = installation.OpenRuntimeAddressSpace();
        var game = new SuperMetroidGame(bus, gameOptions: null, renderGameplayFrames: false);
        InstalledInputReplay.Bind(game, installation);
        typeof(SuperMetroidGame).GetMethod("CreateGameplayRuntime", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(game, [false]);
        var runtime = game.RuntimeForVerification!;
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xd913);
        var level = runtime.LevelData!;
        bool found = false;
        for (int index = 0; index < level.ForegroundEntries.Length; index++)
        {
            var block = level.GetCollisionBlockByIndex(index);
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            var candidate = level.ResolveDoorCollision(bus, block.Behavior, runtime.Samus!.Pose, publishDoorSideEffects: false);
            if (candidate.Pointer != 0xa8e8) continue;
            level.ResolveDoorCollision(bus, block.Behavior, runtime.Samus.Pose, publishDoorSideEffects: true);
            found = true; break;
        }
        if (!found) throw new InvalidDataException("Colosseum entry door not found.");
        var transition = new DoorTransitionState();
        transition.Begin(runtime);
        var audio = new CartridgeAudioState();
        CartridgePaletteTransition? expectedFade = null;
        var expectedCgram = new SnesCgram();
        int fadeChecks = 0;
        for (int frame = 0; transition.IsActive && frame < 320; frame++)
        {
            var phase = transition.Phase;
            if (phase == DoorTransitionPhase.HandleTransition)
            {
                var fade = typeof(DoorTransitionState).GetField("paletteTransition", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(transition)!;
                var target = (ushort[])typeof(CartridgePaletteTransition).GetField("target", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(fade)!;
                expectedFade = new CartridgePaletteTransition(target, 12);
                for (int color = 0; color < 256; color++) expectedCgram.SetColor(color, runtime.Cgram.Colors[color]);
            }
            transition.Step(runtime, audio, 0, SuperMetroid.Core.Runtime.LagFreeDoorLoaderProgress.Instance);
            if (phase == DoorTransitionPhase.FadeInDestinationPalette) expectedFade!.Step(expectedCgram);
            if (expectedFade is not null)
            {
                for (int color = 36; color < 44; color++)
                    if (runtime.Cgram.Colors[color] != expectedCgram.Colors[color])
                        throw new InvalidDataException($"Sand bypassed room fade: {phase}, color {color}, expected {expectedCgram.Colors[color]:X4}, actual {runtime.Cgram.Colors[color]:X4}.");
                fadeChecks++;
            }
            if (runtime.ActiveRoom!.Pointer == 0xd72a)
                Console.WriteLine($"{frame} {phase} -> {transition.Phase} sand={string.Join(' ', runtime.Cgram.Colors.Slice(36,8).ToArray().Select(c => c.ToString("X4")))}");
        }
        if (transition.IsActive) throw new InvalidDataException("Colosseum door transition did not finish.");
        ushort[] completedSand = runtime.Cgram.Colors.Slice(36, 8).ToArray();
        for (int frame = 0; frame < 11; frame++)
        {
            runtime.StepFrame(0); runtime.RunNmi(0, true);
            if (frame == 0 && !runtime.Cgram.Colors.Slice(36, 8).SequenceEqual(completedSand))
                throw new InvalidDataException("First gameplay frame changed the completed sand fade colors.");
            Console.WriteLine($"gameplay {frame} sand={string.Join(' ', runtime.Cgram.Colors.Slice(36,8).ToArray().Select(c => c.ToString("X4")))}");
        }
        if (runtime.Cgram.Colors.Slice(36, 8).SequenceEqual(completedSand))
            throw new InvalidDataException("Sand animation did not resume after door completion.");
        if (fadeChecks != 16) throw new InvalidDataException($"Expected 16 sand fade checks, got {fadeChecks}.");
        Console.WriteLine("PASS: sand follows all 16 destination setup/fade steps before palette animation resumes.");
        return 0;
    }
}
