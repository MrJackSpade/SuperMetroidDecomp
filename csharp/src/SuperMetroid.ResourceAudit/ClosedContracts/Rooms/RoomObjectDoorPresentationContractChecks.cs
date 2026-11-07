using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static SuperMetroid.ResourceAudit.ClosedContracts.Rooms.RoomObjectPresentationContractChecks;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.ResourceAudit.RoomObjectPrograms;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Confirms identified flat/single-word provider coverage and alias shapes, without door or pickup gameplay.</summary>
internal static class RoomObjectDoorPresentationContractChecks
{
    /// <summary>Checks constructed provider coverage and classifies sixteen compiled door and block consumer calls.</summary>
    internal static void Run()
    {
        ConfirmConstructorCoverage();
        var trees = RoomObjectDoorClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        // The consumer names escape-gate draws through this internal enum; compiled from source so its constants bind.
        trees.AddRange(new[]
        {
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/MotherBrainEscapeGateDraw.cs", "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectDraw.cs", "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectFrameDraw.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/StationDraw.cs", "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/GrappleBlockDraw.cs",
            "csharp/src/SuperMetroid.Core/Rooms/Doors/EyeDoorOrientation.cs", "csharp/src/SuperMetroid.Core/Rooms/Doors/EyeDoorComponent.cs", "csharp/src/SuperMetroid.Core/Rooms/Doors/EyeDoorObjectFrame.cs",
        }.Where(path => trees.All(tree => tree.FilePath != path))
            .Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), path: path)));
        const string consumer = """
            using SuperMetroid.Core.Bosses.MotherBrain;
            using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
            using SuperMetroid.Core.Rooms.Doors;
            using SuperMetroid.Core.Rooms.RoomObjects;
            class RoomObjectDoorConsumer {
                void Inspect(InstalledRoomObjectBlueDoorVisuals blue, InstalledRoomObjectColoredDoorVisuals color,
                    InstalledRoomObjectGreyDoorVisuals grey, InstalledRoomObjectEyeDoorVisuals eye,
                    RoomObjectEscapeGateVisualCatalog escape, InstalledRoomObjectCollectibleVisuals item,
                    InstalledRoomObjectGrappleBlockVisuals grapple, RoomObjectDraw dynamicDraw) {
                    blue.GetAppearance(BlueDoorCaps.Alias(ColoredDoorOrientation.Left), 3);
                    blue.GetAppearance(BlueDoorCaps.Alias(ColoredDoorOrientation.Left), 4);
                    color.GetAppearance(dynamicDraw, 3);
                    color.GetAppearance(RoomObjectDraw.Of(StationDraw.MapFrame0), 0);
                    grey.GetAppearance(dynamicDraw, 3);
                    grey.GetAppearance(dynamicDraw, 4);
                    eye.GetAppearance(EyeDoorObjectDrawDefinitions.MirroredOpeningClear, 3);
                    eye.GetAppearance(EyeDoorObjectDrawDefinitions.MirroredOpeningClear, 4);
                    eye.GetAppearance(EyeDoorDraws.DrawList(EyeDoorOrientation.Left, EyeDoorComponent.Eye, EyeDoorObjectFrame.Eye0), 1);
                    eye.GetAppearance(EyeDoorDraws.DrawList(EyeDoorOrientation.Left, EyeDoorComponent.Eye, EyeDoorObjectFrame.Eye0), 2);
                    escape.GetAppearance(RoomObjectDraw.Of(MotherBrainEscapeGateDraw.Closed), 3);
                    escape.GetAppearance(RoomObjectDraw.Of(MotherBrainEscapeGateDraw.Closed), 4);
                    item.GetAppearance(draw: RoomObjectCollectibleDrawDefinitions.Empty);
                    item.GetAppearance(draw: RoomObjectDraw.Of(StationDraw.MapFrame0));
                    grapple.GetAppearance(RoomObjectDraw.Of(GrappleBlockDraw.Intact));
                    grapple.GetAppearance(RoomObjectDraw.Of(StationDraw.MapFrame0));
                }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-plm-door-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        var compilation = CSharpCompilation.Create("RoomObjectDoorProviderFixture", trees,
            platforms.Append(typeof(InstalledRoomObjectBlueDoorVisuals).Assembly.Location).Distinct()
                .Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        FixtureConsumerBinding.Require(compilation, calls, "RoomObjectDoorPresentationContractChecks");
        var exports = new ResourceIndex();
        var adapter = new ClosedPresentationAudit(compilation, exports);
        var before = new AuditReport();
        var report = new AuditReport();
        foreach (InvocationExpressionSyntax call in calls.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, before);
            ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, report, adapter);
        }
        Require(before.UnresolvedCount == 16, "sixteen calls must reproduce absent adapters first" + " (" + before.Describe() + ")");
        Require(report.Classifications.Count == 8 && report.UnresolvedCount == 8 && report.MissingCount == 0 &&
            report.Consumers.Count == 16, "known aliases qualify but foreign draws and narrow widths still fail (expected 8/8/0/16; " +
            report.Describe() + ")");
        Require(report.Findings.Any(item => item.Message.Contains("is not owned", StringComparison.Ordinal)) &&
            report.Findings.Any(item => item.Message.Contains("word/block", StringComparison.Ordinal)),
            "foreign draws and words retain exact shape findings");
    }

    /// <summary>Checks each PLM catalog constructor and authored alias against the production word lookup.</summary>
    private static void ConfirmConstructorCoverage()
    {
        ConfirmCoverage(RoomObjectBlueDoorVisualFiles.Editable, frame =>
            new RoomObjectVisualEntry(RoomObjectBlueDoorVisualFiles.VisualId(frame.Draw), VisualRuns(frame)[0]),
            entries => _ = RoomObjectBlueDoorVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectColoredDoorVisualFiles.All, frame =>
            new RoomObjectVisualEntry(RoomObjectColoredDoorVisualFiles.VisualId(frame.Draw), VisualRuns(frame)[0]),
            entries => _ = RoomObjectColoredDoorVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectGreyDoorVisualFiles.All, frame =>
            new RoomObjectVisualEntry(RoomObjectGreyDoorVisualFiles.VisualId(frame.Draw), VisualRuns(frame)[0]),
            entries => _ = RoomObjectGreyDoorVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectEyeDoorVisualFiles.Editable, frame =>
            new RoomObjectVisualEntry(RoomObjectEyeDoorVisualFiles.VisualId(frame.Draw), VisualRuns(frame)[0]),
            entries => _ = RoomObjectEyeDoorVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectEscapeGateVisualFiles.All, frame =>
            new RoomObjectVisualEntry(RoomObjectEscapeGateVisualFiles.VisualId(frame.Draw), VisualRuns(frame)[0]),
            entries => _ = RoomObjectEscapeGateVisualFiles.Create(entries));
        RoomObjectShotBlockDrawDefinitions.DrawList[] itemShapes =
            (RoomObjectVisualDomainDefinitions.Get((typeof(InstalledRoomObjectCollectibleVisuals).FullName ?? throw new InvalidOperationException("Verification requires typeof(InstalledRoomObjectCollectibleVisuals).FullName."))) ?? throw new InvalidOperationException("Verification requires RoomObjectVisualDomainDefinitions.Get((typeof(InstalledRoomObjectCollectibleVisuals).FullName ?? thr...."));
        ConfirmCoverage((itemShapes), frame => {
            Require(RoomObjectCollectibleDrawDefinitionsTooling.TryGet(frame.Draw, out var item), "compiled item shape owns its ID");
            return new RoomObjectVisualBlockEntry(item.Id, VisualRuns(frame)[0][0]);
        }, entries => _ = RoomObjectCollectibleVisualFiles.Create(entries));
        ConfirmCoverage((RoomObjectVisualDomainDefinitions.Get((typeof(InstalledRoomObjectGrappleBlockVisuals).FullName ?? throw new InvalidOperationException("Verification requires typeof(InstalledRoomObjectGrappleBlockVisuals).FullName."))) ?? throw new InvalidOperationException("Verification requires RoomObjectVisualDomainDefinitions.Get((typeof(InstalledRoomObjectGrappleBlockVisuals).FullName ?? th....")), frame =>
            new RoomObjectVisualBlockEntry(RoomObjectGrappleBlockVisualFiles.VisualId(RoomObjectDraw.FrameOf<GrappleBlockDraw>(frame.Draw, "Grapple-block")),
                VisualRuns(frame)[0][0]),
            entries => _ = RoomObjectGrappleBlockVisualFiles.Create(entries));

        // Confirm the reviewed reuse mappings against production GetAppearance, not
        // just the source adapter. The constructed catalog contains no ROM art.
        var blue = RoomObjectBlueDoorVisualFiles.Create(RoomObjectBlueDoorVisualFiles.Editable.Select(frame =>
            new RoomObjectVisualEntry(RoomObjectBlueDoorVisualFiles.VisualId(frame.Draw), VisualRuns(frame)[0])));
        foreach (var frame in BlueDoorObjectDrawDefinitionsTooling.All.Where(frame => BlueDoorCaps.VisualSource(frame.Draw) != frame.Draw))
            Require(Enumerable.Range(0, frame.Runs.Span[0].Count).All(word =>
                blue.GetAppearance(frame.Draw, word) == blue.GetAppearance(BlueDoorCaps.VisualSource(frame.Draw), word)),
                "each compiled closed cap resolves to its required opening artwork");
        var eye = RoomObjectEyeDoorVisualFiles.Create(RoomObjectEyeDoorVisualFiles.Editable.Select(frame =>
            new RoomObjectVisualEntry(RoomObjectEyeDoorVisualFiles.VisualId(frame.Draw), VisualRuns(frame)[0])));
        RoomObjectDraw mirrored = EyeDoorObjectDrawDefinitions.MirroredOpeningClear;
        Require(Enumerable.Range(0, 4).All(word => eye.GetAppearance(mirrored, word) ==
            Mirrored(eye.GetAppearance(EyeDoorDraws.VisualSource(mirrored), word))),
            "mirrored clear uses the required authored clear with horizontal mirroring");
        static RoomBlockAppearance Mirrored(RoomBlockAppearance shared) =>
            shared.WithMirroring(shared.Mirroring ^ BlockMirroring.Horizontal);
    }

    /// <summary>Reports a failed door provider contract with its specific reason.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("PLM door provider confirmation failed: " + reason);
    }
}
