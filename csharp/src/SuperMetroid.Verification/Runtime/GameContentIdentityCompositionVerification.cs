using SuperMetroid.AssetExtraction.Installation;
namespace SuperMetroid.Verification.Runtime;

/// <summary>Checks how component digests and the compiled-definition build compose the installed content identity.</summary>
internal static class GameContentIdentityCompositionVerification
{
    /// <summary>Checks component digests and build identity affect compatibility warnings independently.</summary>
    internal static void VerifyGameContentIdentityComposition()
    {
        string audio = new('A', 64);
        string maps = new('B', 64);
        string projectiles = new('C', 64);
        Guid definitions = Guid.Parse("89abcdef-0123-4567-89ab-cdef01234567");
        GameContentIdentity baseline = GameContentIdentity.Create(audio, maps, projectiles, definitions);
        AssertEqual(
            baseline,
            GameContentIdentity.Create(audio, maps, projectiles, definitions),
            "aggregate installation identity is deterministic");
        AssertTrue(
            baseline.CompositeSha256 != GameContentIdentity.Create(new string('D', 64), maps, projectiles, definitions).CompositeSha256,
            "audio identity invalidates aggregate installation identity");
        AssertTrue(
            baseline.CompositeSha256 != GameContentIdentity.Create(audio, new string('D', 64), projectiles, definitions).CompositeSha256,
            "map identity invalidates aggregate installation identity");
        AssertTrue(
            baseline.CompositeSha256 != GameContentIdentity.Create(audio, maps, new string('D', 64), definitions).CompositeSha256,
            "projectile identity invalidates aggregate installation identity");
        AssertTrue(
            baseline.CompositeSha256 != GameContentIdentity.Create(audio, maps, projectiles, Guid.Empty).CompositeSha256,
            "compiled-definition build invalidates aggregate installation identity");
        AssertThrows<ArgumentException>(
            () => GameContentIdentity.Create("not-a-sha", maps, projectiles, definitions),
            "aggregate installation identity rejects malformed component digest");

        GameContentIdentitySnapshot recorded = baseline.ToSnapshot();
        AssertEqual(0, baseline.GetCompatibilityWarnings(recorded, "test artifact").Count,
            "matching recording identity produces no compatibility warning");
        AssertTrue(
            baseline.GetCompatibilityWarnings(null, "test artifact").Single().Contains("Legacy", StringComparison.Ordinal),
            "legacy recording explains unavailable installed-content comparison");
        AssertTrue(
            baseline.GetCompatibilityWarnings(recorded with
            {
                AudioContentSha256 = Convert.FromHexString(new string('D', 64)),
            }, "test artifact").Single().Contains("audio", StringComparison.Ordinal),
            "audio drift receives a component-specific warning");
        AssertTrue(
            baseline.GetCompatibilityWarnings(recorded with
            {
                CompiledDefinitionsBuildId = Guid.Empty,
            }, "test artifact").Single().Contains("Compiled gameplay definitions", StringComparison.Ordinal),
            "compiled-definition drift receives a specific warning");
        AssertTrue(
            baseline.GetCompatibilityWarnings(recorded with
            {
                CompositeSha256 = Convert.FromHexString(new string('D', 64)),
            }, "test artifact").Single().Contains("Aggregate", StringComparison.Ordinal),
            "unexplained aggregate drift cannot pass component comparison");
    }
}
