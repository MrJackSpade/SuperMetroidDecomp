using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Hardware;

internal static class PauseMapCursorAudit
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
        samus.CollectedItems = (ushort)(SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit);
        samus.EquippedItems = (ushort)SamusEquipmentFlags.VariaSuit;
        samus.CollectedBeams = samus.EquippedBeams = 0;
        samus.LoadSuitPalette(bus, runtime.Cgram);
        typeof(SuperMetroidGame).GetProperty("GameState")!.SetValue(game, SuperMetroidGameState.MainGameplay);
        runtime.Camera!.SetPosition(0, 0);
        Step(0);

        Step(SnesButton.Start);
        for (int frame = 0; frame < 100 && game.GameState != SuperMetroidGameState.PausedB; frame++) Step(0);
        if (game.GameState != SuperMetroidGameState.PausedB) throw new InvalidDataException("Did not reach pause map.");
        object menu = typeof(SuperMetroidGame).GetField("pauseMenu", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(game)!;
        var type = menu.GetType();
        T Field<T>(string name) => (T)type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(menu)!;
        var oam = Field<OamBuffer>("oam");
        var vram = Field<SnesVram>("vram");
        var cgram = Field<SnesCgram>("cgram");
        for (int frame = 0; frame < 24; frame++)
        {
            Step(0);
            oam.BeginFrame();
            type.GetMethod("DrawMapPositionIndicator", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(menu, null);
            oam.FinalizeFrame();
            var pixels = SnesObjRenderer.Render(oam, vram, cgram, 1);
            var opaque = pixels.Where(pixel => pixel.A != 0).ToArray();
            var colors = opaque.Distinct().ToArray();
            Console.WriteLine($"frame {frame}: {string.Join(", ", colors)}");
            if (colors.Length != 2 || !colors.Any(p => p.R == 255 && p.G == 255 && p.B == 255) ||
                !colors.Any(p => p.R == 0 && p.G == 0 && p.B == 0))
                throw new InvalidDataException("Map cursor must render white with black outline.");
            for (int sprite = 0; sprite < 4; sprite++)
                if (((oam.LowTable[sprite * 4 + 3] >> 1) & 7) != 7)
                    throw new InvalidDataException("Map cursor must use native palette 7.");
        }
        Console.WriteLine("PASS: complete cursor animation renders white and black using cartridge palette 7.");
        return 0;
        void Step(SnesButton input) => game.Step((ushort)input);
    }
}
