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
        return Type.GetType(name, throwOnError: false);
    }
}
