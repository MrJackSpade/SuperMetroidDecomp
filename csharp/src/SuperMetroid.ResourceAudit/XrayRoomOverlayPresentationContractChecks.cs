using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified missing required-overlay admission boundary with authored tiles.</summary>
internal static class XrayRoomOverlayPresentationContractChecks
{
    /// <summary>Confirms reviewed X-ray room overlay ownership and lookup behavior.</summary>
    internal static void Run()
    {
        bool rejected = false;
        try { _ = new XrayOverlayVisualCatalog(new ushort[8], []); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "production construction must reject missing required room overlays");
        ushort first = XrayRoomOverlaySourceDefinitions.All.First();
        XrayRoomOverlayVisual[] tiles = [new(1, 2, 3)];
        var rooms = XrayRoomOverlaySourceDefinitions.All.Select(pointer =>
            (pointer, (IReadOnlyList<XrayRoomOverlayVisual>)tiles)).ToArray();
        var catalog = new XrayOverlayVisualCatalog(new ushort[8], rooms);
        tiles[0] = new(0, 0, 0);
        Require(catalog.RoomTiles(first).Single() == new XrayRoomOverlayVisual(1, 2, 3),
            "installed overlays must be independent of caller tile arrays");

        ClosedPresentationContract contract = RoomRevealClosedContractDefinitions.All.Single(item =>
            item.Type == typeof(XrayOverlayVisualCatalog).FullName);
        var trees = contract.Sources.Select(source => CSharpSyntaxTree.ParseText(
            File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<room-overlay-usings>"));
        SyntaxTree calls = CSharpSyntaxTree.ParseText($$"""
            using SuperMetroid.Core.Rooms;
            class OverlayConsumer {
                void Inspect(XrayOverlayVisualCatalog overlays, ushort selected) {
                    overlays.RoomTiles({{first}});
                    overlays.RoomTiles(selected);
                    overlays.RoomTiles(0);
                }
            }
            """, path: "fixture-room-overlay-consumer.cs");
        trees.Add(calls);
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
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
        Require(before.UnresolvedCount == 3 && after.Classifications.Count == 2 && after.UnresolvedCount == 1 &&
            after.MissingCount == 0 && after.Consumers.Count == 3, "overlay completeness must not accept zero as a resource key");
        SyntaxTree bypass = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Rooms;
            class UnsafeOverlayFactory {
                object Make() => XrayOverlayVisualCatalog.FromOverlaysForVerification(null!, null!);
            }
            """, path: "fixture-production-overlay-bypass.cs");
        AuditReport revoked = Inspect(Compile(trees.Append(bypass)), true);
        Require(revoked.Classifications.Count == 0 && revoked.UnresolvedCount == 3,
            "Core use of the partial fixture factory must revoke required-overlay completeness");
    }

    /// <summary>Throws when an X-ray room overlay expectation is not satisfied.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("X-ray room-overlay confirmation failed: " + reason);
    }
}
