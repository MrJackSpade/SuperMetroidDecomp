using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.TestSupport.RepositoryInstallation;
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
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks compiled charge flare compositions in production draw and saved-state rebinding.</summary>
internal static class ChargeFlareProductionVerification
{
    /// <summary>Compares normal and Hyper flare frames with native OAM, then checks visual edits leave projectile state intact.</summary>
    internal static void VerifyChargeFlareProduction(SuperMetroidAddressSpace bus,
        InstalledSamusBodyArtwork body, MapPresentationInstalledRoomAssets roomAssets,
        AreaMapPresentation maps)
    {
        var placement = ChargeFlarePlacements.Load(new MemoryStream(ChargeFlarePlacementExtractor.Extract(bus)));
        int ticks = 0;
        foreach (SamusPoseId pose in new[]
        {
            SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose,
            SamusPoseId.MovingRightNormalPose, SamusPoseId.MovingLeftNormalPose
        })
        foreach (bool hyper in new[] { false, true })
        {
            var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)), HyperBeamEnabled = hyper };
            samus.Presentation.BodyFrames.BindBodyFixture(body);
            SamusProjectileSystem native = Create(), compiled = Create();
            for (int tick = 0; tick < 160 && native.Charge.FlareCounter != 0; tick++)
            {
                OamBuffer a = native.ChargeFlareOam(samus, new(0, 0), placement);
                OamBuffer b = compiled.ChargeFlareOam(samus, new(0, 0), placement);
                AssertTrue(a.ImageEquals(b), "Normal/Hyper flare producer preserves complete native sprite output");
                AssertEqual(a.LastFinalizedSpriteCount, b.LastFinalizedSpriteCount, "Flare producer preserves its sprite count");
                AssertTrue(GraphDigest(native).SequenceEqual(GraphDigest(compiled)), "Drawing leaves projectile, flare-counter and animation state identical");
                ticks++;
            }
        }
        // A selector past the $93:A1A1 table (the native wrap of a flare frame to $FF, as in
        // the #1275 Mother Brain glitch movie) makes native draw WRAM/I/O bytes as a spritemap.
        // The port draws nothing for it and never reads adjacent ROM bytes.
        foreach (ushort outOfTableFrame in new ushort[] { 54, 0xffff })
        {
            var invalidSelector = Create();
            Array.Fill(invalidSelector.Charge.FlareFrames, outOfTableFrame);
            var subject = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)) };
            subject.Presentation.BodyFrames.BindBodyFixture(body);
            OamBuffer outOfTableOam = invalidSelector.ChargeFlareOam(subject, new(0, 0), placement);
            AssertEqual(0, outOfTableOam.LastFinalizedSpriteCount,
                $"Out-of-table flare frame ${outOfTableFrame:X4} draws no sprites");
        }

        var random = new SharedRandom();
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(HostGameplayOverrides.None, InstalledFixtures.SamusOverreads, initialPaletteArt: roomAssets.InitialPalettes, random: random);
        runtime.Artwork.MapPresentation = maps;
        roomAssets.Bind(runtime);
        runtime.Artwork.ChargeFlarePlacement = placement;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.Player.IntroducedSamus.Pose = SamusPoseId.FacingRightNormalPose;
        runtime.Player.IntroducedSamus.Position = runtime.Player.IntroducedSamus.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
        SamusPresentation.InitializeAnimation(runtime.Player.IntroducedSamus); runtime.Player.IntroducedSamus.Presentation.PrimeGraphics(runtime.Player.IntroducedSamus);
        InstalledFixtures.BindProjectileArtwork(runtime);
        runtime.Projectiles.Charge.FlareCounter = 30;
        Array.Fill(runtime.Projectiles.Charge.FlareTimers, WrappingCountdown.Start(100));
        var game = new SuperMetroid.Core.Frontend.SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(), null, SuperMetroid.Core.Frontend.SuperMetroidGameHostServices.Default, random);
        game.AdoptRuntime(runtime);
        SpriteImage PowerBeamSheet(InstalledSpriteSheets sheets) => sheets.Get(SpriteSheetId.ProjectilesPower).Image;
        AssertTrue(FlareDraws(runtime).All(draw => ReferenceEquals(PowerBeamSheet(InstalledFixtures.SpriteSheets), draw.Image.Sheet)),
            "Runtime actor pass draws the flare from the bound power-beam sheet");
        InstalledSpriteSheets rebound = RepositoryInstallation.Installation.LoadSpriteSheets();
        game.BindArtwork(game.Artwork with { SpriteSheets = rebound });
        AssertTrue(FlareDraws(runtime).All(draw => ReferenceEquals(PowerBeamSheet(rebound), draw.Image.Sheet)),
            "Rebinding the sprite sheets reaches the actor pass");
        game.BindArtwork(game.Artwork with { SpriteSheets = null });
        byte[] unbound = GraphDigest(game);
        game.BindArtwork(game.Artwork with { SpriteSheets = rebound });
        byte[] bound = SerializeGraph(game);
        AssertTrue(unbound.SequenceEqual(System.Security.Cryptography.SHA256.HashData(bound)), "Sprite sheets are excluded from saved frontend/runtime graphs");
        using var stream = new MemoryStream(bound);
        var restored = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(stream);
        var restoredRuntime = restored.Session.GameplayRuntime;
        AssertTrue(restoredRuntime.Artwork.SpriteSheets is null, "Saved state does not retain old sheet content");
        roomAssets.Bind(restoredRuntime);
        restoredRuntime.Artwork.MapPresentation = maps;
        restoredRuntime.Artwork.ChargeFlarePlacement = placement;
        restoredRuntime.Player.IntroducedSamus.Presentation.BodyFrames.BindBodyFixture(body);
        restoredRuntime.Player.IntroducedSamus.ArmCannon.Artwork = body.ArmCannon;
        restored.BindArtwork(restored.Artwork with { SpriteSheets = rebound });
        AssertTrue(Draw(runtime).SequenceEqual(Draw(restoredRuntime)), "Restored actor pass draws the current sheets at the saved animation state");
        Console.WriteLine($"Charge-flare production: {ticks} normal and Hyper ticks, state isolation and real actor sheet restore/rebind pass.");

        static IEnumerable<ImageSpriteDraw> FlareDraws(SuperMetroidRuntime target)
        {
            target.Sprites.BeginFrame();
            target.ActorDrawing.DrawGameplayActors(target.Room.LoadedScope, SamusDrawOwner.Gameplay, SamusPaletteEffects.Hold);
            target.Sprites.FinalizeFrame();
            ImageSpriteDraw[] flare = [.. target.Sprites.Draws.Where(draw => draw.Source.Kind == SpriteSourceKind.SamusChargeFlare).Cast<ImageSpriteDraw>()];
            AssertTrue(flare.Length > 0, "The actor pass draws the charge flare");
            return flare;
        }

        static byte[] Draw(SuperMetroidRuntime target)
        {
            target.Sprites.BeginFrame();
            target.ActorDrawing.DrawGameplayActors(target.Room.LoadedScope, SamusDrawOwner.Gameplay, SamusPaletteEffects.Hold);
            target.Sprites.FinalizeFrame();
            OamBuffer image = target.Sprites.FinalizedAsOam();
            return image.LowTable.ConcatToArray(image.HighTable).Concat(BitConverter.GetBytes(image.LastFinalizedSpriteCount)).ToArray();
        }
        static SamusProjectileSystem Create()
        {
            var system = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads);
            system.Charge.FlareCounter = 30;
            var frames = system.Charge.FlareFrames;
            Array.Fill(frames, (ushort)3);
            Array.Fill(system.Charge.FlareTimers, WrappingCountdown.Start(3));
            return system;
        }
    }

    /// <summary>Blocks live reads of the native flare selector table and every referenced spritemap body.</summary>
    internal static CartridgeReadGuard NewChargeFlareCompositionGuard(ISnesAddressSpace source)
    {
        var forbidden = new HashSet<int>();
        for (int i = 0; i < ChargeFlareSpriteSelection.Selectors.Count * 2; i++)
            forbidden.Add(ChargeFlareSpriteDefinitions.SelectorTable + i);
        foreach (ushort pointer in ChargeFlareSpriteDefinitions.NativePointers)
        {
            int address = 0x930000 | pointer;
            int size = 2 + 5 * RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), address);
            for (int i = 0; i < size; i++) forbidden.Add(address + i);
        }
        return new CartridgeReadGuard(source).Forbid(forbidden.Contains,
            address => $"Flare composition read from ROM at {address:X6}.");
    }
}
