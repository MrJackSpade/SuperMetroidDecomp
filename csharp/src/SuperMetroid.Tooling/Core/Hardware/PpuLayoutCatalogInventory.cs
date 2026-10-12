using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SuperMetroid.Core.Hardware;

/// <summary>One PPU layout catalog cell found by reflection: its declaring catalog, member and descriptor.</summary>
internal sealed record PpuLayoutCatalogEntry(Type Owner, string Member, PpuLayoutCell Cell)
{
    /// <summary>The <c>Owner.Member</c> reference callers write.</summary>
    internal string Reference => $"{Owner.Name}.{Member}";
}

/// <summary>
/// Enumerates every <see cref="PpuLayoutCell"/> declared by the production layout catalogs (#630),
/// checks the catalog rules (unique identities; storage shared only through a declared alias) and
/// renders the repository inventory <c>docs/ppu-layout-inventory.json</c>.
/// </summary>
[PpuLayoutBoundary("catalog inventory: reflects every layout cell for the repository inventory and its checks")]
internal static class PpuLayoutCatalogInventory
{
    /// <summary>The repository-relative inventory path.</summary>
    internal const string InventoryPath = "docs/ppu-layout-inventory.json";

    /// <summary>The production assemblies whose catalogs make up the PPU layout inventory.</summary>
    internal static IReadOnlyList<Assembly> ProductionAssemblies =>
    [
        typeof(PpuLayoutCell).Assembly,
        typeof(SuperMetroid.AssetExtraction.CartridgeImportAddressSpace).Assembly,
        typeof(PpuLayoutCatalogInventory).Assembly,
    ];

    /// <summary>Every static <see cref="PpuLayoutCell"/> field or property, ordered by owner then member.</summary>
    internal static IReadOnlyList<PpuLayoutCatalogEntry> Enumerate(IEnumerable<Assembly> assemblies)
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        var entries = new List<PpuLayoutCatalogEntry>();
        foreach (Assembly assembly in assemblies)
        foreach (Type type in assembly.GetTypes())
        {
            foreach (FieldInfo field in type.GetFields(flags))
                if (field.FieldType == typeof(PpuLayoutCell) && !field.Name.Contains('<'))
                    entries.Add(new(type, field.Name, (PpuLayoutCell)(field.GetValue(null) ??
                        throw new InvalidOperationException($"{type.FullName}.{field.Name} is a null PPU layout cell."))));
            foreach (PropertyInfo property in type.GetProperties(flags))
                if (property.PropertyType == typeof(PpuLayoutCell) && property.GetIndexParameters().Length == 0)
                    entries.Add(new(type, property.Name, (PpuLayoutCell)(property.GetValue(null) ??
                        throw new InvalidOperationException($"{type.FullName}.{property.Name} is a null PPU layout cell."))));
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
    /// from its member, an alias target that no catalog declares or that shares no storage with
    /// its alias, or two cells that share storage without one reaching, through its declared
    /// alias chain, a cell that covers the other.
    /// </summary>
    internal static IReadOnlyList<string> Violations(IReadOnlyList<PpuLayoutCatalogEntry> entries)
    {
        var violations = new List<string>();
        foreach (IGrouping<(string?, string), PpuLayoutCatalogEntry> group in
                 entries.GroupBy(entry => (entry.Owner.FullName, entry.Cell.Name)))
            if (group.Count() > 1)
                violations.Add($"{group.Key.Item1}.{group.Key.Item2} is declared {group.Count()} times.");
        foreach (PpuLayoutCatalogEntry entry in entries)
        {
            if (entry.Cell.Name != entry.Member)
                violations.Add($"{entry.Owner.FullName}.{entry.Member} names its cell '{entry.Cell.Name}'.");
            if (entry.Cell.AliasOf is not { } target)
                continue;
            if (Find(entries, target) is null)
                violations.Add($"{entry.Owner.FullName}.{entry.Member} aliases a cell no catalog declares.");
            if (!Overlaps(entry.Cell, target))
                violations.Add($"{entry.Owner.FullName}.{entry.Member} {entry.Cell} aliases {target}, which it does not share storage with.");
        }
        for (int first = 0; first < entries.Count; first++)
        for (int second = first + 1; second < entries.Count; second++)
        {
            PpuLayoutCell left = entries[first].Cell, right = entries[second].Cell;
            if (!Overlaps(left, right) || AliasCovers(left, right) || AliasCovers(right, left))
                continue;
            violations.Add($"{entries[first].Owner.Name}.{entries[first].Member} {left} and " +
                $"{entries[second].Owner.Name}.{entries[second].Member} {right} share storage without a declared alias.");
        }
        return violations;
    }

    /// <summary>Whether two cells reach at least one common unit of the same store.</summary>
    internal static bool Overlaps(PpuLayoutCell left, PpuLayoutCell right) =>
        left.Region == right.Region && left.Offset < right.Offset + right.Count && right.Offset < left.Offset + left.Count;

    /// <summary>The <c>$xxxx</c> form of a cell's first unit.</summary>
    internal static string FormatOffset(PpuLayoutCell cell) =>
        cell.Region == PpuLayoutRegion.Register ? $"$21{cell.Offset:X2}" : $"${cell.Offset:X4}";

    /// <summary>
    /// Renders the inventory: one record per cell with its owner, name, store, offset, count,
    /// encoding, alias and the repository source files that reference it.
    /// </summary>
    internal static string RenderJson(IReadOnlyList<PpuLayoutCatalogEntry> entries, string repositoryRoot)
    {
        (string Path, string Text)[] sources = SourceFiles(repositoryRoot)
            .Select(path => (path, File.ReadAllText(System.IO.Path.Combine(repositoryRoot, path))))
            .ToArray();
        var records = new List<object>();
        foreach (PpuLayoutCatalogEntry entry in entries)
        {
            var reference = new Regex($@"\b{Regex.Escape(entry.Owner.Name)}\s*\.\s*{Regex.Escape(entry.Member)}\b");
            string[] consumers = sources.Where(source => reference.IsMatch(source.Text)).Select(source => source.Path).ToArray();
            records.Add(new
            {
                owner = entry.Owner.FullName,
                name = entry.Member,
                region = entry.Cell.Region.ToString(),
                offset = FormatOffset(entry.Cell),
                count = entry.Cell.Count,
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

    /// <summary>Find.</summary>
    private static PpuLayoutCatalogEntry? Find(IReadOnlyList<PpuLayoutCatalogEntry> entries, PpuLayoutCell cell) =>
        entries.FirstOrDefault(entry => ReferenceEquals(entry.Cell, cell));

    /// <summary>Whether <paramref name="cell"/>'s alias chain reaches <paramref name="other"/> or a cell covering it.</summary>
    private static bool AliasCovers(PpuLayoutCell cell, PpuLayoutCell other)
    {
        for (PpuLayoutCell? target = cell.AliasOf; target is not null; target = target.AliasOf)
            if (ReferenceEquals(target, other) || Covers(target, other))
                return true;
        return false;
    }

    /// <summary>Covers.</summary>
    private static bool Covers(PpuLayoutCell outer, PpuLayoutCell inner) =>
        outer.Region == inner.Region && outer.Offset <= inner.Offset && inner.Offset + inner.Count <= outer.Offset + outer.Count;

    /// <summary>Source files.</summary>
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
