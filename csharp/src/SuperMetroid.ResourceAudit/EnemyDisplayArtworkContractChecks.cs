using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified display-binding availability contracts, not enemy behavior.</summary>
internal static class EnemyDisplayArtworkContractChecks
{
    internal static void Run()
    {
        var simpleFrames = EnemySpritemapDefinitions.Frames.ToArray().ToDictionary(frame => frame.Name, _ => Array.Empty<SpriteVisualPart>());
        var simpleBindings = simpleFrames.Keys.ToDictionary(name => name, name => name);
        var simpleDocument = new EnemySpritemapDocument {
            Version = (int)EnemySpritemapSchema.Current, Frames = simpleFrames, DisplayFrames = simpleBindings };
        Stream Json<T>(T document) => new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemySpritemapCatalog simple = EnemySpritemapCatalog.Load(Json(simpleDocument));
        EnemySpritemapDefinition simpleId = EnemySpritemapDefinitions.Frames[0];
        Require(simple.TryGetDisplay(simpleId.Bank, simpleId.Pointer, out var parts) && parts.IsEmpty,
            "installed simple binding resolves authored parts");
        simpleBindings[simpleId.Name] = "unowned-target";
        Reject(() => EnemySpritemapCatalog.Load(Json(simpleDocument)), "unowned simple display target");
        simpleBindings[simpleId.Name] = simpleId.Name;

        var extendedFrames = EnemyExtendedFrameDefinitions.Frames.ToArray().ToDictionary(frame => frame.Name, _ => new[] {
            new EnemyExtendedVisualComponent { OffsetX = 0, OffsetY = 0, Parts = [] } });
        var extendedBindings = extendedFrames.Keys.ToDictionary(name => name, name => name);
        var extendedDocument = new EnemyExtendedFrameDocument {
            Version = (int)EnemyExtendedFrameSchema.Current, Frames = extendedFrames, DisplayFrames = extendedBindings };
        EnemyExtendedFrameCatalog extended = EnemyExtendedFrameCatalog.Load(Json(extendedDocument));
        EnemyExtendedFrameDefinition extendedId = EnemyExtendedFrameDefinitions.Frames.First();
        Require(extended.TryGetDisplay(extendedId.Bank, extendedId.Pointer, out var components) && components.Length == 1 &&
            extended.TryGetDisplay(0xa5, CommonEnemyEmptyExtendedFrameDefinitions.Frame, out components) && components.IsEmpty,
            "installed extended binding and compiled empty fallback");
        extendedBindings[extendedId.Name] = "unowned-target";
        Reject(() => EnemyExtendedFrameCatalog.Load(Json(extendedDocument)), "unowned extended display target");
        ConfirmStaticCalls(simpleId, extendedId);
    }

    private static void ConfirmStaticCalls(EnemySpritemapDefinition simple, EnemyExtendedFrameDefinition extended)
    {
        var trees = EnemyDisplayArtworkClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<enemy-display-usings>"));
        SyntaxTree calls = CSharpSyntaxTree.ParseText($$"""
            using SuperMetroid.Core.Assets;
            class EnemyDisplayConsumer {
                void Inspect(EnemySpritemapCatalog simple, EnemyExtendedFrameCatalog extended, byte bank, ushort pointer) {
                    simple.TryGetDisplay({{simple.Bank}}, {{simple.Pointer}}, out _);
                    simple.TryGetDisplay(0, 0, out _);
                    simple.TryGetDisplay(bank, pointer, out _);
                    extended.TryGetDisplay({{extended.Bank}}, {{extended.Pointer}}, out _);
                    extended.TryGetDisplay(0, 0, out _);
                    extended.TryGetDisplay(bank, pointer, out _);
                    extended.TryGetDisplay(165, {{CommonEnemyEmptyExtendedFrameDefinitions.Frame}}, out _);
                }
            }
            """, path: "fixture-enemy-display-consumer.cs");
        trees.Add(calls);
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
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
        Require(before.MissingCount == 5 && before.UnresolvedCount == 2 && before.Consumers.Count == 7, "original absent display adapter boundaries");
        Require(after.MissingCount == 0 && after.UnresolvedCount == 2 && after.Classifications.Count == 5 && after.Consumers.Count == 7,
            "valid pairs and compiled empty fallback qualify; unowned pairs remain findings");
        const string provider = "csharp/src/SuperMetroid.Core/Assets/EnemySpritemapCatalog.cs";
        var stale = trees.Select(tree => tree.FilePath == provider ? CSharpSyntaxTree.ParseText(
            tree.GetText().ToString().Replace("document.DisplayFrames.Count != identities.Count", "false", StringComparison.Ordinal),
                path: tree.FilePath) : tree);
        AuditReport revoked = Inspect(Compile(stale), true);
        Require(revoked.Classifications.Count == 0 && revoked.UnresolvedCount == 7,
            "changed shared binding admission revokes both actual artwork rules");
        SyntaxTree extraDefinition = CSharpSyntaxTree.ParseText(
            "namespace SuperMetroid.Core.Assets; internal static partial class EnemySpritemapDefinitions { }",
            path: "fixture-unreviewed-simple-definitions.cs");
        revoked = Inspect(Compile(trees.Append(extraDefinition)), true);
        Require(revoked.Classifications.Count == 3 && revoked.UnresolvedCount == 4 && revoked.Consumers.Count == 7,
            "additional simple metadata declaration revokes only its rule, not the independent extended rule");
        foreach (var (file, original, replacement) in new[] {
            ("EnemyExtendedFrameSequence", "yield return frame;", "yield return default;"),
            ("BossOamFrameDefinitions", "RidleyStart + 34 * index", "RidleyStart + 10 * index"),
            ("PirateArtworkNameDefinitions", "walking_pirate_look_shared", "unreviewed_shared_key") })
        {
            string path = $"csharp/src/SuperMetroid.Core/Assets/{file}.cs";
            var changed = trees.Select(tree => tree.FilePath == path ? CSharpSyntaxTree.ParseText(
                tree.GetText().ToString().Replace(original, replacement, StringComparison.Ordinal), path: path) : tree);
            revoked = Inspect(Compile(changed), true);
            Require(revoked.Classifications.Count == 2 && revoked.UnresolvedCount == 5 &&
                revoked.Findings.Count(item => item.Message.Contains("stale", StringComparison.Ordinal)) == 4,
                $"changed {file} must revoke extended availability while preserving the simple rule");
        }
        SyntaxTree extraGenerator = CSharpSyntaxTree.ParseText(
            "namespace SuperMetroid.Core.Assets; internal static partial class PirateArtworkNameDefinitions { }",
            path: "fixture-unreviewed-pirate-name-generator.cs");
        revoked = Inspect(Compile(trees.Append(extraGenerator)), true);
        Require(revoked.Classifications.Count == 2 && revoked.UnresolvedCount == 5,
            "additional generator declaration must revoke extended availability");
    }

    private static void Reject(Action action, string reason)
    {
        try { action(); }
        catch (InvalidDataException) { return; }
        throw new InvalidOperationException("Enemy display confirmation did not reject " + reason);
    }

    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Enemy display confirmation failed: " + reason);
    }
}
