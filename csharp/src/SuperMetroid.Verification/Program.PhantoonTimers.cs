using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class PhantoonTimersVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for Phantoon timers")]
    internal static void VerifyCompiledPhantoonTimers(SuperMetroidAddressSpace rom)
    {
        for (int i = 0; i < 8; i++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cd41 + i * 2)), PhantoonTimerDefinitions.VulnerableWindow[i], "Native vulnerable timer");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cd53 + i * 2)), PhantoonTimerDefinitions.EyeClosed[i], "Native eye-closed timer");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cd63 + i * 2)), PhantoonTimerDefinitions.RainHiding[i], "Native rain timer");
        }
        var enemies = new RoomEnemySystem();
        var body = enemies.SlotTable.Slots[0];
        var eye = enemies.SlotTable.Slots[1];
        var state = new PhantoonEnemyState(body) { Eye = eye, Tentacles = enemies.SlotTable.Slots[2], Mouth = enemies.SlotTable.Slots[3] };
        var open = new Action<PhantoonEnemyState>(enemies.PhantoonBehavior.BeginPhantoonEyeTracking);
        var second = new Action<PhantoonEnemyState>(enemies.PhantoonBehavior.PickPhantoonSecondRoundPattern);
        var first = new Action<RoomEnemySlot, PhantoonEnemyState>(enemies.PhantoonBehavior.RunPhantoonPickFirstRoundPattern);
        var rain = new Action<RoomEnemySlot, PhantoonEnemyState>(enemies.PhantoonBehavior.RunPhantoonFlameRainFadeOut);
        ushort randomValue = 0;
        int calls = 0;
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => { calls++; return randomValue; });
        // No address space is installed: every tested path must use compiled timers.
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            randomValue = (ushort)raw;
            int priorCalls = calls;
            open(state);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cd41 + (raw & 7) * 2)), body.VariableE, "Real open-window timer masks full RNG word");
            AssertEqual((ushort)PhantoonAiFunction.EyeTracksSamus, body.VariableF, "Open-window phase handoff");
            second(state);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cd53 + (raw & 7) * 2)), eye.VariableA, "Real second-round timer masks full RNG word");
            eye.VariableF = 1; // Completed fade; retain the actual fade-to-rain transition.
            rain(body, state);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cd63 + (raw & 7) * 2)), body.VariableE, "Real rain hiding timer masks full RNG word");
            AssertEqual((ushort)PhantoonAiFunction.SpawnFlameRain, body.VariableF, "Rain timer phase handoff");
            AssertEqual(priorCalls + 3, calls, "Each selection consumes exactly one RNG call");
        }
        // $A7:D5A6 indexes the eye-closed timers with the 16-bit NMI_FrameCounter.
        for (int nmi = 0; nmi < 256; nmi++)
        {
            body.VariableE = 1;
            int priorCalls = calls;
            enemies.FrameContext._enemyFrameNmiFrameCounter = (ushort)nmi;
            first(body, state);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cd53 + ((nmi >> 1) & 3) * 2)), eye.VariableA, "Real first-round NMI selector uses four entries");
            AssertEqual(priorCalls + 1, calls, "First-round direction preserves RNG consumption");
        }
        Console.WriteLine("Phantoon timers: 24 native words, 196608 random-selector calls and all 256 first-round NMI values match without a bus.");
    }
}
