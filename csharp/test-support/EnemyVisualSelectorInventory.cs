using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>
/// Cartridge-backed development inventory, never a production ROM fallback.
/// Flags plausible five-byte OAM records, not a rendering proof: an extended
/// or BG2 command payload may coincidentally pass the shallow count check.
/// Every family still needs a consumer and exact OAM parity test before extraction.
/// </summary>
/// <param name="Keyed">Resolved full banked operand addresses and the native spritemap pointers they contain.</param>
/// <param name="Unresolved">Catalog names skipped because their mechanics or bank ownership could not be resolved.</param>
internal sealed record EnemyVisualSelectorInventory(Dictionary<int, ushort> Keyed, List<string> Unresolved)
{
    /// <summary>
    /// Scans every Core instruction-program catalog's presentation operands against the cartridge:
    /// each bank-resolved selector address with its native target, and the catalogs skipped because
    /// their bank cannot be resolved by the standard probe.
    /// </summary>
    internal static EnemyVisualSelectorInventory Collect(ISnesAddressSpace rom)
    {
        var keyed = new Dictionary<int, ushort>();
        var unresolved = new List<string>();

        // One-frame programs declare a single operand instead of an indexed list.
        // Ignoring that shape omitted Kzan's live operand (#1166).
        foreach (var catalog in InstructionProgramCatalog.All()
                     .Where(catalog => catalog.PresentationOperands is not null || catalog.SinglePresentationOperand is not null))
        {
            string name = catalog.Type.Name;
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
            foreach (ushort address in presentation)
            {
                int source = (banks[0] << 16) | address;
                ushort pointer = ReadWord(rom, source);
                if (keyed.TryGetValue(source, out ushort previous) &&
                    previous != pointer)
                    throw new InvalidDataException(
                        $"Conflicting visual selector ${source:X6}.");
                keyed[source] = pointer;
            }
        }
        // Ceres Baby deliberately split its old mixed presentation list into
        // independently typed OAM and palette operands. Only the former belong
        // in this sprite-pointer catalog; the thirteen palette addresses must
        // never be interpreted as enemy spritemaps.
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
        }
        return new(keyed, unresolved);
    }

    /// <summary>Reads a little-endian word while keeping the high byte in the same bank as the low byte.</summary>
    /// <param name="bus">Address space containing the cartridge bytes.</param>
    /// <param name="address">Full banked address of the low byte.</param>
    /// <returns>The word formed from the low byte and the following bank-local byte.</returns>
    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) |
            bus.ReadByte((address & 0xff0000) |
                unchecked((ushort)(address + 1))) << 8);
}
