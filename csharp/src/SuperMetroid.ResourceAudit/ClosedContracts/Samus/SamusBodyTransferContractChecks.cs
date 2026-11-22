using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Samus;

/// <summary>Confirms the identified missing-record admission gap using authored data, not poses in gameplay.</summary>
internal static class SamusBodyTransferContractChecks
{
    /// <summary>
    /// Checks the static consumer classification of the installed body's frame placements. The import decodes which
    /// definition each frame selects once (#2556); the retail decoding is compared with the cartridge by the body artwork
    /// verification, so no fixture body is built here.
    /// </summary>
    internal static void Run() => ConfirmStaticCalls();

    /// <summary>Compiles representative transfer calls and checks audit behavior under valid and stale source contracts.</summary>
    private static void ConfirmStaticCalls()
    {
        var trees = SamusBodyTransferClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(File.ReadAllText(source.Path),
                path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Samus.Artwork;
            class SamusTransferConsumer {
                void Inspect(InstalledSamusBodyArtwork body, SuperMetroid.Core.Samus.Poses.SamusPoseId pose, SuperMetroid.Core.Samus.Poses.SamusAnimationFrame frame) {
                    body.OriginRise(pose, frame);
                    body.OriginRise((SuperMetroid.Core.Samus.Poses.SamusPoseId)(-1), default(SuperMetroid.Core.Samus.Poses.SamusAnimationFrame));
                }
            }
            """, path: "fixture-samus-transfer-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("SamusTransferFixture", sources,
            platforms.Append(FixtureAssemblies.Core).Distinct().Select(path => MetadataReference.CreateFromFile(path)),
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
        Require(before.UnresolvedCount == 2 && before.Consumers.Count == 2, "original unaccounted frame-placement boundaries" + " (" + before.Describe() + ")");
        Require(after.Classifications.Count == 1 && after.UnresolvedCount == 1 && after.Consumers.Count == 2,
            "valid-domain frame-placement availability does not suppress a known invalid pose constant" + " (" + after.Describe() + ")");
        var extra = CSharpSyntaxTree.ParseText("namespace SuperMetroid.Core.Samus.Artwork; public sealed partial class InstalledSamusBodyArtwork { }",
            path: "fixture-unreviewed-body-partial.cs");
        Require(Inspect(Compile(trees.Append(extra)), true).UnresolvedCount == 2, "unreviewed private-state access revokes frame-placement closure");
    }

    /// <summary>Reports a failed transfer-contract assertion with its specific reason.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Samus transfer confirmation failed: " + reason);
    }
}
