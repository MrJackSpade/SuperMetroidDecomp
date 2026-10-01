using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified complete BG2/effect/fragment and owned-length contracts using source fixtures.</summary>
internal static class BackgroundTransferPresentationContractChecks
{
    internal static void Run()
    {
        var trees = BackgroundTransferClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<bg-transfer-usings>"));
        SyntaxTree calls = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Assets;
            using SuperMetroid.Core.Game;
            using SuperMetroid.Core.Hardware;
            class BackgroundConsumer {
                void Inspect(PhantoonBg2FrameCatalog phantoon, DraygonBg2FrameCatalog draygon,
                    CrocomireBg2FrameCatalog crocomire, MotherBrainBodyBg2FrameCatalog motherBrain,
                    RoomFxLayer3TilemapCatalog pages, RoomFxPaletteBlendCatalog blends,
                    EndingObjectArtworkCatalog ending, GunshipLiftoffArtworkCatalog ship, SnesCgram cgram) {
                    phantoon.TryGet(0, out _);
                    draygon.TryGet(0, out _);
                    crocomire.TryGet(0, out _);
                    motherBrain.TryGet(MotherBrainBodyVisualDefinitions.InitialDummyFrame, out _);
                    pages.Resolve(RoomFxType.Lava);
                    pages.Resolve((RoomFxType)255);
                    blends.Apply(cgram, 0);
                    blends.Apply(cgram, RoomFxPaletteBlendDefinitions.Lava);
                    blends.Apply(cgram, 1);
                    blends.Resolve(0);
                    blends.Resolve(RoomFxPaletteBlendDefinitions.Fog);
                    ending.Fragment(EndingObjectFragmentId.Segment7C);
                    ending.Fragment((EndingObjectFragmentId)4);
                    ship.Resolve(VramAssetId.GunshipLiftoffFirstTiles);
                    ship.Resolve(VramAssetId.GunshipLiftoffFifthTiles);
                    ship.Resolve(VramAssetId.BeamPowerTiles);
                    ship.TryResolve(GunshipLiftoffTransferDefinitions.First.SourceAddress, GunshipLiftoffTransferDefinitions.ByteCount, out _);
                    ship.TryResolve(GunshipLiftoffTransferDefinitions.First.SourceAddress, GunshipLiftoffTransferDefinitions.ByteCount - 1, out _);
                    ship.TryResolve(0, GunshipLiftoffTransferDefinitions.ByteCount - 1, out _);
                }
            }
            """, path: "fixture-bg-transfer-consumer.cs");
        trees.Add(calls);
        // First.SourceAddress is an immutable record field, not a language constant.
        // Materialize its actual compiled value so the negative case tests constant correlation.
        calls = CSharpSyntaxTree.ParseText(calls.GetText().ToString().Replace(
            "GunshipLiftoffTransferDefinitions.First.SourceAddress", GunshipLiftoffTransferDefinitions.First.SourceAddress
                .ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal), path: calls.FilePath);
        trees[^1] = calls;
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
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
        Require(before.UnresolvedCount == 19, "constructed calls must first expose the missing adapters");
        Require(after.Classifications.Count == 13 && after.UnresolvedCount == 6 && after.MissingCount == 0 &&
            after.Consumers.Count == 19, "complete owned sets/false queries qualify; bad selections and owned lengths fail");
        Require(after.Findings.Any(item => item.Owner == "GunshipLiftoffArtworkCatalog.TryResolve" &&
            item.Message.Contains("requires 1024 bytes", StringComparison.Ordinal)),
            "an owned source with a short length cannot borrow unsupported-source false behavior");
        const string sharedLoader = "csharp/src/SuperMetroid.Core/Assets/EnemyBg2FrameCatalog.cs";
        var changed = trees.Select(tree => tree.FilePath == sharedLoader ?
            CSharpSyntaxTree.ParseText(tree.GetText().ToString().Replace("document.Frames.Count != definitions.Length",
                "false", StringComparison.Ordinal), path: tree.FilePath) : tree);
        AuditReport revoked = Inspect(Compile(changed), true);
        Require(revoked.Classifications.Count == 9 && revoked.UnresolvedCount == 10 &&
            revoked.Findings.Count(item => item.Message.Contains("stale", StringComparison.Ordinal)) == 4,
            "changed shared frame admission must revoke all four wrapper proofs");
    }

    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Background/transfer provider confirmation failed: " + reason);
    }
}
