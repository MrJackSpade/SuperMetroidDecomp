using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.AssetExtraction.World.Artwork;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyExtendedLegacyOverridesVerification;
using static SuperMetroid.Verification.Enemies.Projectiles.EnemyProjectileLegacyOverridesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.EnemyContentIdentityFixtureVerification;
using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Artwork;

/// <summary>Checks historical enemy OAM schema overrides preserve authored
/// frame edits, display remaps, stock inheritance, and content identities.</summary>
internal static class EnemyLegacyOverridesVerification
{
    /// <summary>
    /// Exercises every source-catalogued JSON schema through the production loader.
    /// This is an authored-data compatibility check, not a gameplay or ROM-read probe.
    /// </summary>
    internal static void VerifyEnemyLegacyOverrides()
    {
        var fixture = new EnemyIdentityFixture();
        EnemySpritemapDefinition[] definitions = EnemySpritemapDefinitions.Frames.ToArray();
        EnemySpritemapDocument stockDocument = fixture.OamDocument();
        EnemySpritemapDefinition native = definitions[0], selected = definitions[1], inherited = definitions[2];
        // Non-identity stock bindings make inheritance observable for schemas that
        // predate editable bindings. Distinct art prevents a lost remap going unnoticed.
        (stockDocument.DisplayFrames ?? throw new InvalidOperationException("Verification requires stockDocument.DisplayFrames."))[native.Name] = inherited.Name;
        stockDocument.Frames[inherited.Name] =
            [stockDocument.Frames[inherited.Name][0] with { OffsetX = 19, OffsetY = -7, TileColumn = 3 }];
        EnemySpritemapCatalog stock = Load(stockDocument);
        string stockIdentity = stock.ContentIdentity;
        ExtendedEnemySchemaFixturesResult[] schemas = EnemyOamSchemaFixtures();
        AssertEqual(EnemySpritemapSchema.Current.Revision - EnemySpritemapSchema.Legacy.Revision + 1,
            schemas.Length, "every accepted enemy OAM schema has a compatibility fixture");
        int bindingSchemas = 0;
        for (int index = 0; index < schemas.Length; index++)
        {
            (int version, int count) = schemas[index];
            AssertEqual(EnemySpritemapSchema.Legacy.Revision + index, version,
                "historical enemy OAM schema fixtures are contiguous");
            EnemySpritemapDefinition[] authored = definitions[..count];
            bool hasBindings = version > EnemySpritemapSchema.PreDisplayBindings.Revision;
            EnemySpritemapDocument document = stockDocument with
            {
                Version = version,
                Frames = authored.ToDictionary(frame => frame.Name,
                    frame => stockDocument.Frames[frame.Name].ToArray(), StringComparer.Ordinal),
                DisplayFrames = hasBindings ? authored.ToDictionary(frame => frame.Name,
                    frame => frame.Name, StringComparer.Ordinal) : null,
            };
            document.Frames[native.Name] =
                [document.Frames[native.Name][0] with { OffsetX = -11, OffsetY = 5, TileColumn = 5 }];
            document.Frames[selected.Name] =
                [document.Frames[selected.Name][0] with { OffsetX = 23, OffsetY = -9, TileColumn = 7 }];
            if (hasBindings) (document.DisplayFrames ?? throw new InvalidOperationException("Verification requires document.DisplayFrames."))[native.Name] = selected.Name;

            EnemySpritemapCatalog merged = Load(document, stock);
            string prefix = $"enemy OAM schema {version}: ";
            AssertTrue(merged.ContentIdentity != stockIdentity, prefix + "selected edits change content identity");
            AssertEqual(merged.ContentIdentity, Load(document, stock).ContentIdentity,
                prefix + "reload preserves selected content identity");
            foreach (EnemySpritemapDefinition frame in definitions)
            {
                SpriteVisualPart[] expectedNative = document.Frames.GetValueOrDefault(frame.Name)
                    ?? stockDocument.Frames[frame.Name];
                AssertTrue(merged.TryGet(frame.Bank, frame.Pointer, out var nativeParts),
                    prefix + frame.Name + " retains its native identity");
                AssertTrue(nativeParts.SequenceEqual(EnemySpritemapCompiler.CompileParts(expectedNative, frame.Name)),
                    prefix + frame.Name + " preserves authored art or inherits new stock art");

                string selectedName = document.DisplayFrames?.GetValueOrDefault(frame.Name)
                    ?? stockDocument.DisplayFrames[frame.Name];
                SpriteVisualPart[] expectedDisplay = document.Frames.GetValueOrDefault(selectedName)
                    ?? stockDocument.Frames[selectedName];
                AssertTrue(merged.TryGetDisplay(frame.Bank, frame.Pointer, out var displayed),
                    prefix + frame.Name + " resolves its presentation binding");
                AssertTrue(displayed.SequenceEqual(EnemySpritemapCompiler.CompileParts(expectedDisplay, selectedName)),
                    prefix + frame.Name + " preserves the exact selected display frame");
            }
            AssertTrue(merged.TryGetDisplay(native.Bank, native.Pointer, out var remapped),
                prefix + "the explicit remap is available");
            AssertEqual(hasBindings ? 23 : 19, remapped[0].Offset.X,
                prefix + "authored bindings take precedence; older schemas inherit stock bindings");
            AssertEqual(stockIdentity, stock.ContentIdentity, prefix + "loading an override does not mutate stock");

            if (version != EnemySpritemapSchema.Current.Revision)
                AssertThrows<InvalidDataException>(() => Load(document),
                    prefix + "a legacy override requires complete current stock");
            if (!hasBindings) continue;
            bindingSchemas++;
            AssertThrows<InvalidDataException>(() => Load(document with { DisplayFrames = null }, stock),
                prefix + "missing binding data is rejected rather than silently inherited");
            var missing = new Dictionary<string, string>((document.DisplayFrames ?? throw new InvalidOperationException("Verification requires document.DisplayFrames.")), StringComparer.Ordinal);
            missing.Remove(native.Name);
            AssertThrows<InvalidDataException>(() => Load(document with { DisplayFrames = missing }, stock),
                prefix + "incomplete bindings are rejected");
            var wrongKey = new Dictionary<string, string>(missing, StringComparer.Ordinal) { ["unknown-native"] = selected.Name };
            AssertThrows<InvalidDataException>(() => Load(document with { DisplayFrames = wrongKey }, stock),
                prefix + "correct counts cannot disguise a missing native binding");
            var unknown = new Dictionary<string, string>(document.DisplayFrames, StringComparer.Ordinal) { [native.Name] = "unknown-frame" };
            AssertThrows<InvalidDataException>(() => Load(document with { DisplayFrames = unknown }, stock),
                prefix + "unknown selected frames are rejected");
            var crossBank = new Dictionary<string, string>(document.DisplayFrames, StringComparer.Ordinal)
            {
                [native.Name] = authored.First(frame => frame.Bank != native.Bank).Name,
            };
            AssertThrows<InvalidDataException>(() => Load(document with { DisplayFrames = crossBank }, stock),
                prefix + "cross-bank remaps are rejected");
        }
        AssertThrows<InvalidDataException>(() => Load(stockDocument with { Version = EnemySpritemapSchema.Legacy.Revision - 1 }, stock),
            "enemy OAM rejects schemas older than its accepted compatibility boundary");
        AssertThrows<InvalidDataException>(() => Load(stockDocument with { Version = EnemySpritemapSchema.Current.Revision + 1 }, stock),
            "enemy OAM rejects unknown future schemas");
        Console.WriteLine($"PASS enemy OAM overrides: all {schemas.Length} schemas, {bindingSchemas} binding schemas, " +
            "exact art/remap preservation, stock inheritance, reload identity and malformed-data rejection. No ROM or gameplay probes.");
        return;

        EnemySpritemapCatalog Load(EnemySpritemapDocument value, EnemySpritemapCatalog? baseline = null)
        {
            using var json = fixture.Json(value);
            return EnemySpritemapDocuments.Load(json, baseline);
        }
    }

    /// <summary>
    /// Pairs every declared schema revision with its declared frame count. New revisions automatically join the
    /// coverage; version gaps fail.
    /// </summary>
    private static ExtendedEnemySchemaFixturesResult[] EnemyOamSchemaFixtures() =>
        [.. typeof(EnemySpritemapSchema).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            .Where(field => field.FieldType == typeof(EnemySpritemapSchema))
            .Select(field => (EnemySpritemapSchema)field.GetValue(null)!)
            .Select(schema => new ExtendedEnemySchemaFixturesResult(schema.Revision, schema.FrameCount))
            .OrderBy(schema => schema.Version)];

    /// <summary>Runs ordinary, extended, and projectile legacy override checks.</summary>
    [VerificationCommand("--enemy-legacy-overrides")]
    internal static int EnemyLegacyOverridesCommand()
    {
        Suite(nameof(VerifyEnemyLegacyOverrides), () => VerifyEnemyLegacyOverrides());
        Suite(nameof(VerifyEnemyExtendedLegacyOverrides), () => VerifyEnemyExtendedLegacyOverrides());
        Suite(nameof(VerifyEnemyProjectileLegacyOverrides), () => VerifyEnemyProjectileLegacyOverrides());
        return 0;
    }
}
