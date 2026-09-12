using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledPhantoonCasualFlames(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int pattern = 0; pattern < 4; pattern++)
        {
            int pointer = 0xa70000 | Word(0xa7ccfd + pattern * 2);
            var compiled = PhantoonCasualFlameDefinitions.Pattern(pattern);
            AssertEqual(Word(pointer) + 2, compiled.Length, "Native casual flame schedule length");
            for (int i = 0; i < compiled.Length; i++)
                AssertEqual(Word(pointer + i * 2), compiled[i], "Native casual flame schedule word");
        }
        var enemies = new RoomEnemySystem();
        var body = enemies.Slots[0];
        var mouth = enemies.Slots[3];
        var step = typeof(RoomEnemySystem).GetMethod("StepPhantoonCasualFlameSchedule", BindingFlags.NonPublic | BindingFlags.Instance)!
            .CreateDelegate<Action<RoomEnemySlot, RoomEnemySlot>>(enemies);
        ushort random = 0;
        int calls = 0;
        typeof(RoomEnemySystem).GetField("_nextRandom", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(enemies, (Func<ushort>)(() => { calls++; return random; }));
        // All RNG words enter the actual new-pattern branch with no address space attached.
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            random = (ushort)raw;
            mouth.VariableB = 0;
            mouth.VariableC = 0xffff;
            int before = calls;
            step(body, mouth);
            int pointer = 0xa70000 | Word(0xa7ccfd + (raw & 3) * 2);
            AssertEqual((ushort)(raw & 3), mouth.VariableA, "Actual casual RNG pattern selector");
            AssertEqual(Word(pointer), mouth.VariableC, "Actual casual initial remaining count");
            AssertEqual(Word(pointer + (Word(pointer) + 1) * 2), mouth.VariableB, "Actual casual reverse initial timer");
            AssertEqual(before + 1, calls, "Casual selection consumes exactly one RNG call");
        }
        // Trace multiple complete schedules. Native $CFFF branches on zero as well
        // as negative, forces FFFF, and selects the timer at pointer+2, not the header.
        for (int pattern = 0; pattern < 4; pattern++)
        {
            random = (ushort)pattern;
            mouth.VariableA = (ushort)pattern;
            mouth.VariableB = 0;
            mouth.VariableC = 0xffff;
            ushort timer = 0, count = 0xffff;
            int pointer = 0xa70000 | Word(0xa7ccfd + pattern * 2);
            for (int frame = 0; frame < 2048; frame++)
            {
                mouth.CurrentInstruction = 0;
                mouth.InstructionTimer = 0;
                bool draw = false;
                timer = unchecked((ushort)(timer - 1));
                if ((short)timer <= 0)
                {
                    if ((short)count >= 0)
                    {
                        count = unchecked((ushort)(count - 1));
                        if ((short)count <= 0)
                        {
                            count = 0xffff;
                            timer = Word(pointer + 2);
                        }
                        else timer = Word(pointer + (count + 1) * 2);
                        draw = true;
                    }
                    else
                    {
                        count = Word(pointer);
                        timer = Word(pointer + (count + 1) * 2);
                    }
                }
                step(body, mouth);
                AssertEqual(timer, mouth.VariableB, "Actual casual frame timer");
                AssertEqual(count, mouth.VariableC, "Actual casual frame remaining count");
                AssertEqual(draw ? PhantoonInstructionLists.MouthFollowUp : (ushort)0, mouth.CurrentInstruction, "Exact casual mouth animation trigger frame");
                AssertEqual(draw ? (ushort)1 : (ushort)0, mouth.InstructionTimer, "Exact casual instruction timer reset");
            }
        }
        Console.WriteLine("Phantoon casual schedules: 30 native words, 65536 RNG selections and 8192 frame-exact mouth transitions match without a bus.");
    }
}
