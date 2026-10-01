using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified PLM artwork closure and tuple-shape audit, never a PLM instruction or room.</summary>
internal static class PlmPresentationContractChecks
{
    internal static void Run()
    {
        ConfirmConstructorCoverage();
        var trees = PlmClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<plm-fixture-usings>"));
        const string consumer = """
            using SuperMetroid.Core.Rooms;
            static class FixturePlmFrames {
                internal const ushort ShotSingle = RoomPlmShotBlockDrawDefinitions.SingleFrame0;
            }
            class PlmConsumer {
                void Inspect(RoomPlmShotBlockVisualCatalog shot, RoomPlmStationVisualCatalog station,
                    RoomPlmBombTorizoHandVisualCatalog hand, RoomPlmMotherBrainGlassVisualCatalog glass,
                    RoomPlmNoobTubeVisualCatalog tube, ushort dynamicPointer, int dynamicWord) {
                    shot.GetWord(FixturePlmFrames.ShotSingle, 0, 0);
                    shot.GetWord(FixturePlmFrames.ShotSingle, 0, 1);
                    shot.GetWord(FixturePlmFrames.ShotSingle, 1, 0);
                    shot.GetWord(0, 0, 0);
                    station.GetWord(dynamicPointer, 0, 0);
                    station.GetWord(dynamicPointer, 0, int.MaxValue);
                    hand.GetWord(BombTorizoHandPlmDrawDefinitions.Intact, 0, 0);
                    hand.GetWord(BombTorizoHandPlmDrawDefinitions.Intact, 0, int.MaxValue);
                    glass.GetWord(dynamicPointer, 0, 0);
                    glass.GetWord(dynamicPointer, int.MaxValue, 0);
                    tube.GetWord(dynamicPointer, 0, 0);
                    tube.GetWord(dynamicPointer, 0, int.MaxValue);
                    shot.GetWord(dynamicPointer, 0, dynamicWord);
                }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-plm-consumer.cs");
        trees.Add(calls);
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
        var compilation = CSharpCompilation.Create("PlmProviderFixture", trees,
            platforms.Append(typeof(RoomPlmShotBlockVisualCatalog).Assembly.Location).Distinct()
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
        Require(before.UnresolvedCount == 13, "the thirteen calls must reproduce the identified missing adapters first");
        Require(report.Classifications.Count == 6 && report.UnresolvedCount == 7 && report.MissingCount == 0 &&
            report.Consumers.Count == 13, "complete valid-domain calls qualify, but wrong pointers and tuple components still fail");
        Require(report.Findings.Any(item => item.Message.Contains("drawPointer", StringComparison.Ordinal)) &&
            report.Findings.Any(item => item.Message.Contains("runIndex", StringComparison.Ordinal)) &&
            report.Findings.Any(item => item.Message.Contains("word/block", StringComparison.Ordinal)),
            "pointer, run and word failures must come from precise declared domains");
    }

    private static ushort[][] VisualRuns(RoomPlmShotBlockDrawDefinitions.DrawList frame) =>
        frame.Runs.ToArray().Select(run => run.LevelWords.ToArray()
            .Select(word => new RoomLevelWord(word).VisualWord).ToArray()).ToArray();

    private static void ConfirmConstructorCoverage()
    {
        ConfirmCoverage(RoomPlmShotBlockDrawDefinitions.All, frame =>
            new RoomPlmShotBlockVisualEntry(frame.Pointer, VisualRuns(frame)),
            entries => _ = new RoomPlmShotBlockVisualCatalog(entries));
        ConfirmCoverage(RoomPlmStationDrawDefinitions.All, frame =>
            new RoomPlmStationVisualEntry(RoomPlmStationDrawDefinitions.VisualId(frame.Pointer), VisualRuns(frame)),
            entries => _ = new RoomPlmStationVisualCatalog(entries));
        ConfirmCoverage(BombTorizoHandPlmDrawDefinitions.All, frame =>
            new RoomPlmBombTorizoHandVisualEntry(BombTorizoHandPlmDrawDefinitions.VisualId(frame.Pointer),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = new RoomPlmBombTorizoHandVisualCatalog(entries));
        ConfirmCoverage(MotherBrainGlassPlmDrawDefinitions.All, frame =>
            new RoomPlmMotherBrainGlassVisualEntry(MotherBrainGlassPlmDrawDefinitions.VisualId(frame.Pointer),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = new RoomPlmMotherBrainGlassVisualCatalog(entries));
        ConfirmCoverage(NoobTubePlmDrawDefinitions.All, frame =>
            new RoomPlmNoobTubeVisualEntry(NoobTubePlmDrawDefinitions.VisualId(frame.Pointer),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = new RoomPlmNoobTubeVisualCatalog(entries));
    }

    private static void ConfirmCoverage<T>(IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> definitions,
        Func<RoomPlmShotBlockDrawDefinitions.DrawList, T> entry, Action<IEnumerable<T>> construct)
    {
        T[] complete = definitions.Select(entry).ToArray();
        construct(complete);
        bool rejected = false;
        try { construct(complete.Skip(1)); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "a partial " + typeof(T).Name + " domain must not publish a provider");
        rejected = false;
        try { construct(complete.Append(complete[0])); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "duplicate " + typeof(T).Name + " identities must not satisfy coverage");
    }

    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("PLM provider confirmation failed: " + reason);
    }
}
