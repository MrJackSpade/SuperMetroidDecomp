using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
                    body.ArtOrigin(SuperMetroid.Core.Samus.Poses.SamusPoseId.StandingTransitionAimDiagonalDownLeftPose);
                    body.ArtOrigin((SuperMetroid.Core.Samus.Poses.SamusPoseId)(-1));
                    body.OriginRise(0, default(SuperMetroid.Core.Samus.Poses.SamusAnimationFrame));
                    compositions.TryGetFrame(new SamusBodyHalfFrame(SuperMetroid.Core.Samus.Poses.SamusPoseId.StandingTransitionAimDiagonalDownLeftPose, SamusBodyHalf.Top, 0), out _);
                    compositions.TryGetFrame(new SamusBodyHalfFrame((SuperMetroid.Core.Samus.Poses.SamusPoseId)(-1), SamusBodyHalf.Top, 0), out _);
                    atmosphere.TryResolve(SuperMetroid.AssetExtraction.Samus.SamusAtmosphericPictures.FootstepSplash, byte.MaxValue, out _);
                    arm.CoverTile(SamusProjectileDirectionCodec.FromAimIndex(9), SuperMetroid.Core.Samus.Artwork.SamusArmCannonCoverFrame.FullyOpen);
                    arm.CoverTile(SamusProjectileDirectionCodec.FromAimIndex(9), (SuperMetroid.Core.Samus.Artwork.SamusArmCannonCoverFrame)99);
                    arm.CoverTile((SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection)99, SuperMetroid.Core.Samus.Artwork.SamusArmCannonCoverFrame.SlightlyOpen);
                }
            }
            """, path: "fixture-samus-art-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("SamusArtworkProviderFixture", sources,
            platforms.Append(FixtureAssemblies.Core).Distinct()
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
        Require(before.UnresolvedCount == 9, "constructed calls must first expose absent adapters" + " (" + before.Describe() + ")");
        Require(after.Classifications.Count == 4 && after.UnresolvedCount == 5 && after.MissingCount == 0 &&
            after.Consumers.Count == 9, "complete table indices and false queries qualify; invalid indices and editable frame placements do not" + " (" + after.Describe() + ")");
        Require(after.Findings.Any(item => item.Owner == "InstalledSamusBodyArtwork.OriginRise"),
            "offset closure must not imply installed frame placement closure");

        foreach (string external in new[] {
            "class ExternalArm { object Make() => SuperMetroid.AssetExtraction.Samus.SamusArmCannonSourceArtwork.FromPlacement(null!); }" })
        {
            AuditReport revoked = Inspect(Compile(trees.Append(CSharpSyntaxTree.ParseText(external,
                path: "fixture-external-arm.cs"))), true);
            Require(revoked.Classifications.Count == 3 && revoked.UnresolvedCount == 6 &&
                revoked.Findings.Count(item => (item.Owner.StartsWith("SamusArmCannonSourceArtwork.", StringComparison.Ordinal) ||
                    item.Owner.StartsWith("SamusArmCannonDescriptors.", StringComparison.Ordinal)) &&
                    item.Message.Contains("stale", StringComparison.Ordinal)) == 3,
                "unvalidated placement construction must revoke the arm proof" + " (" + revoked.Describe() + ")");
        }
        AuditReport partial = Inspect(Compile(trees.Append(CSharpSyntaxTree.ParseText("""
            namespace SuperMetroid.Core.Samus.Artwork {
                public sealed partial class InstalledSamusBodyArtwork { void Change() { artOrigins[0] = 0; } }
            }
            """, path: "fixture-extra-body-partial.cs"))), true);
        Require(partial.Classifications.Count == 3 && partial.UnresolvedCount == 6,
            "an unreviewed partial provider declaration must revoke access to the private offset state" + " (" + partial.Describe() + ")");
    }

    /// <summary>Reports a failed Samus artwork provider predicate with its specific cause.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Samus artwork provider confirmation failed: " + reason);
    }
}
