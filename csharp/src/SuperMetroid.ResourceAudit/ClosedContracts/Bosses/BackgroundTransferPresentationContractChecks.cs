using SuperMetroid.Tooling;
using SuperMetroid.AssetExtraction.CartridgeTables;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Bosses;

/// <summary>Confirms the identified complete BG2/effect/fragment and owned-length contracts using source fixtures.</summary>
internal static class BackgroundTransferPresentationContractChecks
{
    /// <summary>Compiles source-backed BG2 and transfer call fixtures, then checks accepted and revoked audit classifications.</summary>
    internal static void Run(bool bg2Only = false)
    {
        var contracts = BackgroundTransferClosedContractDefinitions.All.Where(contract =>
            !bg2Only || contract.Rule.EndsWith("-complete-bg2-membership", StringComparison.Ordinal));
        var trees = contracts.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Bosses.Crocomire.Artwork;
            using SuperMetroid.Core.Bosses.Draygon.Artwork;
            using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
            using SuperMetroid.Core.Bosses.Phantoon.Artwork;
            using SuperMetroid.Core.Enemies.Artwork;
            using SuperMetroid.Core.Frontend.Ending.Artwork;
            using SuperMetroid.Core.Fx;
            using SuperMetroid.Core.Fx.Artwork;
            using SuperMetroid.Core.Hardware;
            class BackgroundConsumer {
                void Inspect(PhantoonBg2FrameCatalog phantoon, DraygonBg2FrameCatalog draygon,
                    CrocomireBg2FrameCatalog crocomire, MotherBrainBodyBg2FrameCatalog motherBrain,
                    RoomFxLayer3TilemapCatalog pages, RoomFxPaletteBlendCatalog blends,
                    EndingObjectArtworkCatalog ending, GunshipLiftoffArtworkCatalog ship, SuperMetroid.Core.Assets.BackgroundColourSets backgrounds) {
                    phantoon.TryGet(0, out _);
                    draygon.TryGet(0, out _);
                    crocomire.TryGet(0, out _);
                    motherBrain.TryGet(SuperMetroid.Core.Enemies.Artwork.EnemyFrameId.MotherBrainBodyInitialDummy, out _);
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
                    ship.TryResolve(GunshipLiftoffTransferDefinitions.First.SourceAddress, GunshipLiftoffTransferDefinitions.ByteCount, out _);
                    ship.TryResolve(GunshipLiftoffTransferDefinitions.First.SourceAddress, GunshipLiftoffTransferDefinitions.ByteCount - 1, out _);
                    ship.TryResolve(0, GunshipLiftoffTransferDefinitions.ByteCount - 1, out _);
                }
            }
            """, path: "fixture-bg-transfer-consumer.cs");
        if (bg2Only)
            calls = CSharpSyntaxTree.ParseText("""
                using SuperMetroid.Core.Bosses.Crocomire.Artwork;
                using SuperMetroid.Core.Bosses.Draygon.Artwork;
                using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
                using SuperMetroid.Core.Bosses.Phantoon.Artwork;
                class BackgroundConsumer {
                    void Inspect(PhantoonBg2FrameCatalog phantoon, DraygonBg2FrameCatalog draygon,
                        CrocomireBg2FrameCatalog crocomire, MotherBrainBodyBg2FrameCatalog motherBrain) {
                        phantoon.TryGet(0, out _);
                        draygon.TryGet(0, out _);
                        crocomire.TryGet(0, out _);
                        motherBrain.TryGet(SuperMetroid.Core.Enemies.Artwork.EnemyFrameId.MotherBrainBodyInitialDummy, out _);
                    }
                }
                """, path: "fixture-bg-transfer-consumer.cs");
        trees.Add(calls);
        // The first page's source is the import's knowledge, not a language constant.
        // Materialize its actual value so the negative case tests constant correlation.
        calls = CSharpSyntaxTree.ParseText(calls.GetText().ToString().Replace(
            "GunshipLiftoffTransferDefinitions.First.SourceAddress", EnemySourceDefinitions.GunshipLiftoffPageSource(0)
                .ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal), path: calls.FilePath);
        trees[^1] = calls;
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("BackgroundTransferProviderFixture", sources,
            platforms.Append(typeof(GunshipLiftoffArtworkCatalog).Assembly.Location).Distinct()
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
        Require(before.UnresolvedCount == (bg2Only ? 4 : 19), "constructed calls must first expose the missing adapters" + " (" + before.Describe() + ")");
        Require(after.Classifications.Count == (bg2Only ? 4 : 13) && after.UnresolvedCount == (bg2Only ? 0 : 6) && after.MissingCount == 0 &&
            after.Consumers.Count == (bg2Only ? 4 : 19), "complete owned sets/false queries qualify; bad selections and owned lengths fail" + " (" + after.Describe() + ")");
        Require(bg2Only || after.Findings.Any(item => item.Owner == "GunshipLiftoffArtworkCatalog.TryResolve" &&
            item.Message.Contains("requires 1024 bytes", StringComparison.Ordinal)),
            "an owned source with a short length cannot borrow unsupported-source false behavior");
    }

    /// <summary>Fails the contract fixture with the specific classification mismatch.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Background/transfer provider confirmation failed: " + reason);
    }
}
