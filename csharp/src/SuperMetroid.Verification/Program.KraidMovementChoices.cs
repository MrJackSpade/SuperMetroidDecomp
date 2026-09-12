using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidMovementChoices(SuperMetroidAddressSpace rom)
    {
        ushort Word(int a) => (ushort)(rom.ReadByte(a) | rom.ReadByte(a + 1) << 8);
        var positions = new ushort[6];
        var targets = new ushort[6, 5];
        var timers = new ushort[6, 5];
        for (int row = 0; row < 6; row++)
        {
            int record = EnemyRomTablePointers.Kraid.SecondPhaseMovementRecords + row * 4;
            positions[row] = Word(record);
            int pointer = 0xa70000 | Word(record + 2);
            for (int choice = 0; choice < 5; choice++)
            {
                targets[row, choice] = Word(pointer + choice * 4);
                timers[row, choice] = Word(pointer + choice * 4 + 2);
            }
        }
        (ushort TargetX, ushort ThinkTimer) Expected(ushort x, ushort random)
        {
            int row = Array.IndexOf(positions, x);
            if (row < 0) row = 1;
            int choice = Math.Min((random & 28) / 4, 4);
            return (targets[row, choice], timers[row, choice]);
        }
        var enemies = new RoomEnemySystem();
        var state = new KraidEnemyState();
        var body = enemies.Slots[0];
        body.EnemyDefinitionPointer = RoomEnemySystem.KraidDefinition;
        var foot = enemies.Slots[5];
        var part = new KraidPartState();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_kraidState", flags)!.SetValue(enemies, state);
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new SlopeHeightNoReadBus());
        ushort rng = 0;
        int reads = 0;
        typeof(RoomEnemySystem).GetField("_readRandomNumber", flags)!.SetValue(enemies, (Func<ushort>)(() => { reads++; return rng; }));
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(enemies, (Func<ushort>)(() => throw new InvalidOperationException("Movement choice must not advance RNG.")));
        var think = typeof(RoomEnemySystem).GetMethod("RunKraidSecondPhaseThinking", flags, null,
            [typeof(RoomEnemySlot), typeof(RoomEnemySlot), typeof(KraidPartState)], null)!
            .CreateDelegate<Action<RoomEnemySlot, RoomEnemySlot, KraidPartState>>(enemies);
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            foreach (ushort position in positions)
                AssertEqual(Expected(position, (ushort)raw), KraidMovementChoices.Select(position, (ushort)raw), "All RNG words follow each native indirect row");
            body.XPosition = (ushort)raw;
            for (int choice = 0; choice < 8; choice++)
            {
                rng = (ushort)(choice * 4);
                reads = 0;
                part.NextWord = 1;
                foot.InstructionTimer = 99;
                var expected = Expected(body.XPosition, rng);
                think(body, foot, part);
                bool right = unchecked((short)(expected.TargetX - raw)) >= 0;
                AssertEqual(expected.TargetX, state.TargetX, "Actual target includes fallback row");
                AssertEqual(expected.ThinkTimer, part.NextWord, "Actual thinking delay includes weighted final choice");
                AssertEqual((ushort)(right ? KraidAiFunction.FootSecondPhaseWalkingRight : KraidAiFunction.FootSecondPhaseWalkingLeft), foot.VariableA, "Native signed walking direction");
                AssertEqual(right ? KraidInstructionLists.FootWalkBack : KraidInstructionLists.Ilist_86F3, foot.CurrentInstruction, "Chosen direction starts matching animation");
                AssertEqual((ushort)1, foot.InstructionTimer, "Choice restarts animation timer");
                AssertEqual(1, reads, "Choice reads current RNG once");
            }
            part.NextWord = (ushort)raw;
            reads = 0;
            think(body, foot, part);
            if (raw != 1)
            {
                AssertEqual(unchecked((ushort)(raw - 1)), part.NextWord, "Only zero after decrement triggers choice");
                AssertEqual(0, reads, "Unexpired thinking does not read RNG");
            }
        }
        Console.WriteLine("Kraid choices: all indirect native pairs, every RNG word per row, 524288 actual coordinate/choice transitions and all timer words pass bus-free.");
    }
}
