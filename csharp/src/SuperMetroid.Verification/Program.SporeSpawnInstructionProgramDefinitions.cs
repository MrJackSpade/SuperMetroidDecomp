using System.Reflection;
using SuperMetroid.Core.Assets;
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

        var guarded = new SporeSpawnInstructionReadGuard(rom) { DenyPresentationReads = true };
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
        AssertEqual(0, guarded.ObservedPresentationWords.Count,
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

    /// <summary>Creates an initialized Spore Spawn fixture and advances its production instruction processor.</summary>
    /// <param name="bus">Guarded address space supplied to the enemy system.</param>
    /// <param name="initialPointer">Instruction address from which the body starts running.</param>
    /// <param name="frames">Number of instruction-processing calls to perform.</param>
    /// <param name="artwork">Optional tile-art catalog; the installed enemy catalog is used when omitted.</param>
    /// <param name="afterFrame">Optional callback invoked after each processed frame.</param>
    /// <returns>The configured enemy system and its Spore Spawn body slot.</returns>
    private static (RoomEnemySystem System, RoomEnemySlot Body) RunSporeSpawnProgram(
        SporeSpawnInstructionReadGuard bus,
        ushort initialPointer,
        int frames,
        EnemyTileArtworkCatalog? artwork = null,
        Action<RoomEnemySystem, RoomEnemySlot>? afterFrame = null)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        enemies.TileArtwork = artwork ?? RepositoryInstallation.EnemyTiles;
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
        {
            process.Invoke(enemies, arguments);
            afterFrame?.Invoke(enemies, body);
        }

        return (enemies, body);
    }

    /// <summary>Reads a little-endian word from bank $A5 in the retail address space.</summary>
    /// <param name="source">Retail address space containing the reference bytes.</param>
    /// <param name="address">Bank-local offset of the word's low byte.</param>
    /// <returns>The adjacent bytes combined as a 16-bit value.</returns>
    private static ushort ReadSporeSpawnProgramWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa50000 | address) |
            source.ReadByte(0xa50000 | unchecked((ushort)(address + 1))) << 8));

    /// <summary>Guards runtime access to compiled Spore Spawn mechanics and optionally to its compiled visual selectors.</summary>
    /// <param name="source">Address space that receives reads and writes allowed by the guard.</param>
    private sealed class SporeSpawnInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        /// <summary>Distinct compiled presentation words read when presentation reads are permitted.</summary>
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];

        /// <summary>Number of blocked mechanics or presentation byte reads attempted.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Whether reads from compiled presentation words are rejected instead of recorded and forwarded.</summary>
        internal bool DenyPresentationReads { get; init; }

        /// <summary>Checks the requested address before forwarding a general memory read.</summary>
        /// <param name="address">SNES address requested by the caller.</param>
        /// <returns>The byte supplied by the wrapped address space when permitted.</returns>
        /// <exception cref="InvalidOperationException">The address is a compiled mechanics byte or a denied presentation byte.</exception>
        public byte ReadByte(int address)
        {
            CheckRead(address);
            return source.ReadByte(address);
        }

        /// <summary>Checks a cartridge read against the guarded tables before delegating to the import source.</summary>
        /// <param name="address">SNES cartridge address requested by the importer.</param>
        /// <returns>The cartridge byte when the address passes the guard.</returns>
        /// <exception cref="InvalidOperationException">The address is a compiled mechanics byte or a denied presentation byte.</exception>
        public byte ReadCartridgeByte(int address)
        {
            CheckRead(address);
            return (source as IImportCartridgeSource ?? throw new InvalidOperationException(
                "Spore Spawn instruction guard requires cartridge data."))
                .ReadCartridgeByte(address);
        }

        /// <summary>Forwards a WRAM read to the wrapped mutable address space.</summary>
        /// <param name="address">WRAM address to read.</param>
        /// <returns>The byte stored at that address.</returns>
        public byte ReadWorkRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Spore Spawn instruction guard requires WRAM."))
            .ReadWorkRamByte(address);

        /// <summary>Forwards a save-RAM read to the wrapped mutable address space.</summary>
        /// <param name="address">Save-RAM address to read.</param>
        /// <returns>The byte stored at that address.</returns>
        public byte ReadSaveRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Spore Spawn instruction guard requires SRAM."))
            .ReadSaveRamByte(address);

        /// <summary>Forwards a byte write unchanged to the wrapped address space.</summary>
        /// <param name="address">SNES address to write.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        /// <summary>Counts and rejects compiled mechanics reads and handles presentation reads according to the configured policy.</summary>
        /// <param name="address">SNES address to classify before an address-space or cartridge read.</param>
        /// <exception cref="InvalidOperationException">The address is a compiled mechanics byte or a denied presentation byte.</exception>
        private void CheckRead(int address)
        {
            if (SporeSpawnInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Spore Spawn mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa50000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < SporeSpawnInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        SporeSpawnInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        if (DenyPresentationReads)
                        {
                            ForbiddenReadAttempts++;
                            throw new InvalidOperationException(
                                $"Installed Spore Spawn reread visual selector ${address:X6}.");
                        }
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }

        }
    }
}
