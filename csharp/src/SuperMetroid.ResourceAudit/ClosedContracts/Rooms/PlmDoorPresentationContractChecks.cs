using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static SuperMetroid.ResourceAudit.ClosedContracts.Rooms.PlmPresentationContractChecks;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.ResourceAudit.PlmPrograms;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Confirms identified flat/single-word provider coverage and alias shapes, without door or pickup gameplay.</summary>
internal static class PlmDoorPresentationContractChecks
{
    /// <summary>Checks constructed provider coverage and classifies sixteen compiled door and block consumer calls.</summary>
    internal static void Run()
    {
        ConfirmConstructorCoverage();
        var trees = PlmDoorClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        // The consumer names escape-gate draws through this internal enum; compiled from source so its constants bind.
        trees.AddRange(new[]
        {
            "csharp/src/SuperMetroid.Core/Rooms/Plms/MotherBrainEscapeGateDraw.cs", "csharp/src/SuperMetroid.Core/Rooms/Plms/PlmDraw.cs", "csharp/src/SuperMetroid.Core/Rooms/Plms/PlmFrameDraw.cs",
            "csharp/src/SuperMetroid.Core/Rooms/Plms/StationDraw.cs", "csharp/src/SuperMetroid.Core/Rooms/Plms/GrappleBlockDraw.cs",
            "csharp/src/SuperMetroid.Core/Rooms/Doors/EyeDoorOrientation.cs", "csharp/src/SuperMetroid.Core/Rooms/Doors/EyeDoorComponent.cs", "csharp/src/SuperMetroid.Core/Rooms/Doors/EyeDoorPlmFrame.cs",
        }.Where(path => trees.All(tree => tree.FilePath != path))
            .Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), path: path)));
        const string consumer = """
            using SuperMetroid.Core.Bosses.MotherBrain;
            using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
            using SuperMetroid.Core.Rooms.Doors;
            using SuperMetroid.Core.Rooms.Plms;
            class PlmDoorConsumer {
                void Inspect(InstalledRoomPlmBlueDoorVisuals blue, InstalledRoomPlmColoredDoorVisuals color,
                    InstalledRoomPlmGreyDoorVisuals grey, InstalledRoomPlmEyeDoorVisuals eye,
                    RoomPlmEscapeGateVisualCatalog escape, InstalledRoomPlmCollectibleVisuals item,
                    InstalledRoomPlmGrappleBlockVisuals grapple, PlmDraw dynamicDraw) {
                    blue.GetWord(BlueDoorCaps.Alias(ColoredDoorOrientation.Left), 3);
                    blue.GetWord(BlueDoorCaps.Alias(ColoredDoorOrientation.Left), 4);
                    color.GetWord(dynamicDraw, 3);
                    color.GetWord(PlmDraw.Of(StationDraw.MapFrame0), 0);
                    grey.GetWord(dynamicDraw, 3);
                    grey.GetWord(dynamicDraw, 4);
                    eye.GetWord(EyeDoorPlmDrawDefinitions.MirroredOpeningClear, 3);
                    eye.GetWord(EyeDoorPlmDrawDefinitions.MirroredOpeningClear, 4);
                    eye.GetWord(EyeDoorDraws.DrawList(EyeDoorOrientation.Left, EyeDoorComponent.Eye, EyeDoorPlmFrame.Eye0), 1);
                    eye.GetWord(EyeDoorDraws.DrawList(EyeDoorOrientation.Left, EyeDoorComponent.Eye, EyeDoorPlmFrame.Eye0), 2);
                    escape.GetAppearance(PlmDraw.Of(MotherBrainEscapeGateDraw.Closed), 3);
                    escape.GetAppearance(PlmDraw.Of(MotherBrainEscapeGateDraw.Closed), 4);
                    item.GetWord(draw: RoomPlmCollectibleDrawDefinitions.Empty);
                    item.GetWord(draw: PlmDraw.Of(StationDraw.MapFrame0));
                    grapple.GetWord(PlmDraw.Of(GrappleBlockDraw.Intact));
                    grapple.GetWord(PlmDraw.Of(StationDraw.MapFrame0));
                }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-plm-door-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        var compilation = CSharpCompilation.Create("PlmDoorProviderFixture", trees,
            platforms.Append(typeof(InstalledRoomPlmBlueDoorVisuals).Assembly.Location).Distinct()
                .Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        FixtureConsumerBinding.Require(compilation, calls, "PlmDoorPresentationContractChecks");
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
        ConfirmCoverage(BlueDoorCaps.Editable, frame =>
            new RoomPlmVisualEntry(BlueDoorCaps.VisualId(frame.Draw), VisualRuns(frame)[0]),
            entries => _ = new InstalledRoomPlmBlueDoorVisuals(entries));
        ConfirmCoverage(ColoredDoorCaps.All, frame =>
            new RoomPlmVisualEntry(ColoredDoorCaps.VisualId(frame.Draw), VisualRuns(frame)[0]),
            entries => _ = new InstalledRoomPlmColoredDoorVisuals(entries));
        ConfirmCoverage(GreyDoorCaps.All, frame =>
            new RoomPlmVisualEntry(GreyDoorCaps.VisualId(frame.Draw), VisualRuns(frame)[0]),
            entries => _ = new InstalledRoomPlmGreyDoorVisuals(entries));
        ConfirmCoverage(EyeDoorDraws.Editable, frame =>
            new RoomPlmVisualEntry(EyeDoorDraws.VisualId(frame.Draw), VisualRuns(frame)[0]),
            entries => _ = new InstalledRoomPlmEyeDoorVisuals(entries));
        ConfirmCoverage(MotherBrainEscapeGateDraws.All, frame =>
            new RoomPlmVisualEntry(MotherBrainEscapeGateDraws.VisualId(frame.Draw), VisualRuns(frame)[0]),
            entries => _ = new RoomPlmEscapeGateVisualCatalog(entries));
        RoomPlmShotBlockDrawDefinitions.DrawList[] itemShapes =
            (PlmVisualDomainDefinitions.Get((typeof(InstalledRoomPlmCollectibleVisuals).FullName ?? throw new InvalidOperationException("Verification requires typeof(InstalledRoomPlmCollectibleVisuals).FullName."))) ?? throw new InvalidOperationException("Verification requires PlmVisualDomainDefinitions.Get((typeof(InstalledRoomPlmCollectibleVisuals).FullName ?? thr...."));
        ConfirmCoverage((itemShapes), frame => {
            Require(RoomPlmCollectibleDrawDefinitionsTooling.TryGet(frame.Draw, out var item), "compiled item shape owns its ID");
            return new RoomPlmVisualWordEntry(item.Id, VisualRuns(frame)[0][0]);
        }, entries => _ = new InstalledRoomPlmCollectibleVisuals(entries));
        ConfirmCoverage((PlmVisualDomainDefinitions.Get((typeof(InstalledRoomPlmGrappleBlockVisuals).FullName ?? throw new InvalidOperationException("Verification requires typeof(InstalledRoomPlmGrappleBlockVisuals).FullName."))) ?? throw new InvalidOperationException("Verification requires PlmVisualDomainDefinitions.Get((typeof(InstalledRoomPlmGrappleBlockVisuals).FullName ?? th....")), frame =>
            new RoomPlmVisualWordEntry(GrappleBlockPlmDraws.VisualId(PlmDraw.FrameOf<GrappleBlockDraw>(frame.Draw, "Grapple-block")),
                VisualRuns(frame)[0][0]),
            entries => _ = new InstalledRoomPlmGrappleBlockVisuals(entries));

        // Confirm the reviewed reuse mappings against production GetWord, not
        // just the source adapter. The constructed catalog contains no ROM art.
        var blue = new InstalledRoomPlmBlueDoorVisuals(BlueDoorCaps.Editable.Select(frame =>
            new RoomPlmVisualEntry(BlueDoorCaps.VisualId(frame.Draw), VisualRuns(frame)[0])));
        foreach (var frame in BlueDoorPlmDrawDefinitionsTooling.All.Where(frame => BlueDoorCaps.VisualSource(frame.Draw) != frame.Draw))
            Require(Enumerable.Range(0, frame.Runs.Span[0].LevelWords.Length).All(word =>
                blue.GetWord(frame.Draw, word) == blue.GetWord(BlueDoorCaps.VisualSource(frame.Draw), word)),
                "each compiled closed cap resolves to its required opening artwork");
        var eye = new InstalledRoomPlmEyeDoorVisuals(EyeDoorDraws.Editable.Select(frame =>
            new RoomPlmVisualEntry(EyeDoorDraws.VisualId(frame.Draw), VisualRuns(frame)[0])));
        PlmDraw mirrored = EyeDoorPlmDrawDefinitions.MirroredOpeningClear;
        Require(Enumerable.Range(0, 4).All(word => eye.GetWord(mirrored, word) ==
            (eye.GetWord(EyeDoorDraws.VisualSource(mirrored), word) ^ (ushort)LevelBlockFlipFlags.Horizontal)),
            "mirrored clear uses the required authored clear with horizontal flip");
    }

    /// <summary>Reports a failed door provider contract with its specific reason.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("PLM door provider confirmation failed: " + reason);
    }
}
