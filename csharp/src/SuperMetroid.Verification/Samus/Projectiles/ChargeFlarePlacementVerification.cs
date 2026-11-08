using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Tooling;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Projectiles.ChargeFlareCompositionsVerification;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend;
namespace SuperMetroid.Verification.Samus.Projectiles;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Gameplay;

/// <summary>Checks installed charge-flare offsets against native OAM placement and the production rendering paths.</summary>
internal static class ChargeFlarePlacementVerification
{
    /// <summary>Compares stock flare placement across pose, direction, coordinate, and Mode 7 cases, then checks state isolation and malformed-document rejection.</summary>
    internal static void VerifyChargeFlarePlacement(SuperMetroidAddressSpace bus)
    {
        Suite(nameof(VerifyChargeFlareCompositions), () => VerifyChargeFlareCompositions(bus));
        byte[] json = ChargeFlarePlacementExtractor.Extract(bus);
        var stock = ChargeFlarePlacementFile.Load(new MemoryStream(json));
        Suite(nameof(VerifyRuntimeFlarePlacement), () => VerifyRuntimeFlarePlacement(stock));
        var system = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads);
        var drawMetadata = new FlareMetadataDraw(system.Animation.DrawFlareComponentWithMetadata);
        var draw = new Action<SpriteFrameBuffer, SamusState, Position16<RoomPixels>, int, TurningFloorTransform?, ChargeFlareArtwork>(system.Animation.DrawFlareComponent);
        ProjectileDrawArtwork objects = InstalledFixtures.ProjectileArtwork();
        int cases = 0;
        foreach (SamusPoseId pose in new[]
        {
            SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose,
            SamusPoseId.MovingRightNormalPose, SamusPoseId.MovingLeftNormalPose
        })
        for (byte direction = 0; direction < ChargeFlarePlacementDefinitions.StandingDirectionCount; direction++)
        foreach (ushort coordinate in new ushort[] { 0, 100, 255, ushort.MaxValue })
        foreach (TurningFloorTransform? transform in new TurningFloorTransform?[] { null, new(0.0666, new Position16<RoomPixels>(128, 112)) })
        for (int component = 0; component < 3; component++)
        {
            // Exercise all original low-nibble operands at the renderer's metadata
            // boundary; compiled pose rules cannot be overridden by fake ROM bytes.
            var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(coordinate, coordinate)) };
            int record = SamusSourceDefinitions.PoseDefinitions.Value + SamusPoseNumbers.Number(pose) * 8;
            bool running = bus.ReadCartridgeByte(record + 1) == SamusMovementTypeNativeWords.Byte(SamusMovementType.Running);
            // The turning placements follow the standing rows only; no running pose selects past the named directions.
            if (running && direction >= ChargeFlarePlacementDefinitions.DirectionCount)
                continue;
            bool facingLeft = bus.ReadCartridgeByte(record) == 4;
            byte yOffset = bus.ReadCartridgeByte(record + 4);
            var native = new OamBuffer();
            int xTable = running ? SamusSourceDefinitions.ChargeFlareRunningXOffsets.Value : SamusSourceDefinitions.ChargeFlareDefaultXOffsets.Value;
            int yTable = running ? SamusSourceDefinitions.ChargeFlareRunningYOffsets.Value : SamusSourceDefinitions.ChargeFlareDefaultYOffsets.Value;
            short Word(int address) => unchecked((short)CartridgeImportSource.Require(bus).ReadCartridgeWord(address));
            var world = new Position16<RoomPixels>(coordinate, coordinate);
            var point = transform is { } selectedTransform ? selectedTransform.Transform(world) : world;
            ushort x = unchecked((ushort)(point.X + Word(xTable + direction * 2)));
            ushort y = unchecked((ushort)(point.Y + Word(yTable + direction * 2) - yOffset));
            ushort selector = (ushort)(component == 0 ? 0 : facingLeft ? (component == 1 ? 42 : 48) : (component == 1 ? 30 : 36));
            // The flare is published wherever it lies and the display clips it (#1541): one the cartridge culls by its Y
            // high byte publishes the parts the cartridge draws for it on the screen.
            bool onScreen = LittleEndianBytes.HighByteInPlace(y) == 0;
            DrawImportedFlareSpritemap(bus, native, selector, new(x, onScreen ? y : (byte)y));
            OamBuffer actual = SpriteFrameBuffer.DrawnOam(frame => drawMetadata(frame, samus, Position16<RoomPixels>.Zero, component, transform,
                new ChargeFlareArtwork { Placement = stock, Objects = objects },
                new ChargeFlarePose { Direction = direction, Running = running, PoseYOffset = yOffset, FacingLeft = facingLeft }));
            // The draw depth replaces each part's priority tier (#1514); everything else must match the cartridge.
            AssertEqual(native.NextByteOffset / 4, actual.LastFinalizedSpriteCount, "Flare placement publishes every native part");
            if (!onScreen)
            {
                cases++;
                continue;
            }
            AssertTrue(SuperMetroid.Verification.Harness.OamPriorityComparison.ImagesEqualApartFromPriority(native, actual), $"Flare placement stock preserves native OAM for running, facing, overread directions and Mode7: {pose} direction {direction} at {coordinate}, floor {transform is not null}, component {component} ({native.FirstDifference(actual)})");
            cases++;
        }
        // A charged turning pose draws its flare from the turn row its restriction's low four bits select, where the
        // cartridge draws it; the pose-reading draw once returned before reaching those rows (#1504).
        foreach (SamusPoseId pose in new[]
        {
            SamusPoseId.TurningRightToLeftAimUpPose, SamusPoseId.TurningLeftToRightPose, SamusPoseId.TurningRightToLeftAimDiagonalDownPose,
        })
        for (int component = 0; component < 3; component++)
        {
            var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)) };
            PrepareRetailSamusFixture(samus);
            int record = SamusSourceDefinitions.PoseDefinitions.Value + SamusPoseNumbers.Number(pose) * 8;
            int turnRow = bus.ReadCartridgeByte(record + 3) & 15;
            AssertTrue(turnRow >= ChargeFlarePlacementDefinitions.DirectionCount, $"{pose} selects a turn row");
            bool facingLeft = bus.ReadCartridgeByte(record) == 4;
            byte yOffset = bus.ReadCartridgeByte(record + 4);
            short Word(int address) => unchecked((short)CartridgeImportSource.Require(bus).ReadCartridgeWord(address));
            ushort x = unchecked((ushort)(100 + Word(SamusSourceDefinitions.ChargeFlareDefaultXOffsets.Value + turnRow * 2)));
            ushort y = unchecked((ushort)(100 + Word(SamusSourceDefinitions.ChargeFlareDefaultYOffsets.Value + turnRow * 2) - yOffset));
            ushort selector = (ushort)(component == 0 ? 0 : facingLeft ? (component == 1 ? 42 : 48) : (component == 1 ? 30 : 36));
            var native = new OamBuffer();
            DrawImportedFlareSpritemap(bus, native, selector, new(x, y));
            OamBuffer actual = SpriteFrameBuffer.DrawnOam(frame => draw(frame, samus, Position16<RoomPixels>.Zero, component, null,
                new ChargeFlareArtwork { Placement = stock, Objects = objects }));
            AssertTrue(actual.LastFinalizedSpriteCount > 0, $"charged {pose} draws flare component {component}");
            AssertTrue(SuperMetroid.Verification.Harness.OamPriorityComparison.ImagesEqualApartFromPriority(native, actual), $"charged {pose} draws flare component {component} at its turn row's cartridge placement apart from the priority tier the draw depth replaces (#1514) ({native.FirstDifference(actual)})");
        }
        // The flare draw places by the low four bits of the pose's shot direction, except for no-firing poses. Every pose
        // selects a named direction or, turning, a turning placement, so a selection past the turning placements standing,
        // or past the named directions running, is rejected rather than read from the rows that follow the tables (#1500).
        for (int direction = ChargeFlarePlacementDefinitions.DirectionCount; direction < 16; direction++)
        {
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(true, direction), $"running flare selection {direction} past the named directions is rejected");
            if (direction >= ChargeFlarePlacementDefinitions.StandingDirectionCount)
                AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(false, direction), $"standing flare selection {direction} past the turning placements is rejected");
        }
        AssertTrue(Enumerable.Range(0, byte.MaxValue + 1).Where(SamusPoseNumbering.NamesPose)
            .Select(number => SamusPoseMetadata.ReadShotDirection(SamusPoseNumbering.FromNumber(number)))
            .Where(shot => shot != byte.MaxValue)
            .All(shot => (shot & 15) < ChargeFlarePlacementDefinitions.StandingDirectionCount),
            "no pose's shot direction selects past the turning placements");
        var subject = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)) };
        PrepareRetailSamusFixture(subject);
        byte[] beforeSamus = GraphDigest(subject, InstalledFixtures.BoundContent), beforeSystem = GraphDigest(system, InstalledFixtures.BoundContent);
        OamBuffer original = SpriteFrameBuffer.DrawnOam(frame => draw(frame, subject, Position16<RoomPixels>.Zero, 0, null,
            new ChargeFlareArtwork { Placement = stock, Objects = objects }));
        AssertTrue(original.LastFinalizedSpriteCount > 0, "Flare placement fixture draws visible sprites");
        AssertTrue(beforeSamus.SequenceEqual(GraphDigest(subject, InstalledFixtures.BoundContent)) && beforeSystem.SequenceEqual(GraphDigest(system, InstalledFixtures.BoundContent)), "Flare drawing changes neither Samus physics nor any projectile/timer state");
        foreach (bool hyper in new[] { false, true })
        {
            var hyperSystem = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads);
            hyperSystem.Charge.FlareCounter = 30;
            Array.Fill(hyperSystem.Charge.FlareFrames, 2);
            subject.HyperBeamEnabled = hyper;
            original = SpriteFrameBuffer.DrawnOam(frame => hyperSystem.DrawChargeFlare(frame, subject, new(0, 0),
                hyperSystem.AdvanceChargeFlare(subject), null,
                new ChargeFlareArtwork { Placement = stock, Objects = objects }));
            AssertTrue(original.LastFinalizedSpriteCount > 0, "Public normal/Hyper flare producer emits visible components");
        }
        var document = RequiredJson.Parse(json);
        document.Required("offsets").AsObject().Remove(ChargeFlarePlacementFile.Key(false, 0));
        AssertThrows<InvalidDataException>(() => Load(document), "Missing flare placement rejected");
        document = JsonNode.Parse(json);
        (document ?? throw new InvalidOperationException("Verification requires document.")).Required("offsets").Required(ChargeFlarePlacementFile.Key(false, 0))["x"] = 32768;
        AssertThrows<InvalidDataException>(() => Load(document), "Out-of-range flare position rejected");
        document = JsonNode.Parse(json); (document ?? throw new InvalidOperationException("Verification requires document."))["damage"] = 1;
        AssertThrows<InvalidDataException>(() => Load(document), "Flare placement cannot contain mechanics fields");
        AssertThrows<InvalidDataException>(() => ChargeFlarePlacementFile.Load(new MemoryStream(Encoding.UTF8.GetBytes("{\"version\":1,\"version\":1}"))), "Duplicate flare properties rejected");
        Console.WriteLine($"Flare placement: {cases} native OAM cases, whole-state isolation and invalid-resource rejection pass.");
        static ChargeFlarePlacements Load(JsonNode node) => ChargeFlarePlacementFile.Load(new MemoryStream(Encoding.UTF8.GetBytes(node.ToJsonString())));
    }
    /// <summary>Invokes the renderer's private flare component path with explicit compiled pose metadata.</summary>
    internal delegate void FlareMetadataDraw(SpriteFrameBuffer sprites, SamusState samus,
        Position16<RoomPixels> cameraPosition, int component, TurningFloorTransform? transform,
        ChargeFlareArtwork artwork, ChargeFlarePose pose);

    /// <summary>Confirms stock flare offsets flow through actor drawing and artwork rebinding.</summary>
    private static void VerifyRuntimeFlarePlacement(ChargeFlarePlacements stock)
    {
        var random = new SharedRandom();
        var runtime = CreateRetailRuntimeFixture(random: random);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.Player.IntroducedSamus.Pose = SamusPoseId.FacingRightNormalPose;
        runtime.Player.IntroducedSamus.Position = runtime.Player.IntroducedSamus.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
        SamusPresentation.InitializeAnimation(runtime.Player.IntroducedSamus); runtime.Player.IntroducedSamus.Presentation.PrimeGraphics(runtime.Player.IntroducedSamus);
        runtime.Projectiles.Charge.FlareCounter = 30;
        Array.Fill(runtime.Projectiles.Charge.FlareTimers, WrappingCountdown.Start(100));
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), random: random);
        game.AdoptRuntime(runtime);
        // The fixture game has no host artwork of its own; adopting the runtime cleared its projectile sheets.
        InstalledFixtures.BindProjectileArtwork(runtime);
        byte[] Draw(SuperMetroid.Core.Runtime.SuperMetroidRuntime target)
        {
            target.Sprites.BeginFrame();
            target.ActorDrawing.DrawGameplayActors(target.Room.LoadedScope, SamusDrawOwner.Gameplay, SamusPaletteEffects.Hold);
            target.Sprites.FinalizeFrame();
            return target.Sprites.FinalizedAsOam().LowTable.ToArray();
        }
        byte[] native = Draw(runtime);
        game.BindArtwork(game.Artwork with
        {
            ChargeFlarePlacement = stock,
        });
        AssertTrue(native.SequenceEqual(Draw(runtime)), "Runtime stock flare placement preserves complete actor OAM");
    }

    /// <summary>Runs the charge-flare placement comparison against the local retail cartridge image.</summary>
    [VerificationCommand("--charge-flare-placement-fixture")]
    internal static int ChargeFlarePlacementFixtureCommand()
    {
        Suite(nameof(VerifyChargeFlarePlacement), () => VerifyChargeFlarePlacement(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
