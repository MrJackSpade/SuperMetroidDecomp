using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>
/// Confirms the room-object frame-picture contracts (#1732) on constructed consumer calls, never an installation: a frame
/// the drawing computes classifies under each family's reviewed loader, and a construction of a family's pictures outside
/// that loader revokes only that family's proof.
/// </summary>
internal static class RoomObjectFramePresentationContractChecks
{
    /// <summary>Compiles the reviewed room-object sources with two consumer calls, then checks each classification.</summary>
    internal static void Run()
    {
        var trees = RoomObjectFrameClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Rooms.RoomObjects;
            class RoomObjectFrameConsumer {
                void Inspect(InstalledRoomObjectBotwoonWallVisuals wall, InstalledRoomObjectBlueDoorVisuals door, RoomObjectDraw draw) {
                    wall.Image(draw);
                    door.Image(draw);
                }
            }
            """, path: "fixture-room-object-frame-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation compilation = CSharpCompilation.Create("RoomObjectFrameProviderFixture", trees,
            platforms.Append(typeof(SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectBotwoonWallVisuals).Assembly.Location)
                .Append(typeof(SuperMetroid.AssetExtraction.Rooms.RoomObjectBotwoonWallVisualFiles).Assembly.Location).Distinct()
                .Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        FixtureConsumerBinding.Require(compilation, calls, nameof(RoomObjectFramePresentationContractChecks));
        AuditReport before = Inspect(compilation, calls, adapted: false), after = Inspect(compilation, calls, adapted: true);
        Require(before.UnresolvedCount == 2, "constructed calls must first expose the missing adapter (" + before.Describe() + ")");
        Require(after.Classifications.Count == 2 && after.UnresolvedCount == 0 && after.MissingCount == 0 && after.Consumers.Count == 2,
            "a computed frame of each reviewed family classifies (" + after.Describe() + ")");

        // Only a family's reviewed loader proves its pictures complete; a construction elsewhere revokes that family alone.
        CSharpCompilation forged = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Rooms.RoomObjects;
            class FramePictureForger {
                InstalledRoomObjectBotwoonWallVisuals Forge() => new InstalledRoomObjectBotwoonWallVisuals(null!);
            }
            """, path: "fixture-room-object-frame-forger.cs"));
        AuditReport revoked = Inspect(forged, calls, adapted: true);
        Require(revoked.Classifications.Count == 1 && revoked.UnresolvedCount == 1 && revoked.Consumers.Count == 2,
            "a construction of a family's pictures outside its loader must revoke that family's proof (" + revoked.Describe() + ")");
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

    /// <summary>Fails the room-object frame confirmation with the classification that did not hold.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Room-object frame provider confirmation failed: " + reason);
    }
}
