using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Frontend;

/// <summary>Confirms the identified atomic map install without loading files or running a screen.</summary>
internal static class AreaMapPresentationContractChecks
{
    /// <summary>Checks that only installed area maps and their owned VRAM transfers receive closed-contract classifications.</summary>
    internal static void Run()
    {
        var trees = AreaMapClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Enemies.Artwork;
            using SuperMetroid.Core.Frontend.Map.Artwork;
            using SuperMetroid.Core.Hardware;
            using SuperMetroid.Core.Rooms;
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
                    enemies.LoadPaletteTo(default, default);
                }
            }
            """, path: "fixture-area-map-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
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
        Require(before.UnresolvedCount == 10, "the fixture must first expose absent adapters" + " (" + before.Describe() + ")");
        Require(after.Classifications.Count == 6 && after.UnresolvedCount == 4 && after.MissingCount == 0 &&
            after.Consumers.Count == 10, "all installed areas and owned transfers qualify, not unknown areas or another provider's IDs" + " (" + after.Describe() + ")");
        Require(after.Findings.Any(item => item.Owner == "EnemyTileArtworkCatalog.LoadPaletteTo"),
            "an enemy catalog without its own reviewed sources must not borrow the atomic map install proof");
    }

    /// <summary>Rejects a map contract fixture when its expected classification or revocation fails.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Area map provider confirmation failed: " + reason);
    }
}
