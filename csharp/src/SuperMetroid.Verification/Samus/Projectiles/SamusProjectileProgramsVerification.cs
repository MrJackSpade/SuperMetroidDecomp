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
                    SamusProjectileRecord record = SamusProjectileRecordDefinitions.Get(show.Frame);
                    AssertEqual(cursor, SamusProjectileProgramAddresses.RecordAddress(show.Frame.Record), $"{at}: record address");
                    AssertEqual(Word(cursor + 2), record.Spritemap, $"{at}: spritemap");
                    ushort radii = Word(cursor + 4);
                    AssertEqual(new Extent16<RoomPixels>(LittleEndianBytes.LowByte(radii), LittleEndianBytes.HighByte(radii)), record.Radius, $"{at}: radii");
                    AssertEqual(Word(cursor + 6), record.AnimationFrame, $"{at}: animation frame");
                    records.Add(cursor);
                    cursor = unchecked((ushort)(cursor + SamusProjectileListLayout.TimedRecordByteCount));
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
        foreach (ushort header in Headers())
            for (int field = 0; field < HeaderListFields; field++, fields++)
            {
                ushort native = Word(header + sizeof(ushort) * (1 + field));
                string at = $"projectile header $93:{header:X4} field {field}";
                if (native == 0)
                    AssertEqual(null, SamusProjectileHeaderDefinitions.Program(header, field), $"{at}: no list");
                else if (byEntry.TryGetValue(native, out SamusProjectileProgram program))
                    AssertEqual(program, SamusProjectileHeaderDefinitions.Program(header, field), $"{at}: list");
                else
                    AssertThrows<InvalidDataException>(() => SamusProjectileHeaderDefinitions.Program(header, field), $"{at}: a word that is not a list fails");
            }
        Console.WriteLine($"  Samus projectile programs: {byEntry.Count} programs ({steps} steps, {records.Count} records) and {fields} header fields match bank $93.");
    }

    /// <summary>The list fields a header's selection may read: the ten a directional header holds.</summary>
    private const int HeaderListFields = 10;

    /// <summary>Every projectile header: the 24 beam headers, then the named ones.</summary>
    internal static IEnumerable<ushort> Headers() =>
        Enum.GetValues<SamusProjectileHeader>().Select(SamusProjectileHeaderNative.Address);

    /// <summary>
    /// The word the selection tables hold at <paramref name="address"/>: a header's list field is the entry of the typed
    /// program it selects (zero for none); every other word is the selection definitions'.
    /// </summary>
    internal static ushort SelectionWord(int address)
    {
        if (HeaderListField(address) is not { } field)
            return SamusProjectileSelectionDefinitions.ReadWord(address);
        return SamusProjectileHeaderDefinitions.Program(field.Header, field.Index) is { } program
            ? SamusProjectileProgramAddresses.Entry(program) : (ushort)0;
    }

    /// <summary>The header and list field <paramref name="address"/> lies in, or null outside the headers' list fields.</summary>
    private static HeaderListFieldResult? HeaderListField(int address)
    {
        int link = 0x930000 | SamusProjectileHeaderNative.Address(SamusProjectileHeader.SuperMissileLink);
        int projectile25 = 0x930000 | SamusProjectileHeaderNative.Address(SamusProjectileHeader.Projectile25);
        int projectile27 = 0x930000 | SamusProjectileHeaderNative.Address(SamusProjectileHeader.Projectile27);
        if (address < SamusProjectileDamageDefinitions.BeamHeaderStart || address >= projectile27 + 2 * sizeof(ushort))
            return null;
        (int start, int stride) = address < link
            ? new HeaderListFieldCase(SamusProjectileDamageDefinitions.BeamHeaderStart, SamusProjectileDamageDefinitions.BeamHeaderStride)
            : address < projectile25 ? new HeaderListFieldCase(link, 2 * sizeof(ushort))
            : address < projectile27 ? new HeaderListFieldCase(projectile25, SamusProjectileDamageDefinitions.BeamHeaderStride)
            : new HeaderListFieldCase(projectile27, 2 * sizeof(ushort));
        int header = start + (address - start) / stride * stride;
        int offset = address - header;
        return offset == 0 ? null : new HeaderListFieldResult((ushort)header, offset / sizeof(ushort) - 1);
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
