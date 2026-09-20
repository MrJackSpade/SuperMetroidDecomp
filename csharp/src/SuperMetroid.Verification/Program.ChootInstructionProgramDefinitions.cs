using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyChootInstructionProgramDefinitions()
    {
        VerifyChootInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyChootInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
        for (int index = 0;
             index < ChootInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            ChootInstructionMechanicsWord definition =
                ChootInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadChootInstructionWord(rom, 0xa20000 | definition.Address),
                $"Choot instruction mechanics word $A2:{definition.Address:X4}");
        }

        var guard = new ChootInstructionProgramReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type enemySystemType = typeof(RoomEnemySystem);
        enemySystemType.GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = enemySystemType.GetMethod("InitializeChoot", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        var prepareJump = enemySystemType.GetMethod("PrepareChootJump", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ChootEnemyState>>();
        var runJump = enemySystemType.GetMethod("RunChootJump", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ChootEnemyState>>();
        MethodInfo process = enemySystemType.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.ChootDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        slot.Parameter1 = 0x0001;
        slot.Parameter2 = 0;
        slot.XPosition = 0x0100;
        slot.YPosition = 0x0100;
        initialize(slot);
        ChootEnemyState state = enemies.ChootStates[0]!;
        object?[] processArguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];

        RunChootInstructionProgram(
            enemies,
            process,
            processArguments,
            slot,
            ChootInstructionProgramDefinitions.Idle,
            timedFrameCount: 1);
        AssertTrue((slot.Properties & (ushort)EnemyProperties.ProcessOffScreen) == 0,
            "Choot idle program disables off-screen processing");

        state.JumpDelayTimer = 0;
        prepareJump(slot, state);
        AssertEqual(ChootInstructionProgramDefinitions.Jumping, slot.CurrentInstruction,
            "Choot real preparation handoff installs jumping program");
        RunChootInstructionProgram(
            enemies,
            process,
            processArguments,
            slot,
            ChootInstructionProgramDefinitions.Jumping,
            timedFrameCount: 2);
        AssertTrue((slot.Properties & (ushort)EnemyProperties.ProcessOffScreen) != 0,
            "Choot jumping program enables off-screen processing");

        state.YSpeedTableIndex = 0x0100;
        runJump(slot, state);
        AssertEqual(ChootInstructionProgramDefinitions.Falling, slot.CurrentInstruction,
            "Choot real apex handoff installs falling program");
        RunChootInstructionProgram(
            enemies,
            process,
            processArguments,
            slot,
            ChootInstructionProgramDefinitions.Falling,
            timedFrameCount: 2);

        AssertEqual(
            ChootInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "Choot spritemap words remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Choot mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => ChootInstructionProgramDefinitions.ReadMechanicsWord(0xd830),
            "Choot spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => ChootInstructionProgramDefinitions.ReadMechanicsWord(0xd84c),
            "adjacent Choot path table is rejected as mechanics");

        _ = ProbeChootInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeChootInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Choot allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Choot mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Choot instruction mechanics: eleven compiled words, all three real " +
            "program handoffs, and five live spritemap reads pass with mechanics bytes " +
            "forbidden.");
    }

    private static void RunChootInstructionProgram(
        RoomEnemySystem enemies,
        MethodInfo process,
        object?[] processArguments,
        RoomEnemySlot slot,
        ushort entry,
        int timedFrameCount)
    {
        AssertEqual(entry, slot.CurrentInstruction, "Choot compiled program entry");
        for (int frame = 0; frame < timedFrameCount; frame++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, processArguments);
        }
        slot.InstructionTimer = 1;
        process.Invoke(enemies, processArguments);
        AssertEqual(unchecked((ushort)(entry + timedFrameCount * 4 + 2)),
            slot.CurrentInstruction,
            "Choot program reaches terminal sleep");
    }

    private static int ProbeChootInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ChootInstructionProgramDefinitions.ReadMechanicsWord(
                (index % 3) switch
                {
                    0 => ChootInstructionProgramDefinitions.Idle,
                    1 => ChootInstructionProgramDefinitions.Jumping,
                    _ => ChootInstructionProgramDefinitions.Falling,
                });
        }
        return checksum;
    }

    private static ushort ReadChootInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class ChootInstructionProgramReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (ChootInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Choot mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < ChootInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        ChootInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                    }
                }
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
