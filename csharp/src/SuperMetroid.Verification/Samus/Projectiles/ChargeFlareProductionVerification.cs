using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks compiled charge flare compositions in production draw and saved-state rebinding.</summary>
internal static class ChargeFlareProductionVerification
{
    /// <summary>Compares normal and Hyper flare frames with native OAM and checks drawing leaves projectile state intact.</summary>
    internal static void VerifyChargeFlareProduction(CartridgeImportAddressSpace bus,
        SamusBodySources body, MapPresentationInstalledRoomAssets roomAssets,
        InstalledScreenContent maps)
    {
        var placement = ChargeFlarePlacementFile.Load(new MemoryStream(ChargeFlarePlacementExtractor.Extract(bus)));
        int ticks = 0;
        foreach (SamusPoseId pose in new[]
        {
            SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose,
            SamusPoseId.MovingRightNormalPose, SamusPoseId.MovingLeftNormalPose
        })
        foreach (bool hyper in new[] { false, true })
        {
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)) }.WithHyperBeam(hyper).InPose(pose);
            samus.BindBodyFixture(body);
            SamusProjectileSystem native = Create(), compiled = Create();
            for (int tick = 0; tick < 160 && native.Charge.UpdatesHeld != 0; tick++)
            {
                OamBuffer a = native.ChargeFlareOam(samus, new(0, 0), placement);
                OamBuffer b = compiled.ChargeFlareOam(samus, new(0, 0), placement);
                AssertTrue(a.ImageEquals(b), "Normal/Hyper flare producer preserves complete native sprite output");
                AssertEqual(a.LastFinalizedSpriteCount, b.LastFinalizedSpriteCount, "Flare producer preserves its sprite count");
                // The flare draw writes only the charge's flare counter, frames and timers: each tick compares those, and
                // the first tick of each pose compares the whole projectile state (#1567).
                AssertEqual(native.Charge.UpdatesHeld, compiled.Charge.UpdatesHeld, "Drawing leaves the flare counters identical");
                AssertTrue(Enum.GetValues<ChargeFlareComponent>().All(component => new[] { HorizontalDirection.Left, HorizontalDirection.Right }
                        .All(facing => native.Charge.FlareComposition(component, facing) == compiled.Charge.FlareComposition(component, facing))),
                    "Drawing leaves the flare showing the same pictures");
                if (tick == 0)
                    AssertTrue(SameGraph(native, compiled, InstalledFixtures.BoundContent), "Drawing leaves projectile, flare-counter and animation state identical");
                ticks++;
            }
        }
        // A component the Hyper Beam's count runs past its first picture stands, natively, at a selector past the $93:A1A1
        // table (the native wrap of a flare frame, as in the #1275 Mother Brain glitch movie), which makes native draw
        // WRAM/I/O bytes as a spritemap. The port's component past its first picture draws nothing (#2114), and the Hyper
        // Beam's count goes on drawing nothing (#2396).
        {
            var invalidSelector = Create();
            // The Hyper Beam's shot counts every component back from its last picture until it steps back past its first.
            invalidSelector.Charge.StartHyperBeamFlare();
            foreach (ChargeFlareComponent component in Enum.GetValues<ChargeFlareComponent>())
            {
                while (invalidSelector.Charge.FlareComposition(component, HorizontalDirection.Right) is not null)
                    _ = invalidSelector.Charge.PlayHyperBeamFlareBackward(component);
            }
            var subject = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)) }.WithHyperBeam(true).InPose(SamusPoseId.FacingRightNormalPose);
            subject.BindBodyFixture(body);
            OamBuffer outOfTableOam = invalidSelector.ChargeFlareOam(subject, new(0, 0), placement);
            AssertEqual(0, outOfTableOam.LastFinalizedSpriteCount,
                "A flare whose components have run past their first pictures draws no sprites");
        }

        var random = new SharedRandom();
<<<<<<< HEAD
        var game = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.CreateGame(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), hostServices: SuperMetroidGameHostServices.Default, random: random);
||||||| 418ca79935
        var game = global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Frontend.SuperMetroidGame>([typeof(global::SuperMetroid.Core.Saves.SaveSlotStore), typeof(global::SuperMetroid.Core.Gameplay.GameplayAssists), typeof(global::SuperMetroid.Core.Frontend.SuperMetroidGameHostServices), typeof(global::SuperMetroid.Core.Compatibility.SharedRandom)], new object?[] { new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), null, SuperMetroidGameHostServices.Default, random });
=======
        var game = new global::SuperMetroid.Core.Frontend.SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), null, SuperMetroidGameHostServices.Default, random);
>>>>>>> origin/main
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(GameplayAssists.None, InstalledFixtures.GameArtwork.Gameplay, random: random, audio: game.AudioForVerification);
        runtime.Artwork.ScreenContent = maps;
        roomAssets.Bind(runtime);
        runtime.Artwork.ChargeFlarePlacement = placement;
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        runtime.Player.Samus.ChangePoseKeepingAnimation(SamusPoseId.FacingRightNormalPose);
        runtime.Player.Samus.Position = runtime.Player.Samus.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
        SamusPresentation.InitializeAnimation(runtime.Player.Samus);
        InstalledFixtures.BindProjectileArtwork(runtime);
        runtime.Projectiles.Charge.SetFlareCount(30);
        // Each component's first picture with three updates of its wait left, so both draws, each of which plays the flare
        // one update on, show it.
        runtime.Projectiles.Charge.PlayFlareTo([0, 0, 0], [3, 3, 3]);
        game.AdoptRuntime(runtime);
        SpriteImage PowerBeamSheet(InstalledSpriteSheets sheets) => sheets.Get(SpriteSheetId.ProjectilesPower).Image;
        AssertTrue(FlareDraws(runtime).All(draw => ReferenceEquals(PowerBeamSheet(InstalledFixtures.SpriteSheets), draw.Image.Sheet)),
            "Runtime actor pass draws the flare from the bound power-beam sheet");
        InstalledSpriteSheets rebound = InstalledFixtures.SpriteSheetsReloading(SpriteSheetId.ProjectilesPower);
        game.BindArtwork(InstalledFixtures.GameArtwork with { SpriteSheets = rebound });
        AssertTrue(FlareDraws(runtime).All(draw => ReferenceEquals(PowerBeamSheet(rebound), draw.Image.Sheet)),
            "Rebinding the sprite sheets reaches the actor pass");
        Console.WriteLine($"Charge-flare production: {ticks} normal and Hyper ticks, state isolation and actor sheet rebinding pass.");

        static IEnumerable<SamusColouredDraw> FlareDraws(SuperMetroidRuntime target)
        {
            target.Sprites.BeginFrame();
            target.ActorDrawing.DrawGameplayActors(target.Room.LoadedScope, target.Player.Samus, SamusDrawOwner.Gameplay, SamusColourEffects.Hold, deathStep: null);
            target.Sprites.FinalizeFrame();
            SamusColouredDraw[] flare = [.. target.Sprites.Draws.Where(draw => draw.Source.Kind == SpriteSourceKind.SamusChargeFlare).Cast<SamusColouredDraw>()];
            AssertTrue(flare.Length > 0, "The actor pass draws the charge flare");
            return flare;
        }

        static SamusProjectileSystem Create()
        {
            var system = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake());
            system.Charge.SetFlareCount(30);
            system.Charge.PlayFlareTo([3, 3, 3], [2, 2, 2]);
            return system;
        }
    }
}
