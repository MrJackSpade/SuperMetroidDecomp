using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledPhantoonTimers(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int i = 0; i < 8; i++)
        {
            AssertEqual(Word(0xa7cd41 + i * 2), PhantoonTimerDefinitions.VulnerableWindow[i], "Native vulnerable timer");
            AssertEqual(Word(0xa7cd53 + i * 2), PhantoonTimerDefinitions.EyeClosed[i], "Native eye-closed timer");
            AssertEqual(Word(0xa7cd63 + i * 2), PhantoonTimerDefinitions.RainHiding[i], "Native rain timer");
        }
        var enemies = new RoomEnemySystem();
        var body = enemies.Slots[0];
        var eye = enemies.Slots[1];
        var state = new PhantoonEnemyState(body) { Eye = eye, Tentacles = enemies.Slots[2], Mouth = enemies.Slots[3] };
        T Call<T>(string name) where T : Delegate => typeof(RoomEnemySystem).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.CreateDelegate<T>(enemies);
        var open = Call<Action<PhantoonEnemyState>>("BeginPhantoonEyeTracking");
        var second = Call<Action<PhantoonEnemyState, byte>>("PickPhantoonSecondRoundPattern");
        var first = Call<Action<RoomEnemySlot, PhantoonEnemyState, byte>>("RunPhantoonPickFirstRoundPattern");
        var rain = Call<Action<RoomEnemySlot, PhantoonEnemyState, byte>>("RunPhantoonFlameRainFadeOut");
        ushort randomValue = 0;
        int calls = 0;
        typeof(RoomEnemySystem).GetField("_nextRandom", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(enemies, (Func<ushort>)(() => { calls++; return randomValue; }));
        // No address space is installed: every tested path must use compiled timers.
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            randomValue = (ushort)raw;
            int priorCalls = calls;
            open(state);
            AssertEqual(Word(0xa7cd41 + (raw & 7) * 2), body.VariableE, "Real open-window timer masks full RNG word");
            AssertEqual((ushort)PhantoonAiFunction.EyeTracksSamus, body.VariableF, "Open-window phase handoff");
            second(state, (byte)(raw & 255));
            AssertEqual(Word(0xa7cd53 + (raw & 7) * 2), eye.VariableA, "Real second-round timer masks full RNG word");
            eye.VariableF = 1; // Completed fade; retain the actual fade-to-rain transition.
            rain(body, state, 0);
            AssertEqual(Word(0xa7cd63 + (raw & 7) * 2), body.VariableE, "Real rain hiding timer masks full RNG word");
            AssertEqual((ushort)PhantoonAiFunction.SpawnFlameRain, body.VariableF, "Rain timer phase handoff");
            AssertEqual(priorCalls + 3, calls, "Each selection consumes exactly one RNG call");
        }
        for (int nmi = 0; nmi < 256; nmi++)
        {
            body.VariableE = 1;
            int priorCalls = calls;
            first(body, state, (byte)nmi);
            AssertEqual(Word(0xa7cd53 + ((nmi >> 1) & 3) * 2), eye.VariableA, "Real first-round NMI selector uses four entries");
            AssertEqual(priorCalls + 1, calls, "First-round direction preserves RNG consumption");
        }
        Console.WriteLine("Phantoon timers: 24 native words, 196608 random-selector calls and all 256 first-round NMI values match without a bus.");
    }
}
