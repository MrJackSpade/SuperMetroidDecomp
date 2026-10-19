using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>Checks that grapple draws bind the currently installed grapple artwork, including after a debugger-state restore.</summary>
internal static class GrappleTileBindingVerification
{
    /// <summary>
    /// The actual actor pass publishes grapple draws bound to the installed atlas's endpoint/rope tiles; rebinding host
    /// artwork leaves saved state untouched; a restored state draws with whichever atlas the host binds.
    /// </summary>
    internal static void VerifyGrappleTileBinding(GrappleTileAtlas stock, GrappleTileAtlas edited)
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug); runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var samus = runtime.Player.IntroducedSamus;
        samus.Pose = SamusPoseId.FacingRightNormalPose; samus.Position = samus.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
        samus.Presentation.InitializeAnimation(); samus.Presentation.PrimeGraphics();
        samus.Grapple.Phase = GrapplePhase.ConnectedLocked;
        samus.Grapple.PointAnimationFrame = 1; samus.Grapple.PointAnimationTimer = NativeCountdown.Start(4);
        samus.Grapple.BeamStart = samus.Grapple.BeamStart.WithX(samus.Position.X.Whole); samus.Grapple.BeamStart = samus.Grapple.BeamStart.WithY(samus.Position.Y.Whole);
        samus.Grapple.Angle = SnesAngle.FromRaw(0x6000);
        // A connected rope with on-screen endpoint, so the beam's draws exist to bind.
        samus.Grapple.RopeLength = 32;
        samus.Grapple.Anchor = new(samus.Position.X.Whole, (ushort)(samus.Position.Y.Whole - 32));
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore());
        game.Session.runtime = runtime;
        game.BindArtwork(game.Artwork with { GrappleArtwork = stock });
        AssertBoundTo(runtime, stock, "Actual actor pass binds the installed grapple tiles");

        byte[] stockGraph = GraphDigest(game);
        game.BindArtwork(game.Artwork with { GrappleArtwork = edited });
        AssertTrue(stockGraph.SequenceEqual(GraphDigest(game)), "Changing Grapple PNG does not change saved state before the next draw");

        game.BindArtwork(game.Artwork with { GrappleArtwork = stock });
        using var stream = new MemoryStream(SerializeGraph(game));
        var restored = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(stream);
        var restoredRuntime = restored.Session.GameplayRuntime;
        AssertTrue(restoredRuntime.Artwork.GrappleArtwork is null, "Restored state requires current host Grapple artwork");
        BindRetailRuntimeFixture(restoredRuntime);
        restored.BindArtwork(restored.Artwork with { GrappleArtwork = edited });
        AssertBoundTo(restoredRuntime, edited, "Restored state draws with the newly bound Grapple PNG");
        Console.WriteLine("Grapple tile binding: actor draws bind installed tiles; rebinding leaves state untouched; restored states draw with the bound atlas.");

        static void AssertBoundTo(SuperMetroidRuntime target, GrappleTileAtlas atlas, string context)
        {
            target.Sprites.BeginFrame();
            target.ActorDrawing.DrawGameplayActors(false, false);
            target.Sprites.FinalizeFrame();
            SamusGrappleState grapple = target.Player.IntroducedSamus.Grapple;
            SpriteDraw[] grappleDraws = [.. target.Sprites.Draws.Select(ordered => ordered.Draw)
                .Where(draw => draw.Source.Kind is SpriteSourceKind.SamusGrappleEndpoint or SpriteSourceKind.SamusGrappleRope)];
            AssertTrue(grappleDraws.Length > 0, context + ": the grapple beam is drawn");
            SpriteArtwork expected = atlas.OnStandardObjects(target.Artwork.StandardObjects ?? throw new InvalidOperationException(
                "Verification requires the runtime's standard OBJ page."), grapple.PointAnimationFrame, grapple.Angle.RawValue);
            AssertTrue(grappleDraws.All(draw => ReferenceEquals(expected, draw.Artwork)), context);
        }
    }
}
