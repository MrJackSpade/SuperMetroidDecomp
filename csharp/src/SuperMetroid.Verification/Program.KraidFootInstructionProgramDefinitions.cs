using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidFootInstructionProgramDefinitions()
    {
        VerifyKraidFootInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyKraidFootInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        AssertEqual(193, KraidFootInstructionProgramDefinitions.MechanicsWordCount,
            "Kraid foot compiled mechanics word count");
        AssertEqual(106, KraidFootInstructionProgramDefinitions.PresentationWordCount,
            "Kraid foot live presentation word count");
        for (int index = 0;
             index < KraidFootInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            KraidFootInstructionMechanicsWord definition =
                KraidFootInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadKraidFootInstructionWord(rom, definition.Address),
                $"Kraid foot mechanics word $A7:{definition.Address:X4}");
        }

        var guard = new KraidFootInstructionReadGuard(rom);
        RoomEnemySystem enemies = CreateKraidFootInstructionSystem(guard);
        RoomEnemySlot body = enemies.Slots[0];
        RoomEnemySlot foot = enemies.Slots[5];
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        RunKraidFootProgram(process, enemies, foot,
            KraidFootInstructionProgramDefinitions.Initial, calls: 2);
        AssertEqual((ushort)0x86eb, foot.CurrentInstruction,
            "initial Kraid foot program reaches terminal sleep");

        RunKraidFootProgram(process, enemies, foot,
            KraidFootInstructionProgramDefinitions.Neutral, calls: 2);
        AssertEqual((ushort)0x86f1, foot.CurrentInstruction,
            "neutral Kraid foot program reaches terminal sleep");

        body.XPosition = 500;
        body.YPosition = 500;
        RunKraidFootProgram(process, enemies, foot,
            KraidFootInstructionProgramDefinitions.WalkingForward, calls: 36);
        AssertEqual(KraidFootInstructionProgramDefinitions.WalkingForwardFinished,
            foot.CurrentInstruction,
            "walking-forward Kraid foot reaches terminal sleep command");
        AssertEqual((ushort)458, body.XPosition,
            "walking-forward callbacks move Kraid left fourteen times");
        AssertEqual((ushort)500, body.YPosition,
            "walking-forward callbacks balance vertical movement");
        AssertKraidFootSoundAndQuake(enemies, "walking-forward");
        RunKraidFootProgram(process, enemies, foot,
            KraidFootInstructionProgramDefinitions.WalkingForwardFinished, calls: 1);
        AssertEqual(KraidFootInstructionProgramDefinitions.WalkingForwardFinished,
            foot.CurrentInstruction,
            "walking-forward terminal sleep retains its cursor");

        body.XPosition = 500;
        body.YPosition = 500;
        RunKraidFootProgram(process, enemies, foot,
            KraidFootInstructionProgramDefinitions.LungeForward, calls: 36);
        AssertEqual(KraidFootInstructionProgramDefinitions.LungeForwardFinished,
            foot.CurrentInstruction,
            "lunge Kraid foot reaches terminal sleep command");
        AssertEqual((ushort)458, body.XPosition,
            "lunge callbacks move Kraid left fourteen times");
        AssertEqual((ushort)500, body.YPosition,
            "lunge callbacks balance vertical movement");
        AssertKraidFootSoundAndQuake(enemies, "lunge");
        RunKraidFootProgram(process, enemies, foot,
            KraidFootInstructionProgramDefinitions.LungeForwardFinished, calls: 1);
        AssertEqual(KraidFootInstructionProgramDefinitions.LungeForwardFinished,
            foot.CurrentInstruction,
            "lunge terminal sleep retains its cursor");

        body.XPosition = 500;
        body.YPosition = 500;
        RunKraidFootProgram(process, enemies, foot,
            KraidFootInstructionProgramDefinitions.WalkingBackward, calls: 32);
        AssertEqual(KraidFootInstructionProgramDefinitions.WalkingBackwardLoop,
            foot.CurrentInstruction,
            "walking-backward Kraid foot reaches loop command");
        AssertEqual((ushort)542, body.XPosition,
            "walking-backward callbacks move Kraid right fourteen times");
        AssertEqual((ushort)500, body.YPosition,
            "walking-backward callbacks balance vertical movement");
        AssertKraidFootSoundAndQuake(enemies, "walking-backward");
        foot.InstructionTimer = 1;
        InvokeKraidFootInstructionProcessor(process, enemies, foot);
        AssertEqual((ushort)0x888f, foot.CurrentInstruction,
            "walking-backward goto begins the next real frame");
        AssertEqual((ushort)545, body.XPosition,
            "walking-backward goto executes the next loop's first movement callback");

        AssertEqual(106, guard.ObservedPresentationWords.Count,
            "all Kraid foot extended-spritemap operands remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Kraid foot execution avoids compiled mechanics bytes");
        for (int index = 0;
             index < KraidFootInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                KraidFootInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Kraid foot presentation $A7:{address:X4}");
            AssertThrows<InvalidDataException>(
                () => KraidFootInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"Kraid foot presentation $A7:{address:X4} is rejected as mechanics");
        }
        AssertThrows<InvalidDataException>(
            () => KraidFootInstructionProgramDefinitions.ReadMechanicsWord(
                KraidFootInstructionProgramDefinitions.AdjacentUnusedFastBackward),
            "adjacent unused fast-backwards program is rejected as foot mechanics");

        _ = ProbeKraidFootInstructionAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeKraidFootInstructionAllocation();
        AssertTrue(checksum != 0, "Kraid foot allocation probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Kraid foot mechanics lookups allocate no storage");

        Console.WriteLine(
            "Kraid foot instruction mechanics: 193 compiled words, all five production " +
            "entries, seven callbacks, and 106 live presentation reads pass.");
    }

    private static RoomEnemySystem CreateKraidFootInstructionSystem(
        ISnesAddressSpace bus)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField(
            "_bus",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField(
            "_kraidState",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(
                enemies,
                new KraidEnemyState());

        enemies.Slots[0].EnemyDefinitionPointer = RoomEnemySystem.KraidDefinition;
        RoomEnemySlot foot = enemies.Slots[5];
        foot.EnemyDefinitionPointer = RoomEnemySystem.KraidFootDefinition;
        foot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa7 };
        return enemies;
    }

    private static void RunKraidFootProgram(
        MethodInfo process,
        RoomEnemySystem enemies,
        RoomEnemySlot foot,
        ushort entry,
        int calls)
    {
        foot.CurrentInstruction = entry;
        foot.InstructionTimer = 1;
        for (int call = 0; call < calls; call++)
        {
            foot.InstructionTimer = 1;
            InvokeKraidFootInstructionProcessor(process, enemies, foot);
        }
    }

    private static void InvokeKraidFootInstructionProcessor(
        MethodInfo process,
        RoomEnemySystem enemies,
        RoomEnemySlot foot) =>
        process.Invoke(
            enemies,
            [foot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0]);

    private static void AssertKraidFootSoundAndQuake(
        RoomEnemySystem enemies,
        string program)
    {
        AssertEqual((byte)0x76,
            enemies.LastKraidSoundEffect!.Value.SoundEffect.Value,
            $"{program} Kraid foot queues native step sound");
        AssertEqual((ushort)1, enemies.EarthquakeType,
            $"{program} Kraid foot selects native quake type");
        AssertEqual((ushort)10, enemies.EarthquakeTimer,
            $"{program} Kraid foot selects native quake duration");
    }

    private static int ProbeKraidFootInstructionAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += KraidFootInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? KraidFootInstructionProgramDefinitions.Initial
                    : KraidFootInstructionProgramDefinitions.WalkingBackwardLoop);
        }
        return checksum;
    }

    private static ushort ReadKraidFootInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa70000 | address) |
            source.ReadByte(0xa70000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class KraidFootInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (KraidFootInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Kraid foot mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa70000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < KraidFootInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        KraidFootInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
