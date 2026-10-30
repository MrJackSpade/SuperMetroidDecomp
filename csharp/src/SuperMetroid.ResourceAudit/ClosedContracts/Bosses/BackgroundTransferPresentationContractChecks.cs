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
            using SuperMetroid.AssetExtraction.Enemies;
            using SuperMetroid.AssetExtraction.Fx;
            using SuperMetroid.Core.Enemies.Artwork;
            using SuperMetroid.Core.Frontend.Ending.Artwork;
            using SuperMetroid.Core.Fx;
            using SuperMetroid.Core.Fx.Artwork;
            class BackgroundConsumer {
                void Inspect(RoomFxLayer3Pages pages, RoomFxBlendInks blends,
                    EndingObjectArtwork ending, GunshipLiftoffArtworkCatalog ship, SuperMetroid.Core.Assets.BackgroundColourSets backgrounds) {
                    pages.Resolve(RoomFxType.Lava);
                    pages.Resolve((RoomFxType)255);
                    blends.Apply(backgrounds, 0);
                    blends.Apply(backgrounds, RoomFxPaletteBlend.Lava);
                    blends.Apply(backgrounds, (RoomFxPaletteBlend)200);
                    blends.Apply(backgrounds, (RoomFxPaletteBlend)255);
                    blends.Apply(backgrounds, RoomFxPaletteBlend.Fog);
                    ending.Fragment(EndingObjectFragmentId.Segment7C);
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
            platforms.Append(typeof(SuperMetroid.Core.Assets.InstalledArtworkId).Assembly.Location).Distinct()
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
        Require(before.UnresolvedCount == 12, "constructed calls must first expose the missing adapters" + " (" + before.Describe() + ")");
        Require(after.Classifications.Count == 7 && after.UnresolvedCount == 5 && after.MissingCount == 0 &&
            after.Consumers.Count == 12, "complete owned sets/false queries qualify; bad selections and owned lengths fail" + " (" + after.Describe() + ")");
    }

    /// <summary>Fails the contract fixture with the specific classification mismatch.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Background/transfer provider confirmation failed: " + reason);
    }
}
