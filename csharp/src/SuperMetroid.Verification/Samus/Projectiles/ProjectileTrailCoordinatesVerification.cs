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
        Dictionary<ProjectileTrailPlacementList, ushort> starts = Enum.GetValues<ProjectileTrailPlacementList>().ToDictionary(
            list => list, list => ProjectileTrailPlacementSources.Start(CartridgeImportSource.Require(bus), list).Offset);
        int records = 0;
        foreach ((ProjectileTrailPlacementList list, ProjectileTrailPlacement[] placements) in ProjectileTrailPlacementDefinitions.PlacementsByList)
            for (int frame = 0; frame < placements.Length; frame++, records++)
                AssertEqual(NativePlacement(bus, starts[list] + frame * RecordByteCount), placements[frame],
                    $"{list} placement {frame} matches the cartridge");
        AssertEqual(1660, records, "The eighty lists read 1660 placements up to the end of their runs of records");

        Dictionary<ushort, ProjectileTrailPlacementList> lists = starts.ToDictionary(pair => pair.Value, pair => pair.Key);
        int selections = 0;
        foreach ((ProjectileTrailFamilyGroup group, int table) in SelectorTables)
        foreach (SamusBeamCombination beams in Enum.GetValues<SamusBeamCombination>())
        foreach (SamusProjectileDirection direction in SamusProjectileDirectionCodec.InAimOrder)
        {
            AssertEqual(NativeList(bus, lists, table, beams, direction.ToAimIndex()), ProjectileTrailPlacements.List(group, beams, direction),
                $"{group} {beams} direction {direction} selects the list the cartridge's pointers name");
            selections++;
        }
        AssertEqual(480, selections, "Three selector tables of sixteen rows by the ten directions a shot is fired in");

        // Every frame a shot's animation can show past a list's run of placements leaves no trail: what lies past a list is
        // not part of the game's design (#1625).
        int[] frameDomain = [.. SamusProjectileFrameOrder.Frames.Select(record => SamusProjectileFrameDefinitions.Get(record).AnimationFrame)
            .Distinct()
            .Order()];
        int overruns = 0;
        foreach (ProjectileTrailPlacementList list in Enum.GetValues<ProjectileTrailPlacementList>())
        foreach (int frame in frameDomain.Where(frame => frame >= ProjectileTrailPlacementDefinitions.PlacementsByList[list].Length))
        {
            AssertEqual(null, ProjectileTrailPlacements.Start(list, frame), $"{list} at frame {frame}, past its placements, places no trail");
            overruns++;
        }
        AssertTrue(overruns > 0, "Some shot frames lie past a list's placements");
        Suite(nameof(VerifyCoordinateSpawn), () => VerifyCoordinateSpawn(bus, lists));
        Console.WriteLine($"Trail coordinates: {records} placements and {selections} selections match the cartridge; {overruns} frames past the placements place no trail.");
    }

    /// <summary>The list the cartridge's selector row and direction entry name, or null where either names none.</summary>
    private static ProjectileTrailPlacementList? NativeList(
        ISnesAddressSpace bus, Dictionary<ushort, ProjectileTrailPlacementList> lists, int table, SamusBeamCombination beams, int direction)
    {
        ushort family = ReadRetailTrailWord(bus, table + beams.TableIndex * sizeof(ushort));
        if (family < CartridgeHalfStart)
            return null;
        ushort list = ReadRetailTrailWord(bus, 0x9b0000 | unchecked((ushort)(family + direction * sizeof(ushort))));
        return lists.TryGetValue(list, out ProjectileTrailPlacementList named) ? named : null;
    }

    /// <summary>The placement the cartridge stores at bank-$9B offset <paramref name="offset"/>.</summary>
    private static ProjectileTrailPlacement NativePlacement(ISnesAddressSpace bus, int offset)
    {
        sbyte Byte(int index) => unchecked((sbyte)ReadRetailTrailOperandByte(bus, 0x9b0000 | (offset + index)));
        return new(new(Byte(0), Byte(1)), new(Byte(2), Byte(3)));
    }

    /// <summary>Checks actual trail spawn positions for beam, missile, and reflected list-start cases at coordinate wraps.</summary>
    private static void VerifyCoordinateSpawn(ISnesAddressSpace bus, Dictionary<ushort, ProjectileTrailPlacementList> lists)
    {
        // For each animation frame, the list position just past the first record that installs it: a trail spawned there
        // reads that frame.
        var records = SamusProjectileFrameOrder.Frames.OrderBy(SamusProjectileProgramAddresses.RecordAddress).ToArray();
        var afterFrame = Enumerable.Range(0, 22).Select(frame => SamusProjectileProgramAddresses.PlaybackAt(unchecked((ushort)(
            SamusProjectileProgramAddresses.RecordAddress(records.First(record =>
                SamusProjectileFrameDefinitions.Get(record).AnimationFrame == frame)) +
            SuperMetroid.AssetExtraction.CartridgeTables.SamusProjectileListLayout.TimedRecordByteCount)))).ToArray();
        var system = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));
        var spawn = new Action<SamusProjectile>(system.Trails.SpawnTrail);
        int cases = 0;
        // 36 is the Spazer combo's trail echo; no projectile is an echo of Spazer+Wave (37), which the typed identity cannot name.
        foreach (ushort type in new ushort[] { 1, 2, 6, 7, 10, 11, 17, 18, 22, 23, 26, 27, 36, 0x100, 0x200 })
        for (ushort direction = 0; direction < 10; direction++)
        for (ushort frame = 0; frame < 22; frame++)
        foreach (ushort origin in new ushort[] { 0, ushort.MaxValue })
        {
            ushort offsets = DirectionList(bus, type, direction);
            ushort y = unchecked((ushort)(offsets + frame * 4));
            system.Reset();
            var shot = new SamusProjectile(0)
            {
                Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(type),
                Direction = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeDirection(direction),
                Playback = afterFrame[frame],
                Position = new(new(origin, 0), new(origin, 0)),
            };
            spawn(shot);
            if (!TrailSpawnDraws(lists, offsets, frame))
            {
                AssertEqual(0, system.LiveTrails.Count, $"type ${type:X4} direction {direction} frame {frame} draws no trail");
                cases++;
                continue;
            }
            SamusProjectileTrail actual = system.LiveTrails.Single();
            AssertEqual(NativeSpawn(bus, origin, y), new ProjectileTrailCoordinatesExpected(actual.Left.Position.X, actual.Left.Position.Y, actual.Right.Position.X, actual.Right.Position.Y),
                "Real trail spawn matches four native high-byte operand reads");
            AssertEqual(1, actual.Left.FrameHold, "Trail placements retain spawn timing");
            cases++;
        }
        // $90:BE17 reflects a projectile onto the start of its new direction's list. A trail spawned before that list has
        // shown a record has no frame to place it from, and leaves none (#1625).
        foreach ((ushort type, int dataTable, int index) in ReflectableTrailTypes())
        for (ushort direction = 0; direction < 10; direction++)
        {
            ushort data = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), dataTable + index * 2);
            ushort listStart = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x930000 | unchecked((ushort)(data + 2 + direction * 2)));
            system.Reset();
            var shot = new SamusProjectile(0)
            {
                Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(type), Direction = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeDirection(direction), Playback = SamusProjectileProgramAddresses.PlaybackAt(listStart), Position = new(new(0x0100, 0), new(0x0100, 0)),
            };
            spawn(shot);
            AssertEqual(0, system.LiveTrails.Count, $"reflected type ${type:X4} direction {direction} leaves no trail at its list's start");
            cases++;
        }
        Console.WriteLine($"Trail spawn: {cases} beam/charged/SBA/missile direction/frame/origin cases preserve all four native positions or leave no trail, and reflected list starts leave none.");
    }

    /// <summary>The bank-$9B selector table <paramref name="type"/> reads its family from.</summary>
    private static int SelectorTable(ushort type) => (type & 0x20) != 0 ? 0x9ba4e3 : (type & 0x10) != 0 ? 0x9ba4cb : 0x9ba4b3;

    /// <summary>The bank-$9B list <paramref name="type"/>'s selector row and direction entry name.</summary>
    private static ushort DirectionList(ISnesAddressSpace bus, ushort type, ushort direction)
    {
        ushort directions = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SelectorTable(type) + (type & 15) * 2);
        return RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x9b0000 | unchecked((ushort)(directions + direction * 2)));
    }

    /// <summary>The four positions native draws a trail at from the record at bank-$9B offset <paramref name="y"/>.</summary>
    private static ProjectileTrailCoordinatesExpected NativeSpawn(ISnesAddressSpace bus, ushort origin, ushort y)
    {
        byte Read(ushort operand, ushort index) => LittleEndianBytes.HighByte(ReadNativeTrailOperandWord(bus, operand, index));
        ushort Position(byte offset) => unchecked((ushort)(origin + (sbyte)offset - 4));
        return new ProjectileTrailCoordinatesExpected(Position(Read(0, unchecked((ushort)(y - 1)))), Position(Read(0, y)), Position(Read(1, y)), Position(Read(2, y)));
    }

    /// <summary>
    /// Whether a trail draws from frame <paramref name="frame"/> of the list at bank-$9B offset <paramref name="list"/>: the
    /// direction entry names a placement list, and the frame lies within its placements (#1318, #1625). Every shot that
    /// trails selects both programs within the selector tables (#1500).
    /// </summary>
    private static bool TrailSpawnDraws(Dictionary<ushort, ProjectileTrailPlacementList> lists, ushort list, int frame) =>
        lists.TryGetValue(list, out ProjectileTrailPlacementList named) && frame < ProjectileTrailPlacementDefinitions.PlacementsByList[named].Length;

    /// <summary>Every projectile type $90:BE17 can reflect, with its bank-$93 data table and index.</summary>
    private static IEnumerable<ReflectableTrailTypesResult> ReflectableTrailTypes()
    {
        for (ushort combination = 0; combination < 12; combination++)
        {
            yield return new ReflectableTrailTypesResult(combination, 0x9383c1, combination);
            // No projectile is both charged and an echo, so the typed identity names no charged-echo word (#1470).
            yield return new ReflectableTrailTypesResult((ushort)(combination | 0x10), 0x9383d9, combination);
        }
        yield return new ReflectableTrailTypesResult(0x0100, 0x9383f1, 1);
        yield return new ReflectableTrailTypesResult(0x0200, 0x9383f1, 2);
    }

    /// <summary>Reads a native trail table word for the cartridge-side oracle.</summary>
    private static ushort ReadRetailTrailWord(ISnesAddressSpace bus, int address) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);

    /// <summary>Reads a cartridge byte for a native bank-$9B trail operand.</summary>
    private static byte ReadRetailTrailOperandByte(ISnesAddressSpace bus, int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeByte(address);

    /// <summary>Reads a modeled peripheral response or rejects an unmodeled hardware operand.</summary>
    private static byte ReadNativeTrailOperandDeviceByte(ISnesAddressSpace bus, int address) =>
        bus is ISnesCpuPeripheralSource peripheral
            ? peripheral.ReadPeripheralByte(address)
            : throw new InvalidOperationException(
                $"Native trail operand ${address:X6} addresses unmodeled hardware.");

    /// <summary>Resolves the native $9B absolute-indexed word through ROM, WRAM, SRAM, open bus, or devices in CPU read order.</summary>
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

        byte low = ReadData(address, LittleEndianBytes.HighByte(operand));
        byte high = ReadData((address + 1) & SnesCpuAddressLayout.AddressMask, low);
        return LittleEndianBytes.Word(low, high);
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
