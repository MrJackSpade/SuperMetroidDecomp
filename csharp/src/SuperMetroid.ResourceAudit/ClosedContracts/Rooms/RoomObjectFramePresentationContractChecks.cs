using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>
/// Confirms the room-object frame-picture contracts (#1732) on constructed consumer calls, never an installation: a frame
/// each reviewed family's descriptor computes classifies through the room's composed read of every family's pictures
/// (#2571), and a construction of one family's pictures outside its loader revokes that family's proof, no other family's,
/// and the composed read that rests on it.
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
        // A computed frame of two reviewed families, the Botwoon wall's clear and a blue door's closed cap drawn solid, each
        // read as the room reads it.
        SyntaxTree calls = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Rooms.Doors;
            using SuperMetroid.Core.Rooms.RoomObjects;
            using SuperMetroid.Core.World;
            class RoomObjectFrameConsumer {
                void Inspect(RoomObjectVisuals visuals, ColoredDoorOrientation facing) {
                    visuals.Image(BotwoonWallObjectDrawDefinitions.Clear);
                    visuals.Image(BlueDoorCaps.ClosedSolid(facing));
                }
            }
            """, path: "fixture-room-object-frame-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation compilation = CSharpCompilation.Create("RoomObjectFrameProviderFixture", trees,
            platforms.Append(typeof(SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectPictures).Assembly.Location)
                .Append(typeof(SuperMetroid.AssetExtraction.Rooms.RoomObjectBotwoonWallVisualFiles).Assembly.Location).Distinct()
                .Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        FixtureConsumerBinding.Require(compilation, calls, nameof(RoomObjectFramePresentationContractChecks));
        AuditReport before = Inspect(compilation, calls, adapted: false).Report, after = Inspect(compilation, calls, adapted: true).Report;
        Require(before.UnresolvedCount == 2, "constructed calls must first expose the missing adapter (" + before.Describe() + ")");
        Require(after.Classifications.Count == 2 && after.UnresolvedCount == 0 && after.MissingCount == 0 && after.Consumers.Count == 2,
            "a computed frame of each reviewed family classifies (" + after.Describe() + ")");

        // Only a family's reviewed loader proves its pictures complete; a construction elsewhere revokes that family's proof
        // alone among the families, and with it the composed read every frame is read through.
        CSharpCompilation forged = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Rooms.RoomObjects;
            class FramePictureForger {
                InstalledRoomObjectPictures Forge() => new InstalledRoomObjectPictures(null!);
            }
            """, path: "fixture-room-object-frame-forger.cs"));
        (AuditReport revoked, ClosedPresentationAudit? forgedAudit) = Inspect(forged, calls, adapted: true);
        Require(revoked.Classifications.Count == 0 && revoked.UnresolvedCount == 2 && revoked.Consumers.Count == 2,
            "a construction of a family's pictures outside its loader must revoke the composed read (" + revoked.Describe() + ")");
        string[] expectedRevoked = [ContractTypeNames.MetadataName(typeof(SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectPictures)),
            RoomObjectFrameClosedContractDefinitions.Composed.Type];
        string[] actualRevoked = [.. (forgedAudit ?? throw new InvalidOperationException("The adapted inspection has its audit."))
            .InvalidContracts.Where(type => RoomObjectFrameClosedContractDefinitions.All.Any(contract => contract.Type == type)).Order(StringComparer.Ordinal)];
        Require(actualRevoked.SequenceEqual(expectedRevoked.Order(StringComparer.Ordinal), StringComparer.Ordinal),
            "the construction must revoke that family's proof and the composed read, and no other room-object proof (" +
            string.Join(", ", actualRevoked) + ")");
    }

    /// <summary>
    /// Inspects every fixture consumer call, through the closed-provider adapter when <paramref name="adapted"/>, and
    /// returns the report with the adapter it inspected through.
    /// </summary>
    private static (AuditReport Report, ClosedPresentationAudit? Audit) Inspect(CSharpCompilation compilation, SyntaxTree calls, bool adapted)
    {
        var exports = new ResourceIndex();
        var adapter = adapted ? new ClosedPresentationAudit(compilation, exports) : null;
        var report = new AuditReport();
        foreach (InvocationExpressionSyntax call in calls.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, report, adapter);
        return (report, adapter);
    }

    /// <summary>Fails the room-object frame confirmation with the classification that did not hold.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Room-object frame provider confirmation failed: " + reason);
    }
}
