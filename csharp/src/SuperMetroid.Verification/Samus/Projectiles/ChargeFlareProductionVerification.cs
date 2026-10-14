using SuperMetroid.Core;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Map.Artwork;
namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks compiled charge flare compositions in production draw and saved-state rebinding.</summary>
internal static class ChargeFlareProductionVerification
{
    /// <summary>Compares normal and Hyper flare frames with native OAM, then checks visual edits leave projectile state intact.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyChargeFlareProduction(SuperMetroidAddressSpace bus,
        ChargeFlareSpriteCatalog stock, ChargeFlareSpriteCatalog edited,
        SamusBodyArtworkCatalog body, MapPresentationInstalledRoomAssets roomAssets,
        AreaMapPresentationCatalog maps)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var placement = ChargeFlarePlacementCatalog.Load(new MemoryStream(ChargeFlarePlacementExtractor.Extract(bus)));
        var guarded = NewChargeFlareCompositionGuard(bus);
        int ticks = 0;
        foreach (SamusPoseId pose in new[]
        {
            SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose,
            SamusPoseId.MovingRightNormalPose, SamusPoseId.MovingLeftNormalPose
        })
        foreach (ushort hyper in new ushort[] { 0, 1 })
        {
            var samus = new SamusState { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)), HyperBeam = hyper };
            samus.Presentation.TileTransfers.BindArtwork(body);
            SamusProjectileSystem native = Create(), compiled = Create(), changed = Create();
            for (int tick = 0; tick < 160 && native.Charge.FlareCounter != 0; tick++)
            {
                var a = new OamBuffer(); var b = new OamBuffer(); var c = new OamBuffer();
                native.HandleChargeFlareAndDraw(bus, a, samus, new(0, 0),
                    placement: placement, compositions: stock);
                compiled.HandleChargeFlareAndDraw(guarded, b, samus, new(0, 0), placement: placement, compositions: stock);
                changed.HandleChargeFlareAndDraw(guarded, c, samus, new(0, 0), placement: placement, compositions: edited);
                AssertTrue(a.ImageEquals(b), "Normal/Hyper flare producer preserves complete native sprite output");
                AssertEqual(a.NextByteOffset, b.NextByteOffset, "Flare producer preserves OAM cursor");
                AssertEqual(a.NextByteOffset, c.NextByteOffset, "Edited flare producer preserves component count");
                for (int i = 0; i < a.NextByteOffset / 4; i++)
                {
                    AssertEqual((a.GetEntry(i).X + 7) & 511, c.GetEntry(i).X, "All emitted normal/Hyper components consume edited composition");
                    AssertEqual(a.GetEntry(i).Y, c.GetEntry(i).Y, "Composition X edit preserves emitted Y");
                }
                AssertTrue(GraphDigest(native).SequenceEqual(GraphDigest(compiled)) && GraphDigest(native).SequenceEqual(GraphDigest(changed)), "All projectile, flare-counter and animation state remains identical after visual replacement");
                ticks++;
            }
        }
        // A selector past the $93:A1A1 table (the native wrap of a flare frame to $FF, as in
        // the #1275 Mother Brain glitch movie) makes native draw WRAM/I/O bytes as a spritemap.
        // The port draws nothing for it and never reads adjacent ROM bytes.
        foreach (ushort outOfTableFrame in new ushort[] { 54, 0xffff })
        {
            var invalidSelector = Create();
            Array.Fill(invalidSelector.Charge._flareFrames, outOfTableFrame);
            var subject = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)) };
            subject.Presentation.TileTransfers.BindArtwork(body);
            var outOfTableOam = new OamBuffer();
            invalidSelector.HandleChargeFlareAndDraw(
                guarded, outOfTableOam, subject, new(0, 0), placement: placement, compositions: stock);
            AssertEqual(0, outOfTableOam.NextByteOffset,
                $"Out-of-table flare frame ${outOfTableFrame:X4} draws no sprites");
        }

        var runtime = new SuperMetroidRuntime(bus, HostGameplayOverrides.None, initialPaletteArt: roomAssets.InitialPalettes) { Artwork = { MapPresentation = maps } };
        roomAssets.Bind(runtime);
        runtime.Artwork.ChargeFlarePlacement = placement;
        runtime.Artwork.ChargeFlareCompositions = stock;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.Player.IntroducedSamus.Pose = SamusPoseId.FacingRightNormalPose;
        runtime.Player.IntroducedSamus.Position = runtime.Player.IntroducedSamus.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
        runtime.Player.IntroducedSamus.Presentation.InitializeAnimation(bus); runtime.Player.IntroducedSamus.Presentation.PrimeGraphics(bus);
        runtime.Projectiles.Charge.FlareCounter = (ushort)30;
        Array.Fill(runtime.Projectiles.Charge._flareTimers, NativeCountdown.Start(100));
        var game = new SuperMetroidGame(bus);
        game.runtime = runtime;
        byte[] baseline = Draw(runtime);
        game.BindArtwork(game.Artwork with
        {
            ChargeFlareCompositions = stock,
        });
        AssertTrue(baseline.SequenceEqual(Draw(runtime)), "Runtime actor pass preserves stock flare composition");
        game.BindArtwork(game.Artwork with
        {
            ChargeFlareCompositions = edited,
        });
        AssertTrue(!baseline.SequenceEqual(Draw(runtime)), "Runtime actor pass emits edited flare composition");
        game.BindArtwork(game.Artwork with
        {
            ChargeFlareCompositions = null,
        });
        byte[] unbound = GraphDigest(game);
        game.BindArtwork(game.Artwork with
        {
            ChargeFlareCompositions = stock,
        });
        byte[] bound = SerializeGraph(game);
        AssertTrue(unbound.SequenceEqual(System.Security.Cryptography.SHA256.HashData(bound)), "Charge-flare compositions are excluded from saved frontend/runtime graphs");
        using var stream = new MemoryStream(bound);
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(stream);
        var restoredRuntime = restored.GameplayRuntime;
        AssertTrue(restoredRuntime.Artwork.ChargeFlareCompositions is null, "Saved state does not retain old composition content");
        roomAssets.Bind(restoredRuntime);
        restoredRuntime.Artwork.MapPresentation = maps;
        restoredRuntime.Artwork.ChargeFlarePlacement = placement;
        restoredRuntime.Player.IntroducedSamus.Presentation.TileTransfers.BindArtwork(body);
        restoredRuntime.Player.IntroducedSamus.ArmCannon.Artwork = body.ArmCannon;
        game.BindArtwork(game.Artwork with
        {
            ChargeFlareCompositions = edited,
        }); restored.BindArtwork(restored.Artwork with
{
    ChargeFlareCompositions = edited,
});
        AssertTrue(Draw(runtime).SequenceEqual(Draw(restoredRuntime)), "Restored actor pass uses current composition at saved animation state");
        Console.WriteLine($"Charge-flare production: {ticks} native/compiled/edited normal and Hyper ticks, ROM guard, state isolation and real actor restore/rebind pass.");

        static byte[] Draw(SuperMetroidRuntime target)
        {
            target.Oam.BeginFrame();
            target.ActorDrawing.DrawGameplayActors(false, false);
            return target.Oam.LowTable.ConcatToArray(target.Oam.HighTable).Concat(BitConverter.GetBytes(target.Oam.NextByteOffset)).ToArray();
        }
        static SamusProjectileSystem Create()
        {
            var system = new SamusProjectileSystem();
            system.Charge.FlareCounter = (ushort)30;
            var frames = system.Charge._flareFrames;
            Array.Fill(frames, (ushort)3);
            Array.Fill(system.Charge._flareTimers, NativeCountdown.Start(3));
            return system;
        }
    }

    /// <summary>Blocks live reads of the native flare selector table and every referenced spritemap body.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the native charge-flare spritemap extents the guard forbids")]
    internal static CartridgeReadGuard NewChargeFlareCompositionGuard(ISnesAddressSpace source)
    {
        var forbidden = new HashSet<int>();
        for (int i = 0; i < ChargeFlareSpriteDefinitions.Selectors.Length * 2; i++)
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
