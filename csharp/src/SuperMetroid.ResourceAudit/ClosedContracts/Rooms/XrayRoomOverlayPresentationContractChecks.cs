using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Xray;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Confirms the identified missing required-overlay admission boundary with authored tiles.</summary>
internal static class XrayRoomOverlayPresentationContractChecks
{
    /// <summary>Checks required room overlays are copied and present, then
    /// verifies unsupported keys and a production fixture bypass revoke closure.</summary>
    internal static void Run()
    {
        bool rejected = false;
        try { _ = new XrayOverlayVisualCatalog(new ushort[8], []); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "production construction must reject missing required room overlays");
        XrayRoomOverlayDefinition first = XrayRoomOverlayDefinitions.All[0];
        XrayRoomOverlayVisual[] tiles = [new(new(1, 2), new RoomLevelWord(3))];
        var rooms = XrayRoomOverlayDefinitions.All.Select(overlay =>
            (overlay, (IReadOnlyList<XrayRoomOverlayVisual>)tiles)).ToArray();
        var catalog = new XrayOverlayVisualCatalog(new ushort[8], rooms);
        tiles[0] = new(new(0, 0), new RoomLevelWord(0));
        Require(catalog.RoomTiles(first).Single() == new XrayRoomOverlayVisual(new(1, 2), new RoomLevelWord(3)),
            "installed overlays must be independent of caller tile arrays");

        ClosedPresentationContract contract = RoomRevealClosedContractDefinitions.All.Single(item =>
            item.Type == typeof(XrayOverlayVisualCatalog).FullName);
        var trees = contract.Sources.Select(source => CSharpSyntaxTree.ParseText(
            File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText($$"""
            using SuperMetroid.Core.Rooms.Xray;
            class OverlayConsumer {
                void Inspect(XrayOverlayVisualCatalog overlays, XrayRoomOverlayDefinition selected) {
                    overlays.RoomTiles(XrayRoomOverlayDefinitions.{{first.Name}});
                    overlays.RoomTiles(selected);
                }
            }
            """, path: "fixture-room-overlay-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("XrayRoomOverlayFixture", sources,
            platforms.Append(typeof(XrayOverlayVisualCatalog).Assembly.Location).Distinct()
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
        Require(before.UnresolvedCount == 2 && after.Classifications.Count == 2 && after.UnresolvedCount == 0 &&
            after.MissingCount == 0 && after.Consumers.Count == 2, "typed overlay selections must be classified" + " (" + after.Describe() + ")");
    }

    /// <summary>Reports a violated X-ray overlay ownership contract.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("X-ray room-overlay confirmation failed: " + reason);
    }
}
