using System.Reflection;
using System.Runtime.CompilerServices;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Desktop;

internal static class LegacyOptionsMigrationVerification
{
    public static int Run()
    {
        VerifyRegistryMatchesCurrentTypes();
        VerifyOmissionResolution();
        VerifyRuntimeMigration();
        VerifyCameraMigration();
        VerifyAutoJumpMigration();
        string legacyCallback = "SuperMetroid.Core.Runtime.SuperMetroidRuntime+<>c__DisplayClass443_0, SuperMetroid.Core";
        var callback = DebuggerStateTypeIdentity.Resolve(legacyCallback)
            ?? throw new InvalidDataException("Verified legacy room callback did not resolve.");
        if (callback.GetMethod("<LoadCartridgeRoom>b__0", BindingFlags.Instance | BindingFlags.NonPublic)!
            .ReturnType != typeof(SuperMetroid.Core.Game.SamusState))
            throw new InvalidDataException("Legacy room callback no longer resolves the Samus getter.");
        if (DebuggerStateTypeIdentity.Resolve(legacyCallback.Replace("443_0", "999999_0")) is not null)
            throw new InvalidDataException("Unknown compiler closure was aliased speculatively.");
        Console.WriteLine("Legacy layouts: every registered introduction names current serialized fields; whole introductions resolve, partial and unregistered omissions are rejected.");
        return 0;
    }

    /// <summary>A renamed or removed field must fail here, not silently disable an old layout.</summary>
    private static void VerifyRegistryMatchesCurrentTypes()
    {
        var assembly = typeof(SuperMetroidGameOptions).Assembly;
        foreach (DebuggerFieldIntroduction introduction in DebuggerStateFieldMigrations.Introductions)
        {
            Type type = assembly.GetType(introduction.TypeName)
                ?? throw new InvalidDataException($"Registered legacy type {introduction.TypeName} no longer exists.");
            foreach (string name in introduction.Fields)
            {
                FieldInfo? field = null;
                for (Type? current = type; current is not null && field is null; current = current.BaseType)
                    field = current.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field is null || field.IsDefined(typeof(NonSerializedAttribute), false))
                    throw new InvalidDataException($"Registered legacy field {introduction.TypeName}.{name} is not a current serialized field.");
            }
        }
        var duplicates = DebuggerStateFieldMigrations.Introductions
            .SelectMany(introduction => introduction.Fields.Select(field => (introduction.TypeName, field)))
            .GroupBy(entry => entry).Where(group => group.Count() > 1).Select(group => group.Key).ToArray();
        if (duplicates.Length != 0)
            throw new InvalidDataException($"Legacy fields registered by more than one introduction: {string.Join(", ", duplicates)}.");
    }

    private static void VerifyOmissionResolution()
    {
        var type = typeof(SuperMetroidGameOptions);
        string[] timeout = ["<PreventEscapeTimeout>k__BackingField", "<EndingTimeOverrideMinutes>k__BackingField"];
        DebuggerFieldIntroduction[] omitted = DebuggerStateFieldMigrations.ResolveOmissions(type, timeout);
        if (omitted.Length != 1 || !omitted[0].Fields.SequenceEqual(timeout))
            throw new InvalidDataException("Legacy option omission did not resolve to its single verified introduction.");
        var restored = (SuperMetroidGameOptions)RuntimeHelpers.GetUninitializedObject(type);
        DebuggerStateFieldMigrations.InitializeOmitted(restored, omitted);
        if (restored.PreventEscapeTimeout || restored.EndingTimeOverrideMinutes is not null)
            throw new InvalidDataException("Omitted testing options should remain disabled.");
        if (DebuggerStateFieldMigrations.ResolveOmissions(type, []).Length != 0)
            throw new InvalidDataException("The current option layout resolved legacy omissions.");
        Reject(type, [timeout[0]], "a partially omitted introduction");
        Reject(type, ["<Invincibility>k__BackingField"], "an unregistered omission");
    }

    private static void Reject(Type type, string[] omitted, string description)
    {
        try { DebuggerStateFieldMigrations.ResolveOmissions(type, omitted); }
        catch (InvalidDataException) { return; }
        throw new InvalidDataException($"Legacy layout with {description} was accepted.");
    }

    private static void VerifyRuntimeMigration()
    {
        var type = typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime);
        string[] missing = ["_tourianStatues", "_escapeDiagonalFrames",
            "<PreventEscapeTimeout>k__BackingField", "<RoomTreadmills>k__BackingField"];
        var restored = (SuperMetroid.Core.Runtime.SuperMetroidRuntime)RuntimeHelpers.GetUninitializedObject(type);
        DebuggerStateFieldMigrations.InitializeOmitted(restored, DebuggerStateFieldMigrations.ResolveOmissions(type, missing));
        if (restored.RoomTreadmills is null || restored.PreventEscapeTimeout)
            throw new InvalidDataException("Legacy runtime owners or neutral timeout default were not restored.");
        Console.WriteLine("Legacy runtime: four documented additions resolve and omitted owners initialize.");
    }

    private static void VerifyCameraMigration()
    {
        var type = typeof(SuperMetroid.Core.Game.ScrollBoundaryCamera);
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var selected = DebuggerStateFieldMigrations.WithoutIntroductions(type, fields, "<PreviousSamusPoint>k__BackingField");
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

    private static void VerifyAutoJumpMigration()
    {
        var type = typeof(SuperMetroid.Core.Game.SamusState);
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        string[] added = ["<AutoJumpTimer>k__BackingField", "<PreviousDrawHeldInput>k__BackingField", "<AutoJumpInputPending>k__BackingField"];
        _ = DebuggerStateFieldMigrations.ResolveOmissions(type, added);
        _ = DebuggerStateFieldMigrations.ResolveOmissions(type, [.. added, "<PreviousDrawNewInput>k__BackingField", "_poseHistory"]);
        Reject(type, added[..2], "two of the three auto-jump fields");
        var samus = new SuperMetroid.Core.Game.SamusState();
        var legacy = (SuperMetroid.Core.Game.SamusState)RuntimeHelpers.GetUninitializedObject(type);
        if (legacy.PoseHistory.PreviousPose != 0 || legacy.PoseHistory.LastDifferentDirectionAndMovement != 0)
            throw new InvalidDataException("Legacy history invented unavailable pose words.");
        samus.PoseHistory.PreviousPose = 0x0019;
        samus.PoseHistory.PreviousDirectionAndMovement = 0x0308;
        samus.PoseHistory.LastDifferentPose = 0x0084;
        samus.PoseHistory.LastDifferentDirectionAndMovement = 0x1404;
        fields.Single(f => f.Name == added[0]).SetValue(samus, (ushort)8);
        fields.Single(f => f.Name == added[1]).SetValue(samus, (ushort)0x180);
        fields.Single(f => f.Name == added[2]).SetValue(samus, true);
        using var bytes = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(bytes, samus);
        bytes.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<SuperMetroid.Core.Game.SamusState>(bytes);
        if (restored.AutoJumpTimer != 8 || restored.PreviousDrawHeldInput != 0x180 || !restored.AutoJumpInputPending)
            throw new InvalidDataException("Saved state lost the pending auto-jump boundary.");
        if (restored.PoseHistory.PreviousPose != 0x0019 ||
            restored.PoseHistory.PreviousDirectionAndMovement != 0x0308 ||
            restored.PoseHistory.LastDifferentPose != 0x0084 ||
            restored.PoseHistory.LastDifferentDirectionAndMovement != 0x1404)
            throw new InvalidDataException("Saved state lost the four transition-history words.");
        Console.WriteLine("Samus auto-jump: both legacy field lists and live pending-handler graph round trip pass.");
    }
}
