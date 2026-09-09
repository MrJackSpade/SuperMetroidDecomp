using System.Reflection;
using System.Runtime.CompilerServices;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Desktop;

internal static class LegacyOptionsMigrationVerification
{
    public static int Run()
    {
        var type = typeof(SuperMetroidGameOptions);
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var selected = DebuggerStateFieldMigrations.SelectSerializedFields(type, fields, 9);
        string[] omitted = fields.Except(selected).Select(field => field.Name).Order().ToArray();
        if (selected.Length != 9 || !omitted.SequenceEqual(new[]
            { "<EndingTimeOverrideMinutes>k__BackingField", "<PreventEscapeTimeout>k__BackingField" }))
            throw new InvalidDataException("Legacy option migration omitted fields other than the two verified additions.");
        if (!selected.SequenceEqual(fields.Where(field => !omitted.Contains(field.Name))))
            throw new InvalidDataException("Legacy migration reordered surviving fields.");
        var restored = (SuperMetroidGameOptions)RuntimeHelpers.GetUninitializedObject(type);
        if (restored.PreventEscapeTimeout || restored.EndingTimeOverrideMinutes is not null)
            throw new InvalidDataException("Omitted testing options should remain disabled.");
        if (!ReferenceEquals(fields, DebuggerStateFieldMigrations.SelectSerializedFields(type, fields, fields.Length)))
            throw new InvalidDataException("Current option schema should remain unchanged.");
        try { DebuggerStateFieldMigrations.SelectSerializedFields(type, fields, 8); }
        catch (InvalidDataException)
        {
            Console.WriteLine("Legacy options: only two verified additions omitted, surviving order retained, disabled defaults, current schema unchanged, unknown schema rejected.");
            return 0;
        }
        throw new InvalidDataException("Unknown option schema was silently accepted.");
    }
}
