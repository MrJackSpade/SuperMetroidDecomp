namespace SuperMetroid.Desktop;

/// <summary>
/// Assembly-qualified identities of debugger graph types. A state resolves only the types this
/// build has (#1270): there are no aliases for renamed or retired types. Game/APU objects remain
/// in Core; never load a Windows host assembly on Android merely to resolve this envelope.
/// </summary>
internal static class DebuggerStateTypeIdentity
{
    public static Type? Resolve(string name) => Type.GetType(name, throwOnError: false);

    // A type's serialized name is fixed for the process; the writer asks for it for every
    // object and field, and building AssemblyQualifiedName allocates a new string each time.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, string> serializedNames = new();

    internal static string GetSerializedName(Type type) =>
        serializedNames.GetOrAdd(type, CalculateSerializedName);

    private static string CalculateSerializedName(Type type)
    {
        return type.AssemblyQualifiedName ?? throw new InvalidOperationException(
            $"Type {type.FullName} has no assembly-qualified name.");
    }
}
