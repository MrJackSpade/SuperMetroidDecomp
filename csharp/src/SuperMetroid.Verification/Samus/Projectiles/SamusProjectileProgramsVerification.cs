using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>
/// #1301: the Samus projectiles' typed programs, each matched step by step to its bank-$93 list: every timed record's
/// duration, address, spritemap, radii and animation frame, every goto, fall-through and delete, the native address
/// each step stands for, the word before each list, and every projectile header's list fields.
/// </summary>
internal static class SamusProjectileProgramsVerification
{
    /// <summary>Every program, record and header matches bank $93.</summary>
    internal static void VerifySamusProjectilePrograms(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ushort Word(int address) => cartridge.ReadCartridgeWord(SamusSourceDefinitions.ProjectileDataBank.Value | (ushort)address);

        Dictionary<ushort, SamusProjectileProgram> byEntry = Enum.GetValues<SamusProjectileProgram>()
            .ToDictionary(SamusProjectileProgramAddresses.Entry, program => program);
        var records = new SortedSet<ushort>();
        int steps = 0;
        foreach ((ushort entry, SamusProjectileProgram program) in byEntry)
        {
            AssertEqual(Word(entry - sizeof(ushort)), SamusProjectileProgramAddresses.Prefix(program), $"projectile {program}: word before its list");
            var programSteps = SamusProjectileAnimations.Programs.Steps(program);
            ushort cursor = entry;
            int shown = 0;
            for (int index = 0; index < programSteps.Count; index++, steps++)
            {
                string at = $"projectile {program} step {index} at $93:{cursor:X4}";
                var step = programSteps[index];
                AssertEqual(cursor, SamusProjectileProgramAddresses.Address(new AnimationProgramCursor<SamusProjectileProgram>(program, index)), $"{at}: native address");
                if (cursor != entry && byEntry.TryGetValue(cursor, out SamusProjectileProgram next))
                {
                    var fallInto = AssertIs<AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.FallIntoStep>(step, $"{at}: runs on into the next list");
                    AssertEqual(next, fallInto.Target, $"{at}: fall-through target");
                    AssertEqual(programSteps.Count, index + 1, $"{at}: the fall-through ends the program");
                    break;
                }
                ushort word = Word(cursor);
                if (unchecked((short)word) >= 0)
                {
                    var show = AssertIs<AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.ShowStep>(step, $"{at}: timed record");
                    AssertEqual(word, show.Duration, $"{at}: duration");
                    AssertEqual(new SamusProjectileFrame(program, shown++), show.Frame, $"{at}: the program's next record");
                    SamusProjectileRecord record = SamusProjectileRecordDefinitions.Get(show.Frame);
                    AssertEqual(cursor, SamusProjectileProgramAddresses.RecordAddress(show.Frame), $"{at}: record address");
                    AssertEqual(Word(cursor + 2), record.Spritemap, $"{at}: spritemap");
                    ushort radii = Word(cursor + 4);
                    AssertEqual(new Extent16<RoomPixels>(LittleEndianBytes.LowByte(radii), LittleEndianBytes.HighByte(radii)), record.Radius, $"{at}: radii");
                    AssertEqual(Word(cursor + 6), record.AnimationFrame, $"{at}: animation frame");
                    records.Add(cursor);
                    cursor = unchecked((ushort)(cursor + SuperMetroid.AssetExtraction.CartridgeTables.SamusProjectileListLayout.TimedRecordByteCount));
                    continue;
                }
                switch ((SamusProjectileInstruction)word)
                {
                    case SamusProjectileInstruction.GoTo:
                        var goTo = AssertIs<AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.GoToStep>(step, $"{at}: goto");
                        AssertEqual(byEntry[Word(cursor + 2)], goTo.Target, $"{at}: goto target");
                        break;
                    case SamusProjectileInstruction.Delete:
                        AssertTrue(step is AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.StopStep, $"{at}: delete");
                        break;
                    default:
                        throw new InvalidDataException($"{at}: native word ${word:X4} has no typed form.");
                }
                AssertEqual(programSteps.Count, index + 1, $"{at}: the goto or delete ends the program");
            }
        }
        AssertEqual(SamusProjectileRecordDefinitionsConstants.RecordCount, records.Count, "every record is shown by a program");
        AssertTrue(records.SequenceEqual(SamusProjectileRecordDefinitions.TimedRecordAddresses), "the record addresses are the timed records in address order");

        int fields = 0;
        foreach (SamusProjectileHeader kind in Enum.GetValues<SamusProjectileHeader>())
        {
            ushort header = SamusProjectileHeaderNative.Address(kind);
            int kept = FirstListReadHeaders.Contains(kind) ? 1 : HeaderListFields;
            if (kept < HeaderListFields)
                AssertThrows<ArgumentOutOfRangeException>(() => SamusProjectileHeaders.Program(kind, kept), $"{kind} keeps only the list field its shots read");
            for (int field = 0; field < kept; field++, fields++)
            {
                ushort native = Word(header + sizeof(ushort) * (1 + field));
                string at = $"projectile header $93:{header:X4} field {field}";
                if (native == 0)
                    AssertEqual(null, SamusProjectileHeaders.Program(header, field), $"{at}: no list");
                else if (byEntry.TryGetValue(native, out SamusProjectileProgram program))
                    AssertEqual(program, SamusProjectileHeaders.Program(header, field), $"{at}: list");
                else
                    AssertThrows<InvalidDataException>(() => SamusProjectileHeaders.Program(header, field), $"{at}: a word that is not a list fails");
            }
        }
        ushort powerBomb = SamusProjectileHeaderNative.Address(SamusProjectileHeader.PowerBomb);
        foreach (SamusProjectileDirection direction in Enum.GetValues<SamusProjectileDirection>())
        {
            ushort native = Word(powerBomb + sizeof(ushort) * (1 + direction.ToAimIndex()));
            string at = $"the Murder Beam fired {direction}";
            if (native == 0)
                AssertEqual(null, SamusProjectileSelection.BeamShotProgram(SamusProjectileSelectionDefinitions.MurderBeam, charged: true, direction), $"{at}: no list");
            else if (byEntry.TryGetValue(native, out SamusProjectileProgram program))
                AssertEqual(program, SamusProjectileSelection.BeamShotProgram(SamusProjectileSelectionDefinitions.MurderBeam, charged: true, direction), $"{at}: list");
            else
                AssertThrows<InvalidDataException>(() => SamusProjectileSelection.BeamShotProgram(SamusProjectileSelectionDefinitions.MurderBeam, charged: true, direction), $"{at}: a word that is not a list fails");
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
    private static readonly HashSet<SamusProjectileHeader> FirstListReadHeaders =
    [
        SamusProjectileHeader.SuperMissileLink, SamusProjectileHeader.PowerBomb, SamusProjectileHeader.Bomb,
        SamusProjectileHeader.PlasmaSBA, SamusProjectileHeader.WaveSBA, SamusProjectileHeader.SpazerSBA,
    ];

    /// <summary>The headers that hold only their first list field; every other header holds all ten.</summary>
    private static readonly HashSet<SamusProjectileHeader> FirstListOnlyHeaders =
    [
        SamusProjectileHeader.SuperMissileLink, SamusProjectileHeader.PowerBomb, SamusProjectileHeader.Bomb,
        SamusProjectileHeader.PlasmaSBA, SamusProjectileHeader.WaveSBA, SamusProjectileHeader.SpazerSBA,
    ];

    /// <summary>
    /// Compares every selection the port makes with the cartridge selector word that makes it, a Spazer and Plasma
    /// combination's read past its table included, and every header's own words (its damage, and each list field it
    /// holds as the entry of the typed program the field selects, zero for none) with the cartridge's.
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
            AssertEqual(word(selector.Address), SamusProjectileHeaderNative.Address(selector.Header), $"{label}: selector {selector.Address:X6} selects {selector.Header}");
            compared++;
        }
        foreach (SamusProjectileHeader header in Enum.GetValues<SamusProjectileHeader>())
        {
            int at = SamusSourceDefinitions.ProjectileDataBank.Value | SamusProjectileHeaderNative.Address(header);
            AssertEqual(word(at), checked((ushort)SamusProjectileDamage.Read(header)), $"{label}: {header} damage");
            compared++;
            int fields = FirstListOnlyHeaders.Contains(header) ? 1 : HeaderListFields;
            for (int field = 0; field < fields; field++, compared++)
                AssertEqual(word(at + sizeof(ushort) * (1 + field)),
                    SamusProjectileHeaders.Program(header, field) is { } program ? SamusProjectileProgramAddresses.Entry(program) : (ushort)0,
                    $"{label}: {header} list field {field}");
        }
        return compared;
    }

    /// <summary>The typed record at bank-$93 record address <paramref name="address"/>.</summary>
    internal static SamusProjectileRecord RecordAt(ushort address)
    {
        var cursor = SamusProjectileProgramAddresses.CursorAt(address) ?? throw new InvalidDataException("A record address is never zero.");
        var step = SamusProjectileAnimations.Programs.Steps(cursor.Program)[cursor.Position];
        return step is AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.ShowStep show
            ? SamusProjectileRecordDefinitions.Get(show.Frame)
            : throw new InvalidDataException($"$93:{address:X4} is not a timed record.");
    }

    /// <summary>Runs the projectile program oracle.</summary>
    [VerificationCommand("--samus-projectile-programs")]
    internal static int SamusProjectileProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifySamusProjectilePrograms), () => VerifySamusProjectilePrograms(rom));
        return 0;
    }
}
