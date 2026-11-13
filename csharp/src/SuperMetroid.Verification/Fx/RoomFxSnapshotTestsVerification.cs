using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Fx;
using SuperMetroid.TestSupport.FrameRendering;

using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Fx;

/// <summary>Checks captured room FX color math keeps the HUD band and owns its register rows.</summary>
internal static class RoomFxSnapshotTestsVerification
{
    /// <summary>Draws liquid and weather effects while checking HUD exclusion and scroll ownership.</summary>
    internal static void VerifyRoomFxSnapshots()
    {
        var random = new Random(32108);
        var vram = new SnesVram();
        byte[] bytes = new byte[SnesVram.ByteCount]; random.NextBytes(bytes); vram.LoadBytes(0, bytes);
        var cgram = new SnesCgram();
        for (int i = 0; i < SnesCgram.ColorCount; i++) cgram.SetColor(i, RgbColor.FromWord(checked((ushort)random.Next(32768))));
        var oam = new OamBuffer(); oam.BeginFrame(); oam.FinalizeFrame();
        var memory = FramePaintConstants.CaptureGameplay(vram, cgram, oam, 3);
        RgbaPixel[] baseline = DisplayRendering.Render(new LayeredScene(memory, [], 15));
        int samples = 0;
        var configurations = new[]
        {
            new Configuration(RoomFxType.Water, RoomEffectBlendDefinitions.OverlayDarkens),
            new Configuration(RoomFxType.Water, RoomEffectBlendDefinitions.WaterfallDarkensBehind),
            new Configuration(RoomFxType.Water, RoomEffectBlendDefinitions.OverlayBrightens),
            new Configuration(RoomFxType.Lava, RoomEffectBlendDefinitions.OverlayBrightens),
            new Configuration(RoomFxType.Acid, RoomEffectBlendDefinitions.OverlayBrightens),
            new Configuration(RoomFxType.Rain, RoomEffectBlendDefinitions.OverlayBrightens),
            new Configuration(RoomFxType.Fog, RoomEffectBlendDefinitions.OverlayBrightens),
        };
        // Each effect's artwork shows the fixture's random VRAM (#1348, #1514, #1706); its seeded rows repeat nowhere, so a
        // liquid is cut where water's transfer is.
        Dictionary<RoomFxType, FxOverlayArtwork> artworks = configurations.Select(configuration => configuration.First).Distinct()
            .ToDictionary(type => type, type => FxOverlayFixtures.ArtworkFromVram(type, vram.Bytes, cgram.Colors,
                liquidCut: new LiquidTransferRows(BodyFirstRow: 5, BodyRows: 4)));
        foreach ((RoomFxType type, RoomEffectBlend blend) in configurations)
        foreach (short surface in new short[] { -20, 0, 32, 100, 223, 300 })
        foreach (int phase in new[] { 0, 7, 15 })
        foreach (int? currentY in new int?[] { 400, null })
        {
            var fx = new RoomFxOverlayRenderSnapshot(type, blend, new Position16<OverlayPixels>(65530, 511), currentY,
                WaterOverlayWavePhase: phase, WaterSurfaceScreenY: surface) { LiquidSurfaceLine = surface };
            OverlayBlendLayer? layer = GameplayEffectCapture.CaptureRoomFxOverlay(fx, artworks[type]);
            samples++;
            var packet = new DisplayPacket(0,
                new LayeredScene(memory, layer is null ? [] : new DisplayLayer[] { layer }, 15));
            RgbaPixel[] actual = DisplayRendering.Render(packet);
            AssertTrue(actual.AsSpan(0, 256 * 32).SequenceEqual(baseline.AsSpan(0, 256 * 32)),
                $"room FX {type}/{blend.Operation} at {surface}, phase {phase}, Y {currentY?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "none"} preserves HUD band");
        }
        var owned = new OverlayBlendLayer(null, 32, BlendOperation.Add, ScrollBands.Uniform(0));
        AssertThrows<ArgumentNullException>(() => new OverlayBlendLayer(null, 32,
            BlendOperation.Add, null!), "BG math requires its described line scroll");
        AssertThrows<ArgumentOutOfRangeException>(() => new OverlayBlendLayer(null, 32,
            (BlendOperation)255, ScrollBands.Uniform(0)), "BG math rejects unknown equation");
        var encoded = new DisplayPacket(0, new LayeredScene(memory, [owned], 15));
        AssertThrows<InvalidDataException>(() => GameplayEffectCapture.CaptureRoomFxOverlay(
            new(RoomFxType.Water, null, Position16<OverlayPixels>.Zero, null), artworks[RoomFxType.Water]), "capture rejects water that describes no blend");
        Console.WriteLine($"  Room FX snapshots: {samples} water/lava/acid/rain/fog cases, HUD exclusion and ownership agree.");
    }
}
