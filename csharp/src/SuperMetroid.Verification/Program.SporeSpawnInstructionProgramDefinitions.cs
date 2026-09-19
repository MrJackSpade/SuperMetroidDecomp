using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Compares the complete mechanics side of Spore Spawn's mixed streams with the pinned
    /// ROM, then drives all five production entry points with those source bytes forbidden.
    /// </summary>
    private static void VerifySporeSpawnInstructionProgramDefinitions()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Spore Spawn instruction mechanics: cartridge comparison skipped " +
                "(private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        for (int index = 0;
             index < SporeSpawnInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            SporeSpawnInstructionMechanicsWord definition =
                SporeSpawnInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadSporeSpawnProgramWord(rom, definition.Address),
                $"Spore Spawn mechanics word $A5:{definition.Address:X4}");
        }

        var guarded = new SporeSpawnInstructionReadGuard(rom);
        (RoomEnemySystem deadSystem, RoomEnemySlot dead) = RunSporeSpawnProgram(
            guarded,
            SporeSpawnInstructionProgramDefinitions.InitialDead,
            4);
        AssertEqual(0xe6c5, dead.CurrentInstruction,
            "defeated Spore Spawn program sleeps at its native pointer");
        AssertEqual(SporeSpawnFunction.Idle, deadSystem.SporeSpawn!.Function,
            "defeated Spore Spawn program selects idle function");

        (RoomEnemySystem aliveSystem, RoomEnemySlot alive) = RunSporeSpawnProgram(
            guarded,
            SporeSpawnInstructionProgramDefinitions.InitialAlive,
            260);
        AssertEqual(0xe6d3, alive.CurrentInstruction,
            "living Spore Spawn initialization sleeps after descent selection");
        AssertEqual(SporeSpawnFunction.Descending, aliveSystem.SporeSpawn!.Function,
            "living Spore Spawn initialization selects descent function");

        (RoomEnemySystem fightSystem, _) = RunSporeSpawnProgram(
            guarded,
            SporeSpawnInstructionProgramDefinitions.FightStarted,
            2200);
        AssertTrue(fightSystem.SporeSpawn!.MaximumXRadius >= 0x0040,
            "fight program executes compiled radius setup and growth");

        (RoomEnemySystem closeSystem, _) = RunSporeSpawnProgram(
            guarded,
            SporeSpawnInstructionProgramDefinitions.CloseAndMove,
            1600);
        AssertEqual(SporeSpawnFunction.Moving, closeSystem.SporeSpawn!.Function,
            "close-and-move program executes compiled function callback");

        (RoomEnemySystem deathSystem, RoomEnemySlot death) = RunSporeSpawnProgram(
            guarded,
            SporeSpawnInstructionProgramDefinitions.Death,
            500);
        AssertEqual(0xe80f, death.CurrentInstruction,
            "Spore Spawn death program reaches its native sleep pointer");
        AssertTrue(deathSystem.SporeSpawn!.DeathDropRequested,
            "Spore Spawn death program executes compiled drop callback");

        AssertEqual(
            SporeSpawnInstructionProgramDefinitions.PresentationWordCount,
            guarded.ObservedPresentationWords.Count,
            "all live Spore Spawn spritemap words remain cartridge reads");
        for (int index = 0;
             index < SporeSpawnInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                SporeSpawnInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guarded.ObservedPresentationWords.Contains(address),
                $"production execution reads presentation word $A5:{address:X4}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production execution avoids every compiled Spore Spawn mechanics byte");

        AssertThrows<InvalidDataException>(
            () => SporeSpawnInstructionProgramDefinitions.ReadMechanicsWord(0xe6c3),
            "interleaved spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => SporeSpawnInstructionProgramDefinitions.ReadMechanicsWord(0xffff),
            "restored pointer outside the translated programs fails loudly");

        _ = SporeSpawnInstructionProgramDefinitions.ReadMechanicsWord(
            SporeSpawnInstructionProgramDefinitions.FightStarted);
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += SporeSpawnInstructionProgramDefinitions.ReadMechanicsWord(
                SporeSpawnInstructionProgramDefinitions.FightStarted);
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Spore Spawn allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Spore Spawn mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            $"  Spore Spawn instruction mechanics: " +
            $"{SporeSpawnInstructionProgramDefinitions.MechanicsWordCount} words, " +
            $"{SporeSpawnInstructionProgramDefinitions.PresentationWordCount} live " +
            "spritemap words, and all five production entry points pass with mechanics " +
            "reads forbidden.");
    }

    private static (RoomEnemySystem System, RoomEnemySlot Body) RunSporeSpawnProgram(
        SporeSpawnInstructionReadGuard bus,
        ushort initialPointer,
        int frames)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        RoomEnemySlot body = enemies.Slots[0];
        body.EnemyDefinitionPointer = RoomEnemySystem.SporeSpawnDefinition;
        body.Definition = default(RoomEnemyDefinition) with { Bank = 0xa5 };
        body.XPosition = 128;
        body.YPosition = 624;
        body.Health = 960;
        body.CurrentInstruction = initialPointer;
        body.InstructionTimer = 1;

        var state = new SporeSpawnEnemyState(body)
        {
            Function = SporeSpawnFunction.Idle,
            MovementCenterX = body.XPosition,
            MovementCenterY = body.YPosition,
            MaximumXRadius = 48,
            AngleDelta = 1,
        };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
            enemies,
            (Func<ushort>)(() => 0x1234));
        typeof(RoomEnemySystem).GetField("_samusForEnemyDrops", flags)!.SetValue(
            enemies,
            new SamusState { Health = 99, MaxHealth = 99 });
        typeof(RoomEnemySystem).GetField("_sporeSpawn", flags)!.SetValue(enemies, state);

        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        object?[] arguments = [body, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int frame = 0; frame < frames; frame++)
            process.Invoke(enemies, arguments);

        return (enemies, body);
    }

    private static ushort ReadSporeSpawnProgramWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa50000 | address) |
            source.ReadByte(0xa50000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class SporeSpawnInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (SporeSpawnInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Spore Spawn mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa50000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < SporeSpawnInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        SporeSpawnInstructionProgramDefinitions.PresentationWordAddress(index);
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
