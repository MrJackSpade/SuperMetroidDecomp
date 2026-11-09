using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Bosses;

/// <summary>Confirms the identified complete effect/fragment and owned-length contracts using source fixtures.</summary>
internal static class BackgroundTransferPresentationContractChecks
{
    /// <summary>Compiles source-backed transfer call fixtures, then checks accepted and revoked audit classifications.</summary>
    internal static void Run()
    {
        var trees = BackgroundTransferClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Assets;
            using SuperMetroid.AssetExtraction.Artwork;
            using SuperMetroid.AssetExtraction.Enemies;
            using SuperMetroid.AssetExtraction.Fx;
            using SuperMetroid.AssetExtraction.Fx.Artwork;
            using SuperMetroid.Core.Enemies.Artwork;
            using SuperMetroid.Core.Frontend.Ending.Artwork;
            using SuperMetroid.Core.Fx;
            using SuperMetroid.Core.Fx.Artwork;
            class BackgroundConsumer {
                void Inspect(RoomFxLayer3Pages pages, RoomFxBlendInks blends,
                    SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.EndingObjectSourceArtwork ending, GunshipLiftoffArtworkCatalog ship) {
                    pages.Resolve(RoomFxType.Lava);
                    pages.Resolve((RoomFxType)255);
                    blends.Inks(0);
                    blends.Inks(RoomFxPaletteBlend.Lava);
                    blends.Inks((RoomFxPaletteBlend)200);
                    blends.Inks((RoomFxPaletteBlend)255);
                    blends.Inks(RoomFxPaletteBlend.Fog);
                    ending.Fragment(EndingObjectFragmentId.Blank);
                    ending.Fragment((EndingObjectFragmentId)4);
                    ship.Resolve(InstalledArtworkId.GunshipLiftoffFirstTiles);
                    ship.Resolve(InstalledArtworkId.GunshipLiftoffFifthTiles);
                    ship.Resolve(InstalledArtworkId.BeamPowerTiles);
                }
            }
            """, path: "fixture-bg-transfer-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("BackgroundTransferProviderFixture", sources,
            platforms.Append(typeof(SuperMetroid.Core.Assets.RgbColor).Assembly.Location).Distinct()
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
        // A fixture that does not compile leaves calls unbound, so its errors explain a count that comes out short.
        string compileErrors = string.Join(" | ", compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .Select(diagnostic => diagnostic.ToString()).Distinct());
        Require(before.UnresolvedCount == 12, "constructed calls must first expose the missing adapters" + " (" + before.Describe() + ")" +
            (compileErrors.Length == 0 ? string.Empty : "; fixture errors: " + compileErrors));
        Require(after.Classifications.Count == 7 && after.UnresolvedCount == 5 && after.MissingCount == 0 &&
            after.Consumers.Count == 12, "complete owned sets/false queries qualify; bad selections and owned lengths fail" + " (" + after.Describe() + ")");
    }

    /// <summary>Fails the contract fixture with the specific classification mismatch.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Background/transfer provider confirmation failed: " + reason);
    }
}
