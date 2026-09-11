using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyPauseReserveHud()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        var samus = runtime.Samus!;
        samus.Health = 20; samus.MaxHealth = 99;
        samus.ReserveEnergy = 2; samus.MaxReserveEnergy = 100; samus.ReserveTankMode = 1;
        samus.CollectedBeams = (ushort)SamusBeamFlags.Charge;
        runtime.InitializeHud(HudSnapshot.CeresDebug with { Health = 20, ReserveHealth = 2, ReserveMode = 1 });
        runtime.RunNmi(0, true);
        var pause = new PauseMenuState(bus, samus, runtime.System, AreaId.Crateria, 0, 0,
            gameplayVram: runtime.Vram);
        pause.Step((ushort)SnesButton.R, (ushort)SnesButton.R);
        for (int i = 0; i < 32; i++) pause.Step(0, 0);
        pause.Step(0, (ushort)SnesButton.Up);
        var game = new SuperMetroidGame(bus);
        // Construct just the paused dispatcher boundary. Input, HUD updates, queued
        // DMA and presentation below are real production calls, not a fake refill.
        typeof(SuperMetroidGame).GetField("runtime", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("pauseMenu", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, pause);
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.PausedB);
        var displayed = (SnesVram)typeof(PauseMenuState).GetField("vram", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(pause)!;
        ushort originalAuto = displayed.ReadWord(HudState.VramDestination + 8);
        byte[] initialHud = displayed.Bytes.Slice(HudState.VramDestination * 2, HudState.MutableByteCount).ToArray();
        byte[] initialFx = displayed.Bytes.Slice(0xb100, 0x700).ToArray();
        long sequence = 0;
        void Step(SnesButton input = 0) => game.StepCaptured((ushort)input, ++sequence, 1);
        Step(SnesButton.A);
        AssertEqual(2, samus.ReserveTankMode, "paused A switches to manual");
        AssertEqual(originalAuto, displayed.ReadWord(HudState.VramDestination + 8), "AUTO remains until next accepted NMI");
        Step();
        foreach (int index in new[] { 8, 9, 40, 41, 72, 73 })
            AssertEqual(0x2c0f, displayed.ReadWord(HudState.VramDestination + index), "manual clears all six visible AUTO cells");
        Step(SnesButton.Down);
        var beforeRefill = pause.Render();
        ushort oldDigit = displayed.ReadWord(HudState.VramDestination + 71);
        Step(SnesButton.A);
        AssertEqual(21, samus.Health, "manual refill starts in production paused dispatcher");
        AssertEqual(oldDigit, displayed.ReadWord(HudState.VramDestination + 71), "health DMA is not presented early");
        Step();
        AssertEqual(22, samus.Health, "second transfer completes");
        AssertEqual(ReadWord(0x809dc1), displayed.ReadWord(HudState.VramDestination + 71), "first transfer digit visible next NMI");
        Step();
        AssertEqual(ReadWord(0x809dc3), displayed.ReadWord(HudState.VramDestination + 71), "completion digit visible next NMI");
        var afterRefill = pause.Render();
        AssertTrue(Enumerable.Range(24, 8).Any(y => Enumerable.Range(48, 16).Any(x =>
            beforeRefill[y * 256 + x] != afterRefill[y * 256 + x])), "actual rendered health digits change during refill");
        Step(SnesButton.A); Step();
        foreach ((int index, int offset) in new[] { (8, 0), (9, 2), (40, 4), (41, 6), (72, 8), (73, 10) })
            AssertEqual(ReadWord(0x809997 + offset), displayed.ReadWord(HudState.VramDestination + index), "empty AUTO uses native table");
        foreach (int index in Enumerable.Range(0, HudState.MutableTileCount).Except(new[] { 8, 9, 40, 41, 70, 71, 72, 73 }))
            AssertEqual((ushort)(initialHud[index * 2] | initialHud[index * 2 + 1] << 8),
                displayed.ReadWord(HudState.VramDestination + index), "reserve updates preserve other HUD cells");
        AssertTrue(initialFx.AsSpan().SequenceEqual(displayed.Bytes.Slice(0xb100, 0x700)), "HUD synchronization preserves cleared pause FX rows");
        for (int frame = 0; frame < 8 && game.GameState == SuperMetroidGameState.PausedB; frame++)
        {
            samus.Health = (ushort)(31 + frame);
            Step(SnesButton.Start);
        }
        AssertEqual(SuperMetroidGameState.UnpausingA, game.GameState, "Start begins fade after queueing HUD");
        AssertEqual(ReadWord(0x809dbf + (samus.Health - 1) % 10 * 2), displayed.ReadWord(HudState.VramDestination + 71), "last paused HUD write remains queued");
        Step();
        AssertEqual(ReadWord(0x809dbf + samus.Health % 10 * 2), displayed.ReadWord(HudState.VramDestination + 71), "first fade frame accepts final paused HUD write");
        Console.WriteLine("Pause reserve HUD: real frontend input, six AUTO cells and per-frame health DMA pass.");
        ushort ReadWord(int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
    }
}
