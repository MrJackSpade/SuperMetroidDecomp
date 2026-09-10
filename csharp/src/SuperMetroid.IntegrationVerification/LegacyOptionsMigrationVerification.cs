using System.Reflection;
using System.Runtime.CompilerServices;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Desktop;

internal static class LegacyOptionsMigrationVerification
{
    public static int Run()
    {
        VerifyMode7RegisterMigration();
        var subType = typeof(SuperMetroid.Core.Rendering.BgSubscreenAddRenderLayer);
        var subFields = subType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var subSelected = DebuggerStateFieldMigrations.SelectSerializedFields(subType, subFields, 6);
        if (subSelected.Length != 6 || !subSelected.SequenceEqual(subFields.Where(field => field.Name != "<VerticalScroll>k__BackingField")))
            throw new InvalidDataException("Legacy subscreen migration altered prior fields or ordering.");
        VerifyGameplayRegisterMigration();
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

    private static void VerifyMode7RegisterMigration()
    {
        var type = typeof(SuperMetroid.Core.Rendering.Mode7RenderRegisters);
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var selected = DebuggerStateFieldMigrations.SelectSerializedFields(type, fields, 9);
        if (selected.Length != 9 || !selected.SequenceEqual(fields.Where(field => field.Name != "<WrapOutsideMap>k__BackingField")))
            throw new InvalidDataException("Legacy Mode 7 migration omitted or reordered unexpected fields.");
        var restored = (SuperMetroid.Core.Rendering.Mode7RenderRegisters)RuntimeHelpers.GetUninitializedObject(type);
        if (restored.WrapOutsideMap) throw new InvalidDataException("Legacy Mode 7 wrapping must remain disabled.");
        Console.WriteLine("Legacy Mode 7 registers preserve their nine original fields and default to the former overflow policy.");
        var layerType = typeof(SuperMetroid.Core.Rendering.Mode7RenderLayer);
        var layerFields = layerType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var layerSelected = DebuggerStateFieldMigrations.SelectSerializedFields(layerType, layerFields, 2);
        if (layerSelected.Length != 2 || !layerSelected.SequenceEqual(layerFields.Where(field => field.Name != "<AddBg1Subscreen>k__BackingField")))
            throw new InvalidDataException("Legacy Mode 7 layer migration altered existing composition fields.");
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

    private static void VerifyGameplayRegisterMigration()
    {
        var type = typeof(SuperMetroid.Core.Rendering.OrdinaryGameplayRegisters);
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (int count in new[] { 11, 13 })
        {
            var selected = DebuggerStateFieldMigrations.SelectSerializedFields(type, fields, count);
            string[] omitted = count == 11
                ? ["<Bg2FirstScanline>k__BackingField", "<Bg2EndScanline>k__BackingField",
                    "<Windows>k__BackingField", "<MainScreenWindowMask>k__BackingField"]
                : ["<Windows>k__BackingField", "<MainScreenWindowMask>k__BackingField"];
            if (selected.Length != count || !selected.SequenceEqual(fields.Where(field => !omitted.Contains(field.Name))))
                throw new InvalidDataException("Legacy gameplay registers omitted or reordered unexpected fields.");
        }
        if (!ReferenceEquals(fields, DebuggerStateFieldMigrations.SelectSerializedFields(type, fields, fields.Length)))
            throw new InvalidDataException("Current gameplay register schema changed.");
        try { DebuggerStateFieldMigrations.SelectSerializedFields(type, fields, 12); }
        catch (InvalidDataException)
        {
            Console.WriteLine("Gameplay registers: both known legacy schemas preserved; unknown intermediate schema rejected.");
            return;
        }
        throw new InvalidDataException("Unknown gameplay register schema accepted.");
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

    private static void VerifyAutoJumpMigration()
    {
        var type = typeof(SuperMetroid.Core.Game.SamusState);
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        string[] added = ["<AutoJumpTimer>k__BackingField", "<PreviousDrawHeldInput>k__BackingField", "<AutoJumpInputPending>k__BackingField"];
        foreach (bool oldFireLatch in new[] { false, true })
        {
            var expected = fields.Where(f => f.Name != "_poseHistory" && !added.Contains(f.Name) &&
                (!oldFireLatch || f.Name != "<PreviousDrawNewInput>k__BackingField")).ToArray();
            var selected = DebuggerStateFieldMigrations.SelectSerializedFields(type, fields, expected.Length);
            if (!selected.SequenceEqual(expected)) throw new InvalidDataException("Samus auto-jump migration reordered old fields.");
        }
        var samus = new SuperMetroid.Core.Game.SamusState();
        foreach (bool lacksFire in new[] { false, true })
        {
            var expected = fields.Where(f => f.Name != "_poseHistory" &&
                (!lacksFire || f.Name != "<PreviousDrawNewInput>k__BackingField")).ToArray();
            if (!DebuggerStateFieldMigrations.SelectSerializedFields(type, fields, expected.Length).SequenceEqual(expected))
                throw new InvalidDataException("Samus history migration changed older field identities.");
        }
        var legacy = (SuperMetroid.Core.Game.SamusState)RuntimeHelpers.GetUninitializedObject(type);
        bool rejectedUnknownHistoryLayout = false;
        try { DebuggerStateFieldMigrations.SelectSerializedFields(type, fields, fields.Length - 3); }
        catch (InvalidDataException) { rejectedUnknownHistoryLayout = true; }
        if (!rejectedUnknownHistoryLayout)
            throw new InvalidDataException("Unknown intermediate Samus history layout was accepted.");
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
