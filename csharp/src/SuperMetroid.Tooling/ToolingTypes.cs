namespace SuperMetroid.Tooling;

/// <summary>
/// Reflection over a shipped type for development tools. Members only tools read live in the
/// type's adapter, <c>&lt;Owner&gt;Tooling</c> in the owner's namespace (nested owner names
/// flattened), so a complete scan covers both.
/// </summary>
internal static class ToolingTypes
{
    /// <summary>The shipped type followed by its development adapter, when it has one.</summary>
    internal static IEnumerable<Type> WithAdapter(Type owner)
    {
        yield return owner;
        if (Adapter(owner) is { } adapter)
            yield return adapter;
    }

    /// <summary>The development adapter of a shipped type, or null when nothing of it moved.</summary>
    internal static Type? Adapter(Type owner) =>
        typeof(ToolingTypes).Assembly.GetType($"{owner.Namespace}.{FlatName(owner)}Tooling");

    /// <summary>Builds the adapter lookup name by concatenating enclosing type names from outermost to innermost.</summary>
    /// <param name="type">Shipped type whose adapter-name stem is required.</param>
    private static string FlatName(Type type) =>
        type.DeclaringType is { } outer ? FlatName(outer) + type.Name : type.Name;
}
