using SuperMetroid.Core;
using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Samus.Projectiles.ChargeFlareProductionVerification;
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
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>Checks Grapple flare timing, OAM, editable offsets, and restored actor binding against native presentation.</summary>
internal static class GrappleFlarePresentationVerification
{
    /// <summary>Compares flare drawing across phases and coordinates with native cadence while checking an edited composition preserves gameplay state.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyGrappleFlarePresentation(SuperMetroidAddressSpace bus,
        ChargeFlareSpriteCatalog stock, ChargeFlareSpriteCatalog edited,
        SamusBodyArtworkCatalog body, MapPresentationInstalledRoomAssets roomAssets,
        AreaMapPresentationCatalog maps, GrappleTileAtlas grappleArtwork)
    {
        var guard = NewGrappleFlarePresentationGuard(NewChargeFlareCompositionGuard(bus));
        var flarePlacement = ChargeFlarePlacementCatalog.Load(new MemoryStream(
            GrappleFlarePlacementExtractor.Extract(bus)));
        foreach (int address in new[] { SamusGrappleRomData.Firing.RightFlareSpritemapOffsets, SamusGrappleRomData.Firing.LeftFlareSpritemapOffsets })
            AssertEqual(ReadNativeGrappleFlareSpritemapOffset(bus, address), ChargeFlareSpriteDefinitions.MainFlareSelectorOffset, "Both native Grapple-facing rows select the same main flare");
        int cases = 0;
        foreach (GrapplePhase phase in Enum.GetValues<GrapplePhase>())
        foreach (SamusPoseId pose in new[] { SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose, SamusPoseId.MovingRightNormalPose, SamusPoseId.MovingLeftNormalPose })
        foreach (ushort counter in new ushort[] { 0, 1, 2 })
        foreach (ushort coordinate in new ushort[] { 0, 1, 100, 255, 256, 511, 32768, 65535 })
        {
            SamusState native = Seed(), actual = Seed(), changed = Seed();
            Check(native, actual, changed);
            SamusState Seed()
            {
                var samus = new SamusState { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(coordinate, coordinate)) };
                samus.Grapple.Phase = phase;
                samus.Grapple.FireDirection = (byte)(pose is SamusPoseId.FacingLeftNormalPose or SamusPoseId.MovingLeftNormalPose ? 7 : 2);
                samus.Grapple.FlareCounter = counter;
                samus.Grapple.FlareAnimationFrame = 29;
                samus.Grapple.FlareAnimationTimer = NativeCountdown.Start(0);
                samus.Grapple.BeamStart = samus.Grapple.BeamStart.WithX(coordinate); samus.Grapple.BeamStart = samus.Grapple.BeamStart.WithY(coordinate);
                samus.Grapple.RopeStart = samus.Grapple.RopeStart.WithX(70); samus.Grapple.RopeStart = samus.Grapple.RopeStart.WithY(80);
                samus.Grapple.Anchor = samus.Grapple.Anchor.WithX(200); samus.Grapple.Anchor = samus.Grapple.Anchor.WithY(100);
                samus.Grapple.RopeLength = 40; samus.Grapple.AngularVelocity = 123;
                return samus;
            }
        }
        foreach (SamusPoseId pose in new[] { SamusPoseId.MovingRightNormalPose, SamusPoseId.MovingLeftNormalPose })
        {
            var native = new SamusState { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(0, 100)) };
            native.Grapple.Phase = GrapplePhase.Firing;
            native.Grapple.FireDirection = (byte)(pose == SamusPoseId.MovingLeftNormalPose ? 7 : 2);
            native.Grapple.FlareCounter = 1;
            var actual = Clone(native); var changed = Clone(native);
            for (int tick = 0; tick < 512; tick++)
            {
                ushort tickX = (ushort)(tick % 256);
                changed.Position = changed.Position.WithWholeX(tickX);
                actual.Position = actual.Position.WithWholeX(tickX);
                native.Position = native.Position.WithWholeX(tickX);
                Check(native, actual, changed);
                // Native post-Samus tile upload owns this counter, not the flare.
                native.Grapple.FlareCounter = actual.Grapple.FlareCounter = changed.Grapple.FlareCounter = (ushort)Math.Min(120, native.Grapple.FlareCounter + 1);
            }
        }
        // Exercise negative timers across the complete authored main-flare frame domain
        // while vertically culled, without inventing valid sprite IDs.
        foreach (ushort timer in new ushort[] { 0, 1, 2, 32768, 65535 })
        foreach (ushort frame in new ushort[] { 0, 15, 16, 29 })
        {
            var samus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose };
            samus.Grapple.Phase = GrapplePhase.ConnectedLocked;
            samus.Grapple.FlareCounter = 2;
            samus.Grapple.FlareAnimationFrame = frame;
            samus.Grapple.FlareAnimationTimer = NativeCountdown.FromRaw(timer);
            samus.Grapple.BeamStart = samus.Grapple.BeamStart.WithY(256);
            Check(samus, Clone(samus), Clone(samus));
        }
        Suite(nameof(VerifyGrappleFlareActorBinding), () => VerifyGrappleFlareActorBinding(bus, stock, edited, body, roomAssets, maps,
            grappleArtwork));
        Console.WriteLine($"Grapple flare: {cases} phase, position, sentinel, rewind and timer-boundary cases preserve native cadence/OAM and complete Samus state with visual/cadence ROM forbidden; actual actor override/rebind passes.");

        void Check(SamusState native, SamusState actual, SamusState changed)
        {
            foreach (SamusState samus in new[] { native, actual, changed })
            {
                samus.Presentation.TileTransfers.BindArtwork(body);
                samus.Grapple.FlarePlacement = flarePlacement;
            }
            ushort expectedFrame = native.Grapple.FlareAnimationFrame, expectedTimer = native.Grapple.FlareAnimationTimer.RawValue;
            if (SamusGrappleMovement.UsesBeamSpecificDrawingPath(native.Grapple.Phase) && native.Grapple.FlareCounter != 0)
            {
                if (native.Grapple.FlareCounter == 1) { expectedFrame = 16; expectedTimer = 3; }
                NativeWordCounterStep expectedStep = NativeWordCounter.Decrement(expectedTimer);
                expectedTimer = expectedStep.Value;
                if (expectedStep.IsNegative)
                {
                    expectedFrame = unchecked((ushort)(expectedFrame + 1));
                    byte delay = GrappleFlarePresentationRomByte(bus, SamusGrappleRomData.Firing.MainFlareAnimationDelays + expectedFrame);
                    if (delay == 254)
                    {
                        byte rewind = GrappleFlarePresentationRomByte(bus, SamusGrappleRomData.Firing.MainFlareAnimationDelays + unchecked((ushort)(expectedFrame + 1)));
                        expectedFrame = unchecked((ushort)(expectedFrame - rewind));
                        delay = GrappleFlarePresentationRomByte(bus, SamusGrappleRomData.Firing.MainFlareAnimationDelays + expectedFrame);
                    }
                    expectedTimer = delay;
                }
            }
            var a = new OamBuffer(); var b = new OamBuffer(); var c = new OamBuffer();
            bool visible = SamusGrappleMovement.DrawFlareBeforeSamus(bus, native, a, Position16<RoomPixels>.Zero, stock);
            AssertEqual(visible, SamusGrappleMovement.DrawFlareBeforeSamus(guard, actual, b, Position16<RoomPixels>.Zero, stock), "Grapple catalog retains native flare visibility");
            AssertEqual(visible, SamusGrappleMovement.DrawFlareBeforeSamus(guard, changed, c, Position16<RoomPixels>.Zero, edited), "Grapple art edit retains origin visibility gate");
            AssertEqual((expectedFrame, expectedTimer), (actual.Grapple.FlareAnimationFrame, actual.Grapple.FlareAnimationTimer.RawValue), "Grapple shared compiled cadence matches independent native reads");
            AssertTrue(a.ImageEquals(b) && a.NextByteOffset == b.NextByteOffset, "Grapple stock parts preserve native complete OAM");
            AssertEqual(a.NextByteOffset, c.NextByteOffset, "Grapple art edit preserves sprite count");
            for (int i = 0; i < a.NextByteOffset / 4; i++)
            {
                AssertEqual((a.GetEntry(i).X + 7) & 511, c.GetEntry(i).X, "Grapple emits edited visual part offsets");
                AssertEqual(a.GetEntry(i).Y, c.GetEntry(i).Y, "Grapple X-only visual edit preserves Y");
            }
            AssertTrue(GraphDigest(native).SequenceEqual(GraphDigest(actual)) && GraphDigest(native).SequenceEqual(GraphDigest(changed)), "Flare edits preserve complete Samus/Grapple state including physical origins and timers");
            cases++;
        }
        static SamusState Clone(SamusState source)
        {
            using var stream = new MemoryStream(SerializeGraph(source));
            return SuperMetroid.Diagnostics.DebuggerObjectGraphSerializer.Deserialize<SamusState>(stream);
        }
    }

    /// <summary>Reads a native Grapple flare delay byte solely for the verification timing oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for grapple flare presentation")]
    private static byte GrappleFlarePresentationRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks the real actor consumes stock and edited flare compositions and rebinds current art after a saved-state restore.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    private static void VerifyGrappleFlareActorBinding(SuperMetroidAddressSpace bus,
        ChargeFlareSpriteCatalog stock, ChargeFlareSpriteCatalog edited,
        SamusBodyArtworkCatalog body, MapPresentationInstalledRoomAssets roomAssets,
        AreaMapPresentationCatalog maps, GrappleTileAtlas grappleArtwork)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var runtime = new SuperMetroidRuntime(bus, HostGameplayOverrides.None, initialPaletteArt: roomAssets.InitialPalettes) { Artwork = { MapPresentation = maps } };
        roomAssets.Bind(runtime);
        runtime.Artwork.GrappleArtwork = grappleArtwork;
        runtime.Artwork.ChargeFlareCompositions = stock;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug); runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        var samus = runtime.Player.IntroducedSamus;
        samus.Pose = SamusPoseId.FacingRightNormalPose; samus.Position = samus.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
        samus.Presentation.InitializeAnimation(bus); samus.Presentation.PrimeGraphics(bus);
        samus.Grapple.Phase = GrapplePhase.ConnectedLocked;
        samus.Grapple.BeamStart = samus.Grapple.BeamStart.WithX(samus.Position.X.Whole); samus.Grapple.BeamStart = samus.Grapple.BeamStart.WithY(samus.Position.Y.Whole);
        samus.Grapple.FlareCounter = 2; samus.Grapple.FlareAnimationFrame = 16; samus.Grapple.FlareAnimationTimer = NativeCountdown.Start(100);
        var game = new SuperMetroidGame(bus);
        game.runtime = runtime;
        byte[] original = Draw(runtime);
        AssertTrue(runtime.ActorDrawing.LastGrappleFlareDrawn, "Real actor fixture reaches Grapple flare path");
        game.BindArtwork(game.Artwork with
        {
            ChargeFlareCompositions = stock,
        });
        AssertTrue(original.SequenceEqual(Draw(runtime)), "Real Grapple actor stock composition preserves OAM");
        game.BindArtwork(game.Artwork with
        {
            ChargeFlareCompositions = edited,
        });
        AssertTrue(!original.SequenceEqual(Draw(runtime)), "Real Grapple actor consumes current composition override");
        using var stream = new MemoryStream(SerializeGraph(game));
        var restored = SuperMetroid.Diagnostics.DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(stream);
        var restoredRuntime = restored.GameplayRuntime;
        roomAssets.Bind(restoredRuntime);
        restoredRuntime.Artwork.MapPresentation = maps;
        restoredRuntime.Artwork.GrappleArtwork = grappleArtwork;
        restoredRuntime.Player.IntroducedSamus.Presentation.TileTransfers.BindArtwork(body);
        restoredRuntime.Player.IntroducedSamus.ArmCannon.Artwork = body.ArmCannon;
        restored.BindArtwork(restored.Artwork with
        {
            ChargeFlareCompositions = edited,
        });
        AssertTrue(Draw(runtime).SequenceEqual(Draw(restoredRuntime)), "Restored Grapple actor uses current art at the saved timing state");
        static byte[] Draw(SuperMetroidRuntime target)
        {
            target.Oam.BeginFrame();
            target.ActorDrawing.DrawGameplayActors(false, false);
            return target.Oam.LowTable.ConcatToArray(target.Oam.HighTable).Concat(BitConverter.GetBytes(target.Oam.NextByteOffset)).ToArray();
        }
    }

    /// <summary>Forbids production reads from migrated flare cadence and spritemap-selector tables.</summary>
    private static CartridgeReadGuard NewGrappleFlarePresentationGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x90c481 and < 0x90c4b5 or >= 0x93a225 and < 0x93a231,
                address => $"Grapple flare still reads compiled cadence/selector at {address:X6}.");

    /// <summary>Reads one native facing-row selector offset to check both rows choose the shared main flare.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the native Grapple flare spritemap-offset rows")]
    private static ushort ReadNativeGrappleFlareSpritemapOffset(ISnesAddressSpace bus, int address) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
}
