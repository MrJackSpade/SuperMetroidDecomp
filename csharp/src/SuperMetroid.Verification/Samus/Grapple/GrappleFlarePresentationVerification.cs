using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Frontend.Artwork;
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
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Poses;
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
        InstalledScreenContent maps, InstalledGrapplePresentation grappleArtwork)
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
            _ = Check(native, actual, wholeState: cases == 0, new NativeFlare(29, 0));
            SamusState Seed(SamusState samus)
            {
                samus.ChangePoseKeepingAnimation(pose);
                samus.Position = FixedPosition<RoomPixels>.FromWhole(new(coordinate, coordinate));
                samus.Grapple.Phase = phase;
                samus.Grapple.FireDirection = pose is SamusPoseId.FacingLeftNormalPose or SamusPoseId.MovingLeftNormalPose
                    ? SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.Left
                    : SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.Right;
                samus.Grapple.FlareStage = stage;
                samus.Grapple.PlayFlareTo(29, 0);
                samus.Grapple.FlareOrigin = samus.Grapple.FlareOrigin.WithX(coordinate); samus.Grapple.FlareOrigin = samus.Grapple.FlareOrigin.WithY(coordinate);
                samus.Grapple.RopeStart = samus.Grapple.RopeStart.WithX(70); samus.Grapple.RopeStart = samus.Grapple.RopeStart.WithY(80);
                samus.Grapple.Anchor = samus.Grapple.Anchor.WithX(200); samus.Grapple.Anchor = samus.Grapple.Anchor.WithY(100);
                samus.Grapple.RopeLength = 40; samus.Grapple.AngularVelocity = AngularSpeedConstants.FromFractionsOfStep(123);
                return samus;
            }
        }
        foreach (SamusPoseId pose in new[] { SamusPoseId.MovingRightNormalPose, SamusPoseId.MovingLeftNormalPose })
        {
            var native = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(0, 100)) }.InPose(pose);
            native.Grapple.Phase = GrapplePhase.Firing;
            native.Grapple.FireDirection = pose == SamusPoseId.MovingLeftNormalPose
                ? SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.Left
                : SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.Right;
            native.Grapple.FlareStage = GrappleFlareStage.JustFired;
            var actual = Clone(native);
            // A new Samus's grapple flare stands cleared: its first picture with its wait spent.
            var flare = new NativeFlare(0, 0);
            for (int tick = 0; tick < 512; tick++)
            {
                ushort tickX = (ushort)(tick % 256);
                actual.Position = actual.Position.WithWholeX(tickX);
                native.Position = native.Position.WithWholeX(tickX);
                flare = Check(native, actual, wholeState: tick == 0, flare);
                // The post-Samus tile pass moves the flare on a stage, not the flare draw.
                native.Grapple.FlareStage = actual.Grapple.FlareStage = GrappleFlareStage.Shown;
            }
        }
        // Exercise the waits the flare leaves across the complete authored main-flare frame domain while vertically
        // culled, without inventing valid sprite IDs. The flare never leaves a wait run past zero (#2396).
        foreach (ushort timer in new ushort[] { 0, 1, 2 })
        foreach (ushort frame in new ushort[] { 0, 15, 16, 29 })
        {
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()).InPose(SamusPoseId.FacingRightNormalPose);
            samus.Grapple.Phase = GrapplePhase.ConnectedLocked;
            samus.Grapple.FlareStage = GrappleFlareStage.Shown;
            samus.Grapple.PlayFlareTo(frame, timer);
            samus.Grapple.FlareOrigin = samus.Grapple.FlareOrigin.WithY(256);
            _ = Check(samus, Clone(samus), wholeState: timer == 0 && frame == 0, new NativeFlare(frame, timer));
        }
        Suite(nameof(VerifyGrappleFlareActorBinding), () => VerifyGrappleFlareActorBinding(roomAssets, maps, grappleArtwork));
        Console.WriteLine($"Grapple flare: {cases} phase, position, sentinel, rewind and timer-boundary cases preserve native cadence/OAM and complete Samus state; actual actor rebind passes.");

        // The flare draw reads Samus and writes only her grapple's flare frame and timer and, while firing, its rope and
        // beam starts, so each case compares those and the first case of each group compares her whole state (#1567).
        // The original's picture and wait before the draw are the case's own; the native reads give them after it.
        NativeFlare Check(SamusState native, SamusState actual, bool wholeState, NativeFlare before)
        {
            // Binding the body fixture hashes the body's characters, so each Samus state is bound once, on its first case
            // (#1680); every clone is still taken before its first case binds it.
            foreach (SamusState samus in new[] { native, actual })
            {
                if (bound.Add(samus))
                    samus.BindBodyFixture(body);
                samus.Artwork.GrappleFlarePlacement = flarePlacement;
            }
            int expectedFrame = before.Frame;
            ushort expectedTimer = before.Timer;
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

            AssertEqual(ChargeFlareAnimationDefinitions.CompositionNumbered(ChargeFlareComponent.MainFlare, HorizontalDirection.Right, expectedFrame),
                actual.Grapple.Flare.Composition(HorizontalDirection.Right), "Grapple shared compiled cadence shows the picture the independent native reads select");
            AssertTrue(a.ImageEquals(b) && (a.LastFinalizedSpriteCount * 4) == (b.LastFinalizedSpriteCount * 4), "Grapple stock parts preserve native complete OAM");
            AssertEqual(FlareState.Of(native.Grapple), FlareState.Of(actual.Grapple), "Flare draws write the same flare timing and physical origins");
            if (wholeState)
                AssertTrue(SameGraph(native, actual, content), "Flare draws preserve complete Samus state");
            cases++;
            return new NativeFlare(expectedFrame, expectedTimer);
        }
        SamusState Clone(SamusState source) => RoundTrip(source, content);

        static SamusState NewSamus() => new(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
            SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), artwork: global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
    }

    /// <summary>The grapple state a flare draw writes: the picture its flare shows among them.</summary>
    private readonly record struct FlareState(GrappleFlareStage Stage, SuperMetroid.Core.Samus.Artwork.ProjectileSpriteId Picture, Position16<RoomPixels> RopeStart, Position16<RoomPixels> FlareOrigin)
    {
        /// <summary>The flare state <paramref name="grapple"/> holds.</summary>
        internal static FlareState Of(SamusGrappleState grapple) => new(grapple.FlareStage, grapple.Flare.Composition(HorizontalDirection.Right),
            grapple.RopeStart, grapple.FlareOrigin);
    }

    /// <summary>The grapple flare's picture and wait as the original counts them.</summary>
    /// <param name="Frame">The number of the main flare's picture it shows.</param>
    /// <param name="Timer">The original's wait word.</param>
    private readonly record struct NativeFlare(int Frame, ushort Timer);

    /// <summary>Reads a native Grapple flare delay byte solely for the verification timing oracle.</summary>
    private static byte GrappleFlarePresentationRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks the real actor draws the flare from the bound sheets, and from newly bound ones.</summary>
    private static void VerifyGrappleFlareActorBinding(
        MapPresentationInstalledRoomAssets roomAssets, InstalledScreenContent maps, InstalledGrapplePresentation grappleArtwork)
    {
        var random = new SharedRandom();
        var game = new global::SuperMetroid.Core.Frontend.SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), null, SuperMetroidGameHostServices.Default, random);
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(GameplayAssists.None, InstalledFixtures.GameArtwork.Gameplay, random: random, audio: game.AudioForVerification);
        runtime.Artwork.ScreenContent = maps;
        roomAssets.Bind(runtime);
        runtime.Artwork.GrappleFlarePlacement = grappleArtwork.FlarePlacement;
        runtime.Artwork.GrappleSwingFrames = grappleArtwork.SwingFrames;
        runtime.Artwork.SpriteSheets = InstalledFixtures.SpriteSheets;
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame); runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        var samus = runtime.Player.IntroducedSamus;
        samus.ChangePoseKeepingAnimation(SamusPoseId.FacingRightNormalPose); samus.Position = samus.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
        SamusPresentation.InitializeAnimation(samus);
        samus.Grapple.Phase = GrapplePhase.ConnectedLocked;
        samus.Grapple.FlareOrigin = samus.Grapple.FlareOrigin.WithX(samus.Position.X.Whole); samus.Grapple.FlareOrigin = samus.Grapple.FlareOrigin.WithY(samus.Position.Y.Whole);
        // The loop's first picture with three updates of its wait left, which both of the flare's draws below show.
        samus.Grapple.FlareStage = GrappleFlareStage.Shown; samus.Grapple.PlayFlareTo(16, 3);
        game.AdoptRuntime(runtime);
        byte[] original = Draw(runtime);
        samus.Grapple.FlareStage = GrappleFlareStage.None;
        AssertTrue(!original.SequenceEqual(Draw(runtime)), "Real actor fixture reaches Grapple flare path");
        samus.Grapple.FlareStage = GrappleFlareStage.Shown;
        InstalledSpriteSheets rebound = InstalledFixtures.SpriteSheetsReloading(SpriteSheetId.ProjectilesPower);
        game.BindArtwork(InstalledFixtures.GameArtwork with { SpriteSheets = rebound });
        AssertTrue(original.SequenceEqual(Draw(runtime)), "Real Grapple actor draws the same flare from rebound sheets");
        runtime.Sprites.BeginFrame();
        runtime.ActorDrawing.DrawGameplayActors(runtime.Room.LoadedScope, runtime.Player.IntroducedSamus, SamusDrawOwner.Gameplay, SamusColourEffects.Hold, deathStep: null);
        runtime.Sprites.FinalizeFrame();
        AssertTrue(runtime.Sprites.Draws.Where(draw => draw.Source.Kind == SpriteSourceKind.SamusGrappleFlare)
                .All(draw => ReferenceEquals(rebound.Get(SpriteSheetId.ProjectilesPower).Image, ((SamusColouredDraw)draw).Image.Sheet)),
            "Real Grapple actor draws its flare from the rebound projectile sheet");
        static byte[] Draw(SuperMetroidRuntime target)
        {
            target.Sprites.BeginFrame();
            target.ActorDrawing.DrawGameplayActors(target.Room.LoadedScope, target.Player.IntroducedSamus, SamusDrawOwner.Gameplay, SamusColourEffects.Hold, deathStep: null);
            OamBuffer image = target.Sprites.FinalizeAsOam();
            return image.LowTable.ConcatToArray(image.HighTable).Concat(BitConverter.GetBytes(image.LastFinalizedSpriteCount)).ToArray();
        }
    }

    /// <summary>Reads one native facing-row selector offset to check both rows choose the shared main flare.</summary>
    private static ushort ReadNativeGrappleFlareSpritemapOffset(ISnesAddressSpace bus, int address) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
}
