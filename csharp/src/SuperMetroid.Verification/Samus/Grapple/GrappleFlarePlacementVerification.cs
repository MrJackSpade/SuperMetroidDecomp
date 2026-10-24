using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleFiringDefinitionsVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
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

/// <summary>Checks editable Grapple flare origins against cartridge offsets and unchanged grapple mechanics.</summary>
internal static class GrappleFlarePlacementVerification
{
    /// <summary>Exercises authored launch poses and live artwork rebinding while comparing stock and offset-edited Samus state.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Grapple flare placement offsets")]
    internal static void VerifyGrappleFlarePlacement(SuperMetroidAddressSpace rom)
    {
        byte[] json = GrappleFlarePlacementExtractor.Extract(rom);
        var stock = ChargeFlarePlacementCatalog.Load(new MemoryStream(json));
        var document = RequiredJson.Parse(json);
        foreach (bool running in new[] { false, true })
        for (int direction = 0; direction < 16; direction++)
        {
            var offset = stock.Resolve(running, direction);
            int x = running ? SamusGrappleRomData.Firing.RunningFlareX : SamusGrappleRomData.Firing.DefaultFlareX;
            int y = running ? SamusGrappleRomData.Firing.RunningFlareY : SamusGrappleRomData.Firing.DefaultFlareY;
            AssertEqual(unchecked((short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), x + direction * 2)), offset.X, "Extracted Grapple flare X including adjacent rows");
            AssertEqual(unchecked((short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), y + direction * 2)), offset.Y, "Extracted Grapple flare Y including adjacent rows");
            var editedOffset = document.Required("offsets").Required(ChargeFlarePlacementDefinitions.Key(running, direction));
            editedOffset["x"] = unchecked((short)(offset.X + 7));
            editedOffset["y"] = unchecked((short)(offset.Y - 9));
        }
        var edited = ChargeFlarePlacementCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(document.ToJsonString())));
        var refresh = new Action<SamusState, SamusGrappleState>(SamusGrappleBeamDrawing.RefreshFiringDrawOrigins);
        Func<SamusState, SamusGrappleState, Position16<RoomPixels>, GrappleAnchorKind, GrappleMovementResult> connect = SamusGrappleMovement.ConnectAcceptedFiring;
        var empty = CreateRoom(new(64, 64), new ushort[4096], new byte[4096]);
        int locked = 0;
        var authoredPoses = Enumerable.Range(0, 253)
            .Select(pose => new
            {
                Pose = (SamusPoseId)pose,
                Movement = CpuBusRegionRead.Read(rom, SamusCartridgeTables.PoseDefinitions + pose * 8 + 1),
                Direction = CpuBusRegionRead.Read(rom, SamusCartridgeTables.PoseDefinitions + pose * 8 + 3),
            })
            .Where(row => row.Direction < 10)
            .GroupBy(row => new AuthoredPose(Running:row.Movement == (byte)SamusMovementType.Running, row.Direction))
            .Select(group => group.First())
            .OrderBy(row => row.Movement == (byte)SamusMovementType.Running)
            .ThenBy(row => row.Direction)
            .ToArray();
        foreach (var authored in authoredPoses)
        {
            SamusPoseId pose = authored.Pose;
            byte direction = authored.Direction;
            var nativeBus = NewGrappleFiringReadGuard(rom, () => pose);
            var guarded = NewGrappleFlareReadGuard(nativeBus);
            var native = Seed(stock);
            var selected = Seed(InstalledFixtures.Projectiles.GrappleTiles.FlarePlacement);
            var changed = Seed(edited);
            SamusGrappleMovement.BeginFiring(native);
            SamusGrappleMovement.BeginFiring(selected);
            SamusGrappleMovement.BeginFiring(changed);
            Compare();
            refresh(native, native.Grapple); refresh(selected, selected.Grapple); refresh(changed, changed.Grapple);
            Compare();
            for (int tick = 0; tick < 10; tick++)
            {
                SamusGrappleMovement.StepFiring(empty, native, SnesButton.X);
                SamusGrappleMovement.StepFiring(empty, selected, SnesButton.X);
                SamusGrappleMovement.StepFiring(empty, changed, SnesButton.X);
                Compare();
            }
            var expectedConnection = connect(native, native.Grapple, new(512, 512), GrappleAnchorKind.Block);
            var selectedConnection = connect(selected, selected.Grapple, new(512, 512), GrappleAnchorKind.Block);
            var editedConnection = connect(changed, changed.Grapple, new(512, 512), GrappleAnchorKind.Block);
            AssertEqual(expectedConnection, selectedConnection, "Stock placement retains connection result");
            AssertEqual(expectedConnection, editedConnection, "Edited placement retains connection result");
            // A non-swinging connection installs the locked-in-place grapple function.
            bool lockedInPlace = native.Grapple.Phase == GrapplePhase.ConnectedLocked;
            Compare(lockedInPlace);
            if (lockedInPlace) locked++;

            SamusState Seed(ChargeFlarePlacementCatalog? placement)
            {
                var samus = new SamusState { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(512, 512)) };
                PrepareRetailSamusFixture(samus);
                samus.Grapple.FlarePlacement = placement;
                return samus;
            }
            void Compare(bool checkVisibleOffset = true)
            {
                AssertTrue(GraphDigest(native).SequenceEqual(GraphDigest(selected)), "Stock extracted flare preserves complete Samus graph");
                var a = native.Grapple; var b = changed.Grapple;
                if (checkVisibleOffset)
                {
                    AssertEqual(unchecked((ushort)(a.BeamStart.X + 7)), b.BeamStart.X, "Edited Grapple flare X is visible");
                    AssertEqual(unchecked((ushort)(a.BeamStart.Y - 9)), b.BeamStart.Y, "Edited Grapple flare Y is visible");
                }
                var saved = new Saved(b.FlareOffset, b.BeamStart);
                b.FlareOffset = a.FlareOffset;
                b.BeamStart = a.BeamStart;
                AssertTrue(GraphDigest(native).SequenceEqual(GraphDigest(changed)), "Edited flare changes only four presentation words, not full-body/anchor/trajectory state");
                (b.FlareOffset, b.BeamStart) = saved;
            }
        }
        AssertTrue(locked > 0, "Placement fixture exercises locked connections");
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug); runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        byte[] beforeBinding = GraphDigest(runtime.Player.IntroducedSamus);
        var swingFrames = GrappleSwingFrameCatalog.Load(new MemoryStream(GrappleSwingFrameExtractor.Extract(rom)));
        runtime.Artwork.GrappleArtwork = GrappleTileAtlas.Load(new MemoryStream(GrappleTileExtractor.Extract(rom)), flarePlacement: edited, swingFrames: swingFrames);
        AssertTrue(ReferenceEquals(edited, runtime.Player.IntroducedSamus.Grapple.FlarePlacement), "Artwork rebinding reaches current Samus immediately");
        AssertTrue(ReferenceEquals(swingFrames, runtime.Player.IntroducedSamus.Grapple.SwingFrames), "Artwork rebinding reaches current swing-frame selection");
        AssertTrue(beforeBinding.SequenceEqual(GraphDigest(runtime.Player.IntroducedSamus)), "Presentation binding is excluded from Samus saved graph");
        runtime.Player.IntroducedSamus.Grapple.FlarePlacement = null;
        runtime.Player.IntroducedSamus.Grapple.SwingFrames = null;
        runtime.StepFrame(0);
        AssertTrue(ReferenceEquals(edited, runtime.Player.IntroducedSamus.Grapple.FlarePlacement), "Real gameplay frame rebinds missing/restored visual origins before producers");
        AssertTrue(ReferenceEquals(swingFrames, runtime.Player.IntroducedSamus.Grapple.SwingFrames), "Real gameplay frame rebinds swing art before pendulum update");
        runtime.Player.IntroducedSamus.Grapple.FlarePlacement = null;
        runtime.Player.IntroducedSamus.Grapple.SwingFrames = null;
        runtime.ActorDrawing.DrawGameplayActors(runtime.Room.LoadedScope, SamusDrawOwner.Gameplay, SamusPaletteEffects.Hold);
        AssertTrue(ReferenceEquals(edited, runtime.Player.IntroducedSamus.Grapple.FlarePlacement), "Independent actor drawing also rebinds visual origins");
        AssertTrue(ReferenceEquals(swingFrames, runtime.Player.IntroducedSamus.Grapple.SwingFrames), "Independent actor drawing also rebinds swing-frame selection");
        Console.WriteLine($"Grapple flare placement: 32 extracted pairs, {authoredPoses.Length} authored launch/late paths, {authoredPoses.Length * 10} trajectory frames and {locked} locked connections preserve physical state under visual edits and ROM guard.");
    }
    /// <summary>Rejects live reads of the two cartridge flare-offset tables after artwork installation.</summary>
    private static CartridgeReadGuard NewGrappleFlareReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x9bc14a and < 0x9bc172 or >= 0x9bc19a and < 0x9bc1c2,
                _ => "Grapple still reads visual flare origins from ROM.");

    /// <summary>Loads the retail cartridge and runs the Grapple flare placement verification.</summary>
    [VerificationCommand("--grapple-flare-placement")]
    internal static int GrappleFlarePlacementCommand()
    {
        Suite(nameof(VerifyGrappleFlarePlacement), () => VerifyGrappleFlarePlacement(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
