using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidHeadTimers(SuperMetroidAddressSpace rom)
    {
        ushort Word(int a) => (ushort)(rom.ReadByte(a) | rom.ReadByte(a + 1) << 8);
        AssertEqual(Word(0xa796d2), KraidHeadTimers.Roar, "Native roar entry timer");
        AssertEqual(Word(0xa7974a), KraidHeadTimers.EyeGlow, "Native glow entry timer");
        AssertEqual(Word(0xa79764), KraidHeadTimers.Death, "Native death entry timer");
        var enemies = new RoomEnemySystem();
        var state = new KraidEnemyState();
        var guard = new KraidTimerReadGuard(rom);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
        T Method<T>(string name) where T : Delegate => typeof(RoomEnemySystem).GetMethod(name, flags)!.CreateDelegate<T>(enemies);
        var growth = Method<Func<RoomEnemySlot, KraidEnemyState, bool>>("TryBeginKraidGrowth");
        var glow = Method<Action<RoomEnemySlot, KraidEnemyState>>("InitializeKraidEyeGlow");
        var death = Method<Action<RoomEnemySlot, KraidEnemyState>>("InitializeKraidDeath");
        var combat = Method<Action<RoomEnemySlot, KraidEnemyState, VramWriteQueue?>>("RunKraidCombatFunction");
        var body = enemies.Slots[0];
        state.HealthEighthThresholds[6] = 875;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            body.VariableA = (ushort)KraidAiFunction.MainloopThinking;
            state.ThinkingTimer = (ushort)raw;
            body.VariableC = 999;
            combat(body, state, null);
            AssertEqual(raw == 1 ? KraidHeadTimers.Roar : (ushort)999, body.VariableC, "Roar timer loads only on thinker expiry");
            body.VariableA = (ushort)KraidAiFunction.SecondPhaseThinking;
            state.ThinkingTimer = (ushort)raw;
            body.VariableC = 999;
            combat(body, state, null);
            AssertEqual(raw == 1 ? KraidHeadTimers.Roar : (ushort)999, body.VariableC, "Second-phase roar timer loads only on thinker expiry");
            body.Health = 1;
            body.VariableB = 0x8000;
            guard.Tilemap = (ushort)raw;
            AssertTrue(growth(body, state), "Growth threshold admits timer setup");
            int selector = raw switch { 0x97c8 => 50, 0x9ac8 => 42, 0x9dc8 => 34, _ => 26 };
            AssertEqual(Word(0xa796d2 + selector), body.VariableC, "Growth timer follows selected native resume row");
            AssertEqual(unchecked((ushort)(selector - 0x6926)), body.VariableB, "Growth resume cursor remains paired with timer");
        }
        glow(body, state);
        AssertEqual((ushort)(Word(0xa7974a) - 1), body.VariableC, "Eye glow consumes one timer tick on initialization");
        state.HurtFrame = 1;
        body.VariableC = 999;
        death(body, state);
        AssertEqual((ushort)999, body.VariableC, "Active hurt frame delays death timer setup");
        state.HurtFrame = 0;
        death(body, state);
        AssertEqual(Word(0xa79764), body.VariableC, "Death initialization installs native timer without ticking");
        Console.WriteLine("Kraid head timers: all thinker words and growth tilemap selectors plus glow/death entry timing pass with fixed timer reads forbidden.");
    }

    private sealed class KraidTimerReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public ushort Tilemap { get; set; }
        public byte ReadByte(int a)
        {
            if (a == 0xa78002) return (byte)Tilemap;
            if (a == 0xa78003) return (byte)(Tilemap >> 8);
            if (a is 0xa796d2 or 0xa796d3 or 0xa796ec or 0xa796ed or 0xa796f4 or 0xa796f5
                or 0xa796fc or 0xa796fd or 0xa79704 or 0xa79705 or 0xa7974a or 0xa7974b or 0xa79764 or 0xa79765)
                throw new InvalidOperationException("Unexpected migrated head entry timer read.");
            return source.ReadByte(a);
        }
        public void WriteByte(int a, byte value) => throw new InvalidOperationException("Unexpected timer bus write.");
    }
}
