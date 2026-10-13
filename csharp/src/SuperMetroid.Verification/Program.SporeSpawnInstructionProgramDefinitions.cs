using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static EnemyExtendedFrameArtworkSporeSpawnSelectorsVerification;

internal static class SporeSpawnInstructionProgramDefinitionsVerification
{
    /// <summary>
    /// Compares the complete mechanics side of Spore Spawn's mixed streams with the pinned
    /// ROM, then drives all five production entry points with those source bytes forbidden.
    /// </summary>
    internal static void VerifySporeSpawnInstructionProgramDefinitions()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                "  Spore Spawn instruction mechanics: cartridge comparison skipped " +
                "(private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        for (int index = 0;
             index < SporeSpawnInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                SporeSpawnInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadSporeSpawnProgramWord(rom, definition.Address),
                $"Spore Spawn mechanics word $A5:{definition.Address:X4}");
        }

        var guarded = NewSporeSpawnInstructionReadGuard(rom);
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

        var selected = new HashSet<ushort>();
        foreach ((ushort pointer, int frames) in new (ushort, int)[]
                 { (0xe6b9, 4), (0xe6c7, 260), (0xe6d5, 2200), (0xe729, 1600), (0xe77d, 500) })
            _ = ReadReferenceSporeSpawnProgram(rom, pointer, frames, selected);
        AssertEqual(SporeSpawnInstructionProgramDefinitionsTooling.PresentationWordCount,
            selected.Count, "all native Spore Spawn selectors execute in the five fixture programs");
        for (int index = 0; index < SporeSpawnInstructionProgramDefinitionsTooling.PresentationWordCount; index++)
        {
            ushort address = SporeSpawnInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertTrue(selected.Contains(address), "native stream covers the declared selector");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa5, address, out ushort value),
                "Spore Spawn selector is compiled");
            AssertEqual(ReadSporeSpawnProgramWord(rom, address), value,
                $"compiled Spore Spawn selector matches cartridge at {address:X4}");
        }
        AssertEqual(0, guarded.ObservedWords.Count,
            "production Spore Spawn programs never read cartridge selectors");
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
            $"{SporeSpawnInstructionProgramDefinitionsTooling.MechanicsWordCount} words, " +
            $"{SporeSpawnInstructionProgramDefinitionsTooling.PresentationWordCount} live " +
            "spritemap words, and all five production entry points pass with mechanics " +
            "reads forbidden.");
    }

    internal static (RoomEnemySystem System, RoomEnemySlot Body) RunSporeSpawnProgram(
        MutableCartridgeReadGuard bus,
        ushort initialPointer,
        int frames,
        EnemyTileArtworkCatalog? artwork = null,
        Action<RoomEnemySystem, RoomEnemySlot>? afterFrame = null)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        enemies.TileArtwork = artwork ?? InstalledFixtures.EnemyTiles;
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.EnemyDefinitionPointer = EnemyDefinitionId.SporeSpawn;
        body.Definition = default(RoomEnemyDefinition) with { Bank = 0xa5 };
        body.Position = body.Position.WithWhole(new(128, 624));
        body.Health = 960;
        body.CurrentInstruction = initialPointer;
        body.InstructionTimer = NativeCountdown.Start(1);

        var state = new SporeSpawnEnemyState(body)
        {
            Function = SporeSpawnFunction.Idle,
            MovementCenter = body.Position.Whole,
            MaximumXRadius = 48,
            AngleDelta = 1,
        };
        enemies.BindFixtureBus(bus);
        enemies.Presentation._cgram = new SnesCgram();
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0x1234);
        enemies.DeathsAndDrops._samusForEnemyDrops = new SamusState { Health = 99, MaxHealth = 99 };
        enemies.SporeSpawnBehavior._sporeSpawn = state;

        object?[] arguments = [body, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        for (int frame = 0; frame < frames; frame++)
        {
            EnemyInstructionRunner.Process(enemies, arguments);
            afterFrame?.Invoke(enemies, body);
        }

        return (enemies, body);
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for Spore Spawn program words")]
    internal static ushort ReadSporeSpawnProgramWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(0xa50000 | address);

    internal static MutableCartridgeReadGuard NewSporeSpawnInstructionReadGuard(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(SporeSpawnInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Spore Spawn mechanics byte ${address:X6}.")
            .ForbidWords(0xa50000, SporeSpawnInstructionProgramDefinitionsTooling.PresentationWordCount,
                SporeSpawnInstructionProgramDefinitionsTooling.PresentationWordAddress,
                address => $"Installed Spore Spawn reread visual selector ${address:X6}.");
        return guard;
    }

    [VerificationCommand("--spore-spawn-instruction-mechanics")]
    internal static int SporeSpawnInstructionMechanicsCommand()
    {
        Suite(nameof(VerifySporeSpawnInstructionProgramDefinitions), () => VerifySporeSpawnInstructionProgramDefinitions());
        return 0;
    }
}
