using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Samus;

/// <summary>Confirms only the identified complete placement and indexed composition contracts.</summary>
internal static class SamusArtworkPresentationContractChecks
{
    /// <summary>Checks finite Samus artwork calls and revokes closure for unreviewed arm or body source mutations.</summary>
    internal static void Run()
    {
        var trees = SamusArtworkClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.AssetExtraction.Samus;
            using SuperMetroid.Core.Samus.Artwork;
            class SamusArtConsumer {
                void Inspect(InstalledSamusBodyArtwork body, SamusSpritemapSources compositions,
                    SamusAtmosphericSourceArtwork atmosphere, SamusArmCannonSourceArtwork arm) {
                    body.GraphicsYOffset(SuperMetroid.Core.Samus.Poses.SamusPoseId.StandingTransitionAimDiagonalDownLeftPose);
                    body.GraphicsYOffset(SuperMetroid.Core.Samus.Poses.SamusPoseId.TallStillRightPose);
                    body.OriginRise(0, 0);
                    compositions.TryGetComposition(new SamusBodyHalfFrame(SuperMetroid.Core.Samus.Poses.SamusPoseId.StandingTransitionAimDiagonalDownLeftPose, SamusBodyHalf.Top, 0), out _);
                    compositions.TryGetComposition(new SamusBodyHalfFrame(SuperMetroid.Core.Samus.Poses.SamusPoseId.TallStillRightPose, SamusBodyHalf.Top, 0), out _);
                    atmosphere.TryResolve(byte.MaxValue, byte.MaxValue, out _);
                    arm.PoseDescriptor(SuperMetroid.Core.Samus.Poses.SamusPoseId.StandingTransitionAimDiagonalDownLeftPose);
                    arm.PoseDescriptor(SuperMetroid.Core.Samus.Poses.SamusPoseId.TallStillRightPose);
                    arm.ReadDrawingByte(SamusArmCannonArtworkFormat.DrawingDataByteCount - 1);
                    arm.ReadDrawingByte(SamusArmCannonArtworkFormat.DrawingDataByteCount);
                    arm.CoverTileIndex(9, 3);
                    arm.CoverTileIndex(9, 4);
                    arm.CoverTileIndex(10, 0);
                }
            }
            """, path: "fixture-samus-art-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("SamusArtworkProviderFixture", sources,
            platforms.Append(typeof(InstalledSamusBodyArtwork).Assembly.Location).Distinct()
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
        Require(before.UnresolvedCount == 13, "constructed calls must first expose absent adapters" + " (" + before.Describe() + ")");
        Require(after.Classifications.Count == 6 && after.UnresolvedCount == 7 && after.MissingCount == 0 &&
            after.Consumers.Count == 13, "complete table indices and false queries qualify; invalid indices and editable frame placements do not" + " (" + after.Describe() + ")");
        Require(after.Findings.Any(item => item.Owner == "InstalledSamusBodyArtwork.OriginRise"),
            "offset closure must not imply installed frame placement closure");

        foreach (string external in new[] {
            "class ExternalArm { object Make() => SuperMetroid.AssetExtraction.Samus.SamusArmCannonSourceArtwork.FromPlacement(null!); }" })
        {
            AuditReport revoked = Inspect(Compile(trees.Append(CSharpSyntaxTree.ParseText(external,
                path: "fixture-external-arm.cs"))), true);
            Require(revoked.Classifications.Count == 3 && revoked.UnresolvedCount == 10 &&
                revoked.Findings.Count(item => item.Owner.StartsWith("SamusArmCannonSourceArtwork.", StringComparison.Ordinal) &&
                    item.Message.Contains("stale", StringComparison.Ordinal)) == 7,
                "unvalidated placement construction must revoke the arm proof" + " (" + revoked.Describe() + ")");
        }
        AuditReport partial = Inspect(Compile(trees.Append(CSharpSyntaxTree.ParseText("""
            namespace SuperMetroid.Core.Samus.Artwork {
                public sealed partial class InstalledSamusBodyArtwork { void Change() { graphicsYOffsets[0] = 0; } }
            }
            """, path: "fixture-extra-body-partial.cs"))), true);
        Require(partial.Classifications.Count == 5 && partial.UnresolvedCount == 8,
            "an unreviewed partial provider declaration must revoke access to the private offset state" + " (" + partial.Describe() + ")");
    }

    /// <summary>Reports a failed Samus artwork provider predicate with its specific cause.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Samus artwork provider confirmation failed: " + reason);
    }
}
