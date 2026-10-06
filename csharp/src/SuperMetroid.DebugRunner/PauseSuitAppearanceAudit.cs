using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

internal static class PauseSuitAppearanceAudit
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

        // Actual menu toggles: Varia -> Power -> Varia -> Gravity -> Varia.
        foreach (var (item, expectedOffset) in new (int, ushort)[] { (0, 0), (0, 2), (1, 4), (1, 2) })
        {
            ushort initialOffset = samus.EquippedItems.GetSuitPaletteTableOffset();
            for (int color = 1; color < 16; color++)
                Require(runtime.Cgram.Colors[192 + color] == samus.SuitColors!.Resolve(initialOffset, color), "initial suit palette");
            Step(SnesButton.Start);
            Reach(SuperMetroidGameState.PausedB);
            for (int frame = 0; frame < 40; frame++) Step(frame < 8 ? SnesButton.R : 0);
            Require(game.PauseScreenMode == 1 && game.PauseSelectedEquipmentCategory == 2, "equipment page");
            if (item == 1) { Step(SnesButton.Down); Step(0); }
            Require(game.PauseSelectedEquipmentItem == item, "selected suit");
            Step(SnesButton.A); Step(0);
            Require(samus.EquippedItems.GetSuitPaletteTableOffset() == expectedOffset, "equipment toggle");
            for (int frame = 0; frame < 8 && game.GameState == SuperMetroidGameState.PausedB; frame++) Step(SnesButton.Start);
            Require(game.GameState != SuperMetroidGameState.PausedB, "exit menu");
            Reach(SuperMetroidGameState.Unpausing);
            CheckPalette("forced-blank resume");
            Reach(SuperMetroidGameState.MainGameplay);
            CheckPalette("visible gameplay");
            Console.WriteLine($"Suit toggle item {item}: palette offset {expectedOffset}, all 15 opaque displayed colors match after resume.");

            void CheckPalette(string phase)
            {
                var colors = GameplayDisplayCapture.CaptureOrdinaryBase(runtime).Memory.Cgram;
                for (int color = 1; color < 16; color++)
                {
                    ushort expected = samus.SuitColors!.Resolve(expectedOffset, color);
                    if (colors[192 + color] != expected)
                        throw new InvalidDataException($"Suit after {phase}: offset={expectedOffset}, color={color}, expected={expected:X4}, actual={colors[192 + color]:X4}.");
                }
            }
        }
        return 0;

        void Step(SnesButton input) => game.Step((ushort)input);
        void Reach(SuperMetroidGameState state)
        {
            for (int frame = 0; frame < 100 && game.GameState != state; frame++) Step(0);
            Require(game.GameState == state, $"reach {state}, got {game.GameState}");
        }
    }
    private static void Require(bool condition, string property)
    {
        if (!condition) throw new InvalidDataException("Suit menu regression: " + property);
    }
}
