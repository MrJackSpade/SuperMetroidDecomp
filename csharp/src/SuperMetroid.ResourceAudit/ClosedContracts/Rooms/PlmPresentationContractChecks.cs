using SuperMetroid.Core;
using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Confirms the identified PLM artwork closure and tuple-shape audit, never a PLM instruction or room.</summary>
internal static class PlmPresentationContractChecks
{
    /// <summary>Checks PLM visual provider coverage and audit classification for valid and invalid pointer/run/block calls.</summary>
    internal static void Run()
    {
        ConfirmConstructorCoverage();
        var trees = PlmClosedContractDefinitions.All.Concat(PlmActorClosedContractDefinitions.All)
            .Concat(PlmProgressionClosedContractDefinitions.All)
            .SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        // The consumer names draws through these internal enums; compiled from source so its constants bind.
        trees.AddRange(new[]
        {
            "csharp/src/SuperMetroid.Core/Bosses/Draygon/DraygonCannonDraw.cs", "csharp/src/SuperMetroid.Core/Enemies/Chozo/ChozoStatueDraw.cs",
            "csharp/src/SuperMetroid.Core/Areas/Tourian/TourianAccessDraw.cs", "csharp/src/SuperMetroid.Core/Bosses/Kraid/KraidRoomDraw.cs",
            "csharp/src/SuperMetroid.Core/Bosses/Crocomire/CrocomireArenaDraw.cs", "csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainFakeDeathDraw.cs",
            "csharp/src/SuperMetroid.Core/Bosses/Torizo/BombTorizoHandDraw.cs", "csharp/src/SuperMetroid.Core/Enemies/Elevator/ElevatorPlatformDraw.cs",
            "csharp/src/SuperMetroid.Core/Rooms/LevelData/BombBlockRestoreDraw.cs", "csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/SporeSpawnCeilingDraw.cs",
            "csharp/src/SuperMetroid.Core/Rooms/Plms/SamusEaterDraw.cs",
        }.Where(path => trees.All(tree => tree.FilePath != path))
            .Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), path: path)));
        const string consumer = """
            using SuperMetroid.Core.Areas.Maridia;
            using SuperMetroid.Core.Areas.Tourian;
            using SuperMetroid.Core.Bosses.Botwoon;
            using SuperMetroid.Core.Bosses.Crocomire;
            using SuperMetroid.Core.Bosses.Draygon;
            using SuperMetroid.Core.Bosses.Kraid;
            using SuperMetroid.Core.Bosses.MotherBrain;
            using SuperMetroid.Core.Bosses.SporeSpawn;
            using SuperMetroid.Core.Bosses.Torizo;
            using SuperMetroid.Core.Enemies.Chozo;
            using SuperMetroid.Core.Enemies.Elevator;
            using SuperMetroid.Core.Rooms.LevelData;
            using SuperMetroid.Core.Rooms.Plms;
            static class FixturePlmFrames {
                internal const ushort ShotSingle = RoomPlmShotBlockDrawDefinitions.SingleFrame0;
            }
            class PlmConsumer {
                void Inspect(RoomPlmShotBlockVisualCatalog shot, RoomPlmStationVisualCatalog station,
                    RoomPlmBombTorizoHandVisualCatalog hand, RoomPlmMotherBrainGlassVisualCatalog glass,
                    RoomPlmNoobTubeVisualCatalog tube, RoomPlmDownwardGateVisualCatalog gate,
                    RoomPlmElevatorPlatformVisualCatalog elevator, RoomPlmDraygonCannonVisualCatalog cannon,
                    RoomPlmChozoStatueVisualCatalog chozo, RoomPlmLinkedRestoreVisualCatalog restore,
                    RoomPlmTourianAccessVisualCatalog tourian, RoomPlmSpeedBoosterVisualCatalog speed,
                    RoomPlmMaridiaElevatubeVisualCatalog elevatube, RoomPlmSporeSpawnCeilingVisualCatalog spore,
                    RoomPlmSamusEaterVisualCatalog eater, RoomPlmBotwoonWallVisualCatalog botwoon,
                    RoomPlmKraidVisualCatalog kraid, RoomPlmCrocomireVisualCatalog crocomire,
                    RoomPlmMotherBrainFakeDeathVisualCatalog motherBrain,
                    ushort dynamicPointer, int dynamicWord) {
                    shot.GetWord(FixturePlmFrames.ShotSingle, 0, 0);
                    shot.GetWord(FixturePlmFrames.ShotSingle, 0, 1);
                    shot.GetWord(FixturePlmFrames.ShotSingle, 1, 0);
                    shot.GetWord(0, 0, 0);
                    station.GetWord(dynamicPointer, 0, 0);
                    station.GetWord(dynamicPointer, 0, int.MaxValue);
                    hand.GetWord((ushort)BombTorizoHandDraw.Intact, 0, 0);
                    hand.GetWord((ushort)BombTorizoHandDraw.Intact, 0, int.MaxValue);
                    glass.GetWord(dynamicPointer, 0, 0);
                    glass.GetWord(dynamicPointer, int.MaxValue, 0);
                    tube.GetWord(dynamicPointer, 0, 0);
                    tube.GetWord(dynamicPointer, 0, int.MaxValue);
                    shot.GetWord(dynamicPointer, 0, dynamicWord);
                    gate.GetWord(dynamicPointer, 0, 0);
                    gate.GetWord(dynamicPointer, 0, int.MaxValue);
                    elevator.GetWord((ushort)ElevatorPlatformDraw.First, 2, 3);
                    elevator.GetWord((ushort)ElevatorPlatformDraw.First, 0, 1);
                    cannon.GetWord((ushort)DraygonCannonDraw.LeftShieldA, 2, 1);
                    cannon.GetWord((ushort)DraygonCannonDraw.LeftShieldA, 0, 1);
                    chozo.GetWord((ushort)ChozoStatueDraw.ClearSlopeAccess, 0, 13);
                    chozo.GetWord((ushort)ChozoStatueDraw.LowerNorfairClearedHand, 0, 1);
                    restore.GetWord((ushort)BombBlockRestoreDraw.Square, 1, 1);
                    restore.GetWord((ushort)BombBlockRestoreDraw.Square, 1, 2);
                    tourian.GetWord((ushort)TourianAccessDraw.Clear, 5, 3);
                    tourian.GetWord((ushort)TourianAccessDraw.EmptyRow, 1, 0);
                    speed.GetWord(SpeedBoosterBlockPlmDrawDefinitions.BombRevealDraw, 0, 0);
                    speed.GetWord(SpeedBoosterBlockPlmDrawDefinitions.BombRevealDraw, 0, 1);
                    elevatube.GetWord(MaridiaElevatubePlmDefinitions.DrawPointer, 0, 0);
                    elevatube.GetWord(MaridiaElevatubePlmDefinitions.DrawPointer, 1, 0);
                    spore.GetWord((ushort)SporeSpawnCeilingDraw.Clear, 1, 1);
                    spore.GetWord((ushort)SporeSpawnCeilingDraw.Clear, 1, 2);
                    eater.GetWord((ushort)SamusEaterDraw.FloorIdle, 2, 3);
                    eater.GetWord((ushort)SamusEaterDraw.FloorIdle, 0, 2);
                    botwoon.GetWord(BotwoonWallPlmDrawDefinitions.ClearPointer, 0, 8);
                    botwoon.GetWord(BotwoonWallPlmDrawDefinitions.ClearPointer, 0, 9);
                    kraid.GetWord((ushort)KraidRoomDraw.ClearSpikes, 0, 21);
                    kraid.GetWord((ushort)KraidRoomDraw.CrumbleFirst, 0, 1);
                    crocomire.GetWord((ushort)CrocomireArenaDraw.ClearBridge, 0, 9);
                    crocomire.GetWord((ushort)CrocomireArenaDraw.CrumbleBridgeBlock, 0, 1);
                    motherBrain.GetWord((ushort)MotherBrainFakeDeathDraw.ClearBottomLeftTube, 0, 4);
                    motherBrain.GetWord((ushort)MotherBrainFakeDeathDraw.ClearBottomLeftTube, 1, 1);
                }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-plm-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        var compilation = CSharpCompilation.Create("PlmProviderFixture", trees,
            platforms.Append(typeof(RoomPlmShotBlockVisualCatalog).Assembly.Location).Distinct()
                .Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        // An unbound consumer argument has no constant, which silently widens a selection to every draw.
        Diagnostic[] consumerErrors = [.. compilation.GetSemanticModel(calls).GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];
        Require(consumerErrors.Length == 0, "the fixture consumer must bind every selection: " + string.Join(" | ", consumerErrors.Select(error => error.ToString())));
        var exports = new ResourceIndex();
        var adapter = new ClosedPresentationAudit(compilation, exports);
        var before = new AuditReport();
        var report = new AuditReport();
        foreach (InvocationExpressionSyntax call in calls.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, before);
            ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, report, adapter);
        }
        Require(before.UnresolvedCount == 41, "the forty-one calls must reproduce the identified missing adapters first" + " (" + before.Describe() + ")");
        Require(report.Classifications.Count == 20 && report.UnresolvedCount == 21 && report.MissingCount == 0 &&
            report.Consumers.Count == 41, "complete valid-domain calls qualify, but wrong pointers and tuple components still fail " +
            "(expected 20/21/0/41; " + report.Describe() + ")");
        Require(report.Findings.Any(item => item.Message.Contains("drawPointer", StringComparison.Ordinal)) &&
            report.Findings.Any(item => item.Message.Contains("runIndex", StringComparison.Ordinal)) &&
            report.Findings.Any(item => item.Message.Contains("word/block", StringComparison.Ordinal)),
            "pointer, run and word failures must come from precise declared domains");
    }

    /// <summary>Strips collision bits from each physical draw run while preserving its run and word shape.</summary>
    internal static ushort[][] VisualRuns(RoomPlmShotBlockDrawDefinitions.DrawList frame) =>
        frame.Runs.Span.SelectToArray(run => run.LevelWords.Span
            .SelectToArray(word => new RoomLevelWord(word).VisualWord));

    /// <summary>Exercises every identified PLM visual catalog with complete native draw identities.</summary>
    private static void ConfirmConstructorCoverage()
    {
        ConfirmCoverage(RoomPlmShotBlockDrawDefinitions.All, frame =>
            new RoomPlmShotBlockVisualEntry(frame.Pointer, VisualRuns(frame)),
            entries => _ = new RoomPlmShotBlockVisualCatalog(entries));
        ConfirmCoverage(RoomPlmStationDrawDefinitions.All, frame =>
            new RoomPlmVisualRunEntry(RoomPlmStationDrawDefinitions.VisualId(frame.Pointer), VisualRuns(frame)),
            entries => _ = new RoomPlmStationVisualCatalog(entries));
        ConfirmCoverage(BombTorizoHandPlmDrawDefinitions.All, frame =>
            new RoomPlmVisualEntry(BombTorizoHandPlmDrawDefinitions.VisualId(frame.Pointer),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = new RoomPlmBombTorizoHandVisualCatalog(entries));
        ConfirmCoverage(MotherBrainGlassPlmDrawDefinitions.All, frame =>
            new RoomPlmVisualEntry(MotherBrainGlassPlmDrawDefinitions.VisualId(frame.Pointer),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = new RoomPlmMotherBrainGlassVisualCatalog(entries));
        ConfirmCoverage(NoobTubePlmDrawDefinitions.All, frame =>
            new RoomPlmVisualEntry(NoobTubePlmDrawDefinitions.VisualId(frame.Pointer),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = new RoomPlmNoobTubeVisualCatalog(entries));
        ConfirmCoverage(DownwardGatePlmDrawDefinitions.All, frame =>
            new RoomPlmVisualRunEntry(DownwardGatePlmDrawDefinitions.VisualId(frame.Pointer), VisualRuns(frame)),
            entries => _ = new RoomPlmDownwardGateVisualCatalog(entries));
        ConfirmCoverage(ElevatorPlatformPlmDefinitions.DrawLists, frame =>
            new RoomPlmVisualRunEntry(ElevatorPlatformPlmDefinitions.VisualId(frame.Pointer), VisualRuns(frame)),
            entries => _ = new RoomPlmElevatorPlatformVisualCatalog(entries));
        ConfirmCoverage(DraygonCannonPlmDrawDefinitions.All, frame =>
            new RoomPlmVisualEntry(DraygonCannonPlmDrawDefinitions.VisualId(frame.Pointer),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = new RoomPlmDraygonCannonVisualCatalog(entries));
        ConfirmCoverage(ChozoStatuePlmDrawDefinitions.All, frame =>
            new RoomPlmVisualEntry(ChozoStatuePlmDrawDefinitions.VisualId(frame.Pointer),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = new RoomPlmChozoStatueVisualCatalog(entries));
        ConfirmCoverage(RoomPlmLinkedRestoreDrawDefinitions.All, frame =>
            new RoomPlmVisualEntry(RoomPlmLinkedRestoreDrawDefinitions.VisualId(frame.Pointer),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = new RoomPlmLinkedRestoreVisualCatalog(entries));
        ConfirmCoverage(TourianAccessPlmDrawDefinitions.All, frame =>
            new RoomPlmVisualEntry(TourianAccessPlmDrawDefinitions.VisualId(frame.Pointer),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = new RoomPlmTourianAccessVisualCatalog(entries));
        ConfirmCoverage(SpeedBoosterBlockPlmDrawDefinitions.All, frame =>
            new RoomPlmVisualEntry(SpeedBoosterBlockPlmDrawDefinitions.VisualId(frame.Pointer),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = new RoomPlmSpeedBoosterVisualCatalog(entries));
        ConfirmCoverage(MaridiaElevatubePlmDefinitions.AllDraws, frame =>
            new RoomPlmVisualEntry(MaridiaElevatubePlmDefinitions.DrawVisualId(frame.Pointer),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = new RoomPlmMaridiaElevatubeVisualCatalog(entries));
        ConfirmCoverage(SporeSpawnCeilingPlmDrawDefinitions.All, frame =>
            new RoomPlmVisualEntry(SporeSpawnCeilingPlmDrawDefinitions.VisualId(frame.Pointer),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = new RoomPlmSporeSpawnCeilingVisualCatalog(entries));
        ConfirmCoverage(SamusEaterPlmDrawDefinitions.All, frame =>
            new RoomPlmVisualEntry(SamusEaterPlmDrawDefinitions.VisualId(frame.Pointer),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = new RoomPlmSamusEaterVisualCatalog(entries));
        ConfirmCoverage(BotwoonWallPlmDrawDefinitions.All, frame =>
            new RoomPlmVisualEntry(BotwoonWallPlmDrawDefinitions.VisualId(frame.Pointer),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = new RoomPlmBotwoonWallVisualCatalog(entries));
        ConfirmCoverage(KraidRoomPlmDrawDefinitions.All, frame =>
            new RoomPlmVisualEntry(KraidRoomPlmDrawDefinitions.VisualId(frame.Pointer),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = new RoomPlmKraidVisualCatalog(entries));
        ConfirmCoverage(CrocomireArenaPlmDrawDefinitions.All, frame =>
            new RoomPlmVisualEntry(CrocomireArenaPlmDrawDefinitions.VisualId(frame.Pointer),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = new RoomPlmCrocomireVisualCatalog(entries));
        ConfirmCoverage(MotherBrainFakeDeathPlmDrawDefinitions.All, frame =>
            new RoomPlmVisualEntry(MotherBrainFakeDeathPlmDrawDefinitions.VisualId(frame.Pointer),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = new RoomPlmMotherBrainFakeDeathVisualCatalog(entries));
    }

    /// <summary>Requires a provider to admit the full domain and reject missing or duplicated draw identities.</summary>
    internal static void ConfirmCoverage<T>(IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> definitions,
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

    /// <summary>Fails the PLM contract audit with its violated provider condition.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("PLM provider confirmation failed: " + reason);
    }
}
