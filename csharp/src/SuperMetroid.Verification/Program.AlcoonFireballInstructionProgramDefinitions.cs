using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyAlcoonFireballInstructionProgramDefinitions()
    {
        VerifyAlcoonFireballInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyAlcoonFireballInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < AlcoonFireballInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            AlcoonFireballInstructionMechanicsWord definition =
                AlcoonFireballInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadAlcoonFireballInstructionWord(rom, definition.Address),
                $"Alcoon-fireball mechanics word $86:{definition.Address:X4}");
        }

        var guard = new AlcoonFireballInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guard);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions",
            instanceFlags)!;
        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnAlcoonFireball",
            instanceFlags)!;

        var source = new RoomEnemySlot(0)
        {
            XPosition = 128,
            YPosition = 112,
            VramTilesIndex = 0x0200,
            PaletteIndex = 0x0c00,
        };
        var state = new AlcoonEnemyState(
            source,
            new ushort[32],
            new ushort[32],
            new ushort[32],
            new ushort[32],
            new ushort[32]);
        var states = (AlcoonEnemyState?[])typeof(RoomEnemySystem)
            .GetField("_alcoonStates", instanceFlags)!
            .GetValue(enemies)!;
        states[0] = state;

        foreach (short direction in new short[] { -2, 2 })
        {
            state.XVelocity = unchecked((ushort)direction);
            foreach (ushort velocityOffset in new ushort[] { 0, 2, 4 })
                spawn.Invoke(enemies, [source, velocityOffset]);
        }

        RoomEnemyProjectileSlot[] fireballs = enemies.EnemyProjectiles
            .Where(projectile => projectile.Kind == RoomEnemyProjectileKind.AlcoonFireball)
            .ToArray();
        AssertEqual(6, fireballs.Length,
            "all three real Alcoon fire commands spawn in both facings");
        AssertEqual(3, fireballs.Count(projectile => unchecked((short)projectile.XVelocity) < 0),
            "left-facing Alcoon launches all three fireball arcs left");
        AssertEqual(3, fireballs.Count(projectile => unchecked((short)projectile.XVelocity) > 0),
            "right-facing Alcoon launches all three fireball arcs right");

        foreach (RoomEnemyProjectileSlot fireball in fireballs)
        {
            AssertEqual(AlcoonFireballInstructionProgramDefinitions.Initial,
                fireball.InstructionPointer,
                "real Alcoon fireball producer selects the named animation loop");
            RunForcedTicks(fireball, 5);
            AssertEqual(
                unchecked((ushort)(AlcoonFireballInstructionProgramDefinitions.Initial + 4)),
                fireball.InstructionPointer,
                "Alcoon fireball completes all four frames and loops to its first frame");
            AssertTrue(fireball.IsActive,
                "Alcoon fireball animation loop remains active until movement or a hit deletes it");
        }

        fireballs[0].InstructionPointer =
            CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTicks(fireballs[0], 1);
        AssertTrue(!fireballs[0].IsActive,
            "Alcoon fireball shot reaction reaches the compiled shared delete program");

        AssertEqual(AlcoonFireballInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Alcoon-fireball spritemap operands remain cartridge reads");
        for (int index = 0;
             index < AlcoonFireballInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = AlcoonFireballInstructionProgramDefinitions
                .PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Alcoon-fireball presentation $86:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Alcoon-fireball and shared-delete mechanics byte");
        AssertThrows<InvalidDataException>(
            () => AlcoonFireballInstructionProgramDefinitions.ReadMechanicsWord(0x9ea0),
            "Alcoon-fireball spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => AlcoonFireballInstructionProgramDefinitions.ReadMechanicsWord(0x9eb2),
            "adjacent Alcoon-fireball initializer is rejected as mechanics");

        _ = ProbeAlcoonFireballInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeAlcoonFireballInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Alcoon-fireball allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Alcoon-fireball mechanics lookups allocate no storage");

        Console.WriteLine(
            "Alcoon-fireball instruction mechanics: six compiled words, all three real " +
            "fire commands in both facings, six complete loops, shared shot deletion, " +
            "and four live spritemap reads pass with mechanics bytes forbidden.");

        void RunForcedTicks(RoomEnemyProjectileSlot projectile, int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
            }
        }
    }

    private static int ProbeAlcoonFireballInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += AlcoonFireballInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? AlcoonFireballInstructionProgramDefinitions.Initial
                    : AlcoonFireballInstructionProgramDefinitions.Loop);
        }
        return checksum;
    }

    private static ushort ReadAlcoonFireballInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    private sealed class AlcoonFireballInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (AlcoonFireballInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                CommonEnemyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Alcoon-fireball mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < AlcoonFireballInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = AlcoonFireballInstructionProgramDefinitions
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
