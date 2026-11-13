using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>
/// Confirms the spawned-object sheet contract (#1536) on constructed consumer calls, never an installation: for both the
/// frame lookup and the shows-nothing question (#1732), a computed picture and a constant naming a spawned frame classify,
/// a constant naming none fails, and a construction of a sheet
/// outside its reviewed loader revokes the loader's proof.
/// </summary>
internal static class SpawnedObjectSheetContractChecks
{
    /// <summary>Compiles the reviewed spawned-object sources with six consumer calls, then checks each classification.</summary>
    internal static void Run()
    {
        string owned = (ClosedPresentationIdentityDefinitions.GetNames("SpawnedObjectSheet", "Frame", "picture")
            ?? throw new InvalidOperationException("The spawned-object sheet contract names no frame domain.")).First();
        var trees = SpawnedObjectSheetClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText($$"""
            using SuperMetroid.Core.Enemies.Artwork;
            class SpawnedObjectConsumer {
                void Inspect(SpawnedObjectSheet sheet, string picture) {
                    sheet.Frame(picture);
                    sheet.Frame("{{owned}}");
                    sheet.Frame("Projectile.NoSuchFrame");
                    sheet.ShowsNothing(picture);
                    sheet.ShowsNothing("{{owned}}");
                    sheet.ShowsNothing("Projectile.NoSuchFrame");
                }
            }
            """, path: "fixture-spawned-object-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation compilation = CSharpCompilation.Create("SpawnedObjectProviderFixture", trees,
            platforms.Append(typeof(SuperMetroid.Core.Enemies.Artwork.SpawnedObjectSheet).Assembly.Location)
                .Append(typeof(SuperMetroid.AssetExtraction.Installation.RoomBlockSheetFiles).Assembly.Location).Distinct()
                .Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        FixtureConsumerBinding.Require(compilation, calls, nameof(SpawnedObjectSheetContractChecks));
        AuditReport before = Inspect(compilation, calls, adapted: false), after = Inspect(compilation, calls, adapted: true);
        Require(before.UnresolvedCount == 6, "constructed calls must first expose the missing adapter (" + before.Describe() + ")");
        Require(after.Classifications.Count == 4 && after.UnresolvedCount == 2 && after.MissingCount == 0 && after.Consumers.Count == 6,
            "computed and owned pictures classify and an unowned constant fails (" + after.Describe() + ")");

        // Only the reviewed loader's exact-picture check proves a sheet complete; a construction elsewhere revokes it.
        CSharpCompilation forged = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Enemies.Artwork;
            class SheetForger {
                SpawnedObjectSheet Forge() => new SpawnedObjectSheet(null, null!, null!);
            }
            """, path: "fixture-spawned-object-forger.cs"));
        AuditReport revoked = Inspect(forged, calls, adapted: true);
        Require(revoked.Classifications.Count == 0 && revoked.UnresolvedCount == 6 && revoked.Consumers.Count == 6,
            "a construction of a spawned-object sheet outside its loader must revoke its proof (" + revoked.Describe() + ")");
    }

    /// <summary>Inspects every fixture consumer call, through the closed-provider adapter when <paramref name="adapted"/>.</summary>
    private static AuditReport Inspect(CSharpCompilation compilation, SyntaxTree calls, bool adapted)
    {
        var exports = new ResourceIndex();
        var adapter = adapted ? new ClosedPresentationAudit(compilation, exports) : null;
        var report = new AuditReport();
        foreach (InvocationExpressionSyntax call in calls.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, report, adapter);
        return report;
    }

    /// <summary>Fails the spawned-object confirmation with the classification that did not hold.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Spawned-object provider confirmation failed: " + reason);
    }
}
