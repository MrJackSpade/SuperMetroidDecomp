using System.Reflection;
using SuperMetroid.Desktop;

internal static partial class Program
{
    /// <summary>
    /// The legacy layout that keeps exactly <paramref name="retained"/>. Fails unless every field it
    /// drops belongs to a registered introduction that the layout omits in full.
    /// </summary>
    private static FieldInfo[] LegacyLayout(Type type, FieldInfo[] current, IEnumerable<FieldInfo> retained)
    {
        var keep = retained.ToHashSet();
        return DebuggerStateFieldMigrations.WithoutIntroductions(type, current,
            current.Where(field => !keep.Contains(field)).Select(field => field.Name).ToArray());
    }

    /// <summary>Restores a legacy instance that omits exactly these registered fields.</summary>
    private static void RestoreLegacy(object instance, params string[] omittedFields) =>
        DebuggerStateFieldMigrations.InitializeOmitted(instance,
            DebuggerStateFieldMigrations.ResolveOmissions(instance.GetType(), omittedFields));
}
