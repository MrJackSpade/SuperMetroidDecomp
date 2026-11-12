using SuperMetroid.AssetExtraction.Enemies;
using System.Reflection;
using static SuperMetroid.Verification.Enemies.EnemyContentIdentityFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>Checks canonical identity and compatibility reporting for the selected enemy artwork bundle.</summary>
internal static class EnemyContentIdentityVerification
{
    /// <summary>Tests this statically inventoried bundle; it never runs gameplay to discover reads.</summary>
    internal static void VerifyEnemyContentIdentity()
    {
        var fixture = new EnemyIdentityFixture();
        ImportEnemyArtwork stock = fixture.Build();
        string baseline = stock.ContentIdentity;
        AssertEqual(baseline, new EnemyIdentityFixture(reverse: true).Build().ContentIdentity,
            "enemy identity canonicalizes dictionary insertion order");
        // Each edit builds its own bundle from the shared unedited components, so the edits are independent and run at once.
        Parallel.ForEach(fixture.Edits, edit =>
        {
            string changed = new EnemyIdentityFixture(edit).Build().ContentIdentity;
            AssertTrue(changed != baseline, edit + " changes the complete selected enemy bundle");
            AssertTrue(EnemyIdentity(baseline).CompositeSha256 != EnemyIdentity(changed).CompositeSha256,
                edit + " changes installed content identity");
            AssertTrue(EnemyIdentity(baseline).GetCompatibilityWarnings(EnemyIdentity(changed).ToSnapshot(), "test")
                .Any(warning => warning.Contains(GameInstallationLayout.EnemyTileDirectoryName, StringComparison.Ordinal)),
                edit + " names enemy content in state/replay compatibility warnings");
        });
        foreach (string edit in new[] { "json-indent", "png-colors" })
            AssertEqual(baseline, new EnemyIdentityFixture(edit).Build().ContentIdentity,
                edit + " preserves decoded enemy presentation identity");

        // Inspect serialization shape, not data, so computed digests cannot silently change
        // the field count of a debugger snapshot. The hash itself uses typed production fields.
        Type[] catalogs = [typeof(EnemyTileSourceArtwork), typeof(BossSourceArtwork), typeof(AreaSourceArtwork)];
        Type[] childTypes = catalogs.SelectMany(catalog => MetadataShape.Properties(catalog, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
            .Where(property => property.Name != nameof(EnemyTileSourceArtwork.ContentIdentity))
            .Select(property => property.PropertyType)).ToArray();
        AssertEqual(36, childTypes.Length, "all enemy bundle subdomains are accounted for");
        foreach (Type type in childTypes.Concat(catalogs).Append(typeof(EnemyPaletteSheet)))
        {
            // The game's catalogs carry no identity; the import pipeline identifies them (#1502). The import side's
            // identities may be extension members (ActorColourContentIdentity), which the enemy bundle's identity reads, so
            // the compiler already requires each; reflection checks only that no game-side catalog declares one (#1583).
            if (type.Assembly != typeof(EnemyTileSourceArtwork).Assembly)
                AssertTrue(!MetadataShape.DeclaresProperty(type, "ContentIdentity"), type.Name + " is a game catalog and carries no content identity");
            AssertTrue(!MetadataShape.DeclaresField(type, "<ContentIdentity>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic),
                type.Name + " adds no persisted derived-identity field");
        }
        Console.WriteLine($"  Enemy content: {fixture.Edits.Count} independent edits across all 35 subdomains, " +
            "OAM/BG2 ordering, display bindings, encoding invariance and host warnings pass without a ROM.");

        static GameContentIdentity EnemyIdentity(string digest) => GameContentIdentity.Create(
            new string('A', 64), new string('B', 64), new string('C', 64), Guid.Empty,
            [KeyValuePair.Create(GameInstallationLayout.EnemyTileDirectoryName, digest)]);
    }

    /// <summary>Runs selected enemy-artwork identity checks.</summary>
    [VerificationCommand("--enemy-content-identity")]
    internal static int EnemyContentIdentityCommand()
    {
        Suite(nameof(VerifyEnemyContentIdentity), () => VerifyEnemyContentIdentity());
        return 0;
    }
}
