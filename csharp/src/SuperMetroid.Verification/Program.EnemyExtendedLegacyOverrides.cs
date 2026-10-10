using System.Reflection;
using SuperMetroid.Core.Assets;

internal static partial class Program
{
    /// <summary>Verifies declared historical component/binding schemas with authored JSON only.</summary>
    private static void VerifyEnemyExtendedLegacyOverrides()
    {
        var fixture = new EnemyIdentityFixture();
        EnemyExtendedFrameDefinition[] definitions = EnemyExtendedFrameDefinitions.Frames.ToArray();
        EnemyExtendedFrameDocument stockDocument = fixture.ExtendedDocument();
        EnemyExtendedFrameDefinition native = definitions[0], selected = definitions[1], inherited = definitions[2];
        stockDocument.DisplayFrames![native.Name] = inherited.Name;
        stockDocument.Frames[inherited.Name][0] = stockDocument.Frames[inherited.Name][0] with { OffsetX = 19 };
        EnemyExtendedFrameCatalog stock = Load(stockDocument);
        string stockIdentity = stock.ContentIdentity;
        (int Version, int Count)[] schemas = ExtendedEnemySchemaFixtures();
        AssertEqual(EnemyExtendedFrameDefinitions.Version, schemas.Length, "every extended enemy schema has a fixture");
        int bindingSchemas = 0;
        for (int index = 0; index < schemas.Length; index++)
        {
            (int version, int count) = schemas[index];
            AssertEqual(EnemyExtendedFrameDefinitions.FirstVersion + index, version, "extended schema fixtures are contiguous");
            var expectedNames = definitions[..count].ToDictionary(frame => frame.Name, frame => AuthoredName(frame, version));
            bool hasBindings = version > EnemyExtendedFrameDefinitions.PreDisplayBindingsVersion;
            EnemyExtendedFrameDocument document = stockDocument with
            {
                Version = version,
                Frames = definitions[..count].ToDictionary(frame => expectedNames[frame.Name],
                    frame => stockDocument.Frames[frame.Name].ToArray(), StringComparer.Ordinal),
                DisplayFrames = hasBindings ? expectedNames.Values.ToDictionary(name => name, name => name, StringComparer.Ordinal) : null,
            };
            document.Frames[native.Name][0] = document.Frames[native.Name][0] with { OffsetX = -11, OffsetY = 6 };
            document.Frames[selected.Name][0] = document.Frames[selected.Name][0] with { OffsetX = 23, OffsetY = -9 };
            // Change the last authored frame as well, exercising schema-six's old
            // Spore Spawn name and each later generation's newly added family.
            string lastName = expectedNames[definitions[count - 1].Name];
            document.Frames[lastName][0] = document.Frames[lastName][0] with { OffsetY = 17 };
            if (hasBindings) document.DisplayFrames![native.Name] = selected.Name;
            EnemyExtendedFrameCatalog merged = Load(document, stock);
            string prefix = $"extended enemy schema {version}: ";
            foreach (EnemyExtendedFrameDefinition frame in definitions)
            {
                string key = expectedNames.GetValueOrDefault(frame.Name) ?? frame.Name;
                EnemyExtendedVisualComponent[] art = document.Frames.GetValueOrDefault(key) ?? stockDocument.Frames[frame.Name];
                AssertTrue(merged.TryGet(frame.Bank, frame.Pointer, out var actualNative), prefix + frame.Name + " retains native identity");
                AssertComponents(art, actualNative.Span, prefix + frame.Name + " authored/inherited art");
                string displayedName = document.DisplayFrames?.GetValueOrDefault(key) ?? stockDocument.DisplayFrames![frame.Name];
                EnemyExtendedVisualComponent[] display = document.Frames.GetValueOrDefault(displayedName) ?? stockDocument.Frames[displayedName];
                AssertTrue(merged.TryGetDisplay(frame.Bank, frame.Pointer, out var actualDisplay), prefix + frame.Name + " resolves display identity");
                AssertComponents(display, actualDisplay.Span, prefix + frame.Name + " exact display selection");
            }
            AssertTrue(merged.TryGetDisplay(native.Bank, native.Pointer, out var remapped), prefix + "explicit remap exists");
            AssertEqual((short)(hasBindings ? 23 : 19), remapped.Span[0].OffsetX, prefix + "exact remap or inherited stock binding");
            AssertTrue(stockIdentity != merged.ContentIdentity, prefix + "edits change content identity");
            AssertEqual(merged.ContentIdentity, Load(document, stock).ContentIdentity, prefix + "reload preserves identity");
            AssertEqual(stockIdentity, stock.ContentIdentity, prefix + "override does not mutate stock");
            if (version != EnemyExtendedFrameDefinitions.Version)
                AssertThrows<InvalidDataException>(() => Load(document), prefix + "legacy data requires current stock");
            if (!hasBindings) continue;
            bindingSchemas++;
            AssertThrows<InvalidDataException>(() => Load(document with { DisplayFrames = null }, stock), prefix + "missing bindings reject");
            var missing = new Dictionary<string, string>(document.DisplayFrames!, StringComparer.Ordinal);
            missing.Remove(native.Name);
            AssertThrows<InvalidDataException>(() => Load(document with { DisplayFrames = missing }, stock), prefix + "incomplete bindings reject");
            var wrongKey = new Dictionary<string, string>(missing, StringComparer.Ordinal) { ["unknown-native"] = selected.Name };
            AssertThrows<InvalidDataException>(() => Load(document with { DisplayFrames = wrongKey }, stock), prefix + "same-count wrong keys reject");
            var unknown = new Dictionary<string, string>(document.DisplayFrames!, StringComparer.Ordinal) { [native.Name] = "unknown-frame" };
            AssertThrows<InvalidDataException>(() => Load(document with { DisplayFrames = unknown }, stock), prefix + "unknown targets reject");
            var crossFamily = new Dictionary<string, string>(document.DisplayFrames!, StringComparer.Ordinal)
            {
                [native.Name] = expectedNames.First(pair => pair.Key.StartsWith("wall_pirate_", StringComparison.Ordinal)).Value,
            };
            AssertThrows<InvalidDataException>(() => Load(document with { DisplayFrames = crossFamily }, stock), prefix + "cross-family targets reject");
        }
        AssertThrows<InvalidDataException>(() => Load(stockDocument with { Version = EnemyExtendedFrameDefinitions.FirstVersion - 1 }, stock),
            "extended enemy rejects obsolete unknown versions");
        AssertThrows<InvalidDataException>(() => Load(stockDocument with { Version = EnemyExtendedFrameDefinitions.Version + 1 }, stock),
            "extended enemy rejects future unknown versions");
        Console.WriteLine($"PASS extended enemy overrides: all {schemas.Length} schemas and {bindingSchemas} binding schemas; " +
            "exact component/part order, legacy Spore names, remaps, stock inheritance and invalid bindings. No ROM or gameplay probes.");
        return;

        EnemyExtendedFrameCatalog Load(EnemyExtendedFrameDocument value, EnemyExtendedFrameCatalog? baseline = null)
        {
            using var json = fixture.Json(value);
            return EnemyExtendedFrameCatalog.Load(json, baseline);
        }
        static string AuthoredName(EnemyExtendedFrameDefinition frame, int version) =>
            version == EnemyExtendedFrameDefinitions.PreSporeIdentityVersion && frame.Name.StartsWith("spore_spawn_oam_", StringComparison.Ordinal)
                ? $"draygon_oam_{frame.Pointer:X4}" : frame.Name;
        static void AssertComponents(EnemyExtendedVisualComponent[] expected, ReadOnlySpan<EnemyExtendedDrawComponent> actual, string context)
        {
            AssertEqual(expected.Length, actual.Length, context + " component count");
            for (int index = 0; index < expected.Length; index++)
            {
                AssertEqual((short)expected[index].OffsetX, actual[index].OffsetX, context + " component X");
                AssertEqual((short)expected[index].OffsetY, actual[index].OffsetY, context + " component Y");
                AssertTrue(actual[index].Parts.SequenceEqual(EnemySpritemapCatalog.CompileParts(expected[index].Parts, context)),
                    context + " exact ordered OAM parts");
            }
        }
    }

    /// <summary>Builds version-ordered fixtures pairing each declared historical extended-enemy schema with its cumulative frame count.</summary>
    /// <returns>One version/count pair for each discovered schema version.</returns>
    private static (int Version, int Count)[] ExtendedEnemySchemaFixtures()
    {
        Type catalog = typeof(EnemyExtendedFrameDefinitions);
        const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
        return catalog.GetFields(flags)
            .Where(field => field.IsLiteral && field.FieldType == typeof(int) && field.Name.EndsWith("Version", StringComparison.Ordinal))
            .Select(field => (Version: (int)field.GetRawConstantValue()!, Count: field.Name switch
            {
                nameof(EnemyExtendedFrameDefinitions.Version) => EnemyExtendedFrameDefinitions.ExpectedFrameCount,
                nameof(EnemyExtendedFrameDefinitions.FirstVersion) => EnemyExtendedFrameDefinitions.WalkingFrameCount,
                nameof(EnemyExtendedFrameDefinitions.PreviousVersion) => EnemyExtendedFrameDefinitions.WalkingFrameCount + EnemyExtendedFrameDefinitions.WallFrameCount,
                nameof(EnemyExtendedFrameDefinitions.PreDisplayBindingsVersion) or nameof(EnemyExtendedFrameDefinitions.PirateDisplayBindingsVersion) => EnemyExtendedFrameDefinitions.PirateFrameCount,
                nameof(EnemyExtendedFrameDefinitions.PreSporeIdentityVersion) => EnemyExtendedFrameDefinitions.PreCeresSteamFrameCount,
                _ => (int)(catalog.GetField(field.Name[..^"Version".Length] + "FrameCount", flags)
                    ?? throw new InvalidOperationException($"Extended enemy schema {field.Name} lacks a historical count."))
                    .GetRawConstantValue()!,
            })).OrderBy(schema => schema.Version).ToArray();
    }
}
