using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;

internal static partial class Program
{
    private static void VerifyCacatacInstructionProgramDefinitions()
    {
        VerifyCacatacInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyCacatacInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < CacatacInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            CacatacInstructionMechanicsWord definition =
                CacatacInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadCacatacInstructionWord(rom, 0xa20000 | definition.Address),
                $"Cacatac instruction mechanics word $A2:{definition.Address:X4}");
        }

        var guard = new CacatacInstructionProgramReadGuard(rom);
        VerifyIdle(upsideUp: true, CacatacInstructionProgramDefinitions.UpsideUpIdle);
        VerifyIdle(upsideUp: false, CacatacInstructionProgramDefinitions.UpsideDownIdle);
        VerifyAttack(
            upsideUp: true,
            CacatacInstructionProgramDefinitions.UpsideUpAttack,
            [
                CacatacSpikeDirection.LeftFacingUp,
                CacatacSpikeDirection.UpLeft,
                CacatacSpikeDirection.Up,
                CacatacSpikeDirection.UpRight,
                CacatacSpikeDirection.RightFacingUp,
            ]);
        VerifyAttack(
            upsideUp: false,
            CacatacInstructionProgramDefinitions.UpsideDownAttack,
            [
                CacatacSpikeDirection.LeftFacingDown,
                CacatacSpikeDirection.DownLeft,
                CacatacSpikeDirection.Down,
                CacatacSpikeDirection.DownRight,
                CacatacSpikeDirection.RightFacingDown,
            ]);

        for (int index = 0;
             index < CacatacInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = CacatacInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertEqual(ReadCacatacInstructionWord(rom, 0xa20000 | address),
                EnemySpritemapDefinitions.CacatacFrameAt(address),
                $"compiled Cacatac visual selector $A2:{address:X4} matches cartridge");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Cacatac mechanics and visual bytes");
        AssertThrows<InvalidDataException>(
            () => CacatacInstructionProgramDefinitions.ReadMechanicsWord(0x9e8e),
            "Cacatac spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => CacatacInstructionProgramDefinitions.ReadMechanicsWord(0x9f2a),
            "adjacent Cacatac callback code is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.CacatacFrameAt(0x9f30),
            "uncompiled Cacatac visual selector is rejected loudly");

        _ = ProbeCacatacInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeCacatacInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Cacatac allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Cacatac mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Cacatac instruction mechanics: fifty-six compiled words, four complete " +
            "idle/attack programs, ten real spike spawns, and twenty-four compiled visual " +
            "selectors pass with source bytes forbidden.");

        void VerifyIdle(bool upsideUp, ushort program)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, CacatacEnemyState state) =
                CreateSystem(upsideUp);
            RunFrames(enemies, slot, 65);
            AssertEqual(unchecked((ushort)(program + 6)), slot.CurrentInstruction,
                $"Cacatac {(upsideUp ? "upright" : "inverted")} idle loops");
            AssertEqual(CacatacEnemyFunction.MovingLeft, state.Function,
                $"Cacatac {(upsideUp ? "upright" : "inverted")} idle restores patrol");
        }

        void VerifyAttack(
            bool upsideUp,
            ushort program,
            CacatacSpikeDirection[] expectedDirections)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, CacatacEnemyState state) =
                CreateSystem(upsideUp);
            state.Function = CacatacEnemyFunction.Stopped;
            typeof(RoomEnemySystem).GetMethod("SetCacatacInstructionList", flags)!
                .Invoke(null, [slot, program]);
            RunFrames(enemies, slot, 53);

            ushort idle = upsideUp
                ? CacatacInstructionProgramDefinitions.UpsideUpIdle
                : CacatacInstructionProgramDefinitions.UpsideDownIdle;
            AssertEqual(unchecked((ushort)(idle + 6)), slot.CurrentInstruction,
                $"Cacatac {(upsideUp ? "upright" : "inverted")} attack returns to idle");
            AssertEqual(CacatacEnemyFunction.MovingLeft, state.Function,
                $"Cacatac {(upsideUp ? "upright" : "inverted")} attack resumes patrol");
            AssertEqual((ushort)0x0034, enemies.LastCacatacSoundEffect!.Value,
                $"Cacatac {(upsideUp ? "upright" : "inverted")} attack queues sound");

            ushort[] actualDirections = enemies.EnemyProjectiles
                .Where(projectile => projectile.Kind == RoomEnemyProjectileKind.CacatacSpike)
                .Select(projectile => projectile.Variable0)
                .Order()
                .ToArray();
            ushort[] expected = expectedDirections.Select(value => (ushort)value).Order().ToArray();
            AssertEqual(expected.Length, actualDirections.Length,
                $"Cacatac {(upsideUp ? "upright" : "inverted")} spike count");
            for (int index = 0; index < expected.Length; index++)
            {
                AssertEqual(expected[index], actualDirections[index],
                    $"Cacatac {(upsideUp ? "upright" : "inverted")} spike {index}");
            }
        }

        (RoomEnemySystem Enemies, RoomEnemySlot Slot, CacatacEnemyState State) CreateSystem(
            bool upsideUp)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeCacatac", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.CacatacDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            slot.Parameter1 = upsideUp ? (ushort)0x0100 : (ushort)0;
            initialize(slot);
            return (enemies, slot, enemies.CacatacStates[0]!);
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

    private static int ProbeCacatacInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += CacatacInstructionProgramDefinitions.ReadMechanicsWord(
                CacatacInstructionProgramDefinitions.UpsideUpIdle);
        }
        return checksum;
    }

    private static ushort ReadCacatacInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class CacatacInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (CacatacInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                IsCompiledPresentationByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Cacatac instruction ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        private static bool IsCompiledPresentationByte(int address)
        {
            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < CacatacInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        CacatacInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
