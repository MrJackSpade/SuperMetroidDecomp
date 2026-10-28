using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Hardware;

/// <summary>Checks Mode 7 gameplay packets keep their registers, and a captured Ceres Ridley scene draws as the live frame did.</summary>
internal static class Mode7GameplaySnapshotTestsVerification
{
    /// <summary>Checks the band layer across matrix and scroll variants, then packet retention after live scene data changes.</summary>
    internal static void VerifyMode7GameplaySnapshots()
    {
        var random = new Random(32109);
        byte[] bytes = new byte[SnesVram.ByteCount]; random.NextBytes(bytes);
        byte[] oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount]; random.NextBytes(oam);
        ushort[] palette = Enumerable.Range(0, SnesCgram.ColorCount).Select(_ => (ushort)random.Next(32768)).ToArray();
        var memory = FramePaint.FromOam(bytes, ToColors(palette), oam, 128, 3);
        // The layer's pictures are drawn from the fixture's own VRAM, which the frame no longer carries (#1403).
        var fixtureVram = new SnesVram();
        fixtureVram.LoadBytes(0, bytes);
        int samples = 0;
        foreach (short matrix in new short[] { 0, 128, 256, -256, 511, short.MinValue })
        foreach (ushort scroll in new ushort[] { 0, 255, 511, ushort.MaxValue })
        {
            // The floor band below shows the room's layer-2 plane (#1403); the retail Ridley capture below draws it.
            var m = new AffineTransform(matrix, 17, -33, matrix, new Offset16<FloorPixels>(128, 96), new Offset16<FloorPixels>(-13, unchecked((short)scroll)));
            var layer = new GameplayFloorRenderLayer(m, Layer3CellsFixtures.FromVram(fixtureVram.Bytes, ToColors(palette)), AffineFloorFixtures.Floor(fixtureVram.Bytes, ToColors(palette)), ScreenGeometry.GameplayHudHeightPixels);
            var packet = new DisplayPacket(new(++samples, 1, 0), new LayeredScene(memory, new RenderLayer[] { layer }, 15));
            AssertEqual(layer, (GameplayFloorRenderLayer)(packet.Layers ?? throw new InvalidOperationException("Verification requires packet.Layers.")).Layers[0], "the Mode-7 band layer keeps all register fields");
            if (samples == 1)
            {
                AssertThrows<ArgumentException>(() => new LayeredScene(memory,
                    new RenderLayer[] { new SpriteRenderLayer(), layer }, 15), "Mode-7 base cannot erase earlier layers");
            }
        }
        // Retail room assets with explicitly constructed getaway registers exercise
        // the runtime producer, without pretending this is a controller-driven battle.
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug); runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.CeresRidley, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var getaway = runtime.Enemies.RidleyEncounter.CeresRidley;
        (getaway ?? throw new InvalidOperationException("Verification requires getaway.")).GetawayActive = true;
        getaway.FloorMatrixA = SignedEightEight.FromSignedRaw(240); getaway.FloorMatrixB = SignedEightEight.FromSignedRaw(33);
        getaway.FloorMatrixC = SignedEightEight.FromSignedRaw(-33); getaway.FloorMatrixD = SignedEightEight.FromSignedRaw(240);
        getaway.FloorCenter = new Position16<FloorPixels>(128, 120);
        getaway.FloorOffset = new Offset16<FloorPixels>(7, 19);
        // Room main's M7A..M7Y shadows reach the PPU at the accepted NMI (#1307).
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        RgbaPixel[] retail = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame());
        var retained = new DisplayPacket(new(100, 1, 0), GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()));
        AssertTrue(retail.SequenceEqual(RenderForComparison(retained)), "retail Ridley capture resolves mixed bands and haze");
        getaway.GetawayActive = false; getaway.FloorMatrixA = default;
        runtime.Vram.LoadBytes(0, new byte[SnesVram.ByteCount]);
        for (int i = 0; i < SnesCgram.ColorCount; i++) runtime.Colours.AsPaletteColors().SetColor(i, new RgbColor(0, 0, 0));
        AssertTrue(retail.SequenceEqual(RenderForComparison(retained)), "Ridley packet survives mode exit and live memory replacement");
        AssertThrows<ArgumentOutOfRangeException>(() => new GameplayFloorRenderLayer(default, RoomBackgroundsFixtures.BlankBg3(), RoomBackgroundsFixtures.BlankMode7Floor(), 31), "HUD must cover complete tile rows");
        AssertThrows<ArgumentOutOfRangeException>(() => new GameplayFloorRenderLayer(default, RoomBackgroundsFixtures.BlankBg3(), RoomBackgroundsFixtures.BlankMode7Floor(), 32,
            new Mode1FloorBand(31, new Position16<Bg2Pixels>(0, 0), new Extent16<TilemapCells>(64, 32), RoomBackgroundsFixtures.Blank().Bg1)),
            "floor cannot overlap HUD");
        Console.WriteLine($"  Mode-7 gameplay snapshots: {samples} matrix/scroll layers keep their registers; the Ridley packet is retained.");
    }
}
