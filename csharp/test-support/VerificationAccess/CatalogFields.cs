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

    private static Type? Companion(Type catalog) =>
        typeof(CatalogFields).Assembly.GetType(FlatName(catalog) + "Constants");

    private static string FlatName(Type type) =>
        type.DeclaringType is { } outer ? FlatName(outer) + type.Name : type.Name;
}
