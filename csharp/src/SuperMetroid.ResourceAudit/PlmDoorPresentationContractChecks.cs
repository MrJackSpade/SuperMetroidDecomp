using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.ResourceAudit.PlmPresentationContractChecks;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms identified flat/single-word provider coverage and alias shapes, without door or pickup gameplay.</summary>
internal static class PlmDoorPresentationContractChecks
{
    internal static void Run()
    {
        ConfirmConstructorCoverage();
        var trees = PlmDoorClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<plm-door-usings>"));
        const string consumer = """
            using SuperMetroid.Core.Rooms;
            static class FixtureDoorFrames {
                internal const ushort ClosedBlue = @CLOSED_BLUE@;
                internal const ushort TwoWordEye = @TWO_WORD_EYE@;
            }
            class PlmDoorConsumer {
                void Inspect(RoomPlmBlueDoorVisualCatalog blue, RoomPlmColoredDoorVisualCatalog color,
                    RoomPlmGreyDoorVisualCatalog grey, RoomPlmEyeDoorVisualCatalog eye,
                    RoomPlmEscapeGateVisualCatalog escape, RoomPlmCollectibleVisualCatalog item,
                    RoomPlmGrappleBlockVisualCatalog grapple, ushort dynamicPointer) {
                    blue.GetWord(FixtureDoorFrames.ClosedBlue, 3);
                    blue.GetWord(FixtureDoorFrames.ClosedBlue, 4);
                    color.GetWord(dynamicPointer, 3);
                    color.GetWord(0, 0);
                    grey.GetWord(dynamicPointer, 3);
                    grey.GetWord(dynamicPointer, 4);
                    eye.GetWord(EyeDoorPlmDrawDefinitions.MirroredOpeningClear, 3);
                    eye.GetWord(EyeDoorPlmDrawDefinitions.MirroredOpeningClear, 4);
                    eye.GetWord(FixtureDoorFrames.TwoWordEye, 1);
                    eye.GetWord(FixtureDoorFrames.TwoWordEye, 2);
                    escape.GetWord(MotherBrainEscapeGatePlmDrawDefinitions.Closed, 3);
                    escape.GetWord(MotherBrainEscapeGatePlmDrawDefinitions.Closed, 4);
                    item.GetWord(pointer: RoomPlmCollectibleDrawDefinitions.Empty);
                    item.GetWord(pointer: 0);
                    grapple.GetWord(RoomPlmGrappleBlockDrawDefinitions.Grapple);
                    grapple.GetWord(0);
                }
            }
            """;
        // Feed named constant declarations from the actual compiled identities,
        // not address arithmetic borrowed from an unrelated family.
        ushort closedBlue = BlueDoorPlmDrawDefinitionsTooling.All.First(frame =>
            BlueDoorPlmDrawDefinitions.VisualSource(frame.Pointer) != frame.Pointer).Pointer;
        ushort twoWordEye = EyeDoorPlmDrawDefinitions.Editable.Single(frame =>
            EyeDoorPlmDrawDefinitions.VisualId(frame.Pointer) == "left-eye-frame-0").Pointer;
        string selectedConsumer = consumer.Replace("@CLOSED_BLUE@", $"0x{closedBlue:X4}", StringComparison.Ordinal)
            .Replace("@TWO_WORD_EYE@", $"0x{twoWordEye:X4}", StringComparison.Ordinal);
        SyntaxTree calls = CSharpSyntaxTree.ParseText(selectedConsumer, path: "fixture-plm-door-consumer.cs");
        trees.Add(calls);
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
        var compilation = CSharpCompilation.Create("PlmDoorProviderFixture", trees,
            platforms.Append(typeof(RoomPlmBlueDoorVisualCatalog).Assembly.Location).Distinct()
                .Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var exports = new ResourceIndex();
        var adapter = new ClosedPresentationAudit(compilation, exports);
        var before = new AuditReport();
        var report = new AuditReport();
        foreach (InvocationExpressionSyntax call in calls.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, before);
            ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, report, adapter);
        }
        Require(before.UnresolvedCount == 16, "sixteen calls must reproduce absent adapters first");
        Require(report.Classifications.Count == 8 && report.UnresolvedCount == 8 && report.MissingCount == 0 &&
            report.Consumers.Count == 16, "known aliases qualify but unsupported pointers and narrow widths still fail");
        Require(report.Findings.Any(item => item.Message.Contains("drawPointer", StringComparison.Ordinal)) &&
            report.Findings.Any(item => item.Message.Contains("word/block", StringComparison.Ordinal)),
            "invalid pointers and words retain exact shape findings");
    }

    private static void ConfirmConstructorCoverage()
    {
        ConfirmCoverage(BlueDoorPlmDrawDefinitions.Editable, frame =>
            new RoomPlmBlueDoorVisualEntry(BlueDoorPlmDrawDefinitions.VisualId(frame.Pointer), VisualRuns(frame)[0]),
            entries => _ = new RoomPlmBlueDoorVisualCatalog(entries));
        ConfirmCoverage(ColoredDoorPlmDrawDefinitions.All, frame =>
            new RoomPlmColoredDoorVisualEntry(ColoredDoorPlmDrawDefinitions.VisualId(frame.Pointer), VisualRuns(frame)[0]),
            entries => _ = new RoomPlmColoredDoorVisualCatalog(entries));
        ConfirmCoverage(GreyDoorPlmDrawDefinitions.All, frame =>
            new RoomPlmGreyDoorVisualEntry(GreyDoorPlmDrawDefinitions.VisualId(frame.Pointer), VisualRuns(frame)[0]),
            entries => _ = new RoomPlmGreyDoorVisualCatalog(entries));
        ConfirmCoverage(EyeDoorPlmDrawDefinitions.Editable, frame =>
            new RoomPlmEyeDoorVisualEntry(EyeDoorPlmDrawDefinitions.VisualId(frame.Pointer), VisualRuns(frame)[0]),
            entries => _ = new RoomPlmEyeDoorVisualCatalog(entries));
        ConfirmCoverage(MotherBrainEscapeGatePlmDrawDefinitions.All, frame =>
            new RoomPlmEscapeGateVisualEntry(MotherBrainEscapeGatePlmDrawDefinitions.VisualId(frame.Pointer), VisualRuns(frame)[0]),
            entries => _ = new RoomPlmEscapeGateVisualCatalog(entries));
        RoomPlmShotBlockDrawDefinitions.DrawList[] itemShapes =
            PlmVisualDomainDefinitions.Get(typeof(RoomPlmCollectibleVisualCatalog).FullName!)!;
        ConfirmCoverage(itemShapes, frame => {
            Require(RoomPlmCollectibleDrawDefinitions.TryGet(frame.Pointer, out var item), "compiled item shape owns its ID");
            return new RoomPlmCollectibleVisualEntry(item.Id, VisualRuns(frame)[0][0]);
        }, entries => _ = new RoomPlmCollectibleVisualCatalog(entries));
        ConfirmCoverage(PlmVisualDomainDefinitions.Get(typeof(RoomPlmGrappleBlockVisualCatalog).FullName!)!, frame =>
            new RoomPlmGrappleBlockVisualEntry(frame.Pointer, VisualRuns(frame)[0][0]),
            entries => _ = new RoomPlmGrappleBlockVisualCatalog(entries));

        // Confirm the reviewed reuse mappings against production GetWord, not
        // just the source adapter. The constructed catalog contains no ROM art.
        var blue = new RoomPlmBlueDoorVisualCatalog(BlueDoorPlmDrawDefinitions.Editable.Select(frame =>
            new RoomPlmBlueDoorVisualEntry(BlueDoorPlmDrawDefinitions.VisualId(frame.Pointer), VisualRuns(frame)[0])));
        foreach (var frame in BlueDoorPlmDrawDefinitionsTooling.All.Where(frame => BlueDoorPlmDrawDefinitions.VisualSource(frame.Pointer) != frame.Pointer))
            Require(Enumerable.Range(0, frame.Runs.Span[0].LevelWords.Length).All(word =>
                blue.GetWord(frame.Pointer, word) == blue.GetWord(BlueDoorPlmDrawDefinitions.VisualSource(frame.Pointer), word)),
                "each compiled closed cap resolves to its required opening artwork");
        var eye = new RoomPlmEyeDoorVisualCatalog(EyeDoorPlmDrawDefinitions.Editable.Select(frame =>
            new RoomPlmEyeDoorVisualEntry(EyeDoorPlmDrawDefinitions.VisualId(frame.Pointer), VisualRuns(frame)[0])));
        ushort mirrored = EyeDoorPlmDrawDefinitions.MirroredOpeningClear;
        Require(Enumerable.Range(0, 4).All(word => eye.GetWord(mirrored, word) ==
            (eye.GetWord(EyeDoorPlmDrawDefinitions.VisualSource(mirrored), word) ^ (ushort)LevelBlockFlipFlags.Horizontal)),
            "mirrored clear uses the required authored clear with horizontal flip");
    }

    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("PLM door provider confirmation failed: " + reason);
    }
}
