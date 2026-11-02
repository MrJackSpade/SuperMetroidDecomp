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
    /// <summary>Checks that only installed area maps receive closed-contract classifications; the HUD's and escape timer's characters are the import's (#1466, #1502).</summary>
    internal static void Run()
    {
        var trees = AreaMapClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Assets;
            using SuperMetroid.Core.Enemies.Artwork;
            using SuperMetroid.Core.Frontend.Map.Artwork;
            using SuperMetroid.Core.Progression;
            using SuperMetroid.Core.Rooms;
            class MapConsumer {
                void Inspect(AreaMapPresentation maps, EnemyTileArtworkCatalog enemies) {
                    maps.Get(AreaId.Crateria);
                    maps.Get(AreaId.Ceres);
                    maps.Get((AreaId)7);
                    enemies.LoadPaletteTo(default, default);
                }
            }
            """, path: "fixture-area-map-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("AreaMapProviderFixture", sources,
            platforms.Append(typeof(AreaMapPresentation).Assembly.Location).Distinct()
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
        FixtureConsumerBinding.Require(compilation, calls, "AreaMapPresentationContractChecks");
        AuditReport before = Inspect(compilation, false), after = Inspect(compilation, true);
        Require(before.UnresolvedCount == 4, "the fixture must first expose absent adapters" + " (" + before.Describe() + ")");
        Require(after.Classifications.Count == 2 && after.UnresolvedCount == 2 && after.MissingCount == 0 &&
            after.Consumers.Count == 4, "all installed areas qualify, not unknown areas" + " (" + after.Describe() + ")");
        Require(after.Findings.Any(item => item.Owner == "EnemyTileArtworkCatalog.LoadPaletteTo"),
            "an enemy catalog without its own reviewed sources must not borrow the atomic map install proof");
    }

    /// <summary>Rejects a map contract fixture when its expected classification or revocation fails.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Area map provider confirmation failed: " + reason);
    }
}
