using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;

internal static partial class Program
{
    /// <summary>Tests this statically inventoried bundle; it never runs gameplay to discover reads.</summary>
    private static void VerifyEnemyContentIdentity()
    {
        var fixture = new EnemyIdentityFixture();
        EnemyTileArtworkCatalog stock = fixture.Build();
        string baseline = stock.ContentIdentity;
        AssertEqual(baseline, new EnemyIdentityFixture(reverse: true).Build().ContentIdentity,
            "enemy identity canonicalizes dictionary insertion order");
        foreach (string edit in fixture.Edits)
        {
            string changed = new EnemyIdentityFixture(edit).Build().ContentIdentity;
            AssertTrue(changed != baseline, edit + " changes the complete selected enemy bundle");
            AssertTrue(EnemyIdentity(baseline).CompositeSha256 != EnemyIdentity(changed).CompositeSha256,
                edit + " changes installed content identity");
            AssertTrue(EnemyIdentity(baseline).GetCompatibilityWarnings(EnemyIdentity(changed).ToSnapshot(), "test")
                .Any(warning => warning.Contains(GameInstallationLayout.EnemyTileDirectoryName, StringComparison.Ordinal)),
                edit + " names enemy content in state/replay compatibility warnings");
        }
        foreach (string edit in new[] { "json-indent", "png-colors" })
            AssertEqual(baseline, new EnemyIdentityFixture(edit).Build().ContentIdentity,
                edit + " preserves decoded enemy presentation identity");

        Suite(nameof(VerifyLegacyEnemyIdentityMerges), () => VerifyLegacyEnemyIdentityMerges());
        // Inspect serialization shape, not data, so computed digests cannot silently change
        // the field count of a debugger snapshot. The hash itself uses typed production fields.
        Type[] childTypes = typeof(EnemyTileArtworkCatalog).GetProperties()
            .Where(property => property.Name != nameof(EnemyTileArtworkCatalog.ContentIdentity))
            .Select(property => property.PropertyType).ToArray();
        AssertEqual(35, childTypes.Length, "all enemy bundle subdomains are accounted for");
        foreach (Type type in childTypes.Append(typeof(EnemyTileArtworkCatalog)).Append(typeof(EnemyPaletteSheet)))
        {
            AssertTrue(type.GetProperty("ContentIdentity") is not null,
                type.Name + " exposes typed selected content identity");
            AssertTrue(type.GetField("<ContentIdentity>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic) is null,
                type.Name + " adds no persisted derived-identity field");
        }
        Console.WriteLine($"  Enemy content: {fixture.Edits.Count} independent edits across all 35 subdomains, " +
            "OAM/BG2 ordering, display bindings, legacy stock merges, encoding invariance and host warnings pass without a ROM.");

        static GameContentIdentity EnemyIdentity(string digest) => GameContentIdentity.Create(
            new string('A', 64), new string('B', 64), new string('C', 64), Guid.Empty,
            [KeyValuePair.Create(GameInstallationLayout.EnemyTileDirectoryName, digest)]);
    }

    private static void VerifyLegacyEnemyIdentityMerges()
    {
        var baseline = new EnemyIdentityFixture();
        EnemySpritemapCatalog stockOam = baseline.Oam();
        var legacyOam = baseline.OamDocument() with
        {
            Version = EnemySpritemapDefinitions.PreDisplayBindingsVersion,
            Frames = baseline.OamDocument().Frames.Take(EnemySpritemapDefinitions.PreDisplayBindingsFrameCount).ToDictionary(),
            DisplayFrames = null,
        };
        AssertEqual(stockOam.ContentIdentity,
            EnemySpritemapCatalog.Load(baseline.Json(legacyOam), stockOam).ContentIdentity,
            "older OAM overrides hash the merged current stock frames and bindings");
        EnemySpritemapCatalog changedOam = new EnemyIdentityFixture("oam-x").Oam();
        AssertTrue(stockOam.ContentIdentity != EnemySpritemapCatalog.Load(baseline.Json(legacyOam), changedOam).ContentIdentity,
            "inherited new stock OAM changes are not hidden by an unchanged old override");

        EnemyExtendedFrameCatalog stockExtended = baseline.Extended();
        var legacyExtended = baseline.ExtendedDocument() with
        {
            Version = (int)EnemyExtendedFrameSchema.PreDisplayBindings,
            Frames = baseline.ExtendedDocument().Frames.Take(EnemyExtendedFrameDefinitions.PirateFrameCount).ToDictionary(),
            DisplayFrames = null,
        };
        AssertEqual(stockExtended.ContentIdentity,
            EnemyExtendedFrameCatalog.Load(baseline.Json(legacyExtended), stockExtended).ContentIdentity,
            "older extended overrides hash merged stock components and bindings");
        EnemyExtendedFrameCatalog changedExtended = new EnemyIdentityFixture("extended-x").Extended();
        AssertTrue(stockExtended.ContentIdentity !=
            EnemyExtendedFrameCatalog.Load(baseline.Json(legacyExtended), changedExtended).ContentIdentity,
            "inherited new extended frames contribute to the selected identity");

        EnemyProjectileSpritemapCatalog stockProjectiles = baseline.Projectiles();
        var legacyProjectiles = baseline.ProjectileDocument() with
        {
            Version = EnemyProjectileSpritemapDefinitions.CeresOnlyVersion,
            Frames = baseline.ProjectileDocument().Frames.Take(EnemyProjectileSpritemapDefinitions.LegacyFrameCount).ToDictionary(),
            ProgramFrames = null,
        };
        AssertEqual(stockProjectiles.ContentIdentity,
            EnemyProjectileSpritemapCatalog.Load(baseline.Json(legacyProjectiles), stockProjectiles).ContentIdentity,
            "older projectile overrides include current instruction-selected compositions");
        EnemyProjectileSpritemapCatalog changedProjectiles = new EnemyIdentityFixture("projectile-program-x").Projectiles();
        AssertTrue(stockProjectiles.ContentIdentity !=
            EnemyProjectileSpritemapCatalog.Load(baseline.Json(legacyProjectiles), changedProjectiles).ContentIdentity,
            "inherited projectile program-frame changes contribute to the selected identity");
    }
}
