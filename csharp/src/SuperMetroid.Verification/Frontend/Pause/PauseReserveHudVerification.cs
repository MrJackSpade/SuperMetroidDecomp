using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Frontend.Pause;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Progression;

/// <summary>Checks pause-menu reserve input and HUD DMA timing through the real frontend dispatcher.</summary>
internal static class PauseReserveHudVerification
{
    /// <summary>HUD cells the reserve display rewrites; every other mutable cell must be preserved.</summary>
    private static readonly int[] ReserveDisplayCells =
        [8, 9, 40, 41, 70, 71, 72, 73];

    /// <summary>Confirms AUTO label changes, manual refill digits, queued NMI presentation, and preservation of unrelated HUD cells.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for pause reserve HUD")]
    internal static void VerifyPauseReserveHud()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var random = new CartridgeRandom();
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
        pause.Step(0, SnesButton.Up);
        var game = new SuperMetroid.Core.Frontend.SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(), null, SuperMetroid.Core.Frontend.SuperMetroidGameHostServices.Default, random);
        // Construct just the paused dispatcher boundary. Input, HUD updates, queued
        // DMA and presentation below are real production calls, not a fake refill.
        game.AdoptRuntime(runtime);
        game.Session.Show(pause);
        game.GameState = SuperMetroidGameState.PausedB;
        SnesVram Displayed() => PauseScreenFixtures.Vram(pause.screen);
        ushort originalAuto = Displayed().ReadWord(HudState.VramDestination + 8);
        byte[] initialHud = Displayed().Bytes.Slice(HudState.VramDestination * 2, HudState.MutableByteCount).ToArray();
        byte[] initialFx = Displayed().Bytes.Slice(0xb100, 0x700).ToArray();
        long sequence = 0;
        void Step(SnesButton input = SnesButton.None) => game.StepCaptured(input, ++sequence, 1);
        Step(SnesButton.A);
        AssertEqual(ReserveTankMode.Manual, samus.ReserveTankMode, "paused A switches to manual");
        AssertEqual(originalAuto, Displayed().ReadWord(HudState.VramDestination + 8), "AUTO remains until next accepted NMI");
        Step();
        foreach (int index in new[] { 8, 9, 40, 41, 72, 73 })
            AssertEqual(0x2c0f, Displayed().ReadWord(HudState.VramDestination + index), "manual clears all six visible AUTO cells");
        Step(SnesButton.Down);
        var beforeRefill = pause.Render();
        ushort oldDigit = Displayed().ReadWord(HudState.VramDestination + 71);
        Step(SnesButton.A);
        AssertEqual(21, samus.Health, "manual refill starts in production paused dispatcher");
        AssertEqual(oldDigit, Displayed().ReadWord(HudState.VramDestination + 71), "health DMA is not presented early");
        Step();
        AssertEqual(22, samus.Health, "second transfer completes");
        AssertEqual(bus.ReadCartridgeWord(0x809dc1), Displayed().ReadWord(HudState.VramDestination + 71), "first transfer digit visible next NMI");
        Step();
        AssertEqual(bus.ReadCartridgeWord(0x809dc3), Displayed().ReadWord(HudState.VramDestination + 71), "completion digit visible next NMI");
        var afterRefill = pause.Render();
        AssertTrue(Enumerable.Range(24, 8).Any(y => Enumerable.Range(48, 16).Any(x =>
            beforeRefill[y * 256 + x] != afterRefill[y * 256 + x])), "actual rendered health digits change during refill");
        Step(SnesButton.A); Step();
        AssertTrue(!samus.HealthWarning.IsActive, "paused input does not invoke ordinary low-health beta check");
        foreach ((int index, int offset) in new[] { (8, 0), (9, 2), (40, 4), (41, 6), (72, 8), (73, 10) })
            AssertEqual(bus.ReadCartridgeWord((0x809997 + offset)), Displayed().ReadWord(HudState.VramDestination + index), "empty AUTO uses native table");
        foreach (int index in Enumerable.Range(0, HudState.MutableTileCount).Except(ReserveDisplayCells))
            AssertEqual(SnesLittleEndian.Word(initialHud[index * 2], initialHud[index * 2 + 1]),
                Displayed().ReadWord(HudState.VramDestination + index), "reserve updates preserve other HUD cells");
        AssertTrue(initialFx.SequenceEqual(Displayed().Bytes.Slice(0xb100, 0x700)), "HUD synchronization preserves cleared pause FX rows");
        for (int frame = 0; frame < 8 && game.GameState == SuperMetroidGameState.PausedB; frame++)
        {
            samus.Health = (ushort)(31 + frame);
            Step(SnesButton.Start);
        }
        AssertEqual(SuperMetroidGameState.UnpausingA, game.GameState, "Start begins fade after queueing HUD");
        AssertEqual(bus.ReadCartridgeWord((0x809dbf + (samus.Health - 1) % 10 * 2)), Displayed().ReadWord(HudState.VramDestination + 71), "last paused HUD write remains queued");
        Step();
        AssertEqual(bus.ReadCartridgeWord((0x809dbf + samus.Health % 10 * 2)), Displayed().ReadWord(HudState.VramDestination + 71), "first fade frame accepts final paused HUD write");
        Console.WriteLine("Pause reserve HUD: real frontend input, six AUTO cells and per-frame health DMA pass.");
    }

    /// <summary>Runs the paused reserve and HUD synchronization reproduction.</summary>
    [VerificationCommand("--pause-reserve-hud")]
    internal static int PauseReserveHudCommand()
    {
        Suite(nameof(VerifyPauseReserveHud), () => VerifyPauseReserveHud());
        return 0;
    }
}
