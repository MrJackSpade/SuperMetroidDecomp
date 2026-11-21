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
        // The phase, pose, stage and coordinate cases each reseed every value the flare draw reads, but not the flare, on one
        // pair of Samus states built and bound once, so a case costs a draw, not two whole Samus states (#1680). The flare
        // plays on from case to case as each case's draw moves it, which the original's cadence follows (#2554).
        SamusState seededNative = NewSamus(), seededActual = NewSamus();
        var seededCadence = new NativeFlareCadence(bus);
        var bound = new HashSet<SamusState>(ReferenceEqualityComparer.Instance);
        foreach (GrapplePhase phase in Enum.GetValues<GrapplePhase>())
        foreach (SamusPoseId pose in new[] { SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose, SamusPoseId.MovingRightNormalPose, SamusPoseId.MovingLeftNormalPose })
        foreach (GrappleFlareStage stage in Enum.GetValues<GrappleFlareStage>())
        foreach (ushort coordinate in new ushort[] { 0, 1, 100, 255, 256, 511, 32768, 65535 })
        {
            SamusState native = Seed(seededNative), actual = Seed(seededActual);
            Check(native, actual, wholeState: cases == 0, seededCadence);
            SamusState Seed(SamusState samus)
            {
                samus.Pose = pose;
                samus.Position = FixedPosition<RoomPixels>.FromWhole(new(coordinate, coordinate));
                samus.Grapple.Phase = phase;
                samus.Grapple.FireDirection = pose is SamusPoseId.FacingLeftNormalPose or SamusPoseId.MovingLeftNormalPose
                    ? SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.Left
                    : SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.Right;
                samus.Grapple.FlareStage = stage;
                samus.Grapple.FlareOrigin = samus.Grapple.FlareOrigin.WithX(coordinate); samus.Grapple.FlareOrigin = samus.Grapple.FlareOrigin.WithY(coordinate);
                samus.Grapple.RopeStart = samus.Grapple.RopeStart.WithX(70); samus.Grapple.RopeStart = samus.Grapple.RopeStart.WithY(80);
                samus.Grapple.Anchor = samus.Grapple.Anchor.WithX(200); samus.Grapple.Anchor = samus.Grapple.Anchor.WithY(100);
                samus.Grapple.RopeLength = 40; samus.Grapple.AngularVelocity = AngularSpeedConstants.FromFractionsOfStep(123);
                return samus;
            }
        }
        foreach (SamusPoseId pose in new[] { SamusPoseId.MovingRightNormalPose, SamusPoseId.MovingLeftNormalPose })
        {
            var native = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(0, 100)) };
            native.Grapple.Phase = GrapplePhase.Firing;
            native.Grapple.FireDirection = pose == SamusPoseId.MovingLeftNormalPose
                ? SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.Left
                : SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection.Right;
            native.Grapple.FlareStage = GrappleFlareStage.JustFired;
            var actual = Clone(native);
            var cadence = new NativeFlareCadence(bus);
            for (int tick = 0; tick < 512; tick++)
            {
                ushort tickX = (ushort)(tick % 256);
                actual.Position = actual.Position.WithWholeX(tickX);
                native.Position = native.Position.WithWholeX(tickX);
                Check(native, actual, wholeState: tick == 0, cadence);
                // The post-Samus tile pass moves the flare on a stage, not the flare draw.
                native.Grapple.FlareStage = actual.Grapple.FlareStage = GrappleFlareStage.Shown;
            }
        }
        // The flare fired while vertically culled plays through every picture of its loop, and every wait it leaves, twice
        // over, without inventing valid sprite IDs (#2396, #2554).
        {
            SamusState culled = NewSamus();
            culled.Pose = SamusPoseId.FacingRightNormalPose;
            culled.Grapple.Phase = GrapplePhase.ConnectedLocked;
            culled.Grapple.FlareStage = GrappleFlareStage.JustFired;
            culled.Grapple.FlareOrigin = culled.Grapple.FlareOrigin.WithY(256);
            SamusState culledCopy = Clone(culled);
            var cadence = new NativeFlareCadence(bus);
            for (int update = 0; update < CulledFlareUpdates; update++)
            {
                Check(culled, culledCopy, wholeState: update == 0, cadence);
                culled.Grapple.FlareStage = culledCopy.Grapple.FlareStage = GrappleFlareStage.Shown;
            }
        }
        Suite(nameof(VerifyGrappleFlareActorBinding), () => VerifyGrappleFlareActorBinding(roomAssets, maps, grappleArtwork));
        Console.WriteLine($"Grapple flare: {cases} phase, position, sentinel, rewind and timer-boundary cases preserve native cadence/OAM and complete Samus state; actual actor rebind passes.");

        // The flare draw reads Samus and writes only her grapple's flare and, while firing, its rope and beam starts, so each
        // case compares the picture the flare shows with the original's cadence's, and the starts, and the first case of each
        // group compares her whole state (#1567, #2554).
        void Check(SamusState native, SamusState actual, bool wholeState, NativeFlareCadence cadence)
        {
            // Binding the body fixture hashes the body's characters, so each Samus state is bound once, on its first case
            // (#1680); every clone is still taken before its first case binds it.
            foreach (SamusState samus in new[] { native, actual })
            {
                if (bound.Add(samus))
                    samus.BindBodyFixture(body);
                samus.Artwork.GrappleFlarePlacement = flarePlacement;
            }
            cadence.Draw(native.Grapple.Phase, native.Grapple.FlareStage);
            var aFrame = new SpriteFrameBuffer(); var bFrame = new SpriteFrameBuffer();
            bool visible = SamusGrappleBeamDrawing.DrawFlareBeforeSamus(native, aFrame, Position16<RoomPixels>.Zero, artwork);
            OamBuffer a = aFrame.FinalizeAsOam();
            AssertEqual(visible, SamusGrappleBeamDrawing.DrawFlareBeforeSamus(actual, bFrame, Position16<RoomPixels>.Zero, artwork), "Grapple sheet retains native flare visibility");
            OamBuffer b = bFrame.FinalizeAsOam();

            AssertEqual(cadence.Picture, actual.Grapple.Flare.Picture, "Grapple flare shows the picture the native cadence shows");
            AssertTrue(a.ImageEquals(b) && (a.LastFinalizedSpriteCount * 4) == (b.LastFinalizedSpriteCount * 4), "Grapple stock parts preserve native complete OAM");
            AssertEqual(FlareState.Of(native.Grapple), FlareState.Of(actual.Grapple), "Flare draws write the same flare timing and physical origins");
            if (wholeState)
                AssertTrue(SameGraph(native, actual, content), "Flare draws preserve complete Samus state");
            cases++;
        }
        SamusState Clone(SamusState source) => RoundTrip(source, content);

        static SamusState NewSamus() => new(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
            SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), artwork: global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
    }

    /// <summary>The grapple state a flare draw writes, the flare as the picture it shows.</summary>
    private readonly record struct FlareState(GrappleFlareStage Stage, ChargeFlarePicture Picture, Position16<RoomPixels> RopeStart, Position16<RoomPixels> FlareOrigin)
    {
        /// <summary>The flare state <paramref name="grapple"/> holds.</summary>
        internal static FlareState Of(SamusGrappleState grapple) => new(grapple.FlareStage, grapple.Flare.Picture,
            grapple.RopeStart, grapple.FlareOrigin);
    }

    /// <summary>Flare draws the vertically culled flare runs from firing: more than two plays of its loop's pictures and waits.</summary>
    private const int CulledFlareUpdates = 128;

    /// <summary>
    /// The original's grapple flare cadence (#2396), read independently from the cartridge's delay table: the main flare's
    /// picture number and the wait it keeps, moved on as each of the original's flare draws moves them, from the first
    /// picture with its wait spent.
    /// </summary>
    /// <param name="bus">The cartridge.</param>
    private sealed class NativeFlareCadence(CartridgeImportAddressSpace bus)
    {
        /// <summary>The delay entry that rewinds the picture number by the entry after it.</summary>
        private const byte Rewind = 254;

        /// <summary>The main flare's picture number.</summary>
        private int frame;

        /// <summary>The wait: the updates the picture still shows less one.</summary>
        private ushort timer;

        /// <summary>The picture the original shows.</summary>
        internal ChargeFlarePicture Picture => ChargeFlareAnimationDefinitions.PictureNumbered(ChargeFlareComponent.MainFlare, frame);

        /// <summary>Moves the cadence on as the original's flare draw in <paramref name="phase"/> at <paramref name="stage"/> does.</summary>
        internal void Draw(GrapplePhase phase, GrappleFlareStage stage)
        {
            if (SamusGrappleBeamDrawing.DrawingFor(phase) != SamusGrappleDrawing.Beam || stage == GrappleFlareStage.None)
                return;
            if (stage == GrappleFlareStage.JustFired) { frame = 16; timer = 3; }
            CountdownStep step = WrappingCounter.Decrement(timer);
            timer = step.Value;
            if (!step.IsNegative)
                return;
            frame = unchecked((ushort)(frame + 1));
            byte delay = Delay(frame);
            if (delay == Rewind)
            {
                frame = unchecked((ushort)(frame - Delay(unchecked((ushort)(frame + 1)))));
                delay = Delay(frame);
            }
            timer = delay;
        }

        /// <summary>The native delay table's entry <paramref name="index"/>.</summary>
        private byte Delay(int index) => bus.ReadCartridgeByte(GrappleBeamDefinitions.Firing.MainFlareAnimationDelays + index);
    }

    /// <summary>Checks the real actor draws the flare from the bound sheets, and from newly bound ones.</summary>
    private static void VerifyGrappleFlareActorBinding(
        MapPresentationInstalledRoomAssets roomAssets, InstalledScreenContent maps, InstalledGrapplePresentation grappleArtwork)
    {
        var random = new SharedRandom();
        var game = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.CreateGame(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), hostServices: SuperMetroidGameHostServices.Default, random: random);
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(GameplayAssists.None, InstalledFixtures.GameArtwork.Gameplay, random: random, audio: game.AudioForVerification);
        runtime.Artwork.ScreenContent = maps;
        roomAssets.Bind(runtime);
        runtime.Artwork.GrappleFlarePlacement = grappleArtwork.FlarePlacement;
        runtime.Artwork.GrappleSwingFrames = grappleArtwork.SwingFrames;
        runtime.Artwork.SpriteSheets = InstalledFixtures.SpriteSheets;
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame); runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        var samus = runtime.Player.Samus;
        samus.Pose = SamusPoseId.FacingRightNormalPose; samus.Position = samus.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
        SamusPresentation.InitializeAnimation(samus);
        samus.Grapple.Phase = GrapplePhase.ConnectedLocked;
        samus.Grapple.FlareOrigin = samus.Grapple.FlareOrigin.WithX(samus.Position.X.Whole); samus.Grapple.FlareOrigin = samus.Grapple.FlareOrigin.WithY(samus.Position.Y.Whole);
        // The grapple is fired: its flare shows the loop's first picture through the three draws below (#2554).
        samus.Grapple.FlareStage = GrappleFlareStage.JustFired;
        game.AdoptRuntime(runtime);
        byte[] original = Draw(runtime);
        samus.Grapple.FlareStage = GrappleFlareStage.None;
        AssertTrue(!original.SequenceEqual(Draw(runtime)), "Real actor fixture reaches Grapple flare path");
        samus.Grapple.FlareStage = GrappleFlareStage.Shown;
        InstalledSpriteSheets rebound = InstalledFixtures.SpriteSheetsReloading(SpriteSheetId.ProjectilesPower);
        game.BindArtwork(InstalledFixtures.GameArtwork with { SpriteSheets = rebound });
        AssertTrue(original.SequenceEqual(Draw(runtime)), "Real Grapple actor draws the same flare from rebound sheets");
        runtime.Sprites.BeginFrame();
        runtime.ActorDrawing.DrawGameplayActors(runtime.Room.LoadedScope, runtime.Player.Samus, SamusDrawOwner.Gameplay, SamusColourEffects.Hold, deathStep: null);
        runtime.Sprites.FinalizeFrame();
        AssertTrue(runtime.Sprites.Draws.Where(draw => draw.Source.Kind == SpriteSourceKind.SamusGrappleFlare)
                .All(draw => ReferenceEquals(rebound.Get(SpriteSheetId.ProjectilesPower).Image, ((SamusColouredDraw)draw).Image.Sheet)),
            "Real Grapple actor draws its flare from the rebound projectile sheet");
        static byte[] Draw(SuperMetroidRuntime target)
        {
            target.Sprites.BeginFrame();
            target.ActorDrawing.DrawGameplayActors(target.Room.LoadedScope, target.Player.Samus, SamusDrawOwner.Gameplay, SamusColourEffects.Hold, deathStep: null);
            OamBuffer image = target.Sprites.FinalizeAsOam();
            return image.LowTable.ConcatToArray(image.HighTable).Concat(BitConverter.GetBytes(image.LastFinalizedSpriteCount)).ToArray();
        }
    }

    /// <summary>Reads one native facing-row selector offset to check both rows choose the shared main flare.</summary>
    private static ushort ReadNativeGrappleFlareSpritemapOffset(ISnesAddressSpace bus, int address) =>
        RomDataReader.ReadWordFixedBank(bus, address);
}
