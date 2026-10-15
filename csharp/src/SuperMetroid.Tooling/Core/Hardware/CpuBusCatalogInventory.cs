using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Enumerates every <see cref="CpuBusCell"/> declared by the production catalogs (#629), checks
/// the catalog rules (unique identities; overlap only through a declared alias) and renders the
/// repository inventory <c>docs/cpu-bus-inventory.json</c>.
/// </summary>
[CpuBusBoundary("catalog inventory: reflects every catalog cell for the repository inventory and its checks")]
internal static class CpuBusCatalogInventory
{
    /// <summary>The repository-relative inventory path.</summary>
    internal const string InventoryPath = "docs/cpu-bus-inventory.json";

    /// <summary>The production assemblies whose catalogs make up the CPU-bus inventory.</summary>
    internal static IReadOnlyList<Assembly> ProductionAssemblies =>
    [
        typeof(CpuBusCell).Assembly,
        typeof(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace).Assembly,
        typeof(CpuBusCatalogInventory).Assembly,
    ];

    /// <summary>Every static <see cref="CpuBusCell"/> field or property, ordered by owner then member.</summary>
    [ReflectionAudit("inventory generation: every static CPU-bus cell declared anywhere in the production assemblies is the subject")]
    internal static IReadOnlyList<CpuBusCatalogEntry> Enumerate(IEnumerable<Assembly> assemblies)
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        var entries = new List<CpuBusCatalogEntry>();
        foreach (Assembly assembly in assemblies)
        foreach (Type type in assembly.GetTypes())
        {
            foreach (FieldInfo field in type.GetFields(flags))
                if (field.FieldType == typeof(CpuBusCell) && !field.Name.Contains('<'))
                    entries.Add(new(type, field.Name, (CpuBusCell)(field.GetValue(null) ??
                        throw new InvalidOperationException($"{type.FullName}.{field.Name} is a null CPU-bus cell."))));
            foreach (PropertyInfo property in type.GetProperties(flags))
                if (property.PropertyType == typeof(CpuBusCell) && property.GetIndexParameters().Length == 0)
                    entries.Add(new(type, property.Name, (CpuBusCell)(property.GetValue(null) ??
                        throw new InvalidOperationException($"{type.FullName}.{property.Name} is a null CPU-bus cell."))));
        }
        entries.Sort((left, right) =>
        {
            int owner = string.CompareOrdinal(left.Owner.FullName, right.Owner.FullName);
            return owner != 0 ? owner : string.CompareOrdinal(left.Member, right.Member);
        });
        return entries;
    }

    /// <summary>
    /// The catalog rule violations: a duplicated (owner, name) identity, a cell whose name differs
    /// from its member, an alias target that no catalog declares, or two cells that share bytes
    /// without one reaching, through its declared alias chain, a cell that covers the other.
    /// </summary>
    internal static IReadOnlyList<string> Violations(IReadOnlyList<CpuBusCatalogEntry> entries)
    {
        var violations = new List<string>();
        foreach (IGrouping<(string?, string), CpuBusCatalogEntry> group in
                 entries.GroupBy(entry => (entry.Owner.FullName, entry.Cell.Name)))
            if (group.Count() > 1)
                violations.Add($"{group.Key.Item1}.{group.Key.Item2} is declared {group.Count()} times.");
        foreach (CpuBusCatalogEntry entry in entries)
        {
            if (entry.Cell.Name != entry.Member)
                violations.Add($"{entry.Owner.FullName}.{entry.Member} names its cell '{entry.Cell.Name}'.");
            if (entry.Cell.AliasOf is { } target && Find(entries, target) is null)
                violations.Add($"{entry.Owner.FullName}.{entry.Member} aliases a cell no catalog declares.");
        }
        for (int first = 0; first < entries.Count; first++)
        for (int second = first + 1; second < entries.Count; second++)
        {
            CpuBusCell left = entries[first].Cell, right = entries[second].Cell;
            if (!Overlaps(left, right) || AliasCovers(left, right) || AliasCovers(right, left))
                continue;
            violations.Add($"{entries[first].Owner.Name}.{entries[first].Member} {left} and " +
                $"{entries[second].Owner.Name}.{entries[second].Member} {right} share bytes without a declared alias.");
        }
        return violations;
    }

    /// <summary>Whether two cells reach at least one common physical byte of the same store.</summary>
    internal static bool Overlaps(CpuBusCell left, CpuBusCell right)
    {
        if (left.Region != right.Region)
            return false;
        int leftStart = PhysicalStart(left), rightStart = PhysicalStart(right);
        return leftStart < rightStart + right.ByteCount && rightStart < leftStart + left.ByteCount;
    }

    /// <summary>
    /// The offset of a cell's first byte in its physical store: WRAM bank $7E/$7F or the
    /// low-window mirror of $7E, the 8-KiB SRAM image, or the CPU address of a cartridge range.
    /// </summary>
    internal static int PhysicalStart(CpuBusCell cell)
    {
        int bank = SnesAddressMath.BankNumber(cell.Address), offset = SnesAddressMath.OffsetInBank(cell.Address);
        return cell.Region switch
        {
            CpuBusRegion.WorkRam when bank is 0x7e or 0x7f => (bank - 0x7e) * 0x10000 + offset,
            CpuBusRegion.WorkRam when (bank & 0x7f) < 0x40 && offset < 0x2000 => offset,
            CpuBusRegion.WorkRam => throw new InvalidDataException($"{cell} is not a WRAM address."),
            CpuBusRegion.SaveRam => offset & 0x1fff,
            CpuBusRegion.Cartridge => cell.Address,
            _ => throw new InvalidOperationException($"Undefined {nameof(CpuBusRegion)} {(int)cell.Region}."),
        };
    }

    /// <summary>The <c>$BB:OOOO</c> form of a 24-bit CPU address.</summary>
    internal static string FormatAddress(int address) => $"${SnesAddressMath.BankNumber(address):X2}:{SnesAddressMath.OffsetInBank(address):X4}";

    /// <summary>
    /// Renders the inventory: one record per cell with its owner, name, store, address, size,
    /// encoding, alias and the repository source files that reference it.
    /// </summary>
    internal static string RenderJson(IReadOnlyList<CpuBusCatalogEntry> entries, string repositoryRoot)
    {
        string[] sources = SourceFiles(repositoryRoot);
        var records = new List<object>();
        foreach (CpuBusCatalogEntry entry in entries)
        {
            var reference = new Regex($@"\b{Regex.Escape(entry.Owner.Name)}\s*\.\s*{Regex.Escape(entry.Member)}\b");
            string[] consumers = sources
                .Where(path => reference.IsMatch(File.ReadAllText(Path.Combine(repositoryRoot, path))))
                .ToArray();
            records.Add(new
            {
                owner = entry.Owner.FullName,
                name = entry.Member,
                region = entry.Cell.Region.ToString(),
                address = FormatAddress(entry.Cell.Address),
                byteCount = entry.Cell.ByteCount,
                encoding = entry.Cell.Encoding,
                aliasOf = entry.Cell.AliasOf is { } target
                    ? (Find(entries, target) ?? throw new InvalidDataException(
                        $"{entry.Reference} aliases a cell no catalog declares.")).Reference
                    : null,
                consumers,
            });
        }
        string json = JsonSerializer.Serialize(new { cells = records }, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
        return json.ReplaceLineEndings("\n") + "\n";
    }

    /// <summary>Locates the declaration of the same cell object referenced by an alias.</summary>
    private static CpuBusCatalogEntry? Find(IReadOnlyList<CpuBusCatalogEntry> entries, CpuBusCell cell) =>
        entries.FirstOrDefault(entry => ReferenceEquals(entry.Cell, cell));

    /// <summary>Whether <paramref name="cell"/>'s alias chain reaches <paramref name="other"/> or a cell covering it.</summary>
    private static bool AliasCovers(CpuBusCell cell, CpuBusCell other)
    {
        for (CpuBusCell? target = cell.AliasOf; target is not null; target = target.AliasOf)
            if (ReferenceEquals(target, other) || Covers(target, other))
                return true;
        return false;
    }

    /// <summary>Checks whether one physical store range wholly contains another.</summary>
    private static bool Covers(CpuBusCell outer, CpuBusCell inner)
    {
        if (outer.Region != inner.Region)
            return false;
        int outerStart = PhysicalStart(outer), innerStart = PhysicalStart(inner);
        return outerStart <= innerStart && innerStart + inner.ByteCount <= outerStart + outer.ByteCount;
    }

    /// <summary>Collects repository C# paths for inventory consumer references, excluding build output.</summary>
    private static string[] SourceFiles(string repositoryRoot)
    {
        string root = Path.Combine(repositoryRoot, "csharp");
        if (!Directory.Exists(Path.Combine(root, "src")))
            throw new DirectoryNotFoundException($"'{repositoryRoot}' is not the repository root: csharp/src is missing.");
        return Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(repositoryRoot, path).Replace('\\', '/'))
            .Where(path => !path.Split('/').Any(part => part is "bin" or "obj"))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }
}
