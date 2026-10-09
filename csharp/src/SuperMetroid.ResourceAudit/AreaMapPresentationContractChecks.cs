using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified atomic map install without loading files or running a screen.</summary>
internal static class AreaMapPresentationContractChecks
{
    /// <summary>Confirms that the reviewed map provider installs only its finite owned identities.</summary>
    internal static void Run()
    {
        var trees = AreaMapClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<area-map-usings>"));
        SyntaxTree calls = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Assets;
            using SuperMetroid.Core.Game;
            using SuperMetroid.Core.Hardware;
            class MapConsumer {
                void Inspect(AreaMapPresentationCatalog maps, EnemyTileArtworkCatalog enemies) {
                    maps.Get(AreaId.Crateria);
                    maps.Get(AreaId.Ceres);
                    maps.Get((AreaId)7);
                    maps.Resolve(VramAssetId.StandardHudTiles);
                    maps.Resolve(VramAssetId.KraidBg3RestoreQuarter3);
                    maps.Resolve(VramAssetId.EscapeTimerFirstTiles);
                    maps.Resolve(VramAssetId.EscapeTimerSecondTiles);
                    maps.Resolve(VramAssetId.None);
                    maps.Resolve(VramAssetId.BeamPowerTiles);
                    enemies.LoadPaletteTo(0, null!, 0);
                }
            }
            """, path: "fixture-area-map-consumer.cs");
        trees.Add(calls);
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("AreaMapProviderFixture", sources,
            platforms.Append(typeof(AreaMapPresentationCatalog).Assembly.Location).Distinct()
                .Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        AuditReport Inspect(CSharpCompilation compilation, bool adapted)
        {
            var exports = new ResourceIndex();
            var adapter = adapted ? new ClosedPresentationAudit(compilation, exports) : null;
            var report = new AuditReport();
            foreach (InvocationExpressionSyntax call in calls.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
                ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, report, adapter);
            return report;
        }
        CSharpCompilation compilation = Compile(trees);
        AuditReport before = Inspect(compilation, false), after = Inspect(compilation, true);
        Require(before.UnresolvedCount == 10, "the fixture must first expose absent adapters");
        Require(after.Classifications.Count == 6 && after.UnresolvedCount == 4 && after.MissingCount == 0 &&
            after.Consumers.Count == 10, "all installed areas and owned transfers qualify, not unknown areas or another provider's IDs");
        Require(after.Findings.Any(item => item.Owner == "EnemyTileArtworkCatalog.LoadPaletteTo"),
            "an enemy catalog without its own reviewed sources must not borrow the atomic map install proof");
        const string catalog = "csharp/src/SuperMetroid.Core/Assets/AreaMapPresentationCatalog.cs";
        var changed = trees.Select(tree => tree.FilePath == catalog ? CSharpSyntaxTree.ParseText(
            tree.GetText().ToString().Replace("foreach (AreaId area in Enum.GetValues<AreaId>())",
                "foreach (AreaId area in new[] { AreaId.Crateria })", StringComparison.Ordinal), path: tree.FilePath) : tree);
        AuditReport revoked = Inspect(Compile(changed), true);
        Require(revoked.Classifications.Count == 0 && revoked.UnresolvedCount == 10 &&
            revoked.Findings.Count(item => item.Owner.StartsWith("AreaMapPresentationCatalog.", StringComparison.Ordinal) &&
                item.Message.Contains("stale", StringComparison.Ordinal)) == 9,
            "changed atomic area admission must revoke both map operations, including valid-looking constants");
    }

    /// <summary>Fails the contract check when a fixture does not demonstrate its intended condition.</summary>
    /// <param name="valid">Whether the asserted contract holds.</param>
    /// <param name="reason">Explanation included in the failure exception.</param>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Area map provider confirmation failed: " + reason);
    }
}
