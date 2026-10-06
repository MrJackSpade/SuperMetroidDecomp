using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Hardware;

internal static class ScrewAttackPaletteAudit
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

        samus.CollectedItems = samus.EquippedItems = (ushort)(SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.ScrewAttack);
        samus.LoadSuitPalette(bus, runtime.Cgram);
        int checkedFrames = 0;
        int greenFrames = 0;
        ushort phase = 0;
        for (int frame = 0; frame < 32; frame++)
        {

            game.Step((ushort)((ushort)SnesButton.Right | (frame >= 3 ? runtime.ControllerBindings.Jump : 0)));
            if (samus.ReadMovementType(bus) != SamusMovementType.SpinJumping || samus.AnimationFrame is 0 or >= 27) continue;
            int phaseIndex = phase / 2;
            int shade = Math.Min(phaseIndex, 6 - phaseIndex);
            var expected = new SnesCgram();
            samus.FullBodyCycleColors!.Apply(expected, (ushort)(0x9ea0 + shade * 0x20));
            var displayed = GameplayDisplayCapture.CaptureOrdinaryBase(runtime).Memory.Cgram;
            for (int color = 1; color < 16; color++)
                if (displayed[192 + color] != expected.Colors[192 + color])
                    throw new InvalidDataException($"Screw frame {samus.AnimationFrame}, phase {phase}, color {color}: expected {expected.Colors[192 + color]:X4}, displayed {displayed[192 + color]:X4}.");
            phase = (ushort)((phase + 2) % 12);
            checkedFrames++;
            if (shade != 0) greenFrames++;
        }
        if (checkedFrames < 12 || greenFrames < 6) throw new InvalidDataException($"Screw fixture missed active palette: {checkedFrames}/{greenFrames}.");
        samus.HorizontalSpeed.UpdateSpeedBoosterPalette(bus, runtime.Cgram, SamusMovementType.SpinJumping, 27,
            samus.EquippedItems, suitColors: samus.SuitColors, cycleColors: samus.FullBodyCycleColors);
        for (int color = 1; color < 16; color++)
            if (runtime.Cgram.Colors[192 + color] != samus.SuitColors!.Resolve(2, color))
                throw new InvalidDataException("Screw frame 27 did not restore normal Varia colors.");
        Console.WriteLine($"PASS: {checkedFrames} real spin-jump frames match all 15 displayed colors; {greenFrames} tinted phases; frame 27 restores normal suit.");
        return 0;
        void Step(SnesButton input) => game.Step((ushort)input);
    }
}
