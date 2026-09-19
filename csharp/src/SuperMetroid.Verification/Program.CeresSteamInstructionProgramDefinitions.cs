using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresSteamInstructionProgramDefinitions()
    {
        VerifyCeresSteamInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyCeresSteamInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < CeresSteamInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            CeresSteamInstructionMechanicsWord definition =
                CeresSteamInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadCeresSteamInstructionWord(rom, 0xa60000 | definition.Address),
                $"Ceres steam instruction mechanics word $A6:{definition.Address:X4}");
        }

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

        AssertEqual(CeresSteamInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Ceres steam extended-spritemap words remain cartridge reads");
        for (int index = 0;
             index < CeresSteamInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                CeresSteamInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Ceres steam presentation $A6:{address:X4}");
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
            "directional cycles, and thirty-six live extended-spritemap reads pass with " +
            "mechanics bytes forbidden.");

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

        static void RunFrames(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

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

    private static ushort ReadCeresSteamInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class CeresSteamInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (CeresSteamInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Ceres steam mechanics ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa60000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < CeresSteamInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        CeresSteamInstructionProgramDefinitions.PresentationWordAddress(index);
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
