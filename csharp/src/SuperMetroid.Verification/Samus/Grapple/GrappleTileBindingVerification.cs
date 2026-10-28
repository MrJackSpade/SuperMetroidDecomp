using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>Checks that grapple draws show the currently bound grapple sheet, including after a debugger-state restore.</summary>
internal static class GrappleTileBindingVerification
{
    /// <summary>
    /// The actual actor pass publishes grapple draws from the bound grapple sheet; rebinding host sheets leaves saved
    /// state untouched; a restored state draws from whichever sheets the host binds.
    /// </summary>
    internal static void VerifyGrappleTileBinding()
    {
        var random = new SharedRandom();
        var runtime = CreateRetailRuntimeFixture(random: random);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug); runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var samus = runtime.Player.IntroducedSamus;
        samus.Pose = SamusPoseId.FacingRightNormalPose; samus.Position = samus.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
        SamusPresentation.InitializeAnimation(samus); samus.Presentation.PrimeGraphics(samus);
        samus.Grapple.Phase = GrapplePhase.ConnectedLocked;
        samus.Grapple.PointAnimationFrame = 1; samus.Grapple.PointAnimationTimer = WrappingCountdown.Start(4);
        samus.Grapple.BeamStart = samus.Grapple.BeamStart.WithX(samus.Position.X.Whole); samus.Grapple.BeamStart = samus.Grapple.BeamStart.WithY(samus.Position.Y.Whole);
        samus.Grapple.Angle = RotationAngle.FromRaw(0x6000);
        // A connected rope with on-screen endpoint, so the beam's draws exist to bind.
        samus.Grapple.RopeLength = 32;
        samus.Grapple.Anchor = new(samus.Position.X.Whole, (ushort)(samus.Position.Y.Whole - 32));
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), random: random);
        game.AdoptRuntime(runtime);
        InstalledSpriteSheets stock = InstalledFixtures.SpriteSheets, rebound = RepositoryInstallation.Installation.LoadSpriteSheets();
        game.BindArtwork(game.Artwork with { SpriteSheets = stock });
        AssertBoundTo(runtime, stock, "Actual actor pass draws the installed grapple sheet");

        byte[] stockGraph = GraphDigest(game);
        game.BindArtwork(game.Artwork with { SpriteSheets = rebound });
        AssertTrue(stockGraph.SequenceEqual(GraphDigest(game)), "Rebinding the sheets does not change saved state before the next draw");

        game.BindArtwork(game.Artwork with { SpriteSheets = stock });
        using var stream = new MemoryStream(SerializeGraph(game));
        var restored = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(stream);
        var restoredRuntime = restored.Session.GameplayRuntime;
        AssertTrue(restoredRuntime.Artwork.SpriteSheets is null, "Restored state requires the current host sheets");
        BindRetailRuntimeFixture(restoredRuntime);
        restored.BindArtwork(restored.Artwork with { SpriteSheets = rebound });
        AssertBoundTo(restoredRuntime, rebound, "Restored state draws the newly bound grapple sheet");
        Console.WriteLine("Grapple sheet binding: actor draws show the installed sheet; rebinding leaves state untouched; restored states draw the bound sheet.");

        static void AssertBoundTo(SuperMetroidRuntime target, InstalledSpriteSheets sheets, string context)
        {
            target.Sprites.BeginFrame();
            target.ActorDrawing.DrawGameplayActors(target.Room.LoadedScope, SamusDrawOwner.Gameplay, SamusPaletteEffects.Hold);
            target.Sprites.FinalizeFrame();
            SpriteDraw[] grappleDraws = [.. target.Sprites.Draws
                .Where(draw => draw.Source.Kind is SpriteSourceKind.SamusGrappleEndpoint or SpriteSourceKind.SamusGrappleRope)];
            AssertTrue(grappleDraws.Length > 0, context + ": the grapple beam is drawn");
            SpriteImage expected = sheets.Get(SpriteSheetId.Grapple).Image;
            AssertTrue(grappleDraws.All(draw => draw is ImageSpriteDraw image && ReferenceEquals(expected, image.Image.Sheet)), context);
        }
    }
}
