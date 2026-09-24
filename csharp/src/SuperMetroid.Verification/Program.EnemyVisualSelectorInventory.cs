using System.Reflection;
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
    private static void InspectEnemyVisualSelectors()
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
}
