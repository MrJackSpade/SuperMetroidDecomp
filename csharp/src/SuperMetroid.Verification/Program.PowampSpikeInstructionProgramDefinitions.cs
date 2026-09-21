using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyPowampSpikeInstructionProgramDefinitions()
    {
        VerifyPowampSpikeInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyPowampSpikeInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < PowampSpikeInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            PowampSpikeInstructionMechanicsWord definition =
                PowampSpikeInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadPowampSpikeInstructionWord(rom, definition.Address),
                $"Powamp-spike mechanics word $86:{definition.Address:X4}");
        }

        var guard = new PowampSpikeInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", instanceFlags)!;
        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnPowampSpikeBurst", instanceFlags)!;
        MethodInfo beginDeletion = typeof(RoomEnemySystem).GetMethod(
            "BeginPowampSpikeDeletion", staticFlags)!;

        var body = new RoomEnemySlot(0)
        {
            XPosition = 128,
            YPosition = 112,
            VramTilesIndex = 0x0200,
            PaletteIndex = 0x0c00,
        };
        spawn.Invoke(enemies, [body]);

        RoomEnemyProjectileSlot[] spikes = enemies.EnemyProjectiles
            .Where(projectile => projectile.Kind == RoomEnemyProjectileKind.PowampSpike)
            .ToArray();
        AssertEqual(8, spikes.Length,
            "real Powamp burst producer spawns all eight projectile directions");
        AssertTrue(spikes.Select(spike => spike.DirectionParameter).Order().SequenceEqual(
                Enumerable.Range(0, 8).Select(value => (ushort)value)),
            "real Powamp burst owns every native direction exactly once");
        foreach (RoomEnemyProjectileSlot spike in spikes)
        {
            AssertEqual(PowampSpikeInstructionProgramDefinitions.Initial,
                spike.InstructionPointer,
                "real Powamp spike producer selects the named animation loop");
            RunForcedTicks(spike, 4);
            AssertEqual(
                unchecked((ushort)(PowampSpikeInstructionProgramDefinitions.Initial + 4)),
                spike.InstructionPointer,
                "Powamp spike completes all three frames and loops to its first frame");
            AssertTrue(spike.IsActive,
                "Powamp spike animation remains active until room collision deletes it");
        }

        beginDeletion.Invoke(null, [spikes[0]]);
        AssertEqual(PowampSpikeInstructionProgramDefinitions.Delete,
            spikes[0].InstructionPointer,
            "production collision handoff installs the named private delete list");
        RunForcedTicks(spikes[0], 1);
        AssertTrue(!spikes[0].IsActive,
            "Powamp spike private delete list clears the projectile");

        AssertEqual(PowampSpikeInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Powamp-spike spritemap operands remain cartridge reads");
        for (int index = 0;
             index < PowampSpikeInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = PowampSpikeInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Powamp-spike presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Powamp-spike mechanics byte");
        AssertThrows<InvalidDataException>(
            () => PowampSpikeInstructionProgramDefinitions.ReadMechanicsWord(0xd20a),
            "Powamp-spike spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => PowampSpikeInstructionProgramDefinitions.ReadMechanicsWord(0xd21a),
            "adjacent Powamp-spike velocity table is rejected as mechanics");

        _ = ProbePowampSpikeInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbePowampSpikeInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Powamp-spike allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Powamp-spike mechanics lookups allocate no storage");

        Console.WriteLine(
            "Powamp-spike instruction mechanics: six compiled words, all eight real burst " +
            "directions, complete loops, private collision deletion, and three live " +
            "spritemap reads pass with mechanics bytes forbidden.");

        void RunForcedTicks(RoomEnemyProjectileSlot projectile, int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
            }
        }
    }

    private static int ProbePowampSpikeInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += PowampSpikeInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? PowampSpikeInstructionProgramDefinitions.Initial
                    : PowampSpikeInstructionProgramDefinitions.Delete);
        }
        return checksum;
    }

    private static ushort ReadPowampSpikeInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    private sealed class PowampSpikeInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (PowampSpikeInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Powamp-spike mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < PowampSpikeInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = PowampSpikeInstructionProgramDefinitions
                        .PresentationWordAddress(index);
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
