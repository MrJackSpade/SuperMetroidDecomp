using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Confirms the identified static selector/length contract and its revocation guards.</summary>
internal static class EnemyArtworkStaticContractChecks
{
    /// <summary>Checks static selector and length admission, then revokes closure after source contract changes.</summary>
    internal static void Run()
    {
        var trees = EnemyArtworkClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .Select(source => CSharpSyntaxTree.ParseText(File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        EnemyTileSourceDefinition first = EnemyTileSourceDefinitions.All[0];
        SyntaxTree calls = CSharpSyntaxTree.ParseText($$"""
            using SuperMetroid.Core.Enemies.Artwork;
            using SuperMetroid.Core.Hardware;
            class EnemyArtworkConsumer {
                void Inspect(EnemyTileArtworkCatalog art, SuperMetroid.Core.Enemies.EnemyDefinitionId pointer,
                    VramRegion vram, SuperMetroid.Core.Assets.ColourRun cgram) {
                    art.LoadPaletteTo(SuperMetroid.Core.Enemies.EnemyDefinitionId.{{first.DefinitionId}}, cgram);
                    art.LoadPaletteTo(0, cgram);
                    art.LoadPaletteTo(pointer, cgram);
                }
            }
            """, path: "fixture-enemy-artwork-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("EnemyArtworkFixture", sources,
            platforms.Append(typeof(EnemyTileArtworkCatalog).Assembly.Location).Distinct()
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
        Require(before.UnresolvedCount == 3 && before.Consumers.Count == 3, "all identified boundaries are retained before adaptation" + " (" + before.Describe() + ")");
        Require(after.Classifications.Count == 2 && after.UnresolvedCount == 1 && after.MissingCount == 0 && after.Consumers.Count == 3,
            "valid identities/lengths qualify while invalid known selectors remain findings" + " (" + after.Describe() + ")");
        SyntaxTree extraMetadata = CSharpSyntaxTree.ParseText(
            "namespace SuperMetroid.Core.Enemies; public static partial class RoomEnemyGraphicsSetDefinitions { }",
            path: "fixture-unreviewed-enemy-metadata.cs");
        Revoked(Inspect(Compile(trees.Append(extraMetadata)), true), "additional metadata declaration");
    }

    /// <summary>Requires all three calls to become unresolved when a reviewed artwork proof is invalidated.</summary>
    private static void Revoked(AuditReport report, string reason) => Require(
        report.Classifications.Count == 0 && report.UnresolvedCount == 3 && report.Consumers.Count == 3, reason);

    /// <summary>Reports an enemy-artwork contract failure through the audit process boundary.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Enemy artwork static confirmation failed: " + reason);
    }
}
