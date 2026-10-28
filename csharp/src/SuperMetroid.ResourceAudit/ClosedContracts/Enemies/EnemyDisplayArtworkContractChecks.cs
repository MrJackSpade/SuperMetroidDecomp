using SuperMetroid.Core.World.Artwork;
using SuperMetroid.Tooling;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Confirms the identified display-binding availability contracts, not enemy behavior.</summary>
internal static class EnemyDisplayArtworkContractChecks
{
    /// <summary>Checks valid display bindings, compiled empty frames, and rejection of targets outside the authored frame set.</summary>
    internal static void Run()
    {
        var simpleFrames = EnemySpritemapDefinitions.Frames.ToDictionary(frame => frame.Name, _ => Array.Empty<SpriteVisualPart>());
        var simpleBindings = simpleFrames.Keys.ToDictionary(name => name, name => name);
        var simpleDocument = new EnemySpritemapDocument {
            Version = EnemySpritemapSchema.Current.Revision, Frames = simpleFrames, DisplayFrames = simpleBindings };
        Stream Json<T>(T document) => new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemySpritemapCatalog simple = EnemySpritemapDocuments.Load(Json(simpleDocument));
        EnemySpritemapDefinition simpleId = EnemySpritemapDefinitions.Frames[0];
        Require(simple.TryGetDisplay(simpleId.Frame, out var parts) && parts.IsEmpty,
            "installed simple binding resolves authored parts");
        simpleBindings[simpleId.Name] = "unowned-target";
        Reject(() => EnemySpritemapDocuments.Load(Json(simpleDocument)), "unowned simple display target");
        simpleBindings[simpleId.Name] = simpleId.Name;

        var extendedFrames = EnemyExtendedFrameDefinitions.Frames.ToDictionary(frame => frame.Name, _ => new[] {
            new EnemyExtendedVisualComponent { OffsetX = 0, OffsetY = 0, Parts = [] } });
        var extendedBindings = extendedFrames.Keys.ToDictionary(name => name, name => name);
        var extendedDocument = new EnemyExtendedFrameDocument {
            Version = EnemyExtendedFrameSchema.Current.Revision, Frames = extendedFrames, DisplayFrames = extendedBindings };
        EnemyExtendedFrameCatalog extended = EnemyExtendedFrameDocuments.Load(Json(extendedDocument));
        EnemySpritemapDefinition extendedId = EnemyExtendedFrameDefinitions.Frames.First();
        Require(extended.TryGetDisplay(extendedId.Frame, out var components) && components.Length == 1 &&
            extended.TryGetDisplay(EnemyFrameId.EmptyExtended, out components) && components.IsEmpty,
            "installed extended binding and compiled empty fallback");
        extendedBindings[extendedId.Name] = "unowned-target";
        Reject(() => EnemyExtendedFrameDocuments.Load(Json(extendedDocument)), "unowned extended display target");
        ConfirmStaticCalls(simpleId, extendedId);
    }

    /// <summary>Compiles representative call sites and source mutations to confirm when the audit may classify display lookups.</summary>
    private static void ConfirmStaticCalls(EnemySpritemapDefinition simple, EnemySpritemapDefinition extended)
    {
        var trees = EnemyDisplayArtworkClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText($$"""
            using SuperMetroid.Core.Enemies.Artwork;
            using SuperMetroid.Core.Enemies.Drawing;
            class EnemyDisplayConsumer {
                void Inspect(EnemySpritemapCatalog simple, EnemyExtendedFrameCatalog extended, EnemyFrameId frame) {
                    simple.TryGetDisplay(EnemyFrameId.{{simple.Frame}}, out _);
                    simple.TryGetDisplay(EnemyFrameId.Empty, out _);
                    simple.TryGetDisplay(frame, out _);
                    extended.TryGetDisplay(EnemyFrameId.{{extended.Frame}}, out _);
                    extended.TryGetDisplay(EnemyFrameId.{{simple.Frame}}, out _);
                    extended.TryGetDisplay(frame, out _);
                    extended.TryGetDisplay(EnemyFrameId.EmptyExtended, out _);
                }
            }
            """, path: "fixture-enemy-display-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("EnemyDisplayFixture", sources,
            platforms.Append(typeof(EnemySpritemapCatalog).Assembly.Location).Distinct().Select(path => MetadataReference.CreateFromFile(path)),
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
        Require(before.MissingCount == 5 && before.UnresolvedCount == 2 && before.Consumers.Count == 7, "original absent display adapter boundaries" + " (" + before.Describe() + ")");
        Require(after.MissingCount == 0 && after.UnresolvedCount == 2 && after.Classifications.Count == 5 && after.Consumers.Count == 7,
            "valid pairs and compiled empty fallback qualify; unowned pairs remain findings" + " (" + after.Describe() + ")");
        SyntaxTree extraDefinition = CSharpSyntaxTree.ParseText(
            $"namespace {typeof(EnemySpritemapDefinitions).Namespace}; internal static partial class {nameof(EnemySpritemapDefinitions)} {{ }}",
            path: "fixture-unreviewed-simple-definitions.cs");
        AuditReport revoked = Inspect(Compile(trees.Append(extraDefinition)), true);
        Require(revoked.Classifications.Count == 3 && revoked.UnresolvedCount == 4 && revoked.Consumers.Count == 7,
            "additional simple metadata declaration revokes only its rule, not the independent extended rule" + " (" + revoked.Describe() + ")");
        SyntaxTree extraGenerator = CSharpSyntaxTree.ParseText(
            $"namespace {typeof(EnemyExtendedFrameCompositions).Namespace}; internal static partial class {nameof(EnemyExtendedFrameCompositions)} {{ }}",
            path: "fixture-unreviewed-extended-compositions.cs");
        revoked = Inspect(Compile(trees.Append(extraGenerator)), true);
        Require(revoked.Classifications.Count == 2 && revoked.UnresolvedCount == 5,
            "additional extended-composition declaration must revoke extended availability" + " (" + revoked.Describe() + ")");
    }

    /// <summary>Requires malformed display artwork to fail with an asset-validation error.</summary>
    private static void Reject(Action action, string reason)
    {
        try { action(); }
        catch (InvalidDataException) { return; }
        throw new InvalidOperationException("Enemy display confirmation did not reject " + reason);
    }

    /// <summary>Reports a failed display-binding contract with the condition that failed.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Enemy display confirmation failed: " + reason);
    }
}
