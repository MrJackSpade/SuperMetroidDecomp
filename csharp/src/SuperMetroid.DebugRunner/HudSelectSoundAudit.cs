using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

internal static class HudSelectSoundAudit
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
        runtime.LoadCartridgeRoomForDebug(0x91f8);
        var level = runtime.LevelData!;
        for (int y = 0; y < level.HeightInBlocks; y++)
        for (int x = 0; x < level.WidthInBlocks; x++)
        {
            level.SetForegroundEntry(y * level.WidthInBlocks + x, y == 16 ? (ushort)0x8000 : (ushort)0);
            level.SetBehavior(y * level.WidthInBlocks + x, 0);
        }
        foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
        runtime.InitializeDebugGroundedSamus(128, 235, 16);
        var samus = runtime.Samus!;
        samus.Missiles = samus.MaxMissiles = 50;
        samus.SuperMissiles = samus.MaxSuperMissiles = 10;
        samus.PowerBombs = samus.MaxPowerBombs = 10;
        samus.SelectedHudItem = 0;
        runtime.Camera!.SetPosition(0, 0);
        typeof(SuperMetroidGame).GetProperty("GameState")!.SetValue(game, SuperMetroidGameState.MainGameplay);
        var audio = new CartridgeAudioRenderer(installation.LoadAudio());
        for (int frame = 0; frame < 90; frame++) Step(0);
        foreach (int presses in new[] { 1, 6 })
        {
            int selections = 0, writes = 0;
            for (int frame = 0; frame < 64; frame++)
            {
                ushort before = samus.SelectedHudItem;
                var result = Step(frame < presses * 2 && frame % 2 == 0 ? runtime.ControllerBindings.ItemSelect : (ushort)0);
                if (samus.SelectedHudItem != before) selections++;
                foreach (var command in result.AudioCommands)
                    if (command.Kind == CartridgeAudioCommandKind.WritePort && command.Port == 1 && command.Value == 0x39)
                    {
                        writes++;
                        Console.WriteLine($"presses={presses} frame={frame} select-sound send {writes}");
                    }
            }
            if (selections != presses || writes != presses)
                throw new InvalidDataException($"HUD selection duplicate: {presses} presses, {selections} selection changes, {writes} sound sends; expected exactly one send per change.");
            Console.WriteLine($"PASS {presses} Select presses: {selections} changes, {writes} sends, no repeats during idle tail.");
        }
        return 0;

        FrontendFrame Step(ushort input)
        {
            game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
            var result = game.Step(input);
            audio.RenderFrame(result.AudioCommands);
            return result;
        }
    }
}
