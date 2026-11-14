using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>
/// Confirms the pre-coloured room picture contracts (#1536) on constructed consumer calls, never a room or an installation:
/// each lookup the reviewed sources prove complete classifies, a constant outside the proved domain fails, and a
/// construction of the installed room artwork outside its reviewed loader revokes the loader's proof.
/// </summary>
internal static class RoomPicturePresentationContractChecks
{
    /// <summary>Compiles the reviewed room picture sources with sixteen consumer calls, then checks each classification.</summary>
    internal static void Run()
    {
        var trees = RoomPictureClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Fx;
            using SuperMetroid.Core.Fx.Artwork;
            using SuperMetroid.Core.Rooms;
            using SuperMetroid.Core.Rooms.Artwork;
            using SuperMetroid.Core.Rooms.Backgrounds;
            using SuperMetroid.Core.Rooms.Doors;
            using SuperMetroid.Core.Rooms.LevelData;
            using SuperMetroid.Core.Rooms.Scrolling;
            class RoomPictureConsumer {
                void Inspect(InstalledRoomArtwork artwork, RoomFloorSheet floor, MessageBoxSheet messages, WrittenPictureSheet overlay,
                    RoomAppearanceView appearance, TilesetDefinition tileset, LibraryBackground background, DoorDefinition? door,
                    CeresFloorAnimationFrame frame, ShownMessage shown, RoomFxType effect) {
                    artwork.Get(tileset);
                    artwork.FloorOf(tileset);
                    artwork.MessageBox(tileset);
                    artwork.FxOverlay(tileset);
                    artwork.Background(background, door, tileset);
                    artwork.Sky(ScrollingSkyChunkTable.Land, tileset);
                    artwork.Sky((ScrollingSkyChunkTable)99, tileset);
                    floor.Picture(frame);
                    floor.Picture(CeresFloorAnimationFrame.WingsLowered);
                    floor.Picture((CeresFloorAnimationFrame)99);
                    messages.Picture(shown);
                    messages.Picture(new ShownMessage(GameplayMessageId.EnergyTank, null, null));
                    messages.Picture(new ShownMessage((GameplayMessageId)999, null, null));
                    RoomEffectPictures.Artwork(overlay, effect, appearance);
                    RoomEffectPictures.Artwork(overlay, RoomFxType.Lava, appearance);
                    RoomEffectPictures.Artwork(overlay, RoomFxType.None, appearance);
                }
            }
            """, path: "fixture-room-picture-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation compilation = CSharpCompilation.Create("RoomPictureProviderFixture", trees,
            platforms.Append(typeof(SuperMetroid.Core.Rooms.Artwork.InstalledRoomArtwork).Assembly.Location)
                .Append(typeof(SuperMetroid.AssetExtraction.Installation.RoomBlockSheetFiles).Assembly.Location).Distinct()
                .Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        FixtureConsumerBinding.Require(compilation, calls, nameof(RoomPicturePresentationContractChecks));
        AuditReport before = Inspect(compilation, calls, adapted: false), after = Inspect(compilation, calls, adapted: true);
        Require(before.UnresolvedCount == 16, "constructed calls must first expose the missing adapters (" + before.Describe() + ")");
        Require(after.Classifications.Count == 12 && after.UnresolvedCount == 4 && after.MissingCount == 0 && after.Consumers.Count == 16,
            "proved lookups classify and constants outside the proved domains fail (" + after.Describe() + ")");

        // Only the reviewed loader's checks prove the installed room artwork complete; a construction elsewhere revokes them.
        CSharpCompilation forged = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Rooms.Artwork;
            class ArtworkForger {
                InstalledRoomArtwork Forge() => new InstalledRoomArtwork(null!, null!, null!, null!, null!, null!, null!);
            }
            """, path: "fixture-room-artwork-forger.cs"));
        AuditReport revoked = Inspect(forged, calls, adapted: true);
        Require(revoked.Classifications.Count == 6 && revoked.UnresolvedCount == 10 && revoked.Consumers.Count == 16,
            "a construction of the installed room artwork outside its loader must revoke its proof (" + revoked.Describe() + ")");
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

    /// <summary>Fails the room picture confirmation with the classification that did not hold.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Room picture provider confirmation failed: " + reason);
    }
}
