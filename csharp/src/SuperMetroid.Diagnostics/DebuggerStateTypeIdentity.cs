namespace SuperMetroid.Desktop;

/// <summary>
/// Versioned identities and narrow migrations for supported debugger graph types.
/// Game/APU objects remain in Core. Never load a Windows host assembly on Android merely
/// to resolve this envelope, and do not broadly alias arbitrary desktop object types.
/// </summary>
internal static class DebuggerStateTypeIdentity
{
    // Version this logical identity if captures or callback semantics change. Compiler
    // ordinals are deliberately excluded; unrelated runtime members renumber them.
    /// <summary>Stable identity used for supported room-load callbacks in saved graphs.</summary>
    internal const string RoomLoadCallbacksIdentity = "SuperMetroid.DebugState.RoomLoadCallbacks.v1, SuperMetroid.Core";
    /// <summary>Legacy nested debugger root name retained for old save-state captures.</summary>
    private const string LegacyRootName = "SuperMetroid.Desktop.DebuggerSaveStateStore+DebuggerSaveStateRoot";
    /// <summary>Assembly identity that accompanied the historical debugger root.</summary>
    private const string LegacyAssemblyName = "SuperMetroid.Desktop";

    /// <summary>Resolves a serialized type identity, including the narrowly supported historical aliases.</summary>
    /// <param name="name">Assembly-qualified identity read from a debugger-state graph.</param>
    /// <returns>The resolved runtime type, or <see langword="null"/> when the identity is unknown.</returns>
    public static Type? Resolve(string name)
    {
        string[] identity = name.Split(',', 3, StringSplitOptions.TrimEntries);
        if (identity.Length >= 2 && identity[1] == "SuperMetroid.Core" &&
            identity[0] == "SuperMetroid.Core.Rendering.XrayGameplayRenderLayer")
        {
            // This source-only rename leaves every field and the portable layer-16
            // envelope unchanged. Resolve both the object and its declaring-field
            // identity so historical graphs retain exact composition data.
            return typeof(SuperMetroid.Core.Rendering.GameplayColorMathRenderLayer);
        }
        if (identity.Length >= 2 && identity[0] == LegacyRootName && identity[1] == LegacyAssemblyName)
            return typeof(DebuggerSaveStateStore).Assembly.GetType(LegacyRootName, throwOnError: true);
        if (name == RoomLoadCallbacksIdentity ||
            identity.Length >= 2 && identity[1] == "SuperMetroid.Core" &&
            identity[0] is "SuperMetroid.Core.Runtime.SuperMetroidRuntime+<>c__DisplayClass440_0" or
                "SuperMetroid.Core.Runtime.SuperMetroidRuntime+<>c__DisplayClass443_0" or
                "SuperMetroid.Core.Runtime.SuperMetroidRuntime+<>c__DisplayClass461_0" or
                "SuperMetroid.Core.Runtime.SuperMetroidRuntime+<>c__DisplayClass463_0")
        {
            // #350, #391, #606, and v0.3.1 #609 captures retain the same room/runtime fields and named
            // callback signatures. Their serialized fields/signatures are still checked
            // individually by the reader. Do not alias unknown legacy closures.
            var matches = typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime)
                .GetNestedTypes(System.Reflection.BindingFlags.NonPublic)
                .Where(type => type.GetMethod("<LoadCartridgeRoom>b__0",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) is not null)
                .Where(type => type.GetFields(System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                    .Select(field => field.Name).SequenceEqual(new[] { "<>4__this", "room" })).ToArray();
            if (matches.Length != 1)
                throw new InvalidDataException("Legacy room-load callback has no unique verified capture layout.");
            return matches[0];
        }
        return Type.GetType(name, throwOnError: false);
    }

    // A type's serialized name is fixed for the process; the writer asks for it for every
    // object and field, and building AssemblyQualifiedName allocates a new string each time.
    /// <summary>Caches stable serialized names because graph writers request them repeatedly.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, string> serializedNames = new();

    /// <summary>Gets the saved-graph identity for a runtime type.</summary>
    /// <param name="type">Type whose serialized identity is needed.</param>
    /// <returns>Stable logical identity or its assembly-qualified name.</returns>
    internal static string GetSerializedName(Type type) =>
        serializedNames.GetOrAdd(type, CalculateSerializedName);

    /// <summary>Builds and validates the serialized identity for a runtime type.</summary>
    /// <param name="type">Type to name.</param>
    /// <returns>The versioned callback identity or assembly-qualified name.</returns>
    private static string CalculateSerializedName(Type type)
    {
        if (type.DeclaringType == typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime) &&
            type.GetMethod("<LoadCartridgeRoom>b__0",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) is not null)
        {
            if (Resolve(RoomLoadCallbacksIdentity) != type)
                throw new InvalidDataException("Room-load callback does not match its versioned state identity.");
            return RoomLoadCallbacksIdentity;
        }
        return type.AssemblyQualifiedName ?? throw new InvalidOperationException(
            $"Type {type.FullName} has no assembly-qualified name.");
    }
}
