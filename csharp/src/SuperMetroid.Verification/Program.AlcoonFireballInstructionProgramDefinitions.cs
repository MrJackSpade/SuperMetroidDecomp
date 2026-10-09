using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction;

internal static partial class Program
{
    /// <summary>Verifies Alcoon fireball definitions and production behavior against the supported retail ROM.</summary>
    private static void VerifyAlcoonFireballInstructionProgramDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)),
            "Alcoon-fireball oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyAlcoonFireballInstructionProgramDefinitions), () => VerifyAlcoonFireballInstructionProgramDefinitions(rom));
    }

    /// <summary>Checks the four compiled visual operands against native pointers and rejects neighboring nonvisual words.</summary>
    /// <param name="rom">Retail address space containing the native instruction and spritemap words.</param>
    private static void VerifyAlcoonFireballVisualSelectors(SuperMetroidAddressSpace rom)
    {
        ushort[] operands = [0x9ea0, 0x9ea4, 0x9ea8, 0x9eac];
        foreach (ushort operand in operands)
        {
            ushort native = ReadAlcoonFireballInstructionWord(rom, operand);
            AssertEqual(native, EnemyProjectileSpritemapDefinitions.AlcoonFireballFrameAt(operand),
                "Alcoon fireball native visual pointer");
            AssertEqual((byte)1, rom.ReadByte(0x8d0000 | native), "fireball native single-sprite map count");
            AssertEqual((byte)0, rom.ReadByte(0x8d0000 | (native + 1)), "fireball map count high byte");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, operand, out ushort shared),
                "shared fireball selector exists");
            AssertEqual(native, shared, "shared fireball selector matches native pointer");
            AssertTrue(CompiledEnemyVisualSelectors.IsCalculatedSelector(0x860000 | operand),
                "fireball selector excluded from literal regeneration");
        }
        var expected = operands.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(expected.Contains((ushort)address), AlcoonFireballInstructionProgramDefinitions.IsPresentationWord((ushort)address),
                "Alcoon fireball full visual operand domain");
        for (int address = 0x9e9c; address <= 0x9eb4; address++)
            if (!expected.Contains((ushort)address))
            {
                AssertThrows<InvalidDataException>(() => EnemyProjectileSpritemapDefinitions.AlcoonFireballFrameAt((ushort)address),
                    "fireball visual resolver rejects program controls and adjacent data");
                AssertTrue(!CompiledEnemyVisualSelectors.TryGet(0x86, (ushort)address, out ushort missing),
                    "shared fireball catalog rejects holes");
                AssertEqual((ushort)0, missing, "missing fireball selector clears output");
            }
        foreach (ushort address in new ushort[] { 0, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => EnemyProjectileSpritemapDefinitions.AlcoonFireballFrameAt(address),
                "fireball resolver rejects distant invalid inputs");
    }
    /// <summary>Checks the six compiled control words and verifies exact bank-byte ownership and invalid-word rejection.</summary>
    /// <param name="rom">Retail address space used to read the native mechanics words.</param>
    private static void VerifyAlcoonFireballMechanicsMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0x9e9e, 0x9ea2, 0x9ea6, 0x9eaa, 0x9eae, 0x9eb0];
        AssertEqual(addresses.Length, AlcoonFireballInstructionProgramDefinitionsTooling.MechanicsWordCount,
            "Alcoon-fireball mechanics count");
        var bytes = new HashSet<int>();
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort address = addresses[index];
            ushort expected = ReadAlcoonFireballInstructionWord(rom, address);
            var definition = AlcoonFireballInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(address, definition.Address, "Alcoon-fireball native word position");
            AssertEqual(expected, definition.Value, "Alcoon-fireball native enumerated word");
            AssertEqual(expected, AlcoonFireballInstructionProgramDefinitions.ReadMechanicsWord(address),
                "Alcoon-fireball native direct word");
            bytes.Add(address);
            bytes.Add(address + 1);
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool expected = bytes.Contains(address);
            AssertEqual(expected, AlcoonFireballInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x860000 | address),
                "Alcoon-fireball full bank ownership");
            AssertEqual(expected, AlcoonFireballInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x1860000 | address),
                "Alcoon-fireball preserves high-bit masking");
            AssertTrue(!AlcoonFireballInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x850000 | address),
                "Alcoon-fireball rejects other bank");
        }
        var words = addresses.ToHashSet();
        for (int address = 0x9e9c; address <= 0x9eb4; address++)
            if (!words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(
                    () => AlcoonFireballInstructionProgramDefinitions.ReadMechanicsWord((ushort)address),
                    "Alcoon-fireball rejects odd words, visuals and adjacent code");
        foreach (ushort address in new ushort[] { 0, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => AlcoonFireballInstructionProgramDefinitions.ReadMechanicsWord(address),
                "Alcoon-fireball rejects distant invalid word");
        foreach (int index in new[] { int.MinValue, -1, 6, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => AlcoonFireballInstructionProgramDefinitionsTooling.MechanicsWord(index),
                "Alcoon-fireball mechanics ordinal bounds");
    }

    /// <summary>Checks the ordered four-word presentation address list and its ordinal bounds.</summary>
    private static void VerifyAlcoonFireballPresentationAddresses()
    {
        ushort[] expected = [0x9ea0, 0x9ea4, 0x9ea8, 0x9eac];
        AssertEqual(expected.Length, AlcoonFireballInstructionProgramDefinitions.PresentationWordCount,
            "Alcoon-fireball presentation count");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], AlcoonFireballInstructionProgramDefinitions.PresentationWordAddress(index),
                "Alcoon-fireball native presentation position");
        foreach (int index in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => AlcoonFireballInstructionProgramDefinitions.PresentationWordAddress(index),
                "Alcoon-fireball presentation ordinal bounds");
    }

    /// <summary>Exercises real spawned fireballs in both directions, including animation looping, shared deletion, and guarded ROM access.</summary>
    /// <param name="rom">Retail address space used to verify instruction words and presentation selectors.</param>
    private static void VerifyAlcoonFireballInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        Suite(nameof(VerifyAlcoonFireballMechanicsMapping), () => VerifyAlcoonFireballMechanicsMapping(rom));
        Suite(nameof(VerifyAlcoonFireballPresentationAddresses), () => VerifyAlcoonFireballPresentationAddresses());
        Suite(nameof(VerifyAlcoonFireballVisualSelectors), () => VerifyAlcoonFireballVisualSelectors(rom));

        var guard = new AlcoonFireballInstructionReadGuard(rom);
        var observedVisualOperands = new HashSet<ushort>();
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

        AssertTrue(observedVisualOperands.SetEquals(new ushort[] { 0x9ea0, 0x9ea4, 0x9ea8, 0x9eac }),
            "Alcoon-fireball production selects all four installed visual operands");

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids all Alcoon-fireball control/visual bytes and shared-delete mechanics");

        _ = ProbeAlcoonFireballInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeAlcoonFireballInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Alcoon-fireball allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Alcoon-fireball mechanics lookups allocate no storage");

        Console.WriteLine(
            "Alcoon-fireball instruction mechanics: six compiled words, all three real " +
            "fire commands in both facings, six complete loops, shared shot deletion, " +
            "and four installed visual operands pass with original program bytes forbidden.");

        void RunForcedTicks(RoomEnemyProjectileSlot projectile, int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, new SamusState(), (ushort)0, (ushort)0]);
                if (projectile.IsActive)
                {
                    ushort expectedOperand = (ushort)(0x9ea0 + 4 * (tick % 4));
                    AssertEqual(expectedOperand, projectile.PresentationOperandAddress,
                        "Alcoon-fireball frame order including loop restart");
                    AssertEqual((ushort)3, projectile.InstructionTimer,
                        "Alcoon-fireball selected frame duration");
                    observedVisualOperands.Add(projectile.PresentationOperandAddress);
                }
            }
        }
    }

    /// <summary>Repeats compiled mechanics lookups so the caller can measure warmed allocation behavior.</summary>
    /// <returns>A checksum that keeps the lookup results observable.</returns>
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

    /// <summary>Reads a little-endian projectile instruction word from its native bank.</summary>
    /// <param name="source">Address space supplying the instruction bytes.</param>
    /// <param name="address">Bank-relative address of the word's low byte.</param>
    /// <returns>The two adjacent bytes combined into a 16-bit instruction word.</returns>
    private static ushort ReadAlcoonFireballInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    /// <summary>Rejects production reads of the migrated Alcoon fireball words and shared projectile mechanics.</summary>
    /// <param name="source">Underlying address space used for permitted cartridge reads and writes.</param>
    private sealed class AlcoonFireballInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Counts attempts to read instruction bytes that production should resolve from compiled definitions.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes import-source reads through the guard's migrated-word checks.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The underlying byte when the guarded access is allowed.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects migrated fireball and shared-delete mechanics reads, then forwards other addresses.</summary>
        /// <param name="address">Absolute cartridge address to read.</param>
        /// <returns>The underlying source byte for a permitted address.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to migrated fireball or shared-delete mechanics.</exception>
        public byte ReadByte(int address)
        {
            if (((address & 0xff0000) == 0x860000 && (ushort)address is >= 0x9e9e and <= 0x9eb1) ||
                CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read migrated Alcoon-fireball program byte ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        /// <summary>Forwards a cartridge write to the wrapped address space.</summary>
        /// <param name="address">Absolute cartridge address to update.</param>
        /// <param name="value">Byte written to that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
