using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Areas.Escape.LookupEscapeGateStockVerification;
using static SuperMetroid.Verification.Rooms.Plms.RoomPlmPopulationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
namespace SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Checks Mother Brain escape gate visual overrides against PLM collision and install contracts.</summary>
internal static class EscapeGateVisualsVerification
{
    /// <summary>Checks cloned closed and half-closed frames, rejected physical bits, and live and installed edits.</summary>
    internal static void VerifyEscapeGateVisuals(SuperMetroidAddressSpace rom)
    {
        RoomPlmVisualEntry[] entries =
            MotherBrainEscapeGateDraws.All
                .OrderBy(draw => PlmDrawAddressCodec.Of(draw.Draw))
                .Select(draw => new RoomPlmVisualEntry(
                    MotherBrainEscapeGateDraws.VisualId(draw.Draw),
                    draw.Runs.Span[0].Blocks.Span
                        .SelectToArray(block => block.Appearance)))
                .ToArray();
        RoomPlmVisualEntry closed = entries.Single(entry =>
            entry.Id == "closed");
        RoomPlmVisualEntry halfClosed = entries.Single(entry =>
            entry.Id == "half-closed");
        RoomBlockAppearance stockClosed = closed.Blocks[0];
        RoomBlockAppearance stockHalfClosed = halfClosed.Blocks[0];
        closed.Blocks[0] = RoomBlockAppearance.Of(0x53);
        halfClosed.Blocks[0] = RoomBlockAppearance.Of(0x54);
        var edited = new RoomPlmEscapeGateVisualCatalog(entries);
        closed.Blocks[0] = RoomBlockAppearance.Of(0x55);
        halfClosed.Blocks[0] = RoomBlockAppearance.Of(0x56);
        AssertEqual(RoomBlockAppearance.Of(0x53),
            edited.GetAppearance(PlmDraw.Of(MotherBrainEscapeGateDraw.Closed), 0),
            "escape-gate catalog copies closed-frame author data");
        AssertEqual(RoomBlockAppearance.Of(0x54),
            edited.GetAppearance(PlmDraw.Of(MotherBrainEscapeGateDraw.HalfClosed), 0),
            "escape-gate catalog copies transition-frame author data");
        AssertEqual(stockClosed,
            RoomPlmEscapeGateVisualCatalog.Stock().GetAppearance(
                PlmDraw.Of(MotherBrainEscapeGateDraw.Closed), 0),
            "stock escape-gate catalog retains cartridge appearance");
        closed.Blocks[0] = stockClosed;
        halfClosed.Blocks[0] = stockHalfClosed;

        AssertThrows<InvalidDataException>(
            () => new RoomPlmEscapeGateVisualCatalog(entries.Skip(1)),
            "escape-gate catalog rejects a missing frame");
        Suite(nameof(VerifyEscapeGateLiveVisual), () => VerifyEscapeGateLiveVisual(edited));
        Suite(nameof(VerifyEscapeGateVisualInstallation), () => VerifyEscapeGateVisualInstallation(rom));
        Console.WriteLine(
            "Escape-gate visuals: closed/closing edits preserve physical collision, installed overrides survive refresh, and invalid content fails loudly.");
    }

    /// <summary>Runs resident and door-triggered gates to prove edited tiles leave collision unchanged.</summary>
    private static void VerifyEscapeGateLiveVisual(
        RoomPlmEscapeGateVisualCatalog edited)
    {
        const int width = 16;
        const int gateX = 7;
        const int gateY = 2;
        const ushort population = 0x9000;
        int origin = gateY * width + gateX;
        var bus = new TestAddressSpace();
        // Header dispatch remains a cartridge read; only the gate's program and
        // draw payloads are compiled and therefore guarded below.
        SeedPlmProgramWords(bus, 0xc8ca, 0xb3c1,
            RoomPlmInstructionListsTooling.MotherBrainEscapeRoomGateClosed,
            RoomPlmInstructionListsConstants.MotherBrainEscapeRoomGateClosing);
        SeedRoomPlmPopulation(bus, population,
        [
            0xca, 0xc8, gateX, gateY, 0x00, 0x80,
            0x00, 0x00,
        ]);
        var guarded = NewMotherBrainEscapeGateGuard(bus);
        RoomLevelData CreateLevel() => CreateEmptyRoom(new(width, width));
        var system = new SharedGameState();

        RoomLevelData closedLevel = CreateLevel();
        var closed = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmEscapeGateVisuals = edited });
        AssertEqual(1, PlmFixtures.LoadPopulation(ref closed, new PlmRoom(closedLevel, BgArtworkFixtures.For(new SnesVram()), system, AreaId.Tourian), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation)), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "edited escape gate loads its real resident header");
        closed.Step(closedLevel);
        AssertEqual(RoomTerrainBlock.Of(0x30f, BlockMirroring.None, RoomCollisionType.SolidBlock),
            closedLevel.GetCollisionBlockByIndex(origin).Terrain,
            "edited closed gate preserves native physical collision");
        AssertEqual(RoomBlockAppearance.Of(0x53),
            closedLevel.DisplayedAppearance(origin),
            "edited closed gate survives later camera streaming");

        RoomLevelData closingLevel = CreateLevel();
        var closing = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmEscapeGateVisuals = edited });
        AssertEqual(1, PlmFixtures.LoadPopulation(ref closing, new PlmRoom(closingLevel, BgArtworkFixtures.For(new SnesVram()), system, AreaId.Tourian), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation)), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "edited escape closure loads resident gate");
        var enteringDoor = new DoorDefinition(
            Name: nameof(TourianDoorDefinitions.MotherBrain1),
            DestinationRoom: RoomHeaderDefinitions.TourianEscape1,
            Orientation: new(DoorDirection.Left, DoorClosingBehavior.EscapeGateCloses),
            PlmBlock: new(gateX, gateY),
            DestinationScreen: new(0, 0),
            SamusSpeed: DoorSamusSpeed.AxisDefault,
            SetupCode: DoorSetupCode.None);
        AssertTrue(closing.DoorPlms.TrySpawnDoorClosingPlm(closingLevel,
                enteringDoor, system),
            "edited escape closure selects the real secondary instruction list");
        for (int frame = 0; frame < 3; frame++)
            closing.Step(closingLevel);
        AssertEqual(RoomTerrainBlock.Of(0x30f, BlockMirroring.None, RoomCollisionType.SolidBlock),
            closingLevel.GetCollisionBlockByIndex(origin).Terrain,
            "edited half-closed gate preserves native physical collision");
        AssertEqual(RoomBlockAppearance.Of(0x54), closingLevel.DisplayedAppearance(origin),
            "edited half-closed gate is shown");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "escape-gate visuals never reread compiled program or draw payloads");
    }

    /// <summary>Checks extraction, override reload after stock refresh, and tampered stock rejection.</summary>
    private static void VerifyEscapeGateVisualInstallation(SuperMetroidAddressSpace rom)
    {
        using var testRootScratch = new TestScratchDirectory("escape-gate-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmEscapeGateVisualFiles.Extract(rom,
                installation.RoomPlmEscapeGateVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmEscapeGateVisualFiles.ValidateStock(
                installation.RoomPlmEscapeGateVisualDirectory);
            VerifyEscapeGateStockMapping(rom, installation.LoadRoomPlmEscapeGateVisuals());
            string stockPath = Path.Combine(
                installation.RoomPlmEscapeGateVisualDirectory,
                RoomPlmEscapeGateVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted escape-gate JSON is empty.");
            JsonNode closed = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "closed");
            closed.Required("blocks")[0] = InstalledAppearanceJson.Cell(0x53);
            Directory.CreateDirectory(
                installation.RoomPlmEscapeGateVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmEscapeGateVisualOverrideDirectory,
                RoomPlmEscapeGateVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual(RoomBlockAppearance.Of(0x53),
                installation.LoadRoomPlmEscapeGateVisuals().GetAppearance(
                    PlmDraw.Of(MotherBrainEscapeGateDraw.Closed), 0),
                "installed escape-gate override changes selected frame");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmEscapeGateVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual(RoomBlockAppearance.Of(0x53),
                RoomPlmEscapeGateVisualFiles.Load(refreshed,
                    installation.RoomPlmEscapeGateVisualOverrideDirectory).GetAppearance(
                    PlmDraw.Of(MotherBrainEscapeGateDraw.Closed), 0),
                "escape-gate override survives stock replacement");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmEscapeGateVisualFiles.ValidateStock(
                    installation.RoomPlmEscapeGateVisualDirectory),
                "tampered escape-gate stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }
}
