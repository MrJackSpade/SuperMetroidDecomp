using System.Collections.Frozen;
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
    /// <summary>$9B:A4B3 and $9B:A4CB: the uncharged and charged trail selector tables.</summary>
    private static readonly (ProjectileTrailFamilyGroup Group, int Table)[] SelectorTables =
    [
        (ProjectileTrailFamilyGroup.Uncharged, 0x9ba4b3),
        (ProjectileTrailFamilyGroup.Charged, 0x9ba4cb),
    ];

    /// <summary>
    /// $9B:A4E3: the special beam attacks' trail selector table, whose only row a trail reads is the Spazer row the Spazer
    /// special attack's trailing pair selects by its beams (#2348).
    /// </summary>
    private const int SpecialBeamAttackSelectorTable = 0x9ba4e3;

    /// <summary>Bytes in one placement record: the left and right sparkles' signed X and Y offsets.</summary>
    private const int RecordByteCount = 4;

    /// <summary>The first bank-$9B offset the cartridge serves; below it lies direct-page scratch and hardware.</summary>
    private const int CartridgeHalfStart = 0x8000;

    /// <summary>
    /// Compares every frame's placement in every list with the cartridge's placement at the frame's record's trail word,
    /// every selector row and direction-table index with the cartridge, then spawns real trails from every frame each
    /// trailing projectile can show against the native CPU reads (#2124).
    /// </summary>
    internal static void VerifyProjectileTrailCoordinates(ISnesAddressSpace bus)
    {
        Dictionary<ProjectileTrailPlacementList, ushort> starts = Enum.GetValues<ProjectileTrailPlacementList>().ToDictionary(
            list => list, list => ProjectileTrailPlacementSources.Start(CartridgeImportSource.Require(bus), list).Offset);
        ushort[] orderedStarts = [.. starts.Values.Distinct().Order()];
        foreach ((ProjectileTrailPlacementList list, int count) in ProjectileTrailPlacementSources.OwnPlacementCounts)
        {
            // Each list holds only its own placements: none reaches where the next list begins (#1926).
            if (orderedStarts.FirstOrDefault(start => start > starts[list]) is var next && next != 0)
                AssertTrue(starts[list] + count * RecordByteCount <= next, $"{list}'s placements end before the next list begins");
        }
        AssertEqual(838, ProjectileTrailPlacementSources.OwnPlacementCounts.Values.Sum(), "The seventy-six lists hold 838 placements of their own");
        int records = 0;
        foreach ((ProjectileTrailPlacementList list, FrozenDictionary<SamusProjectileFrameId, ProjectileTrailPlacement> placements) in
            ProjectileTrailPlacementDefinitions.PlacementsByList)
        foreach ((SamusProjectileFrameId frame, ProjectileTrailPlacement placement) in placements)
        foreach (int record in SamusProjectileProgramAddresses.RecordAddresses(frame))
        {
            // Every record that shows a frame places its trail where the frame's placement does (#2274).
            int trailWord = TrailWord(bus, record);
            AssertTrue(trailWord < ProjectileTrailPlacementSources.OwnPlacementCounts[list], $"{list} places {frame} at $93:{record:X4} from one of its own placements");
            AssertEqual(NativePlacement(bus, starts[list] + trailWord * RecordByteCount), placement, $"{list} places {frame} at $93:{record:X4} where the cartridge does");
            records++;
        }

        Dictionary<ushort, ProjectileTrailPlacementList> lists = starts.ToDictionary(pair => pair.Value, pair => pair.Key);
        int selections = 0;
        foreach ((ProjectileTrailFamilyGroup group, int table) in SelectorTables)
        foreach (SamusBeamCombination beams in Enum.GetValues<SamusBeamCombination>())
        foreach (SamusProjectileDirection direction in SamusProjectileDirectionCodec.InAimOrder)
        {
            // A Spazer and Plasma combination's row reads past its table into the next one's; the game authors it no
            // trail, so it names none (#1926).
            ProjectileTrailPlacementList? expected = beams.CanBeEquipped ? NativeList(bus, lists, table, beams, direction.ToAimIndex()) : null;
            AssertEqual(expected, ProjectileTrailPlacements.List(group, beams, direction),
                $"{group} {beams} direction {direction} selects the list the cartridge's pointers name");
            selections++;
        }
        AssertEqual(320, selections, "Two selector tables of sixteen rows by the ten directions a shot is fired in");
        // The trailing pair selects its list from the Spazer row of the special beam attacks' table.
        foreach (SamusProjectileDirection direction in SamusProjectileDirectionCodec.InAimOrder)
        {
            AssertEqual(NativeList(bus, lists, SpecialBeamAttackSelectorTable, SamusProjectileType.EchoOf(SamusBeamEcho.SpazerComboTrail).BeamCombination, direction.ToAimIndex()),
                ProjectileTrailPlacements.SpazerAttackTrailingPairList(direction),
                $"the Spazer special attack's trailing pair fired {direction} selects the list the cartridge's pointers name");
            selections++;
        }

        Suite(nameof(VerifyCoordinateSpawn), () => VerifyCoordinateSpawn(bus, lists));
        Console.WriteLine($"Trail coordinates: {records} frame placements and {selections} selections match the cartridge.");
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

    /// <summary>
    /// Checks actual trail spawn positions, at coordinate wraps, from every frame each trailing beam, missile and echo can show
    /// in every direction, and the reflected list-start cases.
    /// </summary>
    private static void VerifyCoordinateSpawn(ISnesAddressSpace bus, Dictionary<ushort, ProjectileTrailPlacementList> lists)
    {
        var system = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake());
        var spawn = new Action<SamusProjectile>(system.Trails.SpawnTrail);
        int cases = 0;
        // 36 is the Spazer combo's trail echo; no projectile is an echo of Spazer+Wave (37), which the typed identity cannot name.
        foreach (ushort type in new ushort[] { 1, 2, 6, 7, 10, 11, 17, 18, 22, 23, 26, 27, 36, 0x100, 0x200 })
        foreach (SamusProjectileFrameId frame in ShownFrames(SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(type)))
        foreach (int record in SamusProjectileProgramAddresses.RecordAddresses(frame))
        for (ushort direction = 0; direction < 10; direction++)
        foreach (ushort origin in new ushort[] { 0, ushort.MaxValue })
        {
            ushort offsets = DirectionList(bus, type, direction);
            int trailWord = TrailWord(bus, record);
            ushort y = unchecked((ushort)(offsets + trailWord * 4));
            system.Reset();
            var shot = new SamusProjectile(0)
            {
                Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(type),
                Direction = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeDirection(direction),
                Playback = SamusProjectileProgramAddresses.PlaybackAt(unchecked((ushort)(record +
                    SuperMetroid.AssetExtraction.CartridgeTables.SamusProjectileListLayout.TimedRecordByteCount))),
                Position = new(new(origin, 0), new(origin, 0)),
            };
            if (SamusProjectilePlayback.BeatOf(shot.Playback ?? throw new InvalidOperationException($"The position after {frame} names no playback.")).Beat == 0)
            {
                // The record ends its list, so the position after it is the next list's start: a trail spawned there has no
                // frame to place it from, and leaves none (#1625).
                spawn(shot);
                AssertEqual(0, system.LiveTrails.Count, $"type ${type:X4} direction {direction} after {frame} at a list start draws no trail");
                cases++;
                continue;
            }
            AssertEqual<SamusProjectileFrameId?>(frame, ProjectileTrails.GetTrailFrame(shot), $"a shot past {frame}'s record shows it");
            spawn(shot);
            if (!TrailSpawnDraws(lists, offsets, trailWord))
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
        Console.WriteLine($"Trail spawn: {cases} beam/charged/special attack/missile direction/frame/origin cases preserve all four native positions or leave no trail, and reflected list starts leave none.");
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
    /// Whether a trail draws from trail word <paramref name="trailWord"/> of the list at bank-$9B offset <paramref name="list"/>:
    /// the direction entry names a placement list, and the word lies within its own placements (#1318, #1625, #1926). Every
    /// shot that trails selects both programs within the selector tables (#1500).
    /// </summary>
    private static bool TrailSpawnDraws(Dictionary<ushort, ProjectileTrailPlacementList> lists, ushort list, int trailWord) =>
        lists.TryGetValue(list, out ProjectileTrailPlacementList named) && trailWord < ProjectileTrailPlacementSources.OwnPlacementCounts[named];

    /// <summary>The trail word of the timed record at bank-$93 <paramref name="record"/>: the placement it reads from its list.</summary>
    private static int TrailWord(ISnesAddressSpace bus, int record) => RomDataReader.ReadWordFixedBank(
        CartridgeImportSource.Require(bus), 0x930000 | unchecked((ushort)(record + 6)));

    /// <summary>
    /// Every frame a projectile of <paramref name="type"/> can show, whatever its direction: the frames of every animation of
    /// each kind it can play, its beam shot's and, charged, its special attack's, a missile's or Super Missile's and its link's,
    /// and an echo's and the Spazer special attack's it trails from.
    /// </summary>
    private static HashSet<SamusProjectileFrameId> ShownFrames(SamusProjectileType type)
    {
        var kinds = new List<SamusProjectileKind>();
        if (type.Echo is { } echo)
        {
            kinds.Add(SamusProjectileSelection.Echo(echo));
            if (echo == SamusBeamEcho.SpazerComboTrail)
                kinds.Add(SamusProjectileKind.SpazerSpecialAttack);
        }
        else if (type.Family == SamusProjectileFamily.Beam)
        {
            kinds.Add(SamusProjectileSelection.BeamShot(type.BeamCombination, type.IsChargedBeam));
            if (type.IsChargedBeam && SamusProjectileSelection.SpecialAttack(type.BeamCombination) is { } attack)
                kinds.Add(attack);
        }
        else
        {
            kinds.Add(SamusProjectileSelection.NonBeam(type.Family));
            if (type.Family == SamusProjectileFamily.SuperMissile)
                kinds.Add(SamusProjectileKind.SuperMissileLink);
        }
        return [.. kinds.SelectMany(SamusProjectileKinds.Animations)
            .SelectMany(start => SamusProjectileAnimations.StartingWith(start).Phases.Select(phase => phase.Segment))
            .SelectMany(segment => SamusProjectileAnimations.Segments.Beats(segment))
            .Select(beat => beat.ShownFrame).OfType<SamusProjectileFrameId>()];
    }

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
