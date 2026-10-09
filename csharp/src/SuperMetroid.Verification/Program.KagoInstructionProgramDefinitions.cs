using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs Kago mechanics and instruction-program checks against the retail ROM in the verification working directory.</summary>
    private static void VerifyKagoInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyKagoInstructionProgramDefinitions), () => VerifyKagoInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled Kago mechanics with cartridge words and exercises slow, fast, and post-hit instruction behavior.</summary>
    /// <param name="rom">Retail address space used as the expected-data source and wrapped by the runtime-read guard.</param>
    private static void VerifyKagoInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < KagoInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
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

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Kago's slow and fast loops use compiled spritemap selectors");
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
            "post-hit loops, the real bug-spawning handoff, and eight compiled spritemap " +
            "selectors pass without instruction or presentation ROM reads.");
    }

    /// <summary>Repeats compiled mechanics lookups so the warmed allocation check measures the steady-state access path.</summary>
    /// <returns>A checksum that keeps the lookup results observable.</returns>
    private static int ProbeKagoInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
            checksum += KagoInstructionProgramDefinitions.ReadMechanicsWord(
                KagoInstructionProgramDefinitions.Slow);
        return checksum;
    }

    /// <summary>Reads a little-endian instruction mechanics word from the cartridge address space.</summary>
    /// <param name="bus">Address space supplying the two bytes.</param>
    /// <param name="address">Address of the low byte; the high byte is read next.</param>
    /// <returns>The combined 16-bit word.</returns>
    private static ushort ReadKagoInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Wraps SNES reads to record Kago presentation accesses and fail if production rereads compiled mechanics bytes.</summary>
    /// <param name="source">Underlying address space receiving permitted reads and writes.</param>
    private sealed class KagoInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Bank-$A8 presentation words observed through this guard.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        /// <summary>Number of attempted reads from mechanics bytes supplied by compiled definitions.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes an import-source request through the guarded read path.</summary>
        /// <param name="address">Cartridge address requested by the caller.</param>
        /// <returns>The underlying byte if the address is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects compiled mechanics reads, records accesses to presentation operands, and delegates other reads.</summary>
        /// <param name="address">Address requested from the wrapped SNES memory.</param>
        /// <returns>The underlying byte when the read is permitted.</returns>
        public byte ReadByte(int address)
        {
            if (KagoInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Kago mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < KagoInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        KagoInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
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

        /// <summary>Forwards a write unchanged to the wrapped address space.</summary>
        /// <param name="address">Destination address.</param>
        /// <param name="value">Byte to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
