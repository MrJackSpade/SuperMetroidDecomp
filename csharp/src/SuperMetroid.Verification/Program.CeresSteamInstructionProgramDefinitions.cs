using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Runs the Ceres steam instruction and mechanics checks against the retail ROM loaded from the verification working directory.</summary>
    private static void VerifyCeresSteamInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyCeresSteamInstructionProgramDefinitions), () => VerifyCeresSteamInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares compiled steam mechanics and executed directional animations with cartridge data while guarding production reads.</summary>
    /// <param name="rom">Retail address space used as the expected data source and selector reference.</param>
    private static void VerifyCeresSteamInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < CeresSteamInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                CeresSteamInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadCeresSteamInstructionWord(rom, 0xa60000 | definition.Address),
                $"Ceres steam instruction mechanics word $A6:{definition.Address:X4}");
        }

        var executedOperands = new HashSet<ushort>();
        var guard = new CeresSteamInstructionProgramReadGuard(rom);
        (CeresSteamVariant Variant, ushort Program, ushort Active)[] programs =
        [
            (CeresSteamVariant.Up,
                CeresSteamInstructionProgramDefinitions.Up,
                CeresSteamInstructionProgramDefinitions.UpActive),
            (CeresSteamVariant.Left,
                CeresSteamInstructionProgramDefinitions.Left,
                CeresSteamInstructionProgramDefinitions.LeftActive),
            (CeresSteamVariant.Down,
                CeresSteamInstructionProgramDefinitions.Down,
                CeresSteamInstructionProgramDefinitions.DownActive),
            (CeresSteamVariant.Right,
                CeresSteamInstructionProgramDefinitions.Right,
                CeresSteamInstructionProgramDefinitions.RightActive),
        ];

        foreach ((CeresSteamVariant variant, ushort program, ushort active) in programs)
        {
            RoomEnemySystem enemies = CreateSystem(variant, out RoomEnemySlot slot);
            RunFrames(enemies, slot, 1);
            AssertTrue(slot.Properties.HasAny(
                    EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision),
                $"Ceres steam {variant} starts hidden and intangible");
            AssertEqual(unchecked((ushort)(program + 6)), slot.CurrentInstruction,
                $"Ceres steam {variant} reaches its activation branch");

            RunFrames(enemies, slot, 1);
            AssertTrue(!slot.Properties.HasAny(
                    EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision),
                $"Ceres steam {variant} activation reveals its plume");
            AssertEqual(unchecked((ushort)(active + 4)), slot.CurrentInstruction,
                $"Ceres steam {variant} begins its active frames");

            RunFrames(enemies, slot, 21);
            AssertTrue(slot.Properties.HasAny(
                    EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision),
                $"Ceres steam {variant} enters its hidden hold");

            RunFrames(enemies, slot, 64);
            AssertTrue(!slot.Properties.HasAny(
                    EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision),
                $"Ceres steam {variant} hold returns to active frames");
            AssertEqual(unchecked((ushort)(active + 4)), slot.CurrentInstruction,
                $"Ceres steam {variant} completes its full cycle");
        }

        AssertEqual(CeresSteamInstructionProgramDefinitionsTooling.PresentationWordCount,
            executedOperands.Count,
            "all executed visual selectors match the cartridge");
        for (int index = 0;
             index < CeresSteamInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address =
                CeresSteamInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"Ceres steam execution covers presentation $A6:{address:X4}");
        }

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "compiled visual selectors require no runtime cartridge reads");
        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Ceres Steam production uses compiled visual selectors without ROM reads");
        for (int index = 0; index < CeresSteamInstructionProgramDefinitionsTooling.PresentationWordCount; index++)
        {
            ushort address = CeresSteamInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa6, address, out ushort selector),
                "Ceres Steam visual operand has a compiled selector");
            AssertEqual(ReadVerificationWord(rom, (0xa6 << 16) | address), selector,
                "Ceres Steam compiled visual selector matches original operand");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Ceres steam mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => CeresSteamInstructionProgramDefinitions.ReadMechanicsWord(0xf051),
            "Ceres steam extended-spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CeresSteamInstructionProgramDefinitions.ReadMechanicsWord(0xf11d),
            "adjacent Ceres steam callback code is rejected as mechanics");

        _ = ProbeCeresSteamInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeCeresSteamInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Ceres steam allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Ceres steam mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Ceres steam instruction mechanics: sixty-eight compiled words, four shared " +
            "directional cycles, and thirty-six executed selectors match cartridge data with " +
            "runtime ROM reads forbidden.");

        RoomEnemySystem CreateSystem(CeresSteamVariant variant, out RoomEnemySlot slot)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
                enemies,
                (Func<ushort>)(() => 0));
            var initialize = typeof(RoomEnemySystem).GetMethod(
                "InitializeCeresSteam", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = CeresSteamDefinitions.EnemyDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
            slot.Parameter1 = (ushort)variant;
            initialize(slot);
            return enemies;
        }

        void RunFrames(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
            {
                process.Invoke(enemies, arguments);
                VerifyExecutedEnemySelector(rom, slot, executedOperands);
            }
        }
    }

    /// <summary>Exercises repeated compiled mechanics lookups so the warmed allocation check can detect per-call storage.</summary>
    /// <returns>A checksum that prevents the lookup loop from being observationally unused.</returns>
    private static int ProbeCeresSteamInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += CeresSteamInstructionProgramDefinitions.ReadMechanicsWord(
                CeresSteamInstructionProgramDefinitions.Up);
        }
        return checksum;
    }

    /// <summary>Reads one little-endian mechanics word from the banked cartridge address space.</summary>
    /// <param name="bus">Address space supplying the two consecutive bytes.</param>
    /// <param name="address">Address of the low byte; the high byte is read from the following address.</param>
    /// <returns>The combined 16-bit word.</returns>
    private static ushort ReadCeresSteamInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Wraps cartridge access to record presentation-word reads and reject reads of compiled mechanics bytes.</summary>
    /// <param name="source">Underlying address space receiving permitted reads and writes.</param>
    private sealed class CeresSteamInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Bank-$A6 presentation words observed through this guard during production execution.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        /// <summary>Number of attempted reads of mechanics bytes that should have come from compiled definitions.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes an import-source byte request through the guarded address-space read path.</summary>
        /// <param name="address">Cartridge address requested by the caller.</param>
        /// <returns>The byte returned by the underlying address space when the read is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects runtime mechanics reads, records accesses to compiled presentation operands, and delegates other reads.</summary>
        /// <param name="address">Address requested from the wrapped SNES address space.</param>
        /// <returns>The underlying byte for an allowed read.</returns>
        public byte ReadByte(int address)
        {
            if (CeresSteamInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Ceres steam mechanics ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa60000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < CeresSteamInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        CeresSteamInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
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
