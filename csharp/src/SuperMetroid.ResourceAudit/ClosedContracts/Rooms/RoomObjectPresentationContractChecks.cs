using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Confirms the identified PLM artwork closure and tuple-shape audit, never a PLM instruction or room.</summary>
internal static class RoomObjectPresentationContractChecks
{
    /// <summary>Checks PLM visual provider coverage and audit classification for valid and invalid draw/run/block calls.</summary>
    internal static void Run()
    {
        ConfirmConstructorCoverage();
        var trees = RoomObjectClosedContractDefinitions.All.Concat(RoomObjectActorClosedContractDefinitions.All)
            .Concat(RoomObjectProgressionClosedContractDefinitions.All)
            .SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        // The consumer names draws through these internal enums; compiled from source so its constants bind.
        trees.AddRange(new[]
        {
            "csharp/src/SuperMetroid.Core/Bosses/Draygon/DraygonCannonDraw.cs", "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/ChozoStatueDraw.cs",
            "csharp/src/SuperMetroid.Core/Areas/Tourian/TourianAccessDraw.cs", "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/KraidRoomDraw.cs",
            "csharp/src/SuperMetroid.Core/Bosses/Crocomire/CrocomireArenaDraw.cs", "csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainFakeDeathDraw.cs",
            "csharp/src/SuperMetroid.Core/Bosses/Torizo/BombTorizoHandDraw.cs", "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/ElevatorPlatformDraw.cs",
            "csharp/src/SuperMetroid.Core/Rooms/LevelData/BombBlockRestoreDraw.cs", "csharp/src/SuperMetroid.Core/Bosses/SporeSpawn/SporeSpawnCeilingDraw.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/SamusEaterDraw.cs", "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/StationDraw.cs",
            "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectDraw.cs", "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectFrameDraw.cs",
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
            using SuperMetroid.Core.Rooms.RoomObjects;
            using SuperMetroid.Core.Areas.Tourian.Artwork;
            using SuperMetroid.Core.Bosses.Crocomire.Artwork;
            using SuperMetroid.Core.Bosses.Draygon.Artwork;
            using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
            using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
            using SuperMetroid.Core.Bosses.Torizo.Artwork;
            class RoomObjectConsumer {
                void Inspect(InstalledRoomObjectShotBlockVisuals shot, InstalledRoomObjectStationVisuals station,
                    RoomObjectBombTorizoHandVisualCatalog hand, RoomObjectMotherBrainGlassVisualCatalog glass,
                    InstalledRoomObjectNoobTubeVisuals tube, InstalledRoomObjectDownwardGateVisuals gate,
                    RoomObjectElevatorPlatformVisualCatalog elevator, RoomObjectDraygonCannonVisualCatalog cannon,
                    RoomObjectChozoStatueVisualCatalog chozo, InstalledRoomObjectLinkedRestoreVisuals restore,
                    RoomObjectTourianAccessVisuals tourian, InstalledRoomObjectSpeedBoosterVisuals speed,
                    InstalledRoomObjectMaridiaElevatubeVisuals elevatube, RoomObjectSporeSpawnCeilingVisualCatalog spore,
                    InstalledRoomObjectSamusEaterVisuals eater, InstalledRoomObjectBotwoonWallVisuals botwoon,
                    InstalledRoomObjectKraidVisuals kraid, RoomObjectCrocomireVisualCatalog crocomire,
                    RoomObjectMotherBrainFakeDeathVisualCatalog motherBrain,
                    RoomObjectDraw dynamicDraw, int dynamicWord) {
                    shot.GetAppearance(ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 0), 0, 0);
                    shot.GetAppearance(ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 0), 0, 1);
                    shot.GetAppearance(ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 0), 1, 0);
                    shot.GetAppearance(RoomObjectDraw.Of(StationDraw.MapFrame0), 0, 0);
                    station.GetAppearance(dynamicDraw, 0, 0);
                    station.GetAppearance(dynamicDraw, 0, int.MaxValue);
                    hand.GetAppearance(RoomObjectDraw.Of(BombTorizoHandDraw.Intact), 0, 0);
                    hand.GetAppearance(RoomObjectDraw.Of(BombTorizoHandDraw.Intact), 0, int.MaxValue);
                    glass.GetAppearance(dynamicDraw, 0, 0);
                    glass.GetAppearance(dynamicDraw, int.MaxValue, 0);
                    tube.GetAppearance(dynamicDraw, 0, 0);
                    tube.GetAppearance(dynamicDraw, 0, int.MaxValue);
                    shot.GetAppearance(dynamicDraw, 0, dynamicWord);
                    gate.GetAppearance(dynamicDraw, 0, 0);
                    gate.GetAppearance(dynamicDraw, 0, int.MaxValue);
                    elevator.GetAppearance(RoomObjectDraw.Of(ElevatorPlatformDraw.First), 2, 3);
                    elevator.GetAppearance(RoomObjectDraw.Of(ElevatorPlatformDraw.First), 0, 1);
                    cannon.GetAppearance(RoomObjectDraw.Of(DraygonCannonDraw.LeftShieldA), 2, 1);
                    cannon.GetAppearance(RoomObjectDraw.Of(DraygonCannonDraw.LeftShieldA), 0, 1);
                    chozo.GetAppearance(RoomObjectDraw.Of(ChozoStatueDraw.ClearSlopeAccess), 0, 13);
                    chozo.GetAppearance(RoomObjectDraw.Of(ChozoStatueDraw.LowerNorfairClearedHand), 0, 1);
                    restore.GetAppearance(RoomObjectDraw.Of(BombBlockRestoreDraw.Square), 1, 1);
                    restore.GetAppearance(RoomObjectDraw.Of(BombBlockRestoreDraw.Square), 1, 2);
                    tourian.GetAppearance(RoomObjectDraw.Of(TourianAccessDraw.Clear), 5, 3);
                    tourian.GetAppearance(RoomObjectDraw.Of(TourianAccessDraw.EmptyRow), 1, 0);
                    speed.GetAppearance(SpeedBoosterBlockObjectDrawDefinitions.BombRevealDraw, 0, 0);
                    speed.GetAppearance(SpeedBoosterBlockObjectDrawDefinitions.BombRevealDraw, 0, 1);
                    elevatube.GetAppearance(MaridiaElevatubeObjectDefinitions.BlockDraw, 0, 0);
                    elevatube.GetAppearance(MaridiaElevatubeObjectDefinitions.BlockDraw, 1, 0);
                    spore.GetAppearance(RoomObjectDraw.Of(SporeSpawnCeilingDraw.Clear), 1, 1);
                    spore.GetAppearance(RoomObjectDraw.Of(SporeSpawnCeilingDraw.Clear), 1, 2);
                    eater.GetAppearance(RoomObjectDraw.Of(SamusEaterDraw.FloorIdle), 2, 3);
                    eater.GetAppearance(RoomObjectDraw.Of(SamusEaterDraw.FloorIdle), 0, 2);
                    botwoon.GetAppearance(BotwoonWallObjectDrawDefinitions.Clear, 0, 8);
                    botwoon.GetAppearance(BotwoonWallObjectDrawDefinitions.Clear, 0, 9);
                    kraid.GetAppearance(RoomObjectDraw.Of(KraidRoomDraw.ClearSpikes), 0, 21);
                    kraid.GetAppearance(RoomObjectDraw.Of(KraidRoomDraw.CrumbleFirst), 0, 1);
                    crocomire.GetAppearance(RoomObjectDraw.Of(CrocomireArenaDraw.ClearBridge), 0, 9);
                    crocomire.GetAppearance(RoomObjectDraw.Of(CrocomireArenaDraw.CrumbleBridgeBlock), 0, 1);
                    motherBrain.GetAppearance(RoomObjectDraw.Of(MotherBrainFakeDeathDraw.ClearBottomLeftTube), 0, 4);
                    motherBrain.GetAppearance(RoomObjectDraw.Of(MotherBrainFakeDeathDraw.ClearBottomLeftTube), 1, 1);
                }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-plm-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        var compilation = CSharpCompilation.Create("RoomObjectProviderFixture", trees,
            platforms.Append(typeof(InstalledRoomObjectShotBlockVisuals).Assembly.Location).Distinct()
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
    internal static RoomBlockAppearance[][] VisualRuns(RoomObjectShotBlockDrawDefinitions.DrawList frame) =>
        frame.Runs.Span.SelectToArray(run => run.Blocks.Span
            .SelectToArray(block => block.Appearance));

    /// <summary>Exercises every identified PLM visual catalog with complete native draw identities.</summary>
    private static void ConfirmConstructorCoverage()
    {
        ConfirmCoverage(RoomObjectShotBlockVisualFiles.All, frame =>
            new RoomObjectVisualRunEntry(RoomObjectShotBlockVisualFiles.VisualId(frame.Draw), VisualRuns(frame)),
            entries => _ = RoomObjectShotBlockVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectStationVisualFiles.All, frame =>
            new RoomObjectVisualRunEntry(RoomObjectStationVisualFiles.VisualId(frame.Draw), VisualRuns(frame)),
            entries => _ = RoomObjectStationVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectBombTorizoHandVisualFiles.All, frame =>
            new RoomObjectVisualEntry(RoomObjectBombTorizoHandVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomObjectBombTorizoHandVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectMotherBrainGlassVisualFiles.All, frame =>
            new RoomObjectVisualEntry(RoomObjectMotherBrainGlassVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomObjectMotherBrainGlassVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectNoobTubeVisualFiles.All, frame =>
            new RoomObjectVisualEntry(RoomObjectNoobTubeVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomObjectNoobTubeVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectDownwardGateVisualFiles.All, frame =>
            new RoomObjectVisualRunEntry(RoomObjectDownwardGateVisualFiles.VisualId(frame.Draw), VisualRuns(frame)),
            entries => _ = RoomObjectDownwardGateVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectElevatorPlatformVisualFiles.DrawLists, frame =>
            new RoomObjectVisualRunEntry(RoomObjectElevatorPlatformVisualFiles.VisualId(frame.Draw), VisualRuns(frame)),
            entries => _ = RoomObjectElevatorPlatformVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectDraygonCannonVisualFiles.All, frame =>
            new RoomObjectVisualEntry(RoomObjectDraygonCannonVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomObjectDraygonCannonVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectChozoStatueVisualFiles.All, frame =>
            new RoomObjectVisualEntry(RoomObjectChozoStatueVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomObjectChozoStatueVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectLinkedRestoreVisualFiles.All, frame =>
            new RoomObjectVisualEntry(RoomObjectLinkedRestoreVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomObjectLinkedRestoreVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectTourianAccessVisualFiles.All, frame =>
            new RoomObjectVisualEntry(RoomObjectTourianAccessVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomObjectTourianAccessVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectSpeedBoosterVisualFiles.All, frame =>
            new RoomObjectVisualEntry(RoomObjectSpeedBoosterVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomObjectSpeedBoosterVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectMaridiaElevatubeVisualFiles.AllDraws, frame =>
            new RoomObjectVisualEntry(RoomObjectMaridiaElevatubeVisualFiles.DrawVisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomObjectMaridiaElevatubeVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectSporeSpawnCeilingVisualFiles.All, frame =>
            new RoomObjectVisualEntry(RoomObjectSporeSpawnCeilingVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomObjectSporeSpawnCeilingVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectSamusEaterVisualFiles.All, frame =>
            new RoomObjectVisualEntry(RoomObjectSamusEaterVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomObjectSamusEaterVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectBotwoonWallVisualFiles.All, frame =>
            new RoomObjectVisualEntry(RoomObjectBotwoonWallVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomObjectBotwoonWallVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectKraidVisualFiles.All, frame =>
            new RoomObjectVisualEntry(RoomObjectKraidVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomObjectKraidVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectCrocomireVisualFiles.All, frame =>
            new RoomObjectVisualEntry(RoomObjectCrocomireVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomObjectCrocomireVisualFiles.Create(entries));
        ConfirmCoverage(RoomObjectMotherBrainFakeDeathVisualFiles.All, frame =>
            new RoomObjectVisualEntry(RoomObjectMotherBrainFakeDeathVisualFiles.VisualId(frame.Draw),
                VisualRuns(frame).SelectMany(run => run).ToArray()),
            entries => _ = RoomObjectMotherBrainFakeDeathVisualFiles.Create(entries));
    }

    /// <summary>Requires a provider to admit the full domain and reject missing or duplicated draw identities.</summary>
    internal static void ConfirmCoverage<T>(IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> definitions,
        Func<RoomObjectShotBlockDrawDefinitions.DrawList, T> entry, Action<IEnumerable<T>> construct)
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
