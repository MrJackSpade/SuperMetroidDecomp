namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// Assembly-qualified identities of debugger graph types. A state resolves only the types this
/// build has (#1270): there are no aliases for renamed or retired types. Game/APU objects remain
/// in Core; never load a Windows host assembly on Android merely to resolve this envelope.
/// </summary>
internal static class DebuggerStateTypeIdentity
{
    /// <summary>Resolves an assembly-qualified name only when its type exists in the current build.</summary>
    public static Type? Resolve(string name) => Type.GetType(name, throwOnError: false);

    /// <summary>Caches serialized type names because rebuilding an assembly-qualified name allocates for every graph object and field.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, string> serializedNames = new();

    /// <summary>Reuses the assembly-qualified name cached for a debugger-graph type.</summary>
    internal static string GetSerializedName(Type type) =>
        serializedNames.GetOrAdd(type, CalculateSerializedName);

    /// <summary>Requires an assembly-qualified identity before a type enters the debugger envelope.</summary>
    private static string CalculateSerializedName(Type type)
    {
        return type.AssemblyQualifiedName ?? throw new InvalidOperationException(
            $"Type {type.FullName} has no assembly-qualified name.");
    }
}
