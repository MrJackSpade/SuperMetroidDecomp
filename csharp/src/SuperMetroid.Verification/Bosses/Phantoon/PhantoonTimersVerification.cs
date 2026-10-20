using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Phantoon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled Phantoon timer tables and live selector behavior
/// against native words for RNG and first-round NMI inputs.</summary>
internal static class PhantoonTimersVerification
{
    /// <summary>Compares all twenty-four timer words, full-word RNG masking,
    /// first-round NMI selection, and RNG call counts without a runtime bus.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Phantoon timers")]
    internal static void VerifyCompiledPhantoonTimers(SuperMetroidAddressSpace rom)
    {
        for (int i = 0; i < 8; i++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cd41 + i * 2)), PhantoonTimerDefinitions.VulnerableWindow[i], "Native vulnerable timer");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cd53 + i * 2)), PhantoonTimerDefinitions.EyeClosed[i], "Native eye-closed timer");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cd63 + i * 2)), PhantoonTimerDefinitions.RainHiding[i], "Native rain timer");
        }
        var enemies = new RoomEnemySystem(new EnemySession(), services: new VerificationRoomEnemyServices(null));
        var body = enemies.SlotTable.Slots[0];
        var eye = enemies.SlotTable.Slots[1];
        var state = new PhantoonEnemyState(body) { Eye = eye, Tentacles = enemies.SlotTable.Slots[2], Mouth = enemies.SlotTable.Slots[3] };
        enemies.PhantoonBehavior.Combat.Phantoon = state;
        var open = new Action<PhantoonEnemyState>(enemies.PhantoonBehavior.BeginPhantoonEyeTracking);
        var second = new Action<PhantoonEnemyState>(enemies.PhantoonBehavior.Patterns.PickPhantoonSecondRoundPattern);
        var first = new Action<RoomEnemySlot, PhantoonEnemyState>(enemies.PhantoonBehavior.Patterns.RunPhantoonPickFirstRoundPattern);
        var rain = new Action<RoomEnemySlot, PhantoonEnemyState>(enemies.PhantoonBehavior.Patterns.RunPhantoonFlameRainFadeOut);
        ushort randomValue = 0;
        int calls = 0;
        enemies.VerificationServices().NextRandom = () => { calls++; return randomValue; };
        // No address space is installed: every tested path must use compiled timers.
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            randomValue = (ushort)raw;
            int priorCalls = calls;
            open(state);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cd41 + (raw & 7) * 2)), state.Parts[body.SlotIndex].FunctionTimer, "Real open-window timer masks full RNG word");
            AssertEqual(PhantoonAiFunction.EyeTracksSamus, state.Parts[body.SlotIndex].Function, "Open-window phase handoff");
            second(state);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cd53 + (raw & 7) * 2)), state.Parts[eye.SlotIndex].ClosedTimer, "Real second-round timer masks full RNG word");
            state.Parts[eye.SlotIndex].FadeComplete = true; // Completed fade; retain the actual fade-to-rain transition.
            rain(body, state);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cd63 + (raw & 7) * 2)), state.Parts[body.SlotIndex].FunctionTimer, "Real rain hiding timer masks full RNG word");
            AssertEqual(PhantoonAiFunction.SpawnFlameRain, state.Parts[body.SlotIndex].Function, "Rain timer phase handoff");
            AssertEqual(priorCalls + 3, calls, "Each selection consumes exactly one RNG call");
        }
        // $A7:D5A6 indexes the eye-closed timers with the 16-bit NMI_FrameCounter.
        for (int nmi = 0; nmi < 256; nmi++)
        {
            state.Parts[body.SlotIndex].FunctionTimer = 1;
            int priorCalls = calls;
            enemies.FrameContext.SeedNmiFrameCounter((ushort)nmi);
            first(body, state);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cd53 + ((nmi >> 1) & 3) * 2)), state.Parts[eye.SlotIndex].ClosedTimer, "Real first-round NMI selector uses four entries");
            AssertEqual(priorCalls + 1, calls, "First-round direction preserves RNG consumption");
        }
        Console.WriteLine("Phantoon timers: 24 native words, 196608 random-selector calls and all 256 first-round NMI values match without a bus.");
    }
}
