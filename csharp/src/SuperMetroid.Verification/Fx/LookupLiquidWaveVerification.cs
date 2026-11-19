using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks compiled water and heat wave tables through their rendering consumers.</summary>
internal static class LookupLiquidWaveVerification
{
    /// <summary>Compares 16 retail displacements and signed scroll wrap across phases and camera offsets.</summary>
    internal static void VerifyMirroredLiquidWave(CartridgeImportAddressSpace rom)
    {
        short Water(int index) => unchecked((short)ReadVerificationWord(rom, 0x88c46e + 2 * index));
        short Heat(int index) => unchecked((short)ReadVerificationWord(rom, 0x88b60a + 2 * index));
        // Each wave's waveform is authored as its first phase shows it (#2367): the surface wave's first phase draws the
        // cartridge's water samples one line later, its background wave's one line earlier, and the vertical haze's as
        // they lie.
        for (int index = 0; index < 16; index++)
        {
            AssertEqual(Water((index + 15) & 15), WaterWaveDefinitions.SurfaceWaveform[index], "Original water displacement, surface wave");
            AssertEqual(Water((index + 1) & 15), WaterWaveDefinitions.BackgroundWaveform[index], "Original water displacement, background wave");
            AssertEqual(Heat(index), LavaAcidWaveDefinitions.HazeWaveforms[HeatHazeDirection.Vertical][index], "Original vertical heat displacement");
        }
        AssertEqual(16, WaterWaveDefinitions.SurfaceWaveform.Length, "Water retains original sixteen-word span");

        // Confirm actual projection consumers over every circular phase and camera
        // low nibble, with signed-displacement overflow at both ushort boundaries.
        // The overlay's pictures do not move its lines; any water's artwork shows where they lie.
        FxOverlayArtwork seededWater = FxOverlayFixtures.SeededLiquid(RoomFxType.Water);
        for (int phase = 0; phase < 16; phase++)
        foreach (ushort baseScroll in new ushort[] { 0, ushort.MaxValue })
        {
            var water = new RoomFxOverlayRenderSnapshot(RoomFxType.Water,
                RoomEffectBlendDefinitions.OverlayDarkens, new Position16<OverlayPixels>(baseScroll, 0),
                Surface: 100, Liquid: new RoomWater(LiquidTide.None, SurfaceDrift: false, BackgroundWave: true, IgnoresSurfaceUntilEnabled: false), WaterOverlayWavePhase: phase,
                WaterBackgroundWavePhase: phase, WaterSurfaceScreenY: 100);
            var captured = GameplayEffectCapture.CaptureRoomFxOverlay(FxOverlayFixtures.Overlay(water, seededWater));
            for (int y = ScreenGeometry.GameplayHudHeightPixels; y < ScreenGeometry.ScreenHeightPixels; y++)
            {
                // The surface wave's phase counts from its first, which shows the cartridge's second.
                int offset = y > 100 ? Water((y - 101 - (phase + 1) + 32) % 16) : 0;
                AssertEqual(unchecked((ushort)(baseScroll + offset)), ScrollShapeLines.Scroll((captured ?? throw new InvalidOperationException("Verification requires captured.")).Horizontal, y),
                    "Water BG3 phase subtraction and signed addition");
            }
            for (int cameraPhase = 0; cameraPhase < 16; cameraPhase++)
            {
                ushort cameraY = (ushort)((baseScroll & 0xfff0) | cameraPhase);
                // The described waves (#1468), evaluated on each gameplay line.
                ushort[] bg2 = ScrollShapeLines.GameplayScrolls(GameplayEffectCapture.BuildWaterBackgroundHorizontalScrolls(water,
                    Offset16<BackgroundPixels>.Wrapped(baseScroll, cameraY)) ?? throw new InvalidOperationException("Verification requires GameplayEffectCapture.BuildWaterBg2HorizontalScrolls(water, new Position16<Bg2Pixel...."));
                var heat = water with { Type = RoomFxType.Lava, Liquid = new RoomLavaOrAcid(LiquidTide.None, HeatHazeDirection.Vertical), LavaAcidBackgroundWavePhase = phase };
                ushort[] heatScroll = ScrollShapeLines.GameplayScrolls(GameplayEffectCapture.BuildLavaAcidBackgroundVerticalScrolls(heat, cameraY) ?? throw new InvalidOperationException("Verification requires GameplayEffectCapture.BuildLavaAcidBg2VerticalScrolls(heat, cameraY)."));
                for (int line = 0; line < bg2.Length; line++)
                {
                    int y = line + ScreenGeometry.GameplayHudHeightPixels;
                    // The background wave's phase counts from its first, which shows the cartridge's second.
                    int waterOffset = y > 100 ? Water((phase + 1 + cameraPhase + y - 101) & 15) : 0;
                    AssertEqual(unchecked((ushort)(baseScroll + waterOffset)), bg2[line],
                        "Water BG2 phase addition, surface boundary and signed wrap");
                    // The haze moves about the row the screen's top shows, one below the camera's (#1514).
                    AssertEqual(unchecked((ushort)(GameplaySceneView.LayerRowAtScreenTop(cameraY) + Heat((cameraPhase + line - phase) & 15))),
                        (heatScroll)[line], "Vertical heat phase drawn back and signed wrap");
                }
            }
        }
    }
}
