using System.Reflection;

/// <summary>
/// Reflection over a cartridge catalog for coverage checks. Catalog values production never reads
/// live in a test-support companion named after the catalog with a <c>Constants</c> suffix
/// (nested names flattened), and values only tools read live in the catalog's development
/// adapter, so a complete scan covers all three.
/// </summary>
internal static class CatalogFields
{
    /// <summary>The catalog's static fields matching <paramref name="flags"/>, its adapter's, then its companion's.</summary>
    internal static FieldInfo[] Of(Type catalog, BindingFlags flags) =>
        [.. SuperMetroid.Tooling.ToolingTypes.WithAdapter(catalog).SelectMany(type => type.GetFields(flags)),
         .. Companion(catalog)?.GetFields(flags) ?? []];

    /// <summary>Finds the test-support constants type named for a catalog, using the flattened nested-type naming convention.</summary>
    /// <param name="catalog">Catalog whose companion type is sought in this assembly.</param>
    /// <returns>The matching companion type, or <see langword="null"/> when the assembly defines none.</returns>
    private static Type? Companion(Type catalog) =>
        typeof(CatalogFields).Assembly.GetType(FlatName(catalog) + "Constants");

    /// <summary>Builds the companion naming key by concatenating nested type names from outermost to innermost.</summary>
    /// <param name="type">Catalog or nested catalog type to flatten.</param>
    /// <returns>The type's simple name, prefixed by each declaring type's simple name when nested.</returns>
    private static string FlatName(Type type) =>
        type.DeclaringType is { } outer ? FlatName(outer) + type.Name : type.Name;
}
