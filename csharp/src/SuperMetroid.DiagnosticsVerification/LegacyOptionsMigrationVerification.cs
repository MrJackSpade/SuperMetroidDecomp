using System.Reflection;
using System.Runtime.CompilerServices;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Desktop;

internal static class LegacyOptionsMigrationVerification
{
    public static int Run()
    {
        VerifyRuntimeMigration();
        VerifyCameraMigration();
        string legacyCallback = "SuperMetroid.Core.Runtime.SuperMetroidRuntime+<>c__DisplayClass443_0, SuperMetroid.Core";
        var callback = DebuggerStateTypeIdentity.Resolve(legacyCallback)
            ?? throw new InvalidDataException("Verified legacy room callback did not resolve.");
        if (callback.GetMethod("<LoadCartridgeRoom>b__0", BindingFlags.Instance | BindingFlags.NonPublic)!
            .ReturnType != typeof(SuperMetroid.Core.Game.SamusState))
            throw new InvalidDataException("Legacy room callback no longer resolves the Samus getter.");
        if (DebuggerStateTypeIdentity.Resolve(legacyCallback.Replace("443_0", "999999_0")) is not null)
            throw new InvalidDataException("Unknown compiler closure was aliased speculatively.");
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

    private static void VerifyRuntimeMigration()
    {
        var type = typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime);
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(field => !field.IsDefined(typeof(NonSerializedAttribute), false)).ToArray();
        var selected = DebuggerStateFieldMigrations.SelectSerializedFields(type, fields, 106);
        string[] missing = ["_tourianStatues", "_escapeDiagonalFrames",
            "<PreventEscapeTimeout>k__BackingField", "<RoomTreadmills>k__BackingField"];
        if (selected.Length != 106 || !selected.SequenceEqual(fields.Where(field => !missing.Contains(field.Name))))
            throw new InvalidDataException("Legacy runtime migration omitted or reordered unexpected fields.");
        var restored = (SuperMetroid.Core.Runtime.SuperMetroidRuntime)RuntimeHelpers.GetUninitializedObject(type);
        DebuggerStateFieldMigrations.InitializeMissingFields(restored, 106);
        if (restored.RoomTreadmills is null || restored.PreventEscapeTimeout || restored.TourianStatues is null)
            throw new InvalidDataException("Legacy runtime owners or neutral timeout default were not restored.");
        var current = new object();
        DebuggerStateFieldMigrations.InitializeMissingFields(current, 106);
        Console.WriteLine("Legacy runtime: four documented additions only, original field order retained, omitted owners initialized.");
    }

    private static void VerifyCameraMigration()
    {
        var type = typeof(SuperMetroid.Core.Game.ScrollBoundaryCamera);
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var selected = DebuggerStateFieldMigrations.SelectSerializedFields(type, fields, 11);
        if (selected.Length != 11 || !selected.SequenceEqual(fields.Where(field =>
            field.Name != "<PreviousSamusPoint>k__BackingField")))
            throw new InvalidDataException("Legacy camera migration changed fields beyond the new checkpoint.");
        // This isolated graph test deliberately has no scroll grid; no camera movement
        // is invoked. It tests preservation of all four checkpoint words, including fractions.
        var camera = (SuperMetroid.Core.Game.ScrollBoundaryCamera)RuntimeHelpers.GetUninitializedObject(type);
        if (camera.PreviousSamusPoint is not null)
            throw new InvalidDataException("Legacy camera should start without a fabricated checkpoint.");
        camera.FinishSamusScrolling(new(128, 12345, 1900, 54321));
        using var bytes = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(bytes, camera);
        bytes.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<SuperMetroid.Core.Game.ScrollBoundaryCamera>(bytes);
        if (restored.PreviousSamusPoint != camera.PreviousSamusPoint)
            throw new InvalidDataException("Camera checkpoint lost whole or fractional words on state restore.");
        Console.WriteLine("Camera checkpoint: explicit legacy omission and exact four-word graph round trip pass.");
    }
}
