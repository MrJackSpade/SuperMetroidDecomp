using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms only the identified complete placement and indexed composition contracts.</summary>
internal static class SamusArtworkPresentationContractChecks
{
    /// <summary>Confirms reviewed Samus artwork providers and pose selection boundaries.</summary>
    internal static void Run()
    {
        var trees = SamusArtworkClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<samus-art-usings>"));
        SyntaxTree calls = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Assets;
            class SamusArtConsumer {
                void Inspect(SamusBodyArtworkCatalog body, SamusSpritemapArtworkCatalog sprites,
                    SamusAtmosphericArtworkCatalog atmosphere, SamusArmCannonArtworkCatalog arm) {
                    body.GraphicsYOffset(252);
                    body.GraphicsYOffset(253);
                    body.TryLandingYOffset(int.MaxValue, out _);
                    body.TryPostureYOffset(-1, out _);
                    body.TryDrainedYOffset(int.MaxValue, out _);
                    body.Frame(0, 0);
                    sprites.TopBase(252);
                    sprites.TopBase(253);
                    sprites.BottomBase(252);
                    sprites.BottomBase(253);
                    sprites.TryGet(SamusSpritemapArtworkCatalog.PointerCount - 1, out _);
                    sprites.TryGet(SamusSpritemapArtworkCatalog.PointerCount, out _);
                    atmosphere.TryResolve(byte.MaxValue, byte.MaxValue, out _);
                    arm.PoseDrawingData(252);
                    arm.PoseDrawingData(253);
                    arm.ReadDrawingByte(SamusArmCannonArtworkFormat.DrawingDataEndExclusive - 1);
                    arm.ReadDrawingByte(SamusArmCannonArtworkFormat.DrawingDataEndExclusive);
                    arm.SpriteAttributes(9);
                    arm.SpriteAttributes(10);
                    arm.TileSource(9, 3);
                    arm.TileSource(9, 4);
                    arm.TileSource(10, 0);
                    arm.TryResolveTile(0, 0, out _);
                }
            }
            """, path: "fixture-samus-art-consumer.cs");
        trees.Add(calls);
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("SamusArtworkProviderFixture", sources,
            platforms.Append(typeof(SamusBodyArtworkCatalog).Assembly.Location).Distinct()
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
        Require(before.UnresolvedCount == 23, "constructed calls must first expose absent adapters");
        Require(after.Classifications.Count == 13 && after.UnresolvedCount == 10 && after.MissingCount == 0 &&
            after.Consumers.Count == 23, "complete table indices and false queries qualify; invalid indices and editable Frame do not");
        Require(after.Findings.Any(item => item.Owner == "SamusBodyArtworkCatalog.Frame"),
            "offset closure must not imply DMA frame-selector closure");

        foreach (string external in new[] {
            "class ExternalArm { object Make() => SuperMetroid.Core.Assets.SamusArmCannonArtworkCatalog.FromPlacement(null!, null!); }",
            "class ExternalArm { void Change() { SuperMetroid.Core.Assets.SamusArmCannonArtworkFormat.TileSourcePointers[0] = 0; } }" })
        {
            AuditReport revoked = Inspect(Compile(trees.Append(CSharpSyntaxTree.ParseText(external,
                path: "fixture-external-arm.cs"))), true);
            Require(revoked.Classifications.Count == 8 && revoked.UnresolvedCount == 15 &&
                revoked.Findings.Count(item => item.Owner.StartsWith("SamusArmCannonArtworkCatalog.", StringComparison.Ordinal) &&
                    item.Message.Contains("stale", StringComparison.Ordinal)) == 10,
                "unvalidated placement construction or mutable tile identities must revoke the arm proof");
        }
        AuditReport partial = Inspect(Compile(trees.Append(CSharpSyntaxTree.ParseText("""
            namespace SuperMetroid.Core.Assets {
                public sealed partial class SamusBodyArtworkCatalog { void Change() { graphicsYOffsets[0] = 0; } }
            }
            """, path: "fixture-extra-body-partial.cs"))), true);
        Require(partial.Classifications.Count == 9 && partial.UnresolvedCount == 14,
            "an unreviewed partial provider declaration must revoke access to the private offset state");
    }

    /// <summary>Throws when a Samus artwork presentation expectation is not satisfied.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Samus artwork provider confirmation failed: " + reason);
    }
}
