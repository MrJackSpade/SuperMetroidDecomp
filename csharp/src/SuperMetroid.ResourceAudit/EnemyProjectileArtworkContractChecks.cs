using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified dynamic projectile availability boundary without projectile simulation.</summary>
internal static class EnemyProjectileArtworkContractChecks
{
    /// <summary>Confirms direct and program-selected enemy projectile artwork admission.</summary>
    internal static void Run()
    {
        var direct = EnemyProjectileSpritemapDefinitions.Frames.ToDictionary(frame => frame.Name, _ => Array.Empty<SpriteVisualPart>());
        var programs = EnemyProjectilePresentationFrameDefinitions.All.ToArray().ToDictionary(frame => frame.Name, _ => Array.Empty<SpriteVisualPart>());
        var document = new EnemyProjectileSpritemapDocument {
            Version = EnemyProjectileSpritemapDefinitions.Version, Frames = direct, ProgramFrames = programs };
        EnemyProjectileSpritemapCatalog Load(EnemyProjectileSpritemapDocument value, EnemyProjectileSpritemapCatalog? stock = null) =>
            EnemyProjectileSpritemapCatalog.Load(new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(value,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })), stock);
        EnemyProjectileSpritemapCatalog catalog = Load(document);
        ushort directId = EnemyProjectileSpritemapDefinitions.Frames[0].Pointer;
        EnemyProjectilePresentationFrameDefinition program = EnemyProjectilePresentationFrameDefinitions.All[0];
        Require(catalog.Get(directId).IsEmpty && catalog.GetProgramFrame(program.OperandAddress).IsEmpty &&
            catalog.Get(EnemyProjectileSpritemapDefinitions.BlankSpritemap).IsEmpty, "complete authored empty compositions and native blank");
        string directName = EnemyProjectileSpritemapDefinitions.Frames[0].Name;
        direct[directName] = null!;
        Reject(() => Load(document), "null direct composition");
        direct.Remove(directName); direct.Add("unowned-direct", []);
        Reject(() => Load(document), "same-count substituted direct composition");
        direct.Remove("unowned-direct"); direct.Add(directName, []);
        programs.Remove(program.Name); programs.Add("unowned-program", []);
        Reject(() => Load(document), "same-count substituted program composition");
        var legacy = document with { Version = EnemyProjectileSpritemapDefinitions.CeresOnlyVersion,
            Frames = EnemyProjectileSpritemapDefinitions.Frames.Take(EnemyProjectileSpritemapDefinitions.LegacyFrameCount)
                .ToDictionary(frame => frame.Name, _ => Array.Empty<SpriteVisualPart>()), ProgramFrames = null };
        Reject(() => Load(legacy), "legacy stock without complete inheritance");
        EnemyProjectileSpritemapCatalog merged = Load(legacy, catalog);
        Require(merged.GetProgramFrame(program.OperandAddress).IsEmpty &&
            merged.Get(EnemyProjectileSpritemapDefinitions.Frames[^1].Pointer).IsEmpty,
            "legacy override inherits program and later direct frames from validated private stock");
        ConfirmStaticCalls(directId, program.OperandAddress);
    }

    /// <summary>Confirms static projectile display calls classify only admitted frame identities.</summary>
    private static void ConfirmStaticCalls(ushort directId, ushort programId)
    {
        var trees = EnemyProjectileArtworkClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .Select(source => CSharpSyntaxTree.ParseText(File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<enemy-projectile-usings>"));
        SyntaxTree calls = CSharpSyntaxTree.ParseText($$"""
            using SuperMetroid.Core.Assets;
            class ProjectileArtworkConsumer {
                void Inspect(EnemyProjectileSpritemapCatalog art, ushort pointer) {
                    art.Get({{directId}});
                    art.Get(EnemyProjectileSpritemapDefinitions.BlankSpritemap);
                    art.Get(0);
                    art.Get(pointer);
                    art.GetProgramFrame({{programId}});
                    art.GetProgramFrame(0);
                    art.GetProgramFrame(pointer);
                }
            }
            """, path: "fixture-enemy-projectile-consumer.cs");
        trees.Add(calls);
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("EnemyProjectileArtworkFixture", sources,
            platforms.Append(typeof(EnemyProjectileSpritemapCatalog).Assembly.Location).Distinct()
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
        Require(before.MissingCount == 5 && before.UnresolvedCount == 2 && before.Consumers.Count == 7, "original absent adapter boundaries");
        Require(after.MissingCount == 0 && after.UnresolvedCount == 2 && after.Classifications.Count == 5 && after.Consumers.Count == 7,
            "valid direct/program IDs and blank qualify; unknown IDs stay findings");
        const string provider = "csharp/src/SuperMetroid.Core/Assets/EnemyProjectileSpritemapCatalog.cs";
        var stale = trees.Select(tree => tree.FilePath == provider ? CSharpSyntaxTree.ParseText(
            tree.GetText().ToString().Replace("document.ProgramFrames.Count != definitions.Length", "false", StringComparison.Ordinal),
                path: tree.FilePath) : tree);
        AuditReport revoked = Inspect(Compile(stale), true);
        Require(revoked.Classifications.Count == 0 && revoked.UnresolvedCount == 7, "changed program admission revokes completeness");
        SyntaxTree external = CSharpSyntaxTree.ParseText("""
            namespace SuperMetroid.Core.Assets;
            class ExternalProjectileDefinitions {
                object Escape() => EnemyProjectileSpritemapDefinitions.Frames;
            }
            """, path: "fixture-external-projectile-array.cs");
        revoked = Inspect(Compile(trees.Append(external)), true);
        Require(revoked.Classifications.Count == 0 && revoked.UnresolvedCount == 7, "external mutable frame-array reference revokes completeness");
    }

    /// <summary>Requires a projectile artwork construction action to reject invalid data.</summary>
    private static void Reject(Action action, string reason)
    {
        try { action(); }
        catch (InvalidDataException) { return; }
        throw new InvalidOperationException("Projectile artwork confirmation did not reject " + reason);
    }

    /// <summary>Throws when a projectile artwork expectation is not satisfied.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Projectile artwork confirmation failed: " + reason);
    }
}
