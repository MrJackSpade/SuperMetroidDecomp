using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

internal static class StatueReleaseSoundAudit
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
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xa66a);
        runtime.InitializeDebugGroundedSamus(128, 160, 16);
        runtime.Camera!.SetPosition(0, 0);
        typeof(SuperMetroidGame).GetProperty("GameState")!.SetValue(game, SuperMetroidGameState.MainGameplay);
        var audio = new CartridgeAudioRenderer(installation.LoadAudio());
        for (int frame = 0; frame < 90; frame++) Step();
        runtime.Enemies.SpawnTourianUnlockEffect(0, soul: false);
        int requests = 0, writes = 0;
        for (int frame = 0; frame < 130; frame++)
        {
            var result = Step();
            foreach (var request in runtime.Enemies.SoundRequests)
                if (request.SoundEffect == SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x19))
                {
                    if (request.MaximumQueued != 6) throw new InvalidDataException("Statue release must use Max6.");
                    requests++;
                    Console.WriteLine($"release request at frame {frame}");
                }
            foreach (var command in result.AudioCommands)
                if (command.Kind == CartridgeAudioCommandKind.WritePort && command.Port == 2 && command.Value == 0x19)
                { writes++; Console.WriteLine($"release port write at frame {frame}"); }
        }
        if (requests != 1 || writes != 1)
            throw new InvalidDataException($"Statue release sound: expected one request and one send, got {requests}/{writes}.");
        Console.WriteLine("PASS: native eye-release instruction reaches audio once, library 2 sound 19, Max6.");
        return 0;
        FrontendFrame Step()
        {
            game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
            var result = game.Step(0);
            audio.RenderFrame(result.AudioCommands);
            return result;
        }
    }
}
