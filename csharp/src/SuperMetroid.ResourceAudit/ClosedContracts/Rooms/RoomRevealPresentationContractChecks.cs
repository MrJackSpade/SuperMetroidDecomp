using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Tooling;
using SuperMetroid.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Rooms.Xray;
using SuperMetroid.Core.Spatial;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Confirms the identified X-ray identity boundary and complete overlay stores, without gameplay.</summary>
internal static class RoomRevealPresentationContractChecks
{
    /// <summary>Checks X-ray reveal identity and verifies only reviewed reveal and overlay calls gain closed presentation adapters.</summary>
    internal static void Run()
    {
        ConfirmSingleXrayIdentity();
        var trees = RoomRevealClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Rooms.LevelData;
            using SuperMetroid.Core.Rooms.RoomObjects;
            using SuperMetroid.Core.Rooms.Xray;
            using SuperMetroid.Core.Samus.Abilities.Artwork;
            class RevealConsumer {
                void Inspect(InstalledXrayReveals reveals, InstalledXrayOverlays overlays) {
                    reveals.Apply(RoomCollisionType.ShootableBlock, RoomBlockBehavior.Reacting(ShootableReaction.RespawningBreak1x2));
                    reveals.Apply(RoomCollisionType.SolidBlock, RoomBlockBehavior.None);
                    reveals.Apply(RoomCollisionType.HorizontalExtension, RoomBlockBehavior.Extending(3));
                    overlays.TankBlock(InWorldCollectibleKind.EnergyTank);
                    overlays.TankBlock(InWorldCollectibleKind.PowerBombTank);
                    overlays.TankBlock(InWorldCollectibleKind.Bombs);
                    overlays.RoomBlocks(XrayRoomOverlayDefinitions.BombTorizoAnimalEscapeBlocks);
                }
            }
            """, path: "fixture-room-reveal-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("RoomRevealProviderFixture", sources,
            platforms.Append(typeof(InstalledXrayReveals).Assembly.Location).Distinct()
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
        Require(before.UnresolvedCount == 7, "the fixture must first expose absent adapters" + " (" + before.Describe() + ")");
        Require(after.Classifications.Count == 6 && after.UnresolvedCount == 1 && after.MissingCount == 0 &&
            after.Consumers.Count == 7, "complete reveals, tanks and required overlays qualify, not an item that is no tank" + " (" + after.Describe() + ")");
    }

    /// <summary>Confirms one collision kind and behaviour select both its compiled reveal command and matching installed blocks.</summary>
    [ReflectionAudit("signature contract: the retired three-input Apply overload must not exist")]
    private static void ConfirmSingleXrayIdentity()
    {
        // Construct the already identified 42-entry provider contract from compiled
        // rules, not ROM or room input. Four chosen requests confirm this API change.
        var entries = new List<XrayRevealVisualDefinition>();
        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        foreach (Enum? reaction in BlockReactionFamilies.ReadingsOf(type))
            if (XrayRevealTable.Find(type, reaction) is { } command && InstalledXrayReveals.IsDrawable(command))
                entries.Add(new(type, reaction, XrayRevealVisualBlocks.ForShape(command,
                    [.. new[] { 1, 2, 3, 4 }.Take(XrayRevealVisualBlocks.CountFor(command))])));
        // The overlays are required with the reveals (#1596); every required room overlay shows one block.
        var overlays = new InstalledXrayOverlays(Enumerable.Repeat(RoomBlockAppearance.Of(0), InstalledXrayOverlays.TankCount),
            XrayRoomOverlayDefinitions.All.Select(overlay => new XrayRoomOverlayBlocks(overlay,
                [new XrayRoomOverlayVisual(Position16<RoomBlocks>.Of(0, 0), RoomBlockAppearance.Of(0))])));
        // The block a backward extension exit shows is installed with them (#1615).
        var catalog = new InstalledXrayReveals(entries, overlays, extensionExitBlock: 5);
        Require(catalog.Apply(RoomCollisionType.ShootableBlock, RoomBlockBehaviorCodec.Decode(2)) is { } tall &&
            tall.Shape == XrayRevealShape.CopyTall && tall.Blocks == XrayRevealVisualBlocks.Tall(1, 2),
            "one identity must select the compiled command and the matching installed operands");
        Require(catalog.Apply(RoomCollisionType.HorizontalExtension, RoomBlockBehaviorCodec.Decode(3)) ==
            new XrayRevealDefinition(XrayRevealShape.HorizontalExtension, null), "extensions must retain their compiled command and show no blocks");
        Require(catalog.Apply(RoomCollisionType.SolidBlock, RoomBlockBehaviorCodec.Decode(0)) is null &&
            catalog.Apply(RoomCollisionType.ShootableBlock, RoomBlockBehaviorCodec.Decode(byte.MaxValue)) is null,
            "unowned collision/BTS pairs must return native no-reveal, not demand missing artwork");
        Require(typeof(InstalledXrayReveals).GetMethods().Where(method => method.Name == "Apply")
            .Single().GetParameters().Length == 2, "the mismatched third input must no longer exist");
    }

    /// <summary>Reports a failed room-reveal source contract with its specific predicate.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Room reveal provider confirmation failed: " + reason);
    }
}
