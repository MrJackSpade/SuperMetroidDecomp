using System.Reflection;
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
        const BindingFlags staticFlags = BindingFlags.Static |
            BindingFlags.NonPublic | BindingFlags.Public;
        const BindingFlags instanceFlags = BindingFlags.Instance |
            BindingFlags.NonPublic | BindingFlags.Public;
        int catalogs = 0;
        int discovered = 0;
        int operands = 0;
        int ordinary = 0;
        int special = 0;
        var unresolved = new List<string>();
        var keyed = new Dictionary<int, ushort>();
        var families = new List<(string Name, int Ordinary, int Special)>();

        foreach (Type type in typeof(RoomEnemySystem).Assembly.GetTypes()
                     .Where(type => type.Name.EndsWith("InstructionProgramDefinitions",
                         StringComparison.Ordinal))
                     .OrderBy(type => type.Name, StringComparer.Ordinal))
        {
            PropertyInfo? countProperty = type.GetProperty(
                "PresentationWordCount", staticFlags);
            FieldInfo? countField = type.GetField("PresentationWordCount", staticFlags);
            object? countValue = countProperty?.GetValue(null) ??
                (countField is { IsLiteral: true } ? countField.GetRawConstantValue() : null);
            MethodInfo? addressMethod = type.GetMethod(
                "PresentationWordAddress", staticFlags);
            MethodInfo? mechanicsMethod = type.GetMethod("MechanicsWord", staticFlags);
            MethodInfo? checkMethod = type.GetMethod(
                "IsCompiledMechanicsByte", staticFlags);
            // One-frame programs expose a constant instead of an indexed list.
            // Ignoring that shape omitted Kzan's live operand (#1166).
            FieldInfo? singleWord = type.GetField("PresentationWord", staticFlags);
            if (addressMethod is null && singleWord is not { IsLiteral: true })
                continue;
            discovered++;
            if (addressMethod is not null && countValue is null)
            {
                unresolved.Add($"{type.Name}: no presentation-word count");
                continue;
            }
            catalogs++;
            FieldInfo? bankField = type.GetField("Bank", staticFlags);
            if (mechanicsMethod is null || (checkMethod is null && bankField is not { IsLiteral: true }))
            {
                unresolved.Add($"{type.Name}: no standard mechanics-word/bank probe");
                continue;
            }
            object firstWord = mechanicsMethod.Invoke(null, [0])!;
            PropertyInfo? addressProperty = firstWord.GetType().GetProperty(
                "Address", instanceFlags);
            if (addressProperty is null)
            {
                unresolved.Add($"{type.Name}: mechanics word has no address");
                continue;
            }
            ushort firstAddress = Convert.ToUInt16(
                addressProperty.GetValue(firstWord));
            // A declared bank also covers catalogs with sparse mechanics words
            // but no byte-level ownership probe, such as the cutscene Baby.
            int? declaredBank = bankField is { IsLiteral: true }
                ? Convert.ToInt32(bankField.GetRawConstantValue()) : null;
            if (declaredBank is > byte.MaxValue) declaredBank >>= 16;
            byte[] banks = declaredBank.HasValue
                ? [checked((byte)declaredBank.Value)]
                : Enumerable.Range(0x80, 0x60)
                    .Where(bank => (bool)checkMethod!.Invoke(null,
                        [(bank << 16) | firstAddress])!)
                    .Select(bank => (byte)bank).ToArray();
            if (banks.Length != 1)
            {
                unresolved.Add($"{type.Name}: {banks.Length} matching banks");
                continue;
            }
            int count = addressMethod is null ? 1 : Convert.ToInt32(countValue);
            int familyOrdinary = 0;
            int familySpecial = 0;
            for (int index = 0; index < count; index++)
            {
                ushort address = Convert.ToUInt16(addressMethod is null
                    ? singleWord!.GetRawConstantValue()
                    : addressMethod.Invoke(null, [index]));
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
            families.Add((type.Name, familyOrdinary, familySpecial));
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
