using SuperMetroid.Core;
using System.Text.Json.Nodes;
using SuperMetroid.Tooling;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Confirms the identified dynamic projectile availability boundary without projectile simulation.</summary>
internal static class EnemyProjectileArtworkContractChecks
{
    /// <summary>Checks complete authored frame admission and static provider coverage without projectile simulation.</summary>
    internal static void Run()
    {
        var direct = EnemyProjectileSpritemapDefinitions.FrameNames.ToDictionary(name => name, _ => Array.Empty<SpriteVisualPart>());
        var programs = EnemyProjectilePresentationFrameSets.All.ToDictionary(frame => frame.Name, _ => Array.Empty<SpriteVisualPart>());
        var document = new EnemyProjectileSpritemapDocument {
            Version = (int)EnemyProjectileSpritemapVersion.Current, Frames = direct, ProgramFrames = programs };
        EnemyProjectileSpritemapCatalog Load(EnemyProjectileSpritemapDocument value, EnemyProjectileSpritemapCatalog? stock = null) =>
            EnemyProjectileSpritemapDocuments.Load(new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(value,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })), stock);
        EnemyProjectileSpritemapCatalog catalog = Load(document);
        EnemyProjectileFrameId directId = EnemyProjectileFrames.Identity(EnemyProjectileSpritemapDefinitions.FrameNames[0]);
        EnemyProjectilePresentationFrameDefinition program = EnemyProjectilePresentationFrameSets.All[0];
        Require(catalog.Get(directId).IsEmpty && catalog.GetProgramFrame(program.Frame).IsEmpty &&
            catalog.Get(EnemyProjectileFrameId.Blank).IsEmpty, "complete authored empty compositions and native blank");
        string directName = EnemyProjectileSpritemapDefinitions.FrameNames[0];
        var camelCase = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        JsonNode withNullFrame = RequiredJson.Parse(JsonSerializer.SerializeToUtf8Bytes(document, camelCase));
        withNullFrame.Required("frames")[directName] = null;
        Reject(() => EnemyProjectileSpritemapDocuments.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(withNullFrame.ToJsonString())), null),
            "null direct composition");
        direct.Remove(directName); direct.Add("unowned-direct", []);
        Reject(() => Load(document), "same-count substituted direct composition");
        direct.Remove("unowned-direct"); direct.Add(directName, []);
        programs.Remove(program.Name); programs.Add("unowned-program", []);
        Reject(() => Load(document), "same-count substituted program composition");
        var legacy = document with { Version = (int)EnemyProjectileSpritemapVersion.CeresOnly,
            Frames = EnemyProjectileSpritemapDefinitions.FrameNames.Take(EnemyProjectileSpritemapDocuments.LegacyFrameCount)
                .ToDictionary(name => name, _ => Array.Empty<SpriteVisualPart>()), ProgramFrames = null };
        Reject(() => Load(legacy), "legacy stock without complete inheritance");
        EnemyProjectileSpritemapCatalog merged = Load(legacy, catalog);
        Require(merged.GetProgramFrame(program.Frame).IsEmpty &&
            merged.Get(EnemyProjectileFrames.Identity(EnemyProjectileSpritemapDefinitions.FrameNames[^1])).IsEmpty,
            "legacy override inherits program and later direct frames from validated private stock");
        ConfirmStaticCalls(directId, program.Frame);
    }

    /// <summary>Compiles direct and program-frame callers to verify valid IDs and stale-definition rejection.</summary>
    private static void ConfirmStaticCalls(EnemyProjectileFrameId directId, EnemyProjectileFrameId programId)
    {
        var trees = EnemyProjectileArtworkClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .Select(source => CSharpSyntaxTree.ParseText(File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText($$"""
            using SuperMetroid.AssetExtraction.Enemies;
            using SuperMetroid.Core.Enemies.Projectiles;
            class ProjectileArtworkConsumer {
                void Inspect(EnemyProjectileSpritemapCatalog art, EnemyProjectileFrameId frame, EnemyProjectileFrameId program) {
                    art.Get(EnemyProjectileFrameId.{{directId}});
                    art.Get(EnemyProjectileFrameId.Blank);
                    art.Get(0);
                    art.Get(frame);
                    art.GetProgramFrame(EnemyProjectileFrameId.{{programId}});
                    art.GetProgramFrame(0);
                    art.GetProgramFrame(program);
                }
            }
            """, path: "fixture-enemy-projectile-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("EnemyProjectileArtworkFixture", sources,
            platforms.Append(typeof(EnemyProjectileSpritemapCatalog).Assembly.Location)
                .Append(typeof(EnemyProjectileFrameId).Assembly.Location).Distinct()
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
        Require(before.MissingCount == 5 && before.UnresolvedCount == 2 && before.Consumers.Count == 7, "original absent adapter boundaries" + " (" + before.Describe() + ")");
        Require(after.MissingCount == 0 && after.UnresolvedCount == 2 && after.Classifications.Count == 5 && after.Consumers.Count == 7,
            "valid direct/program IDs and blank qualify; unknown IDs stay findings" + " (" + after.Describe() + ")");
        SyntaxTree external = CSharpSyntaxTree.ParseText("""
            namespace SuperMetroid.AssetExtraction.Enemies;
            class ExternalProjectileDefinitions {
                object Escape() => EnemyProjectileSpritemapDefinitions.FrameNames;
            }
            """, path: "fixture-external-projectile-array.cs");
        AuditReport revoked = Inspect(Compile(trees.Append(external)), true);
        Require(revoked.Classifications.Count == 0 && revoked.UnresolvedCount == 7, "external mutable frame-array reference revokes completeness" + " (" + revoked.Describe() + ")");
    }

    /// <summary>Requires malformed or incomplete projectile documents to fail during load.</summary>
    private static void Reject(Action action, string reason)
    {
        try { action(); }
        catch (InvalidDataException) { return; }
        throw new InvalidOperationException("Projectile artwork confirmation did not reject " + reason);
    }

    /// <summary>Reports the source-only projectile ownership expectation that failed.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Projectile artwork confirmation failed: " + reason);
    }
}
