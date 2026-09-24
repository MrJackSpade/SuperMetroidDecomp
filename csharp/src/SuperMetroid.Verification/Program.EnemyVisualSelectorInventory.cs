using System.Reflection;
using System.Text;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Cartridge-backed development inventory, never a production ROM fallback.
    /// Flags plausible five-byte OAM records, not a rendering proof: an extended
    /// or BG2 command payload may coincidentally pass the shallow count check.
    /// Every family still needs a consumer and exact OAM parity test before extraction.
    /// </summary>
    private static void InspectEnemyVisualSelectors(bool generateCatalog = false)
    {
        var rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
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
            MethodInfo? addressMethod = type.GetMethod(
                "PresentationWordAddress", staticFlags);
            MethodInfo? mechanicsMethod = type.GetMethod("MechanicsWord", staticFlags);
            MethodInfo? checkMethod = type.GetMethod(
                "IsCompiledMechanicsByte", staticFlags);
            if (addressMethod is null)
                continue;
            discovered++;
            if (countProperty is null)
            {
                unresolved.Add($"{type.Name}: no presentation-word count");
                continue;
            }
            catalogs++;
            if (mechanicsMethod is null || checkMethod is null)
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
            byte[] banks = Enumerable.Range(0x80, 0x60)
                .Where(bank => (bool)checkMethod.Invoke(null,
                    [(bank << 16) | firstAddress])!)
                .Select(bank => (byte)bank).ToArray();
            if (banks.Length != 1)
            {
                unresolved.Add($"{type.Name}: {banks.Length} matching banks");
                continue;
            }
            int count = Convert.ToInt32(countProperty.GetValue(null));
            int familyOrdinary = 0;
            int familySpecial = 0;
            for (int index = 0; index < count; index++)
            {
                ushort address = Convert.ToUInt16(
                    addressMethod.Invoke(null, [index]));
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
        Console.WriteLine(
            $"Discovered={discovered}, catalogs={catalogs}, " +
            $"mapped families={families.Count}, " +
            $"operands={operands}, unique addresses={keyed.Count}, " +
            $"plausible OAM={ordinary}, nonstandard={special}.");
        AssertEqual(138, discovered, "instruction catalogs with visual operands");
        AssertEqual(4162, operands, "counted native visual-operand occurrences");
        AssertEqual(4161, keyed.Count, "distinct native visual-operand addresses");
        if (generateCatalog)
            GenerateCompiledEnemyVisualSelectorCatalog(keyed);
        else
        {
            AssertEqual(keyed.Count, CompiledEnemyVisualSelectors.Count,
                "generated catalog covers every bank-resolved inventory address");
            int index = 0;
            foreach ((int address, ushort pointer) in keyed.OrderBy(pair => pair.Key))
            {
                CompiledEnemyVisualSelector entry =
                    CompiledEnemyVisualSelectors.At(index++);
                AssertEqual(address, entry.Address,
                    "generated visual selector address matches catalog inventory");
                AssertEqual(pointer, entry.Pointer,
                    "generated visual selector target matches catalog inventory");
            }
        }
        foreach ((string name, int frameCount, int specialCount) in families
                     .OrderByDescending(family => family.Ordinary)
                     .ThenBy(family => family.Name, StringComparer.Ordinal))
            Console.WriteLine($"{name}: ordinary={frameCount}, special={specialCount}");
        foreach (string item in unresolved)
            Console.WriteLine($"UNRESOLVED {item}");

        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) |
                bus.ReadByte((address & 0xff0000) |
                    unchecked((ushort)(address + 1))) << 8);
    }

    /// <summary>Checks every checked-in selector directly against the pinned cartridge.</summary>
    private static void VerifyCompiledEnemyVisualSelectors()
    {
        var rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(4161, CompiledEnemyVisualSelectors.Count,
            "generated fixed visual-selector count");
        int previousAddress = -1;
        for (int index = 0; index < CompiledEnemyVisualSelectors.Count; index++)
        {
            CompiledEnemyVisualSelector entry = CompiledEnemyVisualSelectors.At(index);
            AssertTrue(entry.Address > previousAddress,
                "generated visual selectors are distinct and sorted");
            previousAddress = entry.Address;
            ushort native = (ushort)(rom.ReadByte(entry.Address) |
                rom.ReadByte((entry.Address & 0xff0000) |
                    unchecked((ushort)(entry.Address + 1))) << 8);
            AssertEqual(native, entry.Pointer,
                $"compiled visual selector ${entry.Address:X6}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(
                    unchecked((byte)(entry.Address >> 16)),
                    unchecked((ushort)entry.Address), out ushort selected),
                $"compiled visual selector lookup ${entry.Address:X6}");
            AssertEqual(native, selected,
                $"compiled visual selector lookup target ${entry.Address:X6}");
        }
        AssertTrue(!CompiledEnemyVisualSelectors.TryGet(0x80, 0x8000, out _),
            "uncatalogued visual selector is not invented");
        Console.WriteLine(
            "Compiled enemy visuals: 4,161 distinct fixed selectors match the cartridge; " +
            "sorted lookup and unknown-key rejection pass.");
    }

    /// <summary>
    /// Development-only mechanical source generation. The checked-in result contains
    /// only sparse, fixed pointer selectors, not an executable ROM or hidden raw bank.
    /// </summary>
    private static void GenerateCompiledEnemyVisualSelectorCatalog(
        Dictionary<int, ushort> selectors)
    {
        const string outputPath =
            "csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectorDefinitions.cs";
        IGrouping<int, KeyValuePair<int, ushort>>[] banks = selectors
            .GroupBy(pair => pair.Key >> 16)
            .OrderBy(group => group.Key)
            .ToArray();
        foreach (IGrouping<int, KeyValuePair<int, ushort>> bank in banks)
        {
            var bankSource = new StringBuilder(bank.Count() * 40);
            bankSource.AppendLine("// Generated from the pinned retail cartridge by --generate-enemy-visual-selectors.");
            bankSource.AppendLine("namespace SuperMetroid.Core.Assets;");
            bankSource.AppendLine();
            bankSource.AppendLine("internal static partial class CompiledEnemyVisualSelectors");
            bankSource.AppendLine("{");
            bankSource.AppendLine($"    private static CompiledEnemyVisualSelector[] Bank{bank.Key:X2} =>");
            bankSource.AppendLine("    [");
            foreach ((int address, ushort pointer) in bank.OrderBy(pair => pair.Key))
                bankSource.AppendLine($"        new(0x{address:X6}, 0x{pointer:X4}),");
            bankSource.AppendLine("    ];");
            bankSource.AppendLine("}");
            string bankPath =
                $"csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.Bank{bank.Key:X2}.Definitions.cs";
            File.WriteAllText(bankPath, bankSource.ToString(), new UTF8Encoding(false));
        }

        var source = new StringBuilder(2500);
        source.AppendLine("// Generated from the pinned retail cartridge by --generate-enemy-visual-selectors.");
        source.AppendLine("// Edit the producer catalog and regenerate; do not hand-edit individual entries.");
        source.AppendLine("namespace SuperMetroid.Core.Assets;");
        source.AppendLine();
        source.AppendLine("/// <summary>One immutable cartridge visual-pointer operand and its selected target.</summary>");
        source.AppendLine("internal readonly record struct CompiledEnemyVisualSelector(int Address, ushort Pointer);");
        source.AppendLine();
        source.AppendLine("/// <summary>");
        source.AppendLine("/// Sparse fixed visual selectors from compiled instruction catalogs. These are");
        source.AppendLine("/// engine definitions, not editable art, callback code, or a reconstructed ROM.");
        source.AppendLine("/// A selected target still needs its own renderer and presentation asset.");
        source.AppendLine("/// </summary>");
        source.AppendLine("internal static partial class CompiledEnemyVisualSelectors");
        source.AppendLine("{");
        source.AppendLine("    private static readonly CompiledEnemyVisualSelector[] Entries =");
        source.AppendLine("    [");
        foreach (IGrouping<int, KeyValuePair<int, ushort>> bank in banks)
            source.AppendLine($"        .. Bank{bank.Key:X2},");
        source.AppendLine("    ];");
        source.AppendLine();
        source.AppendLine("    internal static int Count => Entries.Length;");
        source.AppendLine("    internal static CompiledEnemyVisualSelector At(int index) => Entries[index];");
        source.AppendLine();
        source.AppendLine("    internal static bool TryGet(byte bank, ushort operandAddress, out ushort pointer)");
        source.AppendLine("    {");
        source.AppendLine("        int key = (bank << 16) | operandAddress;");
        source.AppendLine("        int low = 0;");
        source.AppendLine("        int high = Entries.Length - 1;");
        source.AppendLine("        while (low <= high)");
        source.AppendLine("        {");
        source.AppendLine("            int middle = low + ((high - low) >> 1);");
        source.AppendLine("            CompiledEnemyVisualSelector entry = Entries[middle];");
        source.AppendLine("            if (entry.Address == key)");
        source.AppendLine("            {");
        source.AppendLine("                pointer = entry.Pointer;");
        source.AppendLine("                return true;");
        source.AppendLine("            }");
        source.AppendLine("            if (entry.Address < key)");
        source.AppendLine("                low = middle + 1;");
        source.AppendLine("            else");
        source.AppendLine("                high = middle - 1;");
        source.AppendLine("        }");
        source.AppendLine("        pointer = 0;");
        source.AppendLine("        return false;");
        source.AppendLine("    }");
        source.AppendLine("}");
        File.WriteAllText(outputPath, source.ToString(), new UTF8Encoding(false));
        Console.WriteLine(
            $"Generated {selectors.Count} fixed visual selectors in {banks.Length} bank files and {outputPath}.");
    }
}
