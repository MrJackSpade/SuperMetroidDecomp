using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Checks projectile-trail placements against the cartridge's selector, direction and placement tables.</summary>
internal static class ProjectileTrailCoordinatesVerification
{
    /// <summary>$9B:A4B3, $9B:A4CB and $9B:A4E3: the uncharged, charged and special beam attack trail selector tables.</summary>
    private static readonly (ProjectileTrailFamilyGroup Group, int Table)[] SelectorTables =
    [
        (ProjectileTrailFamilyGroup.Uncharged, 0x9ba4b3),
        (ProjectileTrailFamilyGroup.Charged, 0x9ba4cb),
        (ProjectileTrailFamilyGroup.SpecialBeamAttack, 0x9ba4e3),
    ];

    /// <summary>Bytes in one placement record: the left and right sparkles' signed X and Y offsets.</summary>
    private const int RecordByteCount = 4;

    /// <summary>The first bank-$9B offset the cartridge serves; below it lies direct-page scratch and hardware.</summary>
    private const int CartridgeHalfStart = 0x8000;

    /// <summary>
    /// Compares every list's placements, every selector row and direction-table index, and every installed read past a
    /// list's placements with the cartridge, then spawns real trails against the native CPU reads.
    /// </summary>
    internal static void VerifyProjectileTrailCoordinates(ISnesAddressSpace bus)
    {
        int records = 0;
        foreach ((ProjectileTrailPlacementList list, ProjectileTrailPlacement[] placements) in ProjectileTrailPlacementDefinitions.Records)
            for (int frame = 0; frame < placements.Length; frame++, records++)
                AssertEqual(NativePlacement(bus, ProjectileTrailPlacementSources.Addresses[list] + frame * RecordByteCount), placements[frame],
                    $"{list} placement {frame} matches the cartridge");
        AssertEqual(1660, records, "The eighty lists read 1660 placements up to the end of their runs of records");

        Dictionary<ushort, ProjectileTrailPlacementList> lists = ProjectileTrailPlacementSources.Addresses.ToDictionary(pair => pair.Value, pair => pair.Key);
        int selections = 0;
        foreach ((ProjectileTrailFamilyGroup group, int table) in SelectorTables)
        foreach (SamusBeamCombination beams in Enum.GetValues<SamusBeamCombination>())
        for (int direction = 0; direction < 16; direction++)
        {
            AssertEqual(NativeList(bus, lists, table, beams, direction), ProjectileTrailPlacements.List(group, beams, direction),
                $"{group} {beams} direction {direction} selects the list the cartridge's pointers name");
            selections++;
        }
        AssertEqual(768, selections, "Three selector tables of sixteen rows by sixteen direction-table indexes");

        int overruns = 0;
        foreach (ProjectileTrailPlacementList list in Enum.GetValues<ProjectileTrailPlacementList>())
        foreach (int frame in ProjectileTrailPlacements.OverrunFrames(list, InstalledFixtures.SamusOverreads.ProjectileLists))
        {
            int record = (ProjectileTrailPlacementSources.Addresses[list] + frame * RecordByteCount) & ushort.MaxValue;
            ProjectileTrailPlacement? expected = RecordIsCartridge(record) ? NativePlacement(bus, record) : null;
            AssertEqual(expected, ProjectileTrailPlacements.Start(list, frame, InstalledFixtures.SamusOverreads),
                $"{list} at frame {frame} reads what the cartridge holds past its placements");
            overruns++;
        }
        ProjectileTrailPlacementList shortest = ProjectileTrailPlacementList.UnchargedIcePlasmaUp;
        int pastRecords = ProjectileTrailPlacementDefinitions.Records[shortest].Length;
        AssertThrows<InvalidOperationException>(() => ProjectileTrailPlacements.Start(shortest, pastRecords, null),
            "A read past a list's placements needs the installed overread catalog");
        AssertThrows<ArgumentOutOfRangeException>(() => ProjectileTrailPlacements.Start(shortest, ushort.MaxValue + 1, InstalledFixtures.SamusOverreads),
            "A frame no trail can read is rejected");
        Suite(nameof(VerifyCoordinateSpawn), () => VerifyCoordinateSpawn(bus, lists));
        Console.WriteLine($"Trail coordinates: {records} placements, {selections} selections and {overruns} reads past the placements match the cartridge.");
    }

    /// <summary>The list the cartridge's selector row and direction entry name, or null where either names none.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the trail selector and direction tables")]
    private static ProjectileTrailPlacementList? NativeList(
        ISnesAddressSpace bus, Dictionary<ushort, ProjectileTrailPlacementList> lists, int table, SamusBeamCombination beams, int direction)
    {
        ushort family = ReadRetailTrailWord(bus, table + beams.TableIndex * sizeof(ushort));
        if (family < CartridgeHalfStart)
            return null;
        ushort list = ReadRetailTrailWord(bus, 0x9b0000 | unchecked((ushort)(family + direction * sizeof(ushort))));
        return lists.TryGetValue(list, out ProjectileTrailPlacementList named) ? named : null;
    }

    /// <summary>Whether a bank-$9B offset and the three bytes after it lie in the cartridge half of the bank.</summary>
    private static bool RecordIsCartridge(int offset) => offset is >= CartridgeHalfStart and <= ushort.MaxValue - (RecordByteCount - 1);

    /// <summary>The placement the cartridge stores at bank-$9B offset <paramref name="offset"/>.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for one trail placement record")]
    private static ProjectileTrailPlacement NativePlacement(ISnesAddressSpace bus, int offset)
    {
        sbyte Byte(int index) => unchecked((sbyte)ReadRetailTrailOperandByte(bus, 0x9b0000 | (offset + index)));
        return new(new(Byte(0), Byte(1)), new(Byte(2), Byte(3)));
    }

    /// <summary>Checks actual trail spawn positions for beam, missile, and reflected list-start cases at coordinate wraps.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for trail family, direction and reflected list-start pointer tables")]
    private static void VerifyCoordinateSpawn(ISnesAddressSpace bus, Dictionary<ushort, ProjectileTrailPlacementList> lists)
    {
        // For each animation frame, the list position just past the first record that installs it: a trail spawned there
        // reads that frame.
        var records = Enumerable.Range(0, SamusProjectileRecordDefinitionsConstants.RecordCount)
            .OrderBy(SamusProjectileProgramAddresses.RecordAddress).ToArray();
        var afterFrame = Enumerable.Range(0, 22).Select(frame => SamusProjectileProgramAddresses.CursorAt(unchecked((ushort)(
            SamusProjectileProgramAddresses.RecordAddress(records.First(record =>
                SamusProjectileRecordDefinitions.Get(new SamusProjectileFrame(record)).AnimationFrame == frame)) +
            SamusProjectileRomData.Instructions.AnimationRecordByteCount)))).ToArray();
        var system = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None) { Overreads = InstalledFixtures.SamusOverreads };
        var spawn = new Action<SamusProjectileSlot>(system.Trails.SpawnTrail);
        int cases = 0;
        foreach (ushort type in new ushort[] { 1, 2, 6, 7, 10, 11, 17, 18, 22, 23, 26, 27, 37, 0x100, 0x200 })
        for (ushort direction = 0; direction < 10; direction++)
        for (ushort frame = 0; frame < 22; frame++)
        foreach (ushort origin in new ushort[] { 0, ushort.MaxValue })
        {
            ushort offsets = DirectionList(bus, type, direction);
            ushort y = unchecked((ushort)(offsets + frame * 4));
            system.Reset();
            var shot = new SamusProjectileSlot(0)
            {
                Type = type,
                Direction = direction,
                Cursor = afterFrame[frame],
                Position = new(new(origin, 0), new(origin, 0)),
            };
            spawn(shot);
            var actual = system.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1];
            if (!TrailSpawnDraws(type, lists, offsets, y))
            {
                AssertTrue(actual.Left.InstructionTimer.IsZero, $"type ${type:X4} direction {direction} frame {frame} draws no trail");
                cases++;
                continue;
            }
            AssertEqual(NativeSpawn(bus, origin, y), new ProjectileTrailCoordinatesExpected(actual.Left.Position.X, actual.Left.Position.Y, actual.Right.Position.X, actual.Right.Position.Y),
                "Real trail spawn matches four native high-byte operand reads");
            AssertEqual(1, actual.Left.InstructionTimer.RawValue, "Trail placements retain spawn timing");
            cases++;
        }
        // $90:BE17 reflects a projectile onto the start of its new direction's list. A trail
        // spawned before that list's first record reads the preceding word as its frame; the
        // 13% movie's reflected charged beam reads Grapple swing data at $9B:C1FB this way.
        foreach ((ushort type, int dataTable, int index) in ReflectableTrailTypes())
        for (ushort direction = 0; direction < 10; direction++)
        {
            ushort data = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), dataTable + index * 2);
            ushort listStart = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x930000 | unchecked((ushort)(data + 2 + direction * 2)));
            ushort frame = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x930000 | unchecked((ushort)(listStart - 2)));
            // An empty special beam attack row's direction table lies in direct-page scratch: no list, no trail.
            ushort? offsets = FamilyIsCartridge(bus, type) ? DirectionList(bus, type, direction) : null;
            ushort y = unchecked((ushort)((offsets ?? 0) + frame * 4));
            system.Reset();
            var shot = new SamusProjectileSlot(0)
            {
                Type = type, Direction = direction, Cursor = SamusProjectileProgramAddresses.CursorAt(listStart), Position = new(new(0x0100, 0), new(0x0100, 0)),
            };
            spawn(shot);
            var actual = system.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1];
            if (offsets is not { } list || !TrailSpawnDraws(type, lists, list, y))
            {
                AssertTrue(actual.Left.InstructionTimer.IsZero, $"reflected type ${type:X4} direction {direction} draws no trail");
                cases++;
                continue;
            }
            AssertEqual(NativeSpawn(bus, 0x0100, y), new ProjectileTrailCoordinatesExpected(actual.Left.Position.X, actual.Left.Position.Y, actual.Right.Position.X, actual.Right.Position.Y),
                $"reflected type ${type:X4} direction {direction} trail reads the word before its list start");
            cases++;
        }
        Console.WriteLine($"Trail spawn: {cases} beam/charged/SBA/missile direction/frame/origin and reflected list-start cases preserve all four native positions.");
    }

    /// <summary>The bank-$9B selector table <paramref name="type"/> reads its family from.</summary>
    private static int SelectorTable(ushort type) => (type & 0x20) != 0 ? 0x9ba4e3 : (type & 0x10) != 0 ? 0x9ba4cb : 0x9ba4b3;

    /// <summary>Whether <paramref name="type"/>'s selector row names a direction table in the cartridge half of bank $9B.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for a trail selector row")]
    private static bool FamilyIsCartridge(ISnesAddressSpace bus, ushort type) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SelectorTable(type) + (type & 15) * 2) >= CartridgeHalfStart;

    /// <summary>The bank-$9B list <paramref name="type"/>'s selector row and direction entry name.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for trail family and direction pointer tables")]
    private static ushort DirectionList(ISnesAddressSpace bus, ushort type, ushort direction)
    {
        ushort directions = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SelectorTable(type) + (type & 15) * 2);
        return RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x9b0000 | unchecked((ushort)(directions + direction * 2)));
    }

    /// <summary>The four positions native draws a trail at from the record at bank-$9B offset <paramref name="y"/>.</summary>
    private static ProjectileTrailCoordinatesExpected NativeSpawn(ISnesAddressSpace bus, ushort origin, ushort y)
    {
        byte Read(ushort operand, ushort index) => SnesLittleEndian.HighByte(ReadNativeTrailOperandWord(bus, operand, index));
        ushort Position(byte offset) => unchecked((ushort)(origin + (sbyte)offset - 4));
        return new ProjectileTrailCoordinatesExpected(Position(Read(0, unchecked((ushort)(y - 1)))), Position(Read(0, y)), Position(Read(1, y)), Position(Read(2, y)));
    }

    /// <summary>
    /// Whether a trail for <paramref name="type"/> draws from the record at <paramref name="y"/>: both programs are
    /// selected, the direction entry names a placement list, and the record lies in the cartridge. A read into
    /// direct-page scratch draws no trail instead (#1318).
    /// </summary>
    private static bool TrailSpawnDraws(ushort type, Dictionary<ushort, ProjectileTrailPlacementList> lists, ushort list, ushort y) =>
        TrailProgramsSelected(type) && lists.ContainsKey(list) && RecordIsCartridge(y);

    /// <summary>Whether <paramref name="type"/> selects both trail programs.</summary>
    private static bool TrailProgramsSelected(ushort type)
    {
        int family = SnesLittleEndian.HighByte(type);
        int index = family == 0 ? type & 63 : family + 31;
        return ProjectileTrailDefinitions.Select(index, HorizontalDirection.Left) is not null &&
            ProjectileTrailDefinitions.Select(index, HorizontalDirection.Right) is not null;
    }

    /// <summary>Every projectile type $90:BE17 can reflect, with its bank-$93 data table and index.</summary>
    private static IEnumerable<ReflectableTrailTypesResult> ReflectableTrailTypes()
    {
        for (ushort combination = 0; combination < 12; combination++)
        {
            yield return new ReflectableTrailTypesResult(combination, 0x9383c1, combination);
            yield return new ReflectableTrailTypesResult((ushort)(combination | 0x10), 0x9383d9, combination);
            yield return new ReflectableTrailTypesResult((ushort)(combination | 0x30), 0x9383d9, combination);
        }
        yield return new ReflectableTrailTypesResult(0x0100, 0x9383f1, 1);
        yield return new ReflectableTrailTypesResult(0x0200, 0x9383f1, 2);
    }

    /// <summary>Reads a native trail table word for the cartridge-side oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for trail selector and direction words")]
    private static ushort ReadRetailTrailWord(ISnesAddressSpace bus, int address) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);

    /// <summary>Reads a cartridge byte for a native bank-$9B trail operand.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for a $9B trail operand byte the native CPU reads from the cartridge")]
    private static byte ReadRetailTrailOperandByte(ISnesAddressSpace bus, int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeByte(address);

    /// <summary>Reads a modeled peripheral response or rejects an unmodeled hardware operand.</summary>
    [CpuBusBoundary("exact bus test: device-response oracle for a $9B trail operand outside memory and the cartridge")]
    private static byte ReadNativeTrailOperandDeviceByte(ISnesAddressSpace bus, int address) =>
        bus is ISnesCpuPeripheralSource peripheral
            ? peripheral.ReadPeripheralByte(address)
            : throw new InvalidOperationException(
                $"Native trail operand ${address:X6} addresses unmodeled hardware.");

    /// <summary>Resolves the native $9B absolute-indexed word through ROM, WRAM, SRAM, open bus, or devices in CPU read order.</summary>
    [CpuBusBoundary("exact bus test: reference CPU read of the $9B absolute-indexed trail operand across ROM, WRAM, SRAM, open bus and devices")]
    private static ushort ReadNativeTrailOperandWord(
        ISnesAddressSpace bus, ushort operand, ushort index)
    {
        int address = (0x9b0000 + operand + index) & SnesCpuAddressLayout.AddressMask;
        byte ReadData(int source, byte memoryDataRegister)
        {
            int bank = SnesAddressMath.BankNumber(source);
            int offset = SnesAddressMath.OffsetInBank(source);
            if ((bank & 0x40) == 0 &&
                (offset >= SnesBusLayoutConstants.ReservedBBusStart &&
                 offset <= SnesBusLayoutConstants.ReservedBBusEnd ||
                 offset >= SnesBusLayoutConstants.UnpopulatedExpansionStart &&
                 offset <= SnesBusLayoutConstants.UnpopulatedExpansionEnd))
                return memoryDataRegister;

            return SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(source)) switch
            {
                SnesDmaSourceKind.Cartridge => ReadRetailTrailOperandByte(bus, source),
                SnesDmaSourceKind.WorkRam =>
                    ((ISnesMutableMemory)bus).ReadWorkRamByte(source),
                SnesDmaSourceKind.Unmapped => ReadNativeTrailOperandDeviceByte(bus, source),
                _ => ReadNativeTrailOperandDeviceByte(bus, source),
            };
        }

        byte low = ReadData(address, SnesLittleEndian.HighByte(operand));
        byte high = ReadData((address + 1) & SnesCpuAddressLayout.AddressMask, low);
        return SnesLittleEndian.Word(low, high);
    }

    /// <summary>Runs the trail-coordinate comparison against the local retail cartridge.</summary>
    [VerificationCommand("--projectile-trail-coordinates")]
    internal static int ProjectileTrailCoordinatesCommand()
    {
        Suite(nameof(VerifyProjectileTrailCoordinates), () => VerifyProjectileTrailCoordinates(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
