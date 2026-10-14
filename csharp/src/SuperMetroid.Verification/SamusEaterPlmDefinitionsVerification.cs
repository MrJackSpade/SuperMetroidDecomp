using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;

/// <summary>Checks the floor and ceiling Samus Eater PLM headers, instruction programs, and live trigger spawns.</summary>
internal static class SamusEaterPlmDefinitionsVerification
{
    /// <summary>Compares native header and program data and checks aligned floor and ceiling plant activation.</summary>
    internal static void VerifySamusEaterPlmDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifySamusEaterHeaderInstructionMapping), () => VerifySamusEaterHeaderInstructionMapping(rom));
        Suite(nameof(VerifySamusEaterMountingMapping), () => VerifySamusEaterMountingMapping(rom));
        SamusEaterPlmDefinition[] definitions =
            [SamusEaterPlmDefinitions.Resolve(PlmHeaderId.InsideReactionBrinstarFloorPlant), SamusEaterPlmDefinitions.Resolve(PlmHeaderId.InsideReactionBrinstarCeilingPlant)];
        AssertEqual(2, definitions.Length, "Samus Eater PLM definition count");
        AssertEqual(definitions[0].InstructionListPointer,
            SamusEaterPlmProgramDefinitions.FloorStart,
            "floor plant header enters its compiled instruction program");
        AssertEqual(definitions[1].InstructionListPointer,
            SamusEaterPlmProgramDefinitions.CeilingStart,
            "ceiling plant header enters its compiled instruction program");
        AssertEqual(0xad38, SamusEaterPlmProgramDefinitions.EndExclusive,
            "plant program ends before the Wrecked Ship treadmill");
        Suite(nameof(VerifySamusEaterProgramControls), () => VerifySamusEaterProgramControls(rom));
        Suite(nameof(VerifySamusEaterProgramDraws), () => VerifySamusEaterProgramDraws(rom));
        Suite(nameof(VerifySamusEaterProgramSound), () => VerifySamusEaterProgramSound(rom));

        foreach (PlmHeaderId headerPointer in new ushort[] { 0xb6cb, 0xb6cf })
        {
            SamusEaterPlmDefinition definition = SamusEaterPlmDefinitions.Resolve(headerPointer);
            const int width = 4;
            const int height = 4;
            int blockCount = width * height;
            var levelWords = new ushort[blockCount];
            levelWords[5] = 0xb123;
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(levelWords, new byte[blockCount], new ushort[blockCount]), new byte[8]);
            var plms = new RoomPlmSystem();
            var samus = new SamusState();
            samus.Position = samus.Position.WithWhole(new(24, definition.Ceiling ? (ushort)16 : (ushort)0));
            samus.Kinematics.Radius = new(8, 16);

            RoomCollisionBlock block = level.GetCollisionBlock(new(1, 1));
            plms.SamusEater.TrySpawnSamusEater(level, block, headerPointer, samus);

            RoomPlmSlotSnapshot slot = plms.PopulationSlots.Single();
            AssertEqual(39, slot.NativeSlotIndex, "Samus Eater uses highest free native slot");
            AssertEqual(headerPointer, slot.HeaderPointer, "Samus Eater header identity");
            AssertEqual(5, slot.BlockIndex, "Samus Eater trigger block");
            AssertEqual(
                definition.InstructionListPointer,
                slot.InstructionPointer,
                "Samus Eater compiled initial instruction list");
            AssertEqual(1, slot.InstructionTimer, "Samus Eater starts on timer one");
            AssertEqual(
                0x8123,
                level.GetCollisionBlock(new(1, 1)).LevelWord,
                "Samus Eater deactivates its trigger");
        }

        AssertThrows<InvalidDataException>(
            () => SamusEaterPlmDefinitions.Resolve(PlmHeaderId.MapStation),
            "map-station header cannot enter Samus Eater domain");
        Console.WriteLine(
            "Samus Eater PLMs: both header/list identities, 128 compiled control bytes, and real aligned spawns match cartridge.");
    }

    /// <summary>Checks both plant headers' native initial instruction pointers.</summary>
    private static void VerifySamusEaterHeaderInstructionMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySamusEaterHeaderField), () => VerifySamusEaterHeaderField(rom, false));

    /// <summary>Checks floor versus ceiling mounting against native setup and held-Y direction.</summary>
    private static void VerifySamusEaterMountingMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySamusEaterHeaderField), () => VerifySamusEaterHeaderField(rom, true));

    /// <summary>Checks header selection across all 16-bit identities and one native setup field.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus eater header field")]
    private static void VerifySamusEaterHeaderField(SuperMetroidAddressSpace rom, bool mounting)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort header = (ushort)raw;
            if (header is not ((ushort)PlmHeaderId.InsideReactionBrinstarFloorPlant or (ushort)PlmHeaderId.InsideReactionBrinstarCeilingPlant))
            {
                AssertThrows<InvalidDataException>(() => SamusEaterPlmDefinitions.Resolve((PlmHeaderId)header),
                    "plant header selector rejects every unsupported identity");
                continue;
            }
            var actual = SamusEaterPlmDefinitions.Resolve((PlmHeaderId)header);
            AssertEqual(header == (ushort)PlmHeaderId.InsideReactionBrinstarFloorPlant ? SamusEaterPlmDefinitions.Floor : SamusEaterPlmDefinitions.Ceiling,
                actual, "plant selected header identity");
            if (mounting)
            {
                ushort setup = ReadSamusEaterPlmWord(rom, 0x840000 | header);
                AssertTrue(setup is 0xb0dc or 0xb113, "plant native setup belongs to floor/ceiling pair");
                AssertEqual(setup == 0xb113, actual.Ceiling, "plant mounting follows native setup");
                // The native ceiling subtracts radius and increments held Y; floor adds
                // radius, decrements the edge and decrements held Y.
                AssertEqual((byte)(actual.Ceiling ? 0xed : 0x6d),
                    rom.ReadCartridgeByte(0x840000 | (setup + 4)), "plant native edge add/subtract opcode");
                AssertEqual((byte)(actual.Ceiling ? 0x1a : 0x3a),
                    rom.ReadCartridgeByte(actual.Ceiling ? 0x84b13f : 0x84b10c), "plant native held-Y direction");
            }
            else
                AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | (header + 2)),
                    actual.InstructionListPointer, "plant native initial instruction selection");
        }
    }

    /// <summary>Checks byte and overlapping word ownership across both plant programs against the cartridge.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus eater program controls")]
    private static void VerifySamusEaterProgramControls(SuperMetroidAddressSpace rom)
    {
        int[] drawWords = [0xacc1,0xacc5,0xacc9,0xacd2,0xacd6,0xacda,0xacde,0xace4,0xacf0,0xacf4,
            0xad01,0xad05,0xad09,0xad12,0xad16,0xad1a,0xad1e,0xad24,0xad30,0xad34];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool byteOwned = raw is >= 0xacb8 and < 0xad38;
            bool wordOwned = raw is >= 0xacb8 and < 0xad37;
            AssertEqual(byteOwned, SamusEaterPlmProgramDefinitions.TryReadMechanicsByte(address, out byte actualByte), "plant complete byte ownership");
            AssertEqual(wordOwned, SamusEaterPlmProgramDefinitions.TryReadMechanicsWord(address, out ushort actualWord), "plant complete word ownership");
            if (!byteOwned) AssertEqual((byte)0, actualByte, "plant missing byte zero");
            else if (raw != 0xaccd && raw != 0xad0d && !drawWords.Any(pointer => raw == pointer || raw == pointer + 1))
                AssertEqual(rom.ReadCartridgeByte(0x840000 | raw), actualByte, "plant native control byte and timer operand");
            if (!wordOwned) AssertEqual((ushort)0, actualWord, "plant missing word zero");
            else
            {
                // Every byte-aligned view, including ACF7 crossing between the two lists.
                AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | raw), actualWord, "plant native packed word view");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "plant shared reader ownership");
                AssertEqual(actualWord, shared, "plant shared reader preserves word view");
            }
        }
    }

    /// <summary>Checks twenty compiled draw selector words and their individual bytes.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus eater program draws")]
    private static void VerifySamusEaterProgramDraws(SuperMetroidAddressSpace rom)
    {
        int[] pointers = [0xacc1,0xacc5,0xacc9,0xacd2,0xacd6,0xacda,0xacde,0xace4,0xacf0,0xacf4,
            0xad01,0xad05,0xad09,0xad12,0xad16,0xad1a,0xad1e,0xad24,0xad30,0xad34];
        foreach (int pointer in pointers)
        {
            AssertTrue(SamusEaterPlmProgramDefinitions.TryReadMechanicsWord((ushort)pointer, out ushort actual), "plant draw operand word");
            AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | pointer), actual, "plant original draw selector");
            for (int half = 0; half < 2; half++)
            {
                AssertTrue(SamusEaterPlmProgramDefinitions.TryReadMechanicsByte((ushort)(pointer + half), out byte value), "plant draw operand byte");
                AssertEqual(rom.ReadCartridgeByte(0x840000 | (pointer + half)), value, "plant original draw operand byte");
            }
        }
    }

    /// <summary>Checks the packed sound operands in both plant instruction lists.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus eater program sound")]
    private static void VerifySamusEaterProgramSound(SuperMetroidAddressSpace rom)
    {
        foreach (ushort pointer in new ushort[] {0xaccd,0xad0d})
        {
            AssertTrue(SamusEaterPlmProgramDefinitions.TryReadMechanicsByte(pointer, out byte value), "plant packed sound byte");
            AssertEqual(rom.ReadCartridgeByte(0x840000 | pointer), value, "plant original sound selection");
        }
    }

    /// <summary>Reads a native bank-$84 PLM word for independent header and program comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus eater PLM words")]
    internal static ushort ReadSamusEaterPlmWord(SuperMetroidAddressSpace bus, int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
}
