namespace SuperMetroid.Desktop;

/// <summary>
/// Narrow migration for the one host-owned object persisted in legacy state graphs.
/// Game/APU objects remain in Core. Never load a Windows host assembly on Android merely
/// to resolve this envelope, and do not broadly alias arbitrary desktop object types.
/// </summary>
internal static class DebuggerStateTypeIdentity
{
    private const string LegacyRootName = "SuperMetroid.Desktop.DebuggerSaveStateStore+DebuggerSaveStateRoot";
    private const string LegacyAssemblyName = "SuperMetroid.Desktop";

    public static Type? Resolve(string name)
    {
        string[] identity = name.Split(',', 3, StringSplitOptions.TrimEntries);
        if (identity.Length >= 2 && identity[0] == LegacyRootName && identity[1] == LegacyAssemblyName)
            return typeof(DebuggerSaveStateStore).Assembly.GetType(LegacyRootName, throwOnError: true);
        if (identity.Length >= 2 && identity[1] == "SuperMetroid.Core" &&
            identity[0] == "SuperMetroid.Core.Runtime.SuperMetroidRuntime+<>c__DisplayClass443_0")
        {
            // #391 preserves this exact old closure. The LoadCartridgeRoom lambdas
            // and captures are unchanged since b944f1b5; only the compiler ordinal
            // shifted as runtime members were added. Do not alias other closures.
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
}
