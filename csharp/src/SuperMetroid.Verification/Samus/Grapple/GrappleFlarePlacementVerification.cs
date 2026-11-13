using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core;
using SuperMetroid.Core.Runtime;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Grapple;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Gameplay;

/// <summary>Checks installed Grapple flare origins against cartridge offsets and unchanged grapple mechanics.</summary>
internal static class GrappleFlarePlacementVerification
{
    /// <summary>Exercises authored launch poses and live artwork rebinding while comparing extracted and installed Samus state.</summary>
    internal static void VerifyGrappleFlarePlacement(CartridgeImportAddressSpace rom)
    {
        byte[] json = GrappleFlarePlacementExtractor.Extract(rom);
        var stock = ChargeFlarePlacementFile.LoadGrapple(new MemoryStream(json));
        foreach (bool running in new[] { false, true })
        foreach (SamusProjectileDirection direction in SamusProjectileDirectionCodec.InAimOrder)
        {
            var offset = stock.Resolve(running, SamusFlareAim.AtMuzzle(direction));
            int x = running ? SamusSourceDefinitions.GrappleRunningFlareX.Value : SamusSourceDefinitions.GrappleDefaultFlareX.Value;
            int y = running ? SamusSourceDefinitions.GrappleRunningFlareY.Value : SamusSourceDefinitions.GrappleDefaultFlareY.Value;
            AssertEqual(unchecked((short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), x + direction.ToAimIndex() * 2)), offset.X, "Extracted Grapple flare X including adjacent rows");
            AssertEqual(unchecked((short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), y + direction.ToAimIndex() * 2)), offset.Y, "Extracted Grapple flare Y including adjacent rows");
        }
        var refresh = new Action<SamusState, SamusGrappleState>(SamusGrappleBeamDrawing.RefreshFiringDrawOrigins);
        Func<SamusState, SamusGrappleState, Position16<RoomPixels>, GrappleAnchorKind, GrappleMovementResult> connect = SamusGrappleMovement.ConnectAcceptedFiring;
        var empty = CreateRoom(new(64, 64), new RoomTerrainBlock[4096], new RoomBlockBehavior[4096]);
        int locked = 0;
        var authoredPoses = Enumerable.Range(0, 253).Where(SamusPoseNumbering.NamesPose)
            .Select(pose => new
            {
                Pose = SamusPoseNumbering.FromNumber(pose),
                Movement = CpuBusRegionRead.Read(rom, SamusSourceDefinitions.PoseDefinitions.Value + pose * 8 + 1),
                Direction = CpuBusRegionRead.Read(rom, SamusSourceDefinitions.PoseDefinitions.Value + pose * 8 + 3),
            })
            .Where(row => row.Direction < 10)
            .GroupBy(row => new AuthoredPose(Running:row.Movement == SamusMovementTypeNativeWords.Byte(SamusMovementType.Running), row.Direction))
            .Select(group => group.First())
            .OrderBy(row => row.Movement == SamusMovementTypeNativeWords.Byte(SamusMovementType.Running))
            .ThenBy(row => row.Direction)
            .ToArray();
        foreach (var authored in authoredPoses)
        {
            SamusPoseId pose = authored.Pose;
            var native = Seed(stock);
            var selected = Seed(InstalledFixtures.Projectiles.Grapple.FlarePlacement);
            SamusGrappleMovement.BeginFiring(native);
            SamusGrappleMovement.BeginFiring(selected);
            Compare();
            refresh(native, native.Grapple); refresh(selected, selected.Grapple);
            Compare();
            for (int tick = 0; tick < 10; tick++)
            {
                SamusGrappleMovement.StepFiring(empty, native, ControllerButton.X);
                SamusGrappleMovement.StepFiring(empty, selected, ControllerButton.X);
                Compare();
            }
            var expectedConnection = connect(native, native.Grapple, new(512, 512), GrappleAnchorKind.Block);
            var selectedConnection = connect(selected, selected.Grapple, new(512, 512), GrappleAnchorKind.Block);
            AssertEqual(expectedConnection, selectedConnection, "Stock placement retains connection result");
            Compare();
            // A non-swinging connection installs the locked-in-place grapple function.
            if (native.Grapple.Phase == GrapplePhase.ConnectedLocked) locked++;

            SamusState Seed(ChargeFlarePlacements? placement)
            {
                var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(512, 512)) };
                PrepareRetailSamusFixture(samus);
                samus.Grapple.FlarePlacement = placement;
                return samus;
            }
            // The two states differ only in the placement each is bound to: the extracted one and the installed one. Both
            // graphs name their placement as one bound object, so the comparison holds native to the installed placement
            // while it is captured and both name it by the one installed path, through the fixture's shared index of the
            // installed content; a content list of their own would walk all installed content again (#1532).
            void Compare()
            {
                ChargeFlarePlacements installed = InstalledFixtures.Projectiles.Grapple.FlarePlacement
                    ?? throw new InvalidOperationException("The installation places the grapple flare.");
                AssertTrue(ReferenceEquals(stock, native.Grapple.FlarePlacement) && ReferenceEquals(installed, selected.Grapple.FlarePlacement),
                    "Each Samus keeps the flare placement it was bound to");
                native.Grapple.FlarePlacement = installed;
                try
                {
                    AssertTrue(SameGraph(native, selected, InstalledFixtures.BoundContent), "Stock extracted flare preserves complete Samus graph");
                }
                finally
                {
                    native.Grapple.FlarePlacement = stock;
                }
            }
        }
        AssertTrue(locked > 0, "Placement fixture exercises locked connections");
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug); runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        SamusGrappleState boundGrapple = runtime.Player.IntroducedSamus.Grapple;
        var previousPresentation = (boundGrapple.FlarePlacement, boundGrapple.SwingFrames);
        byte[] beforeBinding = GraphDigest(runtime.Player.IntroducedSamus, InstalledFixtures.BoundContent);
        var swingFrames = GrappleSwingFrameFile.Load(new MemoryStream(GrappleSwingFrameExtractor.Extract(rom)));
        runtime.Artwork.GrappleFlarePlacement = stock;
        runtime.Artwork.GrappleSwingFrames = swingFrames;
        AssertTrue(ReferenceEquals(stock, runtime.Player.IntroducedSamus.Grapple.FlarePlacement), "Artwork rebinding reaches current Samus immediately");
        AssertTrue(ReferenceEquals(swingFrames, runtime.Player.IntroducedSamus.Grapple.SwingFrames), "Artwork rebinding reaches current swing-frame selection");
        (boundGrapple.FlarePlacement, boundGrapple.SwingFrames) = previousPresentation;
        AssertTrue(beforeBinding.SequenceEqual(GraphDigest(runtime.Player.IntroducedSamus, InstalledFixtures.BoundContent)),
            "Presentation binding changes only the two catalogs Samus is bound to");
        (boundGrapple.FlarePlacement, boundGrapple.SwingFrames) = (stock, swingFrames);
        runtime.Player.IntroducedSamus.Grapple.FlarePlacement = null;
        runtime.Player.IntroducedSamus.Grapple.SwingFrames = null;
        runtime.StepFrame(0);
        AssertTrue(ReferenceEquals(stock, runtime.Player.IntroducedSamus.Grapple.FlarePlacement), "Real gameplay frame rebinds missing/restored visual origins before producers");
        AssertTrue(ReferenceEquals(swingFrames, runtime.Player.IntroducedSamus.Grapple.SwingFrames), "Real gameplay frame rebinds swing art before pendulum update");
        runtime.Player.IntroducedSamus.Grapple.FlarePlacement = null;
        runtime.Player.IntroducedSamus.Grapple.SwingFrames = null;
        runtime.ActorDrawing.DrawGameplayActors(runtime.Room.LoadedScope, SamusDrawOwner.Gameplay, SamusPaletteEffects.Hold);
        AssertTrue(ReferenceEquals(stock, runtime.Player.IntroducedSamus.Grapple.FlarePlacement), "Independent actor drawing also rebinds visual origins");
        AssertTrue(ReferenceEquals(swingFrames, runtime.Player.IntroducedSamus.Grapple.SwingFrames), "Independent actor drawing also rebinds swing-frame selection");
        Console.WriteLine($"Grapple flare placement: 32 extracted pairs, {authoredPoses.Length} authored launch/late paths, {authoredPoses.Length * 10} trajectory frames and {locked} locked connections match the installed placement.");
    }
    /// <summary>Loads the retail cartridge and runs the Grapple flare placement verification.</summary>
    [VerificationCommand("--grapple-flare-placement")]
    internal static int GrappleFlarePlacementCommand()
    {
        Suite(nameof(VerifyGrappleFlarePlacement), () => VerifyGrappleFlarePlacement(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
