using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyNoobTubeProjectileInstructionProgramDefinitions() =>
        VerifyNoobTubeProjectileInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));

    private static void VerifyNoobTubeProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        AssertEqual(207,
            NoobTubeProjectileInstructionProgramDefinitions.MechanicsWordCount,
            "n00b-tube catalog contains every mechanics word");
        for (int index = 0;
             index < NoobTubeProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            NoobTubeProjectileInstructionMechanicsWord definition =
                NoobTubeProjectileInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"n00b-tube mechanics word $86:{definition.Address:X4}");
        }

        var guard = new NoobTubeProjectileInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        FieldInfo projectileFrame = typeof(RoomEnemySystem).GetField(
            "_currentEnemyProjectileFrame8", flags)!;

        RoomEnemySystem crackSystem = NewSystem();
        RoomEnemyProjectileSlot crack = Spawn(
            crackSystem,
            RoomEnemyProjectileKind.NoobTubeCrack,
            parameter: 0);
        AssertEqual(NoobTubeProjectileInstructionProgramDefinitions.Crack,
            crack.InstructionPointer,
            "real n00b-tube crack producer selects the named program");
        AssertEqual(48, RunThroughDeletion(crackSystem, crack, maximumTicks: 64),
            "n00b-tube crack completes 47 frames and its terminal deletion tick");

        for (ushort parameter = 0; parameter <= 18; parameter += 2)
        {
            RoomEnemySystem shardSystem = NewSystem();
            RoomEnemyProjectileSlot shard = Spawn(
                shardSystem,
                RoomEnemyProjectileKind.NoobTubeShard,
                parameter);
            AssertEqual(
                NoobTubeProjectileInstructionProgramDefinitions
                    .ShardInstructionLists[parameter / 2],
                shard.InstructionPointer,
                $"real n00b-tube shard producer selects parameter ${parameter:X2} program");
            AssertEqual(305, RunThroughDeletion(shardSystem, shard, maximumTicks: 320),
                $"n00b-tube shard ${parameter:X2} completes both counted flicker phases");
        }

        for (ushort parameter = 0; parameter <= 10; parameter += 2)
        {
            RoomEnemySystem bubbleSystem = NewSystem();
            RoomEnemyProjectileSlot bubble = Spawn(
                bubbleSystem,
                RoomEnemyProjectileKind.NoobTubeReleasedAirBubble,
                parameter);
            AssertEqual(NoobTubeProjectileInstructionProgramDefinitions.ReleasedAirBubble,
                bubble.InstructionPointer,
                $"real released-air-bubble producer selects parameter ${parameter:X2} program");
            AssertEqual(16, RunThroughDeletion(bubbleSystem, bubble, maximumTicks: 24),
                $"released-air bubble ${parameter:X2} completes fifteen frames and deletion");
        }

        RoomEnemySystem shotSystem = NewSystem();
        RoomEnemyProjectileSlot shot = Spawn(
            shotSystem,
            RoomEnemyProjectileKind.NoobTubeCrack,
            parameter: 0);
        shot.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        shot.InstructionTimer = 1;
        Process(shotSystem, shot);
        AssertTrue(!shot.IsActive,
            "n00b-tube shot reaction reaches shared compiled deletion");

        AssertEqual(NoobTubeProjectileInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all n00b-tube spritemap operands remain cartridge reads");
        AssertEqual(90,
            NoobTubeProjectileInstructionProgramDefinitions.PresentationWordCount,
            "n00b-tube catalog retains all ninety presentation operands");
        for (int index = 0;
             index < NoobTubeProjectileInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = NoobTubeProjectileInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production reads n00b-tube presentation $86:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids private and shared n00b-tube mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => NoobTubeProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xd3d9),
            "n00b-tube crack spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => NoobTubeProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xd69a),
            "released-air-bubble callback body is rejected as mechanics");

        _ = ProbeNoobTubeProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeNoobTubeProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "n00b-tube allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed n00b-tube mechanics lookups allocate no storage");

        Console.WriteLine(
            "N00b-tube projectile instruction mechanics: 207 compiled words, all seventeen " +
            "real burst producers, complete crack/shard/bubble lifecycles, shared deletion, " +
            "and ninety live spritemap reads pass.");

        RoomEnemySystem NewSystem()
        {
            var system = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(system, guard);
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!
                .SetValue(system, (Func<ushort>)(() => 0x4040));
            return system;
        }

        RoomEnemyProjectileSlot Spawn(
            RoomEnemySystem system,
            RoomEnemyProjectileKind kind,
            ushort parameter)
        {
            ushort definition = kind switch
            {
                RoomEnemyProjectileKind.NoobTubeCrack => NoobTubePlmRomData.CrackProjectile,
                RoomEnemyProjectileKind.NoobTubeShard => NoobTubePlmRomData.ShardProjectile,
                RoomEnemyProjectileKind.NoobTubeReleasedAirBubble =>
                    NoobTubePlmRomData.ReleasedAirBubbleProjectile,
                _ => throw new ArgumentOutOfRangeException(nameof(kind)),
            };
            system.SpawnNoobTubeProjectile(
                new NoobTubeProjectileRequest(definition, parameter, PlmBlockIndex: 42),
                roomWidthInBlocks: 16);
            return system.EnemyProjectiles.Single(projectile => projectile.Kind == kind);
        }

        int RunThroughDeletion(
            RoomEnemySystem system,
            RoomEnemyProjectileSlot projectile,
            int maximumTicks)
        {
            for (int tick = 1; tick <= maximumTicks; tick++)
            {
                projectileFrame.SetValue(system, unchecked((byte)tick));
                projectile.InstructionTimer = 1;
                Process(system, projectile);
                if (!projectile.IsActive)
                    return tick;
            }
            throw new InvalidOperationException(
                $"N00b-tube projectile {projectile.Kind} exceeded {maximumTicks} forced ticks.");
        }

        void Process(RoomEnemySystem system, RoomEnemyProjectileSlot projectile) =>
            process.Invoke(system, [projectile, new SamusState(), (ushort)0, (ushort)0]);
    }

    private static int ProbeNoobTubeProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += NoobTubeProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? NoobTubeProjectileInstructionProgramDefinitions.Crack
                    : NoobTubeProjectileInstructionProgramDefinitions.ReleasedAirBubble);
        }
        return checksum;
    }

    private sealed class NoobTubeProjectileInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (NoobTubeProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled n00b-tube mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < NoobTubeProjectileInstructionProgramDefinitions
                         .PresentationWordCount;
                     index++)
                {
                    ushort presentation = NoobTubeProjectileInstructionProgramDefinitions
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
