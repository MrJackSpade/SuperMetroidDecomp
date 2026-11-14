using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>Checks Grapple flare timing, OAM, and restored actor binding against native presentation.</summary>
internal static class GrappleFlarePresentationVerification
{
    /// <summary>Compares flare drawing across phases and coordinates with native cadence, then checks the real actor's sheet binding.</summary>
    internal static void VerifyGrappleFlarePresentation(CartridgeImportAddressSpace bus,
        SamusBodySources body, MapPresentationInstalledRoomAssets roomAssets,
        AreaMapPresentation maps, InstalledGrapplePresentation grappleArtwork)
    {
        var flarePlacement = ChargeFlarePlacementFile.LoadGrapple(new MemoryStream(
            GrappleFlarePlacementExtractor.Extract(bus)));
        // The cases bind Samus to this body and flare placement. The body is the shared installed one the fixture content
        // names by path. The extracted placement is no installed content: both graphs of a comparison hold the same one, so
        // each writes the same bytes for it, and no round trip holds it (each clone is taken before Check binds it), so the
        // captures name only the fixture's installed content and never index a content list of their own (#1532).
        object[] content = InstalledFixtures.BoundContent;
        // Every case draws from the same installed projectile sheets, so the draw artwork is built once (#1680).
        SuperMetroid.Core.Samus.Projectiles.ProjectileDrawArtwork artwork = InstalledFixtures.ProjectileArtwork();
        foreach (int address in new[] { GrappleBeamDefinitions.Firing.RightFlareSpritemapOffsets, GrappleBeamDefinitions.Firing.LeftFlareSpritemapOffsets })
            AssertEqual((ushort)0, ReadNativeGrappleFlareSpritemapOffset(bus, address), "Both native Grapple-facing rows start at the selector table's main-flare pictures, which both facings share");
        int cases = 0;
        // The phase, pose, stage and coordinate cases each reseed every value the flare draw reads or writes on one pair of
        // Samus states built and bound once, so a case costs a draw, not two whole Samus states (#1680).
        SamusState seededNative = NewSamus(), seededActual = NewSamus();
        var bound = new HashSet<SamusState>(ReferenceEqualityComparer.Instance);
        foreach (GrapplePhase phase in Enum.GetValues<GrapplePhase>())
        foreach (SamusPoseId pose in new[] { SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose, SamusPoseId.MovingRightNormalPose, SamusPoseId.MovingLeftNormalPose })
        foreach (GrappleFlareStage stage in Enum.GetValues<GrappleFlareStage>())
        foreach (ushort coordinate in new ushort[] { 0, 1, 100, 255, 256, 511, 32768, 65535 })
        {
            SamusState native = Seed(seededNative), actual = Seed(seededActual);
            Check(native, actual, wholeState: cases == 0);
            SamusState Seed(SamusState samus)
            {
                samus.Pose = pose;
                samus.Position = FixedPosition<RoomPixels>.FromWhole(new(coordinate, coordinate));
                samus.Grapple.Phase = phase;
                samus.Grapple.FireDirection = pose is SamusPoseId.FacingLeftNormalPose or SamusPoseId.MovingLeftNormalPose
                    ? SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.Left
                    : SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.Right;
                samus.Grapple.FlareStage = stage;
                samus.Grapple.FlarePicture = new(29);
                samus.Grapple.FlareAnimationTimer = WrappingCountdown.Start(0);
                samus.Grapple.BeamStart = samus.Grapple.BeamStart.WithX(coordinate); samus.Grapple.BeamStart = samus.Grapple.BeamStart.WithY(coordinate);
                samus.Grapple.RopeStart = samus.Grapple.RopeStart.WithX(70); samus.Grapple.RopeStart = samus.Grapple.RopeStart.WithY(80);
                samus.Grapple.Anchor = samus.Grapple.Anchor.WithX(200); samus.Grapple.Anchor = samus.Grapple.Anchor.WithY(100);
                samus.Grapple.RopeLength = 40; samus.Grapple.AngularVelocity = 123;
                return samus;
            }
        }
        foreach (SamusPoseId pose in new[] { SamusPoseId.MovingRightNormalPose, SamusPoseId.MovingLeftNormalPose })
        {
            var native = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(0, 100)) };
            native.Grapple.Phase = GrapplePhase.Firing;
            native.Grapple.FireDirection = pose == SamusPoseId.MovingLeftNormalPose
                ? SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.Left
                : SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.Right;
            native.Grapple.FlareStage = GrappleFlareStage.JustFired;
            var actual = Clone(native);
            for (int tick = 0; tick < 512; tick++)
            {
                ushort tickX = (ushort)(tick % 256);
                actual.Position = actual.Position.WithWholeX(tickX);
                native.Position = native.Position.WithWholeX(tickX);
                Check(native, actual, wholeState: tick == 0);
                // The post-Samus tile pass moves the flare on a stage, not the flare draw.
                native.Grapple.FlareStage = actual.Grapple.FlareStage = GrappleFlareStage.Shown;
            }
        }
        // Exercise negative timers across the complete authored main-flare frame domain
        // while vertically culled, without inventing valid sprite IDs.
        foreach (ushort timer in new ushort[] { 0, 1, 2, 32768, 65535 })
        foreach (ushort frame in new ushort[] { 0, 15, 16, 29 })
        {
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Pose = SamusPoseId.FacingRightNormalPose };
            samus.Grapple.Phase = GrapplePhase.ConnectedLocked;
            samus.Grapple.FlareStage = GrappleFlareStage.Shown;
            samus.Grapple.FlarePicture = new(frame);
            samus.Grapple.FlareAnimationTimer = WrappingCountdown.Start(timer);
            samus.Grapple.BeamStart = samus.Grapple.BeamStart.WithY(256);
            Check(samus, Clone(samus), wholeState: timer == 0 && frame == 0);
        }
        Suite(nameof(VerifyGrappleFlareActorBinding), () => VerifyGrappleFlareActorBinding(roomAssets, maps, grappleArtwork));
        Console.WriteLine($"Grapple flare: {cases} phase, position, sentinel, rewind and timer-boundary cases preserve native cadence/OAM and complete Samus state; actual actor rebind passes.");

        // The flare draw reads Samus and writes only her grapple's flare frame and timer and, while firing, its rope and
        // beam starts, so each case compares those and the first case of each group compares her whole state (#1567).
        void Check(SamusState native, SamusState actual, bool wholeState)
        {
            // Binding the body fixture hashes the body's characters, so each Samus state is bound once, on its first case
            // (#1680); every clone is still taken before its first case binds it.
            foreach (SamusState samus in new[] { native, actual })
            {
                if (bound.Add(samus))
                    samus.Presentation.BindBodyFixture(body);
                samus.Grapple.FlarePlacement = flarePlacement;
            }
            int expectedFrame = native.Grapple.FlarePicture.Index;
            ushort expectedTimer = native.Grapple.FlareAnimationTimer.RawValue;
            if (SamusGrappleBeamDrawing.DrawingFor(native.Grapple.Phase) == SamusGrappleDrawing.Beam && native.Grapple.FlareStage != GrappleFlareStage.None)
            {
                if (native.Grapple.FlareStage == GrappleFlareStage.JustFired) { expectedFrame = 16; expectedTimer = 3; }
                CountdownStep expectedStep = WrappingCounter.Decrement(expectedTimer);
                expectedTimer = expectedStep.Value;
                if (expectedStep.IsNegative)
                {
                    expectedFrame = unchecked((ushort)(expectedFrame + 1));
                    byte delay = GrappleFlarePresentationRomByte(bus, GrappleBeamDefinitions.Firing.MainFlareAnimationDelays + expectedFrame);
                    if (delay == 254)
                    {
                        byte rewind = GrappleFlarePresentationRomByte(bus, GrappleBeamDefinitions.Firing.MainFlareAnimationDelays + unchecked((ushort)(expectedFrame + 1)));
                        expectedFrame = unchecked((ushort)(expectedFrame - rewind));
                        delay = GrappleFlarePresentationRomByte(bus, GrappleBeamDefinitions.Firing.MainFlareAnimationDelays + expectedFrame);
                    }
                    expectedTimer = delay;
                }
            }
            var aFrame = new SpriteFrameBuffer(); var bFrame = new SpriteFrameBuffer();
            bool visible = SamusGrappleBeamDrawing.DrawFlareBeforeSamus(native, aFrame, Position16<RoomPixels>.Zero, artwork);
            OamBuffer a = aFrame.FinalizeAsOam();
            AssertEqual(visible, SamusGrappleBeamDrawing.DrawFlareBeforeSamus(actual, bFrame, Position16<RoomPixels>.Zero, artwork), "Grapple sheet retains native flare visibility");
            OamBuffer b = bFrame.FinalizeAsOam();

            AssertEqual(new CheckCase(expectedFrame, expectedTimer), new CheckCase(actual.Grapple.FlarePicture.Index, actual.Grapple.FlareAnimationTimer.RawValue), "Grapple shared compiled cadence matches independent native reads");
            AssertTrue(a.ImageEquals(b) && (a.LastFinalizedSpriteCount * 4) == (b.LastFinalizedSpriteCount * 4), "Grapple stock parts preserve native complete OAM");
            AssertEqual(FlareState.Of(native.Grapple), FlareState.Of(actual.Grapple), "Flare draws write the same flare timing and physical origins");
            if (wholeState)
                AssertTrue(SameGraph(native, actual, content), "Flare draws preserve complete Samus state");
            cases++;
        }
        SamusState Clone(SamusState source) => RoundTrip(source, content);

        static SamusState NewSamus() => new(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
            SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
    }

    /// <summary>The grapple state a flare draw writes.</summary>
    private readonly record struct FlareState(GrappleFlareStage Stage, int Frame, ushort Timer, Position16<RoomPixels> RopeStart, Position16<RoomPixels> BeamStart)
    {
        /// <summary>The flare state <paramref name="grapple"/> holds.</summary>
        internal static FlareState Of(SamusGrappleState grapple) => new(grapple.FlareStage, grapple.FlarePicture.Index,
            grapple.FlareAnimationTimer.RawValue, grapple.RopeStart, grapple.BeamStart);
    }

    /// <summary>Reads a native Grapple flare delay byte solely for the verification timing oracle.</summary>
    private static byte GrappleFlarePresentationRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks the real actor draws the flare from the bound sheets, and from newly bound ones.</summary>
    private static void VerifyGrappleFlareActorBinding(
        MapPresentationInstalledRoomAssets roomAssets, AreaMapPresentation maps, InstalledGrapplePresentation grappleArtwork)
    {
        var random = new SharedRandom();
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(GameplayAssists.None, InstalledFixtures.BareRuntimeArtwork, random: random);
        runtime.Artwork.MapPresentation = maps;
        roomAssets.Bind(runtime);
        runtime.Artwork.GrappleFlarePlacement = grappleArtwork.FlarePlacement;
        runtime.Artwork.GrappleSwingFrames = grappleArtwork.SwingFrames;
        runtime.Artwork.SpriteSheets = InstalledFixtures.SpriteSheets;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug); runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        var samus = runtime.Player.IntroducedSamus;
        samus.Pose = SamusPoseId.FacingRightNormalPose; samus.Position = samus.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
        SamusPresentation.InitializeAnimation(samus);
        samus.Grapple.Phase = GrapplePhase.ConnectedLocked;
        samus.Grapple.BeamStart = samus.Grapple.BeamStart.WithX(samus.Position.X.Whole); samus.Grapple.BeamStart = samus.Grapple.BeamStart.WithY(samus.Position.Y.Whole);
        samus.Grapple.FlareStage = GrappleFlareStage.Shown; samus.Grapple.FlarePicture = new(16); samus.Grapple.FlareAnimationTimer = WrappingCountdown.Start(100);
        var game = new SuperMetroid.Core.Frontend.SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(), null, SuperMetroid.Core.Frontend.SuperMetroidGameHostServices.Default, random);
        game.AdoptRuntime(runtime);
        byte[] original = Draw(runtime);
        samus.Grapple.FlareStage = GrappleFlareStage.None;
        AssertTrue(!original.SequenceEqual(Draw(runtime)), "Real actor fixture reaches Grapple flare path");
        samus.Grapple.FlareStage = GrappleFlareStage.Shown;
        InstalledSpriteSheets rebound = InstalledFixtures.SpriteSheetsReloading(SpriteSheetId.ProjectilesPower);
        game.BindArtwork(InstalledFixtures.BareRuntimeArtwork with { SpriteSheets = rebound });
        AssertTrue(original.SequenceEqual(Draw(runtime)), "Real Grapple actor draws the same flare from rebound sheets");
        runtime.Sprites.BeginFrame();
        runtime.ActorDrawing.DrawGameplayActors(runtime.Room.LoadedScope, SamusDrawOwner.Gameplay, SamusPaletteEffects.Hold);
        runtime.Sprites.FinalizeFrame();
        AssertTrue(runtime.Sprites.Draws.Where(draw => draw.Source.Kind == SpriteSourceKind.SamusGrappleFlare)
                .All(draw => ReferenceEquals(rebound.Get(SpriteSheetId.ProjectilesPower).Image, ((ImageSpriteDraw)draw).Image.Sheet)),
            "Real Grapple actor draws its flare from the rebound projectile sheet");
        static byte[] Draw(SuperMetroidRuntime target)
        {
            target.Sprites.BeginFrame();
            target.ActorDrawing.DrawGameplayActors(target.Room.LoadedScope, SamusDrawOwner.Gameplay, SamusPaletteEffects.Hold);
            OamBuffer image = target.Sprites.FinalizeAsOam();
            return image.LowTable.ConcatToArray(image.HighTable).Concat(BitConverter.GetBytes(image.LastFinalizedSpriteCount)).ToArray();
        }
    }

    /// <summary>Reads one native facing-row selector offset to check both rows choose the shared main flare.</summary>
    private static ushort ReadNativeGrappleFlareSpritemapOffset(ISnesAddressSpace bus, int address) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
}
