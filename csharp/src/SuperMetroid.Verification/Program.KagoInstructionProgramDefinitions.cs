using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKagoInstructionProgramDefinitions()
    {
        VerifyKagoInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyKagoInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < KagoInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            KagoInstructionMechanicsWord definition =
                KagoInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadKagoInstructionWord(rom, 0xa80000 | definition.Address),
                $"Kago instruction mechanics word $A8:{definition.Address:X4}");
        }

        var guard = new KagoInstructionProgramReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type enemySystemType = typeof(RoomEnemySystem);
        enemySystemType.GetField("_bus", flags)!.SetValue(enemies, guard);
        enemySystemType.GetField("_readRandomNumber", flags)!
            .SetValue(enemies, (Func<ushort>)(() => 0));
        var initialize = enemySystemType.GetMethod("InitializeKago", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = enemySystemType.GetMethod("ProcessInstructions", flags)!;
        MethodInfo resolveShot = enemySystemType.GetMethod(
            "ResolveKagoShotAfterCommon", flags)!;
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.KagoDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        slot.Parameter1 = 10;
        initialize(slot);
        AssertEqual(KagoInstructionProgramDefinitions.Slow, slot.CurrentInstruction,
            "Kago initializer program");

        object?[] processArguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int frame = 0; frame < 41; frame++)
            process.Invoke(enemies, processArguments);
        AssertEqual(unchecked((ushort)(KagoInstructionProgramDefinitions.Slow + 4)),
            slot.CurrentInstruction,
            "Kago slow program completes its native animation loop");
        AssertEqual((ushort)10, slot.InstructionTimer,
            "Kago slow loop restores its ten-frame duration");

        KagoEnemyState state = enemies.KagoStates[0] ?? throw new InvalidDataException(
            "Kago initializer did not publish typed state.");
        resolveShot.Invoke(enemies, [slot, state]);
        AssertTrue(state.UsesFastAnimation, "Kago real shot handoff selects fast animation");
        AssertEqual(KagoInstructionProgramDefinitions.Fast, slot.CurrentInstruction,
            "Kago shot program");
        AssertEqual(1, state.SpawnedBugCount,
            "Kago real shot handoff retains its projectile side effect");
        for (int frame = 0; frame < 13; frame++)
            process.Invoke(enemies, processArguments);
        AssertEqual(unchecked((ushort)(KagoInstructionProgramDefinitions.Fast + 4)),
            slot.CurrentInstruction,
            "Kago fast program completes its native animation loop");
        AssertEqual((ushort)3, slot.InstructionTimer,
            "Kago fast loop restores its three-frame duration");

        AssertEqual(KagoInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Kago spritemap words remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Kago mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => KagoInstructionProgramDefinitions.ReadMechanicsWord(0xab20),
            "Kago spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => KagoInstructionProgramDefinitions.ReadMechanicsWord(0xab46),
            "adjacent Kago initializer code is rejected as instruction mechanics");

        _ = ProbeKagoInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeKagoInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Kago allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Kago mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Kago instruction mechanics: twelve compiled words, the complete slow and " +
            "post-hit loops, the real bug-spawning handoff, and eight live spritemap " +
            "reads pass with mechanics bytes forbidden.");
    }

    private static int ProbeKagoInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
            checksum += KagoInstructionProgramDefinitions.ReadMechanicsWord(
                KagoInstructionProgramDefinitions.Slow);
        return checksum;
    }

    private static ushort ReadKagoInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class KagoInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (KagoInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Kago mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < KagoInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        KagoInstructionProgramDefinitions.PresentationWordAddress(index);
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
