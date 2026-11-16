using SuperMetroid.Core.Audio;
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
using SuperMetroid.Core.Frontend.Map.Artwork;
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
        AreaMapPresentation maps)
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
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)), HyperBeamEnabled = hyper };
            samus.BindBodyFixture(body);
            SamusProjectileSystem native = Create(), compiled = Create();
            for (int tick = 0; tick < 160 && native.Charge.FlareCounter != 0; tick++)
            {
                OamBuffer a = native.ChargeFlareOam(samus, new(0, 0), placement);
                OamBuffer b = compiled.ChargeFlareOam(samus, new(0, 0), placement);
                AssertTrue(a.ImageEquals(b), "Normal/Hyper flare producer preserves complete native sprite output");
                AssertEqual(a.LastFinalizedSpriteCount, b.LastFinalizedSpriteCount, "Flare producer preserves its sprite count");
                // The flare draw writes only the charge's flare counter, frames and timers: each tick compares those, and
                // the first tick of each pose compares the whole projectile state (#1567).
                AssertEqual(native.Charge.FlareCounter, compiled.Charge.FlareCounter, "Drawing leaves the flare counters identical");
                AssertTrue(Enum.GetValues<ChargeFlareComponent>().All(component =>
                        (native.Charge.Flare(component).Picture, native.Charge.Flare(component).Hold) ==
                        (compiled.Charge.Flare(component).Picture, compiled.Charge.Flare(component).Hold)),
                    "Drawing leaves the flare animation state identical");
                if (tick == 0)
                    AssertTrue(SameGraph(native, compiled, InstalledFixtures.BoundContent), "Drawing leaves projectile, flare-counter and animation state identical");
                ticks++;
            }
        }
        // A component the Hyper Beam's count runs past its first picture stands, natively, at a selector past the $93:A1A1
        // table (the native wrap of a flare frame, as in the #1275 Mother Brain glitch movie), which makes native draw
        // WRAM/I/O bytes as a spritemap. The port's component past its first picture draws nothing (#2114).
        {
            var invalidSelector = Create();
            foreach (ChargeFlareComponent component in Enum.GetValues<ChargeFlareComponent>())
                invalidSelector.Charge.Flare(component).RunPastFirstPicture(WrappingCountdown.Start(3));
            var subject = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)) };
            subject.BindBodyFixture(body);
            OamBuffer outOfTableOam = invalidSelector.ChargeFlareOam(subject, new(0, 0), placement);
            AssertEqual(0, outOfTableOam.LastFinalizedSpriteCount,
                "A flare whose components have run past their first pictures draws no sprites");
        }

        var random = new SharedRandom();
        var game = new SuperMetroid.Core.Frontend.SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(), null, SuperMetroid.Core.Frontend.SuperMetroidGameHostServices.Default, random);
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(GameplayAssists.None, InstalledFixtures.GameArtwork.Gameplay, random: random, audio: game.AudioForVerification);
        runtime.Artwork.MapPresentation = maps;
        roomAssets.Bind(runtime);
        runtime.Artwork.ChargeFlarePlacement = placement;
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.Player.IntroducedSamus.Pose = SamusPoseId.FacingRightNormalPose;
        runtime.Player.IntroducedSamus.Position = runtime.Player.IntroducedSamus.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
        SamusPresentation.InitializeAnimation(runtime.Player.IntroducedSamus);
        InstalledFixtures.BindProjectileArtwork(runtime);
        runtime.Projectiles.Charge.FlareCounter = 30;
        foreach (ChargeFlareComponent component in Enum.GetValues<ChargeFlareComponent>())
            runtime.Projectiles.Charge.Flare(component).Show(new(ChargeFlareAnimations.FirstPlace, WrappingCountdown.Start(100)));
        game.AdoptRuntime(runtime);
        SpriteImage PowerBeamSheet(InstalledSpriteSheets sheets) => sheets.Get(SpriteSheetId.ProjectilesPower).Image;
        AssertTrue(FlareDraws(runtime).All(draw => ReferenceEquals(PowerBeamSheet(InstalledFixtures.SpriteSheets), draw.Image.Sheet)),
            "Runtime actor pass draws the flare from the bound power-beam sheet");
        InstalledSpriteSheets rebound = InstalledFixtures.SpriteSheetsReloading(SpriteSheetId.ProjectilesPower);
        game.BindArtwork(InstalledFixtures.GameArtwork with { SpriteSheets = rebound });
        AssertTrue(FlareDraws(runtime).All(draw => ReferenceEquals(PowerBeamSheet(rebound), draw.Image.Sheet)),
            "Rebinding the sprite sheets reaches the actor pass");
        Console.WriteLine($"Charge-flare production: {ticks} normal and Hyper ticks, state isolation and actor sheet rebinding pass.");

        static IEnumerable<ImageSpriteDraw> FlareDraws(SuperMetroidRuntime target)
        {
            target.Sprites.BeginFrame();
            target.ActorDrawing.DrawGameplayActors(target.Room.LoadedScope, SamusDrawOwner.Gameplay, SamusColourEffects.Hold);
            target.Sprites.FinalizeFrame();
            ImageSpriteDraw[] flare = [.. target.Sprites.Draws.Where(draw => draw.Source.Kind == SpriteSourceKind.SamusChargeFlare).Cast<ImageSpriteDraw>()];
            AssertTrue(flare.Length > 0, "The actor pass draws the charge flare");
            return flare;
        }

        static SamusProjectileSystem Create()
        {
            var system = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.PlaceCount));
            system.Charge.FlareCounter = 30;
            foreach (ChargeFlareComponent component in Enum.GetValues<ChargeFlareComponent>())
                system.Charge.Flare(component).Show(new(3, WrappingCountdown.Start(3)));
            return system;
        }
    }
}
