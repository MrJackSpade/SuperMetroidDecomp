using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyAlcoonFireballInstructionProgramDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)),
            "Alcoon-fireball oracle is NTSC J/U v1.0");
        Suite(nameof(VerifyAlcoonFireballInstructionProgramDefinitions), () => VerifyAlcoonFireballInstructionProgramDefinitions(rom));
    }

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
    private static void VerifyAlcoonFireballMechanicsMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0x9e9e, 0x9ea2, 0x9ea6, 0x9eaa, 0x9eae, 0x9eb0];
        AssertEqual(addresses.Length, AlcoonFireballInstructionProgramDefinitions.MechanicsWordCount,
            "Alcoon-fireball mechanics count");
        var bytes = new HashSet<int>();
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort address = addresses[index];
            ushort expected = ReadAlcoonFireballInstructionWord(rom, address);
            var definition = AlcoonFireballInstructionProgramDefinitions.MechanicsWord(index);
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
            AssertEqual(expected, AlcoonFireballInstructionProgramDefinitions.IsCompiledMechanicsByte(0x860000 | address),
                "Alcoon-fireball full bank ownership");
            AssertEqual(expected, AlcoonFireballInstructionProgramDefinitions.IsCompiledMechanicsByte(0x1860000 | address),
                "Alcoon-fireball preserves high-bit masking");
            AssertTrue(!AlcoonFireballInstructionProgramDefinitions.IsCompiledMechanicsByte(0x850000 | address),
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
            AssertThrows<IndexOutOfRangeException>(() => AlcoonFireballInstructionProgramDefinitions.MechanicsWord(index),
                "Alcoon-fireball mechanics ordinal bounds");
    }

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
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (((address & 0xff0000) == 0x860000 && (ushort)address is >= 0x9e9e and <= 0x9eb1) ||
                CommonEnemyProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read migrated Alcoon-fireball program byte ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
