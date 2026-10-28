using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Frontend.Pause;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks pause-menu reserve input and HUD DMA timing through the real frontend dispatcher.</summary>
internal static class PauseReserveHudVerification
{
    /// <summary>HUD cells the reserve display rewrites; every other mutable cell must be preserved.</summary>
    private static readonly int[] ReserveDisplayCells =
        [8, 9, 40, 41, 70, 71, 72, 73];

    /// <summary>BG3 words below the HUD rows whose cleared effect tiles the HUD synchronization must leave alone.</summary>
    private const int PauseClearedEffectWords = 28 * Layer3Plane.Columns;

    /// <summary>Confirms AUTO label changes, manual refill digits, next-NMI presentation, and preservation of unrelated HUD cells.</summary>
    internal static void VerifyPauseReserveHud()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var random = new SharedRandom();
        var runtime = CreateRetailRuntimeFixture(random: random);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        var samus = runtime.Player.IntroducedSamus;
        samus.Health = 20; samus.MaxHealth = 99;
        samus.ReserveEnergy = 2; samus.MaxReserveEnergy = 100; samus.ReserveTankMode = ReserveTankMode.Auto;
        samus.CollectedBeams = SamusBeamFlags.Charge;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug with { Health = 20, ReserveHealth = 2, ReserveMode = ReserveTankMode.Auto });
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var pause = CreateRetailPauseFixture(new PauseMenuEntry { Samus = samus, System = runtime.System, AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), GameplayLayer3 = runtime.AcceptedLayer3Plane });
        EnterPauseEquipment(pause);
        pause.Step(0, ControllerButton.Up);
        var game = new SuperMetroid.Core.Frontend.SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(), null, SuperMetroid.Core.Frontend.SuperMetroidGameHostServices.Default, random);
        // Construct just the paused dispatcher boundary. Input, HUD updates, the NMI
        // latch and presentation below are real production calls, not a fake refill.
        game.AdoptRuntime(runtime);
        game.Session.Show(pause);
        game.GameState = SuperMetroidGameState.PausedB;
        // The pause screen's BG3 words: the HUD's static top row, its three mutable rows, then the cleared effect rows.
        ushort Shown(int hudCell) => pause.screen.Tilemaps.Bg3[Layer3Plane.Columns + hudCell].PackedWord;
        ushort originalAuto = Shown(8);
        ushort[] initialHud = BackgroundTileWordBytes.Words(pause.screen.Tilemaps.Bg3.Slice(Layer3Plane.Columns, HudState.MutableTileCount));
        ushort[] initialFx = BackgroundTileWordBytes.Words(pause.screen.Tilemaps.Bg3.Slice(Layer3Plane.HudRows * Layer3Plane.Columns, PauseClearedEffectWords));
        long sequence = 0;
        void Step(ControllerButton input = ControllerButton.None) => game.StepCaptured(input, ++sequence, 1);
        Step(ControllerButton.A);
        AssertEqual(ReserveTankMode.Manual, samus.ReserveTankMode, "paused A switches to manual");
        AssertEqual(originalAuto, Shown(8), "AUTO remains until next accepted NMI");
        Step();
        foreach (int index in new[] { 8, 9, 40, 41, 72, 73 })
            AssertEqual(0x2c0f, Shown(index), "manual clears all six visible AUTO cells");
        Step(ControllerButton.Down);
        var beforeRefill = pause.Render();
        ushort oldDigit = Shown(71);
        Step(ControllerButton.A);
        AssertEqual(21, samus.Health, "manual refill starts in production paused dispatcher");
        AssertEqual(oldDigit, Shown(71), "health digit is not presented early");
        Step();
        AssertEqual(22, samus.Health, "second transfer completes");
        AssertEqual(bus.ReadCartridgeWord(0x809dc1), Shown(71), "first transfer digit visible next NMI");
        Step();
        AssertEqual(bus.ReadCartridgeWord(0x809dc3), Shown(71), "completion digit visible next NMI");
        var afterRefill = pause.Render();
        AssertTrue(Enumerable.Range(24, 8).Any(y => Enumerable.Range(48, 16).Any(x =>
            beforeRefill[y * 256 + x] != afterRefill[y * 256 + x])), "actual rendered health digits change during refill");
        Step(ControllerButton.A); Step();
        AssertTrue(!samus.HealthWarning.IsActive, "paused input does not invoke ordinary low-health beta check");
        foreach ((int index, int offset) in new[] { new VerifyPauseReserveHudCase(8, 0), new VerifyPauseReserveHudCase(9, 2), new VerifyPauseReserveHudCase(40, 4), new VerifyPauseReserveHudCase(41, 6), new VerifyPauseReserveHudCase(72, 8), new VerifyPauseReserveHudCase(73, 10) })
            AssertEqual(bus.ReadCartridgeWord((0x809997 + offset)), Shown(index), "empty AUTO uses native table");
        foreach (int index in Enumerable.Range(0, HudState.MutableTileCount).Except(ReserveDisplayCells))
            AssertEqual(initialHud[index], Shown(index), "reserve updates preserve other HUD cells");
        AssertTrue(initialFx.AsSpan().SequenceEqual(BackgroundTileWordBytes.Words(pause.screen.Tilemaps.Bg3.Slice(Layer3Plane.HudRows * Layer3Plane.Columns, PauseClearedEffectWords))),
            "HUD synchronization preserves cleared pause FX rows");
        for (int frame = 0; frame < 8 && game.GameState == SuperMetroidGameState.PausedB; frame++)
        {
            samus.Health = (ushort)(31 + frame);
            Step(ControllerButton.Start);
        }
        AssertEqual(SuperMetroidGameState.UnpausingA, game.GameState, "Start begins fade after queueing HUD");
        AssertEqual(bus.ReadCartridgeWord((0x809dbf + (samus.Health - 1) % 10 * 2)), Shown(71), "last paused HUD write waits for the next accepted NMI");
        Step();
        AssertEqual(bus.ReadCartridgeWord((0x809dbf + samus.Health % 10 * 2)), Shown(71), "first fade frame accepts final paused HUD write");
        Console.WriteLine("Pause reserve HUD: real frontend input, six AUTO cells and per-frame health rows pass.");
    }

    /// <summary>Runs the paused reserve and HUD synchronization reproduction.</summary>
    [VerificationCommand("--pause-reserve-hud")]
    internal static int PauseReserveHudCommand()
    {
        Suite(nameof(VerifyPauseReserveHud), () => VerifyPauseReserveHud());
        return 0;
    }
}
