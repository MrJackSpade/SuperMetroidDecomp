using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified required graphics-set sources and contiguous sky upload rules.</summary>
internal static class RoomArtworkPresentationContractChecks
{
    internal static void Run()
    {
        var trees = RoomArtworkClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<room-art-usings>"));
        string consumer = """
            using SuperMetroid.Core.Assets;
            class RoomArtConsumer {
                void Inspect(RoomCharacterAtlasCatalog characters, RoomMetatileCatalog metatiles,
                    RoomStaticPaletteCatalog colors, RoomSkyTilemapCatalog sky, RoomBackgroundTilemapCatalog library) {
                    characters.Get(CHARACTER_SOURCE);
                    characters.Get(0);
                    metatiles.Get(METATILE_SOURCE);
                    metatiles.Get(0);
                    colors.Get(PALETTE_SOURCE);
                    colors.Get(0);
                    sky.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress, RoomSkyTilemapFormat.PageByteCount, out _);
                    sky.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress + 64, 64, out _);
                    sky.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress + 2, RoomSkyTilemapFormat.PageByteCount, out _);
                    sky.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress + 2, 64, out _);
                    sky.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress + 1, 64, out _);
                    sky.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress + RoomSkyTilemapFormat.TotalByteCount - 64, 64, out _);
                    sky.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress + RoomSkyTilemapFormat.TotalByteCount - 2, 64, out _);
                    sky.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress, 0, out _);
                    sky.TryResolve(0, 0, out _);
                    sky.TryResolve(RoomSkyTilemapFormat.FirstSourceAddress - 1, int.MaxValue, out _);
                    library.Get(0);
                }
            }
            """;
        TilesetDefinition first = RoomTilesetDefinitions.Get(0);
        consumer = consumer.Replace("CHARACTER_SOURCE", first.CharacterAddress.ToString(), StringComparison.Ordinal)
            .Replace("METATILE_SOURCE", first.BlockDefinitionsAddress.ToString(), StringComparison.Ordinal)
            .Replace("PALETTE_SOURCE", first.PaletteAddress.ToString(), StringComparison.Ordinal);
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-room-art-consumer.cs");
        trees.Add(calls);
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("RoomArtworkProviderFixture", sources,
            platforms.Append(typeof(RoomCharacterAtlasCatalog).Assembly.Location).Distinct()
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
        Require(before.UnresolvedCount == 17, "constructed calls must first expose absent adapters");
        Require(after.Classifications.Count == 9 && after.UnresolvedCount == 8 && after.MissingCount == 0 &&
            after.Consumers.Count == 17, "required sources and complete sky page/row/false queries qualify, not optional keys or bad transfers");
        Require(after.Findings.Any(item => item.Owner == "RoomBackgroundTilemapCatalog.Get"),
            "an unowned background source must not borrow required graphics-set source closure");
        const string catalog = "csharp/src/SuperMetroid.Core/Assets/RoomCharacterAtlasCatalog.cs";
        var changed = trees.Select(tree => tree.FilePath == catalog ? CSharpSyntaxTree.ParseText(
            tree.GetText().ToString().Replace("atlas is null", "false", StringComparison.Ordinal), path: tree.FilePath) : tree);
        AuditReport revoked = Inspect(Compile(changed), true);
        Require(revoked.Classifications.Count == 8 && revoked.UnresolvedCount == 9 &&
            revoked.Findings.Count(item => item.Owner == "RoomCharacterAtlasCatalog.Get" &&
                item.Message.Contains("stale", StringComparison.Ordinal)) == 2,
            "changed required-source admission must revoke character membership proof");
    }

    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Room artwork provider confirmation failed: " + reason);
    }
}
