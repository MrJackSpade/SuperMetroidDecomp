using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static PhantoonSoundDefinitionsVerification;
using static Program;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class PhantoonCasualFlamesVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for Phantoon casual flames")]
    internal static void VerifyCompiledPhantoonCasualFlames(SuperMetroidAddressSpace rom)
    {
        for (int pattern = 0; pattern < 4; pattern++)
        {
            int pointer = 0xa70000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7ccfd + pattern * 2));
            var compiled = PhantoonCasualFlameDefinitions.Pattern(pattern);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(pointer) + 2, compiled.Length, "Native casual flame schedule length");
            for (int i = 0; i < compiled.Length; i++)
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((pointer + i * 2)), compiled[i], "Native casual flame schedule word");
        }
        var enemies = new RoomEnemySystem();
        var body = enemies.SlotTable.Slots[0];
        var mouth = enemies.SlotTable.Slots[3];
        Action<RoomEnemySlot> step = enemies.PhantoonBehavior.StepPhantoonCasualFlameSchedule;
        ushort random = 0;
        int calls = 0;
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => { calls++; return random; });
        // All RNG words enter the actual new-pattern branch with no address space attached.
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            random = (ushort)raw;
            mouth.VariableB = 0;
            mouth.VariableC = 0xffff;
            int before = calls;
            step(mouth);
            int pointer = 0xa70000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7ccfd + (raw & 3) * 2));
            AssertEqual((ushort)(raw & 3), mouth.VariableA, "Actual casual RNG pattern selector");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(pointer), mouth.VariableC, "Actual casual initial remaining count");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((pointer + (CartridgeImportSource.Require(rom).ReadCartridgeWord(pointer) + 1) * 2)), mouth.VariableB, "Actual casual reverse initial timer");
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
            int pointer = 0xa70000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7ccfd + pattern * 2));
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
                            timer = CartridgeImportSource.Require(rom).ReadCartridgeWord((pointer + 2));
                        }
                        else timer = CartridgeImportSource.Require(rom).ReadCartridgeWord((pointer + (count + 1) * 2));
                        draw = true;
                    }
                    else
                    {
                        count = CartridgeImportSource.Require(rom).ReadCartridgeWord(pointer);
                        timer = CartridgeImportSource.Require(rom).ReadCartridgeWord((pointer + (count + 1) * 2));
                    }
                }
                step(mouth);
                AssertEqual(timer, mouth.VariableB, "Actual casual frame timer");
                AssertEqual(count, mouth.VariableC, "Actual casual frame remaining count");
                AssertEqual(draw ? PhantoonInstructionProgramDefinitions.MouthFollowUp : (ushort)0, mouth.CurrentInstruction, "Exact casual mouth animation trigger frame");
                AssertEqual(draw ? (ushort)1 : (ushort)0, mouth.InstructionTimer, "Exact casual instruction timer reset");
            }
        }
        Console.WriteLine("Phantoon casual schedules: 30 native words, 65536 RNG selections and 8192 frame-exact mouth transitions match without a bus.");
    }

    [VerificationCommand("--lookup-stream5-phantoon-schedules")]
    internal static int LookupStream5PhantoonSchedulesCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyCompiledPhantoonCasualFlames), () => VerifyCompiledPhantoonCasualFlames(rom));
        Suite(nameof(VerifyPhantoonSoundDefinitions), () => VerifyPhantoonSoundDefinitions(rom));
        return 0;
    }
}
