using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>
/// Cartridge-backed development inventory, never a production ROM fallback.
/// Flags plausible five-byte OAM records, not a rendering proof: an extended
/// or BG2 command payload may coincidentally pass the shallow count check.
/// Every family still needs a consumer and exact OAM parity test before extraction.
/// </summary>
internal sealed record EnemyVisualSelectorInventory(
    int Discovered, int Catalogs, int Operands, int Ordinary, int Special,
    Dictionary<int, ushort> Keyed, List<(string Name, int Ordinary, int Special)> Families,
    List<string> Unresolved)
{
    /// <summary>Scans every Core instruction-program catalog's presentation operands against the cartridge.</summary>
    internal static EnemyVisualSelectorInventory Collect(ISnesAddressSpace rom)
    {
        int catalogs = 0;
        int discovered = 0;
        int operands = 0;
        int ordinary = 0;
        int special = 0;
        var unresolved = new List<string>();
        var keyed = new Dictionary<int, ushort>();
        var families = new List<(string Name, int Ordinary, int Special)>();

        // One-frame programs declare a single operand instead of an indexed list.
        // Ignoring that shape omitted Kzan's live operand (#1166).
        foreach (var catalog in InstructionProgramCatalog.All()
                     .Where(catalog => catalog.PresentationOperands is not null || catalog.SinglePresentationOperand is not null))
        {
            string name = catalog.Type.Name;
            discovered++;
            catalogs++;
            if (catalog.MechanicsWords is null || (catalog.CompiledMechanicsByteProbe is null && catalog.DeclaredBank is null))
            {
                unresolved.Add($"{name}: no standard mechanics-word/bank probe");
                continue;
            }
            ushort firstAddress = catalog.MechanicsWords[0].Address;
            // A declared bank also covers catalogs with sparse mechanics words
            // but no byte-level ownership probe, such as the cutscene Baby.
            int? declaredBank = catalog.DeclaredBank;
            if (declaredBank is > byte.MaxValue) declaredBank >>= 16;
            byte[] banks = declaredBank.HasValue
                ? [checked((byte)declaredBank.Value)]
                : Enumerable.Range(0x80, 0x60)
                    .Where(bank => catalog.CompiledMechanicsByteProbe!((bank << 16) | firstAddress))
                    .Select(bank => (byte)bank).ToArray();
            if (banks.Length != 1)
            {
                unresolved.Add($"{name}: {banks.Length} matching banks");
                continue;
            }
            IReadOnlyList<ushort> presentation = catalog.PresentationOperands ?? [catalog.SinglePresentationOperand!.Value];
            int familyOrdinary = 0;
            int familySpecial = 0;
            foreach (ushort address in presentation)
            {
                int source = (banks[0] << 16) | address;
                ushort pointer = ReadWord(rom, source);
                if (keyed.TryGetValue(source, out ushort previous) &&
                    previous != pointer)
                    throw new InvalidDataException(
                        $"Conflicting visual selector ${source:X6}.");
                keyed[source] = pointer;
                operands++;
                bool ordinaryFrame = pointer >= 0x8000;
                if (ordinaryFrame)
                {
                    ushort partCount = ReadWord(rom, (banks[0] << 16) | pointer);
                    ordinaryFrame = partCount is > 0 and <= 128 &&
                        pointer + 2 + partCount * 5 <= 0x10000;
                }
                if (ordinaryFrame)
                {
                    ordinary++;
                    familyOrdinary++;
                }
                else
                {
                    special++;
                    familySpecial++;
                }
            }
            families.Add((name, familyOrdinary, familySpecial));
        }
        // Ceres Baby deliberately split its old mixed presentation list into
        // independently typed OAM and palette operands. Only the former belong
        // in this sprite-pointer catalog; the thirteen palette addresses must
        // never be interpreted as enemy spritemaps.
        discovered++;
        catalogs++;
        for (int index = 0;
             index < CeresBabyInstructionProgramDefinitions.SpritemapOperandCount;
             index++)
        {
            ushort address = CeresBabyInstructionProgramDefinitions
                .SpritemapOperandAddress(index);
            int source = (CeresBabyInstructionProgramDefinitions.Bank << 16) | address;
            ushort pointer = CeresBabyInstructionProgramDefinitions
                .ReadSpritemapOperand(address);
            if (ReadWord(rom, source) != pointer)
                throw new InvalidDataException(
                    $"Ceres Baby sprite selector ${source:X6} does not match its typed owner.");
            int frameAddress =
                (CeresBabyInstructionProgramDefinitions.Bank << 16) | pointer;
            ushort partCount = ReadWord(rom, frameAddress);
            if (partCount is not (> 0 and <= 128) || pointer + 2 + partCount * 5 > 0x10000)
                throw new InvalidDataException(
                    $"Ceres Baby selector ${source:X6} does not target an ordinary OAM frame.");
            if (!keyed.TryAdd(source, pointer))
                throw new InvalidDataException(
                    $"Ceres Baby sprite selector ${source:X6} overlaps another owner.");
            operands++;
            ordinary++;
        }
        families.Add((nameof(CeresBabyInstructionProgramDefinitions),
            CeresBabyInstructionProgramDefinitions.SpritemapOperandCount, 0));
        return new(discovered, catalogs, operands, ordinary, special, keyed, families, unresolved);
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) |
            bus.ReadByte((address & 0xff0000) |
                unchecked((ushort)(address + 1))) << 8);
}
