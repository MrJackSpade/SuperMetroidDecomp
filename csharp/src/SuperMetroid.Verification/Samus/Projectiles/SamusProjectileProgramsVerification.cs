using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>
/// #1301, #1600: the Samus projectiles' typed segments, each matched beat by beat to its bank-$93 list: every timed
/// record's duration, address, radii and animation frame, the native address each place stands for, the word before
/// each list, and every projectile header's list fields; each animation plays the segments its lists' gotos,
/// fall-throughs and deletes play. What each record pictures is
/// installed, and <see cref="ProjectileFrameBindingsVerification"/> checks the import's bindings against each record.
/// </summary>
internal static class SamusProjectileProgramsVerification
{
    /// <summary>Every program, record and header matches bank $93.</summary>
    internal static void VerifySamusProjectilePrograms(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ushort Word(int address) => cartridge.ReadCartridgeWord(SamusSourceDefinitions.ProjectileDataBank.Value | (ushort)address);

        Dictionary<ushort, SamusProjectileSegment> byEntry = Enum.GetValues<SamusProjectileSegment>()
            .ToDictionary(SamusProjectileProgramAddresses.Entry, program => program);
        var records = new SortedSet<ushort>();
        var lists = new Dictionary<SamusProjectileSegment, NativeEnemyList<SamusProjectileSegment>>();
        int steps = 0;
        foreach ((ushort entry, SamusProjectileSegment program) in byEntry)
        {
            AssertEqual(Word(entry - sizeof(ushort)), SamusProjectileProgramAddresses.Prefix(program), $"projectile {program}: word before its list");
            IReadOnlyList<AnimationBeat<SamusProjectileFrame, NoAnimationCue>> beats = SamusProjectileAnimations.Segments.Beats(program);
            var addresses = new List<ushort>();
            NativeEnemyControl<SamusProjectileSegment> end;
            ushort cursor = entry;
            for (int index = 0; ; index++, steps++)
            {
                string at = $"projectile {program} place {index} at $93:{cursor:X4}";
                AssertEqual(cursor, SamusProjectileProgramAddresses.Address(new SamusProjectileSegmentBeat(program, index)), $"{at}: native address");
                if (cursor != entry && byEntry.TryGetValue(cursor, out SamusProjectileSegment next))
                {
                    AssertEqual(beats.Count, index, $"{at}: the run on into the next list ends the segment");
                    end = new(NativeEnemyControlKind.RunOnIntoNext, index, next, null, cursor);
                    break;
                }
                ushort word = Word(cursor);
                if (unchecked((short)word) >= 0)
                {
                    AssertTrue(index < beats.Count, $"{at}: the segment has a beat for the timed record");
                    var show = AssertIs<AnimationBeat<SamusProjectileFrame, NoAnimationCue>.Frame>(beats[index], $"{at}: timed record");
                    AssertEqual(word, show.Duration, $"{at}: duration");
                    AssertEqual(new SamusProjectileFrame(program, index), show.Shown, $"{at}: the segment's next record");
                    SamusProjectileFrameGameplay record = SamusProjectileFrameDefinitions.Get(show.Shown);
                    AssertEqual(cursor, SamusProjectileProgramAddresses.RecordAddress(show.Shown), $"{at}: record address");
                    ushort radii = Word(cursor + 4);
                    AssertEqual(new Extent16<RoomPixels>(LittleEndianBytes.LowByte(radii), LittleEndianBytes.HighByte(radii)), record.Radius, $"{at}: radii");
                    AssertEqual(Word(cursor + 6), record.AnimationFrame, $"{at}: animation frame");
                    records.Add(cursor);
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + SuperMetroid.AssetExtraction.CartridgeTables.SamusProjectileListLayout.TimedRecordByteCount));
                    continue;
                }
                AssertEqual(beats.Count, index, $"{at}: the goto or delete ends the segment");
                end = (SamusProjectileInstruction)word switch
                {
                    SamusProjectileInstruction.GoTo => new(NativeEnemyControlKind.Goto, index, byEntry[Word(cursor + 2)], null, cursor),
                    SamusProjectileInstruction.Delete => new(NativeEnemyControlKind.Stop, index, null, null, cursor),
                    _ => throw new InvalidDataException($"{at}: native word ${word:X4} has no typed form."),
                };
                break;
            }
            lists.Add(program, new NativeEnemyList<SamusProjectileSegment>([.. addresses], [end]));
        }
        // Each animation plays the segments its lists' gotos, runs on and deletes play.
        NativeEnemyAnimationOracle.CompareAnimations(lists, SamusProjectileAnimations.StartingAt);
        AssertEqual(SamusProjectileFrameDefinitionsConstants.FrameCount, records.Count, "every record is shown by a program");
        AssertTrue(records.SequenceEqual(SamusProjectileFrameDefinitions.TimedRecordAddresses), "the record addresses are the timed records in address order");

        int fields = 0;
        foreach (SamusProjectileKind kind in Enum.GetValues<SamusProjectileKind>())
        {
            ushort header = SamusProjectileHeaderNative.Address(kind);
            int kept = FirstListReadHeaders.Contains(kind) ? 1 : HeaderListFields;
            if (kept < HeaderListFields)
            {
                AssertThrows<ArgumentOutOfRangeException>(() => SamusProjectileKinds.ProgramAtField(kind, kept), $"{kind} keeps only the list field its shots read");
                AssertThrows<ArgumentOutOfRangeException>(() => SamusProjectileKinds.Animation(kind, SamusProjectileDirection.UpFacingRight), $"{kind} is read by no direction");
            }
            else
                AssertThrows<ArgumentOutOfRangeException>(() => SamusProjectileKinds.SoleAnimation(kind), $"{kind} is read by direction");
            for (int field = 0; field < kept; field++, fields++)
            {
                ushort native = Word(header + sizeof(ushort) * (1 + field));
                string at = $"projectile header $93:{header:X4} field {field}";
                if (!byEntry.TryGetValue(native, out SamusProjectileSegment program))
                    throw new InvalidDataException($"{at}: native word ${native:X4} is not a list, but every header field the port keeps names one.");
                AssertEqual(program, SamusProjectileKinds.ProgramAtField(header, field), $"{at}: list");
            }
        }
        ushort powerBomb = SamusProjectileHeaderNative.Address(SamusProjectileKind.PowerBomb);
        foreach (SamusProjectileDirection direction in Enum.GetValues<SamusProjectileDirection>())
        {
            ushort native = Word(powerBomb + sizeof(ushort) * (1 + direction.ToAimIndex()));
            string at = $"the Murder Beam fired {direction}";
            if (native == 0)
                AssertEqual(null, SamusProjectileSelection.BeamShotAnimation(SamusProjectileSelectionDefinitions.MurderBeam, charged: true, direction), $"{at}: no list");
            else if (byEntry.TryGetValue(native, out SamusProjectileSegment program))
                AssertEqual(program, SamusProjectileSelection.BeamShotAnimation(SamusProjectileSelectionDefinitions.MurderBeam, charged: true, direction), $"{at}: list");
            else
                AssertThrows<InvalidDataException>(() => SamusProjectileSelection.BeamShotAnimation(SamusProjectileSelectionDefinitions.MurderBeam, charged: true, direction), $"{at}: a word that is not a list fails");
            fields++;
        }
        Console.WriteLine($"  Samus projectile programs: {byEntry.Count} programs ({steps} steps, {records.Count} records) and {fields} header fields match bank $93.");
    }

    /// <summary>The list fields a header's selection may read: the ten a directional header holds.</summary>
    private const int HeaderListFields = 10;

    /// <summary>
    /// The headers whose own selection reads only their first list field: the Super Missile link, the bombs and the
    /// special-attack particles; the port keeps none of the words after it. The Murder Beam's read past the Power Bomb
    /// header is its own rule, checked against the same words.
    /// </summary>
    private static readonly HashSet<SamusProjectileKind> FirstListReadHeaders =
    [
        SamusProjectileKind.SuperMissileLink, SamusProjectileKind.PowerBomb, SamusProjectileKind.Bomb,
        SamusProjectileKind.PlasmaSBA, SamusProjectileKind.WaveSBA, SamusProjectileKind.SpazerSBA,
    ];

    /// <summary>The headers that hold only their first list field; every other header holds all ten.</summary>
    private static readonly HashSet<SamusProjectileKind> FirstListOnlyHeaders =
    [
        SamusProjectileKind.SuperMissileLink, SamusProjectileKind.PowerBomb, SamusProjectileKind.Bomb,
        SamusProjectileKind.PlasmaSBA, SamusProjectileKind.WaveSBA, SamusProjectileKind.SpazerSBA,
    ];

    /// <summary>
    /// Compares every selection the port makes with the cartridge selector word that makes it, a Spazer and Plasma
    /// combination's read past its table included, and every header's own words (its damage, and each list field it
    /// holds as the entry of the typed program the field selects) with the cartridge's.
    /// </summary>
    /// <param name="word">Reads the cartridge word at a bank-$93 address.</param>
    /// <param name="label">Names the check in its failures.</param>
    /// <returns>The number of words compared.</returns>
    internal static int VerifySelectionWords(Func<int, ushort> word, string label)
    {
        Ensure.NotNull(word);
        int compared = 0;
        foreach (ProjectileSelectorWord selector in SamusProjectileHeaderNative.Selectors())
        {
            AssertEqual(word(selector.Address), SamusProjectileHeaderNative.Address(selector.Kind), $"{label}: selector {selector.Address:X6} selects {selector.Kind}");
            compared++;
        }
        foreach (SamusProjectileKind header in Enum.GetValues<SamusProjectileKind>())
        {
            int at = SamusSourceDefinitions.ProjectileDataBank.Value | SamusProjectileHeaderNative.Address(header);
            AssertEqual(word(at), checked((ushort)SamusProjectileDamage.Read(header)), $"{label}: {header} damage");
            compared++;
            int fields = FirstListOnlyHeaders.Contains(header) ? 1 : HeaderListFields;
            for (int field = 0; field < fields; field++, compared++)
                AssertEqual(word(at + sizeof(ushort) * (1 + field)),
                    SamusProjectileProgramAddresses.Entry(SamusProjectileKinds.ProgramAtField(header, field)),
                    $"{label}: {header} list field {field}");
        }
        return compared;
    }

    /// <summary>The typed record at bank-$93 record address <paramref name="address"/>.</summary>
    internal static SamusProjectileFrameGameplay RecordAt(ushort address)
    {
        if (address == 0 || !SamusProjectileProgramAddresses.Layout.TryPlaceAt(address, out SamusProjectileSegmentBeat place))
            throw new InvalidDataException($"$93:{address:X4} is not a Samus projectile list position.");
        IReadOnlyList<AnimationBeat<SamusProjectileFrame, NoAnimationCue>> beats = SamusProjectileAnimations.Segments.Beats(place.Segment);
        return place.Beat < beats.Count && beats[place.Beat] is AnimationBeat<SamusProjectileFrame, NoAnimationCue>.Frame show
            ? SamusProjectileFrameDefinitions.Get(show.Shown)
            : throw new InvalidDataException($"$93:{address:X4} is not a timed record.");
    }

    /// <summary>Runs the projectile program oracle.</summary>
    [VerificationCommand("--samus-projectile-programs")]
    internal static int SamusProjectileProgramsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifySamusProjectilePrograms), () => VerifySamusProjectilePrograms(rom));
        return 0;
    }
}
