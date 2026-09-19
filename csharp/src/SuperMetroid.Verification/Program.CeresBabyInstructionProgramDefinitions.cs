using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresBabyInstructionProgramDefinitions()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Ceres Baby instruction mechanics: cartridge comparison skipped " +
                "(private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        for (int index = 0;
             index < CeresBabyInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            CeresBabyInstructionMechanicsWord definition =
                CeresBabyInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadCeresBabyProgramWord(rom, definition.Address),
                $"Ceres Baby mechanics word $A6:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new CeresBabyInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
        ushort random = 0;
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
            enemies,
            (Func<ushort>)(() => random));
        var advance = typeof(RoomEnemySystem).GetMethod(
                "AdvanceCeresBabyDrawInstruction", flags)!
            .CreateDelegate<Func<RidleyEnemyState, ushort>>(enemies);

        var completeLoop = new RidleyEnemyState
        {
            BabyInstruction = CeresBabyInstructionProgramDefinitions.Initial,
            BabyInstructionTimer = 1,
            BabyVerticalVelocity = 0,
        };
        var observedSpritemaps = new HashSet<ushort>();
        for (int call = 0; call < 500; call++)
            observedSpritemaps.Add(advance(completeLoop));

        AssertTrue(observedSpritemaps.Contains(0xbffd),
            "production Ceres Baby loop displays horizontal-squish spritemap");
        AssertTrue(observedSpritemaps.Contains(0xc018),
            "production Ceres Baby loop displays round spritemap");
        AssertTrue(observedSpritemaps.Contains(0xc033),
            "production Ceres Baby loop displays vertical-squish spritemap");
        AssertEqual(
            CeresBabyInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "complete Ceres Baby loop retains every live presentation operand");
        for (int index = 0;
             index < CeresBabyInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                CeresBabyInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production Ceres Baby loop reads presentation word $A6:{address:X4}");
        }

        // A stationary odd-RNG call takes the authored 50% branch back to the initial
        // program; a moving call bypasses that random branch and enters the expressive
        // palette loop. These are the two conditional control-flow edges in the stream.
        random = 1;
        var randomBranch = new RidleyEnemyState
        {
            BabyInstruction = CeresBabyInstructionProgramDefinitions.ExpressiveLoop,
            BabyInstructionTimer = 1,
            BabyVerticalVelocity = 0,
        };
        _ = advance(randomBranch);
        AssertEqual((ushort)0xbf35, randomBranch.BabyInstruction,
            "stationary odd-RNG branch returns to initial Ceres Baby frames");

        random = 0;
        var movingBranch = new RidleyEnemyState
        {
            BabyInstruction = CeresBabyInstructionProgramDefinitions.Initial,
            BabyInstructionTimer = 1,
            BabyVerticalVelocity = 1,
        };
        _ = advance(movingBranch);
        AssertEqual((ushort)0xbf61, movingBranch.BabyInstruction,
            "moving Ceres Baby branch enters expressive palette frames");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production Ceres Baby interpreter avoids compiled mechanics bytes");

        AssertThrows<InvalidDataException>(
            () => CeresBabyInstructionProgramDefinitions.ReadMechanicsWord(0xbf37),
            "Ceres Baby spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CeresBabyInstructionProgramDefinitions.ReadMechanicsWord(0xbfc9),
            "Ceres Baby restored pointer cannot enter adjacent callback code");

        _ = CeresBabyInstructionProgramDefinitions.ReadMechanicsWord(
            CeresBabyInstructionProgramDefinitions.Initial);
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += CeresBabyInstructionProgramDefinitions.ReadMechanicsWord(
                CeresBabyInstructionProgramDefinitions.Initial);
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Ceres Baby allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Ceres Baby mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            $"  Ceres Baby instruction mechanics: " +
            $"{CeresBabyInstructionProgramDefinitions.MechanicsWordCount} words and " +
            $"{CeresBabyInstructionProgramDefinitions.PresentationWordCount} live " +
            "palette/spritemap operands pass through the complete production loop.");
    }

    private static ushort ReadCeresBabyProgramWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa60000 | address) |
            source.ReadByte(0xa60000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class CeresBabyInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (CeresBabyInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Ceres Baby mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa60000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < CeresBabyInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        CeresBabyInstructionProgramDefinitions.PresentationWordAddress(index);
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
