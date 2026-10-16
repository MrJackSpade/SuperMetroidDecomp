using SuperMetroid.Tooling;
using SuperMetroid.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Rooms.Xray;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Confirms the identified X-ray identity boundary and complete item stores, without gameplay.</summary>
internal static class RoomRevealPresentationContractChecks
{
    /// <summary>Checks X-ray reveal identity and verifies only reviewed reveal and item calls gain closed presentation adapters.</summary>
    internal static void Run()
    {
        ConfirmSingleXrayIdentity();
        var trees = RoomRevealClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Rooms.LevelData;
            using SuperMetroid.Core.Rooms.Plms;
            using SuperMetroid.Core.Rooms.Xray;
            class RevealConsumer {
                void Inspect(XrayRevealVisualCatalog reveals, XrayOverlayVisualCatalog overlays,
                    RoomPlmDynamicCollectibleArtCatalog items) {
                    reveals.Apply(RoomCollisionType.ShootableBlock, 2);
                    reveals.Apply(RoomCollisionType.SolidBlock, 0);
                    reveals.Apply(RoomCollisionType.HorizontalExtension, 3);
                    overlays.ItemMetatile(0);
                    overlays.ItemMetatile(7);
                    overlays.ItemMetatile(8);
                    overlays.RoomTiles(0x986b);
                    items.Resolve(InWorldCollectibleKind.Bombs);
                    items.Resolve(InWorldCollectibleKind.ReserveTank);
                    items.Resolve(InWorldCollectibleKind.EnergyTank);
                }
            }
            """, path: "fixture-room-reveal-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("RoomRevealProviderFixture", sources,
            platforms.Append(typeof(XrayRevealVisualCatalog).Assembly.Location).Distinct()
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
        Require(before.UnresolvedCount == 10, "the fixture must first expose absent adapters" + " (" + before.Describe() + ")");
        Require(after.Classifications.Count == 8 && after.UnresolvedCount == 2 && after.MissingCount == 0 &&
            after.Consumers.Count == 10, "complete reveals, item slots and required overlays qualify, not invalid slots or non-dynamic kinds" + " (" + after.Describe() + ")");
    }

    /// <summary>Confirms one collision/BTS pair selects both its compiled reveal command and matching installed visual words.</summary>
    [ReflectionAudit("signature contract: the retired three-input Apply overload must not exist")]
    private static void ConfirmSingleXrayIdentity()
    {
        // Construct the already identified 305-entry provider contract from compiled
        // rules, not ROM or room input. Four chosen requests confirm this API change.
        var entries = new List<(RoomCollisionType, byte, XrayRevealVisualWords)>();
        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        for (int value = 0; value <= byte.MaxValue; value++)
            if (XrayRevealTable.Find(type, (byte)value) is { } rule && XrayRevealVisualCatalog.IsDrawable(rule.Command))
                entries.Add((type, (byte)value, new(1, 2, 3, 4)));
        var catalog = new XrayRevealVisualCatalog(entries);
        Require(catalog.Apply(RoomCollisionType.ShootableBlock, 2) is { } tall &&
            tall.Command == XrayRevealCommand.CopyTall && tall.TopLeft == 1 && tall.BottomLeft == 3,
            "one identity must select the compiled command and the matching installed operands");
        Require(catalog.Apply(RoomCollisionType.HorizontalExtension, 3) ==
            XrayRevealTable.Find(RoomCollisionType.HorizontalExtension, 3), "extensions must retain their compiled definition");
        Require(catalog.Apply(RoomCollisionType.SolidBlock, 0) is null &&
            catalog.Apply(RoomCollisionType.ShootableBlock, byte.MaxValue) is null,
            "unowned collision/BTS pairs must return native no-reveal, not demand missing artwork");
        Require(typeof(XrayRevealVisualCatalog).GetMethods().Where(method => method.Name == "Apply")
            .Single().GetParameters().Length == 2, "the mismatched third input must no longer exist");
    }

    /// <summary>Reports a failed room-reveal source contract with its specific predicate.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Room reveal provider confirmation failed: " + reason);
    }
}
