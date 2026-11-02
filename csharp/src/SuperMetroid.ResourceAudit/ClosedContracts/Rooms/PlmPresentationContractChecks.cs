using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Confirms the identified PLM artwork closure and tuple-shape audit, never a PLM instruction or room.</summary>
internal static class PlmPresentationContractChecks
{
    /// <summary>Checks PLM visual provider coverage and audit classification for valid and invalid draw/run/block calls.</summary>
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
            "csharp/src/SuperMetroid.Core/Bosses/Draygon/DraygonCannonDraw.cs", "csharp/src/SuperMetroid.Core/Rooms/Plms/ChozoStatueDraw.cs",
            "csharp/src/SuperMetroid.Core/Areas/Tourian/TourianAccessDraw.cs", "csharp/src/SuperMetroid.Core/Rooms/Plms/KraidRoomDraw.cs",
            "csharp/src/SuperMetroid.Core/Bosses/Crocomire/CrocomireArenaDraw.cs", "csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainFakeDeathDraw.cs",
            "csharp/src/SuperMetroid.Core/Bosses/Torizo/BombTorizoHandDraw.cs", "csharp/src/SuperMetroid.Core/Rooms/Plms/ElevatorPlatformDraw.cs",
            "csharp/src/SuperMetroid.Core/Rooms/LevelData/BombBlockRestoreDraw.cs", "csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/SporeSpawnCeilingDraw.cs",
            "csharp/src/SuperMetroid.Core/Rooms/Plms/SamusEaterDraw.cs", "csharp/src/SuperMetroid.Core/Rooms/Plms/StationDraw.cs",
            "csharp/src/SuperMetroid.Core/Rooms/Plms/PlmDraw.cs", "csharp/src/SuperMetroid.Core/Rooms/Plms/PlmFrameDraw.cs",
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
            using SuperMetroid.Core.Enemies.Chozo.Artwork;
            using SuperMetroid.Core.Enemies.Elevator;
            using SuperMetroid.Core.Enemies.Elevator.Artwork;
            using SuperMetroid.Core.Rooms.LevelData;
            using SuperMetroid.Core.Rooms.Plms;
            using SuperMetroid.Core.Areas.Tourian.Artwork;
            using SuperMetroid.Core.Bosses.Crocomire.Artwork;
            using SuperMetroid.Core.Bosses.Draygon.Artwork;
            using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
            using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
            using SuperMetroid.Core.Bosses.Torizo.Artwork;
            class PlmConsumer {
                void Inspect(InstalledRoomPlmShotBlockVisuals shot, InstalledRoomPlmStationVisuals station,
                    RoomPlmBombTorizoHandVisualCatalog hand, RoomPlmMotherBrainGlassVisualCatalog glass,
                    InstalledRoomPlmNoobTubeVisuals tube, InstalledRoomPlmDownwardGateVisuals gate,
                    RoomPlmElevatorPlatformVisualCatalog elevator, RoomPlmDraygonCannonVisualCatalog cannon,
                    RoomPlmChozoStatueVisualCatalog chozo, InstalledRoomPlmLinkedRestoreVisuals restore,
                    RoomPlmTourianAccessVisuals tourian, InstalledRoomPlmSpeedBoosterVisuals speed,
                    InstalledRoomPlmMaridiaElevatubeVisuals elevatube, RoomPlmSporeSpawnCeilingVisualCatalog spore,
                    InstalledRoomPlmSamusEaterVisuals eater, InstalledRoomPlmBotwoonWallVisuals botwoon,
                    InstalledRoomPlmKraidVisuals kraid, RoomPlmCrocomireVisualCatalog crocomire,
                    RoomPlmMotherBrainFakeDeathVisualCatalog motherBrain,
                    PlmDraw dynamicDraw, int dynamicWord) {
                    shot.GetAppearance(ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, 0), 0, 0);
                    shot.GetAppearance(ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, 0), 0, 1);
                    shot.GetAppearance(ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, 0), 1, 0);
                    shot.GetAppearance(PlmDraw.Of(StationDraw.MapFrame0), 0, 0);
                    station.GetAppearance(dynamicDraw, 0, 0);
                    station.GetAppearance(dynamicDraw, 0, int.MaxValue);
                    hand.GetAppearance(PlmDraw.Of(BombTorizoHandDraw.Intact), 0, 0);
                    hand.GetAppearance(PlmDraw.Of(BombTorizoHandDraw.Intact), 0, int.MaxValue);
                    glass.GetAppearance(dynamicDraw, 0, 0);
                    glass.GetAppearance(dynamicDraw, int.MaxValue, 0);
                    tube.GetAppearance(dynamicDraw, 0, 0);
                    tube.GetAppearance(dynamicDraw, 0, int.MaxValue);
                    shot.GetAppearance(dynamicDraw, 0, dynamicWord);
                    gate.GetAppearance(dynamicDraw, 0, 0);
                    gate.GetAppearance(dynamicDraw, 0, int.MaxValue);
                    elevator.GetAppearance(PlmDraw.Of(ElevatorPlatformDraw.First), 2, 3);
                    elevator.GetAppearance(PlmDraw.Of(ElevatorPlatformDraw.First), 0, 1);
                    cannon.GetAppearance(PlmDraw.Of(DraygonCannonDraw.LeftShieldA), 2, 1);
                    cannon.GetAppearance(PlmDraw.Of(DraygonCannonDraw.LeftShieldA), 0, 1);
                    chozo.GetAppearance(PlmDraw.Of(ChozoStatueDraw.ClearSlopeAccess), 0, 13);
                    chozo.GetAppearance(PlmDraw.Of(ChozoStatueDraw.LowerNorfairClearedHand), 0, 1);
                    restore.GetAppearance(PlmDraw.Of(BombBlockRestoreDraw.Square), 1, 1);
                    restore.GetAppearance(PlmDraw.Of(BombBlockRestoreDraw.Square), 1, 2);
                    tourian.GetAppearance(PlmDraw.Of(TourianAccessDraw.Clear), 5, 3);
                    tourian.GetAppearance(PlmDraw.Of(TourianAccessDraw.EmptyRow), 1, 0);
                    speed.GetAppearance(SpeedBoosterBlockPlmDrawDefinitions.BombRevealDraw, 0, 0);
                    speed.GetAppearance(SpeedBoosterBlockPlmDrawDefinitions.BombRevealDraw, 0, 1);
                    elevatube.GetAppearance(MaridiaElevatubePlmDefinitions.BlockDraw, 0, 0);
                    elevatube.GetAppearance(MaridiaElevatubePlmDefinitions.BlockDraw, 1, 0);
                    spore.GetAppearance(PlmDraw.Of(SporeSpawnCeilingDraw.Clear), 1, 1);
                    spore.GetAppearance(PlmDraw.Of(SporeSpawnCeilingDraw.Clear), 1, 2);
                    eater.GetAppearance(PlmDraw.Of(SamusEaterDraw.FloorIdle), 2, 3);
                    eater.GetAppearance(PlmDraw.Of(SamusEaterDraw.FloorIdle), 0, 2);
                    botwoon.GetAppearance(BotwoonWallPlmDrawDefinitions.Clear, 0, 8);
                    botwoon.GetAppearance(BotwoonWallPlmDrawDefinitions.Clear, 0, 9);
                    kraid.GetAppearance(PlmDraw.Of(KraidRoomDraw.ClearSpikes), 0, 21);
                    kraid.GetAppearance(PlmDraw.Of(KraidRoomDraw.CrumbleFirst), 0, 1);
                    crocomire.GetAppearance(PlmDraw.Of(CrocomireArenaDraw.ClearBridge), 0, 9);
                    crocomire.GetAppearance(PlmDraw.Of(CrocomireArenaDraw.CrumbleBridgeBlock), 0, 1);
                    motherBrain.GetAppearance(PlmDraw.Of(MotherBrainFakeDeathDraw.ClearBottomLeftTube), 0, 4);
                    motherBrain.GetAppearance(PlmDraw.Of(MotherBrainFakeDeathDraw.ClearBottomLeftTube), 1, 1);
                }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-plm-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        var compilation = CSharpCompilation.Create("PlmProviderFixture", trees,
            platforms.Append(typeof(InstalledRoomPlmShotBlockVisuals).Assembly.Location).Distinct()
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
            report.Consumers.Count == 41, "complete valid-domain calls qualify, but foreign draws and tuple components still fail " +
            "(expected 20/21/0/41; " + report.Describe() + ")");
        Require(report.Findings.Any(item => item.Message.Contains("is not owned", StringComparison.Ordinal)) &&
            report.Findings.Any(item => item.Message.Contains("runIndex", StringComparison.Ordinal)) &&
            report.Findings.Any(item => item.Message.Contains("word/block", StringComparison.Ordinal)),
            "draw, run and word failures must come from precise declared domains");
    }

    /// <summary>Strips collision bits from each physical draw run while preserving its run and word shape.</summary>
    internal static RoomBlockAppearance[][] VisualRuns(RoomPlmShotBlockDrawDefinitions.DrawList frame) =>
        frame.Runs.Span.SelectToArray(run => run.Blocks.Span
            .SelectToArray(block => block.Appearance));

    /// <summary>Exercises every identified PLM visual catalog with complete native draw identities.</summary>
    private static void ConfirmConstructorCoverage()
    {
        ConfirmCoverage(RoomPlmShotBlockVisualFiles.All, frame =>
            new RoomPlmVisualRunEntry(RoomPlmShotBlockVisualFiles.VisualId(frame.Draw), VisualRuns(frame)),
            entries => _ = RoomPlmShotBlockVisualFiles.Create(entries));
        ConfirmCoverage(RoomPlmStationVisualFiles.All, frame =>
            new RoomPlmVisualRunEntry(RoomPlmStationVisualFiles.VisualId(frame.Draw), VisualRuns(frame)),
            entries => _ = RoomPlmStationVisualFiles.Create(entries));
        ConfirmCoverage(RoomPlmBombTorizoHandVisualFiles.All, frame =>
            new RoomPlmVisualEntry(RoomPlmBombTorizoHandVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomPlmBombTorizoHandVisualFiles.Create(entries));
        ConfirmCoverage(RoomPlmMotherBrainGlassVisualFiles.All, frame =>
            new RoomPlmVisualEntry(RoomPlmMotherBrainGlassVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomPlmMotherBrainGlassVisualFiles.Create(entries));
        ConfirmCoverage(RoomPlmNoobTubeVisualFiles.All, frame =>
            new RoomPlmVisualEntry(RoomPlmNoobTubeVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomPlmNoobTubeVisualFiles.Create(entries));
        ConfirmCoverage(RoomPlmDownwardGateVisualFiles.All, frame =>
            new RoomPlmVisualRunEntry(RoomPlmDownwardGateVisualFiles.VisualId(frame.Draw), VisualRuns(frame)),
            entries => _ = RoomPlmDownwardGateVisualFiles.Create(entries));
        ConfirmCoverage(RoomPlmElevatorPlatformVisualFiles.DrawLists, frame =>
            new RoomPlmVisualRunEntry(RoomPlmElevatorPlatformVisualFiles.VisualId(frame.Draw), VisualRuns(frame)),
            entries => _ = RoomPlmElevatorPlatformVisualFiles.Create(entries));
        ConfirmCoverage(RoomPlmDraygonCannonVisualFiles.All, frame =>
            new RoomPlmVisualEntry(RoomPlmDraygonCannonVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomPlmDraygonCannonVisualFiles.Create(entries));
        ConfirmCoverage(RoomPlmChozoStatueVisualFiles.All, frame =>
            new RoomPlmVisualEntry(RoomPlmChozoStatueVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomPlmChozoStatueVisualFiles.Create(entries));
        ConfirmCoverage(RoomPlmLinkedRestoreVisualFiles.All, frame =>
            new RoomPlmVisualEntry(RoomPlmLinkedRestoreVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomPlmLinkedRestoreVisualFiles.Create(entries));
        ConfirmCoverage(RoomPlmTourianAccessVisualFiles.All, frame =>
            new RoomPlmVisualEntry(RoomPlmTourianAccessVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomPlmTourianAccessVisualFiles.Create(entries));
        ConfirmCoverage(RoomPlmSpeedBoosterVisualFiles.All, frame =>
            new RoomPlmVisualEntry(RoomPlmSpeedBoosterVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomPlmSpeedBoosterVisualFiles.Create(entries));
        ConfirmCoverage(RoomPlmMaridiaElevatubeVisualFiles.AllDraws, frame =>
            new RoomPlmVisualEntry(RoomPlmMaridiaElevatubeVisualFiles.DrawVisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomPlmMaridiaElevatubeVisualFiles.Create(entries));
        ConfirmCoverage(RoomPlmSporeSpawnCeilingVisualFiles.All, frame =>
            new RoomPlmVisualEntry(RoomPlmSporeSpawnCeilingVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomPlmSporeSpawnCeilingVisualFiles.Create(entries));
        ConfirmCoverage(RoomPlmSamusEaterVisualFiles.All, frame =>
            new RoomPlmVisualEntry(RoomPlmSamusEaterVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomPlmSamusEaterVisualFiles.Create(entries));
        ConfirmCoverage(RoomPlmBotwoonWallVisualFiles.All, frame =>
            new RoomPlmVisualEntry(RoomPlmBotwoonWallVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomPlmBotwoonWallVisualFiles.Create(entries));
        ConfirmCoverage(RoomPlmKraidVisualFiles.All, frame =>
            new RoomPlmVisualEntry(RoomPlmKraidVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomPlmKraidVisualFiles.Create(entries));
        ConfirmCoverage(RoomPlmCrocomireVisualFiles.All, frame =>
            new RoomPlmVisualEntry(RoomPlmCrocomireVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomPlmCrocomireVisualFiles.Create(entries));
        ConfirmCoverage(RoomPlmMotherBrainFakeDeathVisualFiles.All, frame =>
            new RoomPlmVisualEntry(RoomPlmMotherBrainFakeDeathVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomPlmMotherBrainFakeDeathVisualFiles.Create(entries));
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
