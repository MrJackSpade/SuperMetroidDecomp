using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified static selector/length contract and its revocation guards.</summary>
internal static class EnemyArtworkStaticContractChecks
{
    internal static void Run()
    {
        var trees = EnemyArtworkClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .Select(source => CSharpSyntaxTree.ParseText(File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<enemy-artwork-usings>"));
        EnemyTileSourceDefinition first = EnemyTileSourceDefinitions.All[0];
        CeresEscapeTileSheetDefinition warning = CeresEscapeTileArtworkDefinitions.WarningText;
        CeresEscapeOverlayTilemapDefinition overlay = CeresEscapeOverlayTilemapDefinitions.Emergency;
        SyntaxTree calls = CSharpSyntaxTree.ParseText($$"""
            using SuperMetroid.Core.Assets;
            using SuperMetroid.Core.Hardware;
            class EnemyArtworkConsumer {
                void Inspect(EnemyTileArtworkCatalog art, ushort pointer, int source, int length, SnesVram vram, SnesCgram cgram) {
                    art.LoadTo({{first.DefinitionPointer}}, {{first.ByteCount}}, vram, 0);
                    art.LoadTo({{first.DefinitionPointer}}, {{first.ByteCount + 1}}, vram, 0);
                    art.LoadTo(0, {{first.ByteCount}}, vram, 0);
                    art.LoadPaletteTo({{first.DefinitionPointer}}, cgram, 0);
                    art.LoadPaletteTo(0, cgram, 0);
                    art.TryResolve({{first.SourceAddress}}, {{first.ByteCount}}, out _);
                    art.TryResolve({{first.SourceAddress}}, 1, out _);
                    art.TryResolve({{warning.SourceAddress + 1}}, 1, out _);
                    art.TryResolve({{warning.SourceAddress}}, int.MaxValue, out _);
                    art.TryResolve({{overlay.SourceAddress}}, {{overlay.WordCount * sizeof(ushort)}}, out _);
                    art.TryResolve({{overlay.SourceAddress}}, 1, out _);
                    art.TryResolve(0, int.MinValue, out _);
                    art.LoadTo(pointer, length, vram, 0);
                    art.LoadPaletteTo(pointer, cgram, 0);
                    art.TryResolve(source, length, out _);
                }
            }
            """, path: "fixture-enemy-artwork-consumer.cs");
        trees.Add(calls);
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
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
        Require(before.UnresolvedCount == 15 && before.Consumers.Count == 15, "all identified boundaries are retained before adaptation");
        Require(after.Classifications.Count == 9 && after.UnresolvedCount == 6 && after.MissingCount == 0 && after.Consumers.Count == 15,
            "valid identities/lengths qualify while invalid known selectors remain findings");
        const string construction = "csharp/src/SuperMetroid.Core/Assets/EnemyTileArtworkCatalog.Construction.cs";
        var stale = trees.Select(tree => tree.FilePath == construction ? CSharpSyntaxTree.ParseText(
            tree.GetText().ToString().Replace("sheet.Transfer.Length != definition.ByteCount", "false", StringComparison.Ordinal),
                path: tree.FilePath) : tree);
        Revoked(Inspect(Compile(stale), true), "removed native size admission");
        SyntaxTree bypass = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Assets;
            class PartialArtworkConsumer {
                object Build() => EnemyTileArtworkCatalog.FromArtworkForVerification(
                    new System.Collections.Generic.Dictionary<ushort, RoomCharacterAtlas>(),
                    new System.Collections.Generic.Dictionary<ushort, EnemyPaletteSheet>());
            }
            """, path: "fixture-core-partial-artwork.cs");
        Revoked(Inspect(Compile(trees.Append(bypass)), true), "Core use of partial verification construction");
        SyntaxTree extraMetadata = CSharpSyntaxTree.ParseText(
            "namespace SuperMetroid.Core.Game; public static partial class RoomEnemyGraphicsSetDefinitions { }",
            path: "fixture-unreviewed-enemy-metadata.cs");
        Revoked(Inspect(Compile(trees.Append(extraMetadata)), true), "additional metadata declaration");
    }

    private static void Revoked(AuditReport report, string reason) => Require(
        report.Classifications.Count == 0 && report.UnresolvedCount == 15 && report.Consumers.Count == 15, reason);

    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Enemy artwork static confirmation failed: " + reason);
    }
}
