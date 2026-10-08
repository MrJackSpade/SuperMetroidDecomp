using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyFuneNamiheFireballInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyFuneNamiheFireballInstructionProgramDefinitions), () => VerifyFuneNamiheFireballInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    private static void VerifyFuneNamiheFireballInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Suite(nameof(VerifyFuneNamiheFireballMechanicsMapping), () => VerifyFuneNamiheFireballMechanicsMapping(rom));
        Suite(nameof(VerifyFuneNamiheFireballPresentationAddresses), () => VerifyFuneNamiheFireballPresentationAddresses());
        Suite(nameof(VerifyFuneNamiheFireballVisualSelectors), () => VerifyFuneNamiheFireballVisualSelectors(rom));

        var guard = new FuneNamiheFireballInstructionReadGuard(rom);
        var observedVisuals = new HashSet<ushort>();
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        var spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnFuneNamiheFireball",
            flags)!.CreateDelegate<Action<RoomEnemySlot, bool, RoomEnemyProjectileKind>>(
                enemies);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions",
            flags)!;
        RoomEnemySlot source = enemies.Slots[0];
        source.XPosition = 0x0120;
        source.YPosition = 0x0080;
        source.Parameter2 = 0;

        foreach (RoomEnemyProjectileKind kind in new[]
                 {
                     RoomEnemyProjectileKind.FuneFireball,
                     RoomEnemyProjectileKind.NamiheFireball,
                 })
        {
            source.Parameter1 = kind == RoomEnemyProjectileKind.FuneFireball
                ? (ushort)0
                : (ushort)1;
            foreach (bool movingRight in new[] { false, true })
            {
                foreach (RoomEnemyProjectileSlot candidate in enemies.EnemyProjectiles)
                    candidate.Clear();

                spawn(source, movingRight, kind);
                RoomEnemyProjectileSlot projectile = enemies.EnemyProjectiles.Single(
                    candidate => candidate.Kind == kind);
                ushort program = movingRight
                    ? FuneNamiheFireballInstructionProgramDefinitions.Right
                    : FuneNamiheFireballInstructionProgramDefinitions.Left;
                AssertEqual(program, projectile.InstructionPointer,
                    $"{kind} {(movingRight ? "right" : "left")} selects named program");

                object?[] arguments = [projectile, null, (ushort)0, (ushort)0];
                for (int frame = 0; frame < 4; frame++)
                {
                    projectile.InstructionTimer = 1;
                    process.Invoke(enemies, arguments);
                    AssertEqual((ushort)(program + 2 + 4 * (frame % 3)), projectile.PresentationOperandAddress,
                        "Fune/Namihe fireball frame order including loop restart");
                    AssertEqual((ushort)5, projectile.InstructionTimer,
                        "Fune/Namihe fireball frame duration");
                    observedVisuals.Add(projectile.PresentationOperandAddress);
                }
                AssertEqual(unchecked((ushort)(program + 4)), projectile.InstructionPointer,
                    $"{kind} {(movingRight ? "right" : "left")} loops to first frame");
            }
        }

        AssertTrue(observedVisuals.SetEquals(new ushort[] { 0xde98, 0xde9c, 0xdea0, 0xdea8, 0xdeac, 0xdeb0 }),
            "production selects all six installed Fune/Namihe fireball visual operands");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Fune/Namihe fireball mechanics byte");
        AssertThrows<InvalidDataException>(
            () => FuneNamiheFireballInstructionProgramDefinitions.ReadMechanicsWord(
                0xde98),
            "Fune/Namihe fireball spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => FuneNamiheFireballInstructionProgramDefinitions.ReadMechanicsWord(
                0xdeb6),
            "adjacent Fune/Namihe velocity table is rejected as mechanics");

        _ = ProbeFuneNamiheFireballInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeFuneNamiheFireballInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Fune/Namihe allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Fune/Namihe fireball lookups allocate no per-frame storage");

        Console.WriteLine(
            "Fune/Namihe fireball instruction mechanics: ten compiled words, both " +
            "directional loops for both species, and six installed visual operands pass " +
            "with mechanics bytes forbidden.");
    }

    private static void VerifyFuneNamiheFireballVisualSelectors(SuperMetroidAddressSpace rom)
    {
        ushort[] operands = [0xde98, 0xde9c, 0xdea0, 0xdea8, 0xdeac, 0xdeb0];
        foreach (ushort operand in operands)
        {
            ushort native = ReadFuneNamiheFireballInstructionWord(rom, operand);
            AssertEqual(native, EnemyProjectileSpritemapDefinitions.FuneNamiheFireballFrameAt(operand),
                "Fune/Namihe fireball native visual pointer");
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
            AssertEqual(expected.Contains((ushort)address), FuneNamiheFireballInstructionProgramDefinitions.IsPresentationWord((ushort)address),
                "Fune/Namihe fireball full visual operand domain");
        for (int address = 0xde94; address <= 0xdeb8; address++)
            if (!expected.Contains((ushort)address))
            {
                AssertThrows<InvalidDataException>(() => EnemyProjectileSpritemapDefinitions.FuneNamiheFireballFrameAt((ushort)address),
                    "fireball visual resolver rejects program controls and adjacent data");
                AssertTrue(!CompiledEnemyVisualSelectors.TryGet(0x86, (ushort)address, out ushort missing),
                    "shared fireball catalog rejects holes");
                AssertEqual((ushort)0, missing, "missing fireball selector clears output");
            }
        foreach (ushort address in new ushort[] { 0, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => EnemyProjectileSpritemapDefinitions.FuneNamiheFireballFrameAt(address),
                "fireball resolver rejects distant invalid inputs");
    }
    private static void VerifyFuneNamiheFireballMechanicsMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0xde96, 0xde9a, 0xde9e, 0xdea2, 0xdea4, 0xdea6, 0xdeaa, 0xdeae, 0xdeb2, 0xdeb4];
        AssertEqual(addresses.Length, FuneNamiheFireballInstructionProgramDefinitionsTooling.MechanicsWordCount,
            "FuneNamihe-fireball mechanics count");
        var bytes = new HashSet<int>();
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort address = addresses[index];
            ushort expected = ReadFuneNamiheFireballInstructionWord(rom, address);
            var definition = FuneNamiheFireballInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(address, definition.Address, "FuneNamihe-fireball native word position");
            AssertEqual(expected, definition.Value, "FuneNamihe-fireball native enumerated word");
            AssertEqual(expected, FuneNamiheFireballInstructionProgramDefinitions.ReadMechanicsWord(address),
                "FuneNamihe-fireball native direct word");
            bytes.Add(address);
            bytes.Add(address + 1);
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool expected = bytes.Contains(address);
            AssertEqual(expected, FuneNamiheFireballInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x860000 | address),
                "FuneNamihe-fireball full bank ownership");
            AssertEqual(expected, FuneNamiheFireballInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x1860000 | address),
                "FuneNamihe-fireball preserves high-bit masking");
            AssertTrue(!FuneNamiheFireballInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x850000 | address),
                "FuneNamihe-fireball rejects other bank");
        }
        var words = addresses.ToHashSet();
        for (int address = 0xde94; address <= 0xdeb8; address++)
            if (!words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(
                    () => FuneNamiheFireballInstructionProgramDefinitions.ReadMechanicsWord((ushort)address),
                    "FuneNamihe-fireball rejects odd words, visuals and adjacent code");
        foreach (ushort address in new ushort[] { 0, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => FuneNamiheFireballInstructionProgramDefinitions.ReadMechanicsWord(address),
                "FuneNamihe-fireball rejects distant invalid word");
        foreach (int index in new[] { int.MinValue, -1, 10, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => FuneNamiheFireballInstructionProgramDefinitionsTooling.MechanicsWord(index),
                "FuneNamihe-fireball mechanics ordinal bounds");
    }

    private static void VerifyFuneNamiheFireballPresentationAddresses()
    {
        ushort[] expected = [0xde98, 0xde9c, 0xdea0, 0xdea8, 0xdeac, 0xdeb0];
        AssertEqual(expected.Length, FuneNamiheFireballInstructionProgramDefinitions.PresentationWordCount,
            "FuneNamihe-fireball presentation count");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], FuneNamiheFireballInstructionProgramDefinitions.PresentationWordAddress(index),
                "FuneNamihe-fireball native presentation position");
        foreach (int index in new[] { int.MinValue, -1, 6, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => FuneNamiheFireballInstructionProgramDefinitions.PresentationWordAddress(index),
                "FuneNamihe-fireball presentation ordinal bounds");
    }

    private static int ProbeFuneNamiheFireballInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += FuneNamiheFireballInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? FuneNamiheFireballInstructionProgramDefinitions.Left
                    : FuneNamiheFireballInstructionProgramDefinitions.Right);
        }
        return checksum;
    }

    private static ushort ReadFuneNamiheFireballInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(EnemyProjectileCodePointers.BankBase | address) |
            source.ReadByte(
                EnemyProjectileCodePointers.BankBase |
                unchecked((ushort)(address + 1))) << 8));

    private sealed class FuneNamiheFireballInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if ((address & 0xff0000) == 0x860000 && (uint)(unchecked((ushort)address) - 0xde96) < 32)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Fune/Namihe mechanics byte ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
