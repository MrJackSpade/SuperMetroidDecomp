namespace SuperMetroid.TestSupport.CatalogInventories;

/// <summary>
/// Marks a static partial method the compiler implements with the named members of <paramref name="owners"/> (#2602): every
/// public static property and field whose type is the method's value type, with its value, its type's name and its own
/// name, so a renderer spells a catalog's members through code the compiler writes rather than by scanning the catalog.
/// </summary>
/// <param name="owners">The catalog types whose members are listed, in order.</param>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class NamedMembersAttribute(params Type[] owners) : Attribute
{
    /// <summary>The catalog types whose members are listed, in order.</summary>
    public IReadOnlyList<Type> Owners { get; } = owners;
}
