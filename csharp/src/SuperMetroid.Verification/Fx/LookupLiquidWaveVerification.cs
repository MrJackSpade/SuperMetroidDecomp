using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks compiled water and heat wave tables through their rendering consumers.</summary>
internal static class LookupLiquidWaveVerification
{
    /// <summary>Compares 16 retail displacements and signed scroll wrap across phases and camera offsets.</summary>
    internal static void VerifyMirroredLiquidWave(SuperMetroidAddressSpace rom)
    {
        short Water(int index) => unchecked((short)ReadVerificationWord(rom, 0x88c46e + 2 * index));
        short Heat(int index) => unchecked((short)ReadVerificationWord(rom, 0x88b60a + 2 * index));
        for (int index = 0; index < 16; index++)
        {
            AssertEqual(Water(index), LiquidEffects.WaterWaveDisplacement(index), "Original water displacement");
            AssertEqual(Heat(index), LiquidEffects.HeatVerticalWaveDisplacement(index), "Original heat displacement");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => LiquidEffects.WaterWaveDisplacement(invalid),
                "Water retains original sixteen-word bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => LiquidEffects.HeatVerticalWaveDisplacement(invalid),
                "Heat alias retains original sixteen-word bounds");
        }

        // Confirm actual projection consumers over every circular phase and camera
        // low nibble, with signed-displacement overflow at both ushort boundaries.
        for (int phase = 0; phase < 16; phase++)
        foreach (ushort baseScroll in new ushort[] { 0, ushort.MaxValue })
        {
            var water = new RoomLayer3FxRenderSnapshot(RoomFxType.Water,
                LayerBlendingConfiguration.WaterSubtractive, new Position16<OverlayPixels>(baseScroll, 0),
                Surface: new(0, 100), LiquidOptions: RoomLiquidOptions.Bg2Wave, WaterBg3WavePhase: phase,
                WaterBg2WavePhase: phase, WaterSurfaceScreenY: 100);
            var captured = GameplayEffectCapture.CaptureRoomLayer3Fx(water, RoomBackgroundsFixtures.BlankBg3());
            for (int y = DisplayScreen.HudHeight; y < DisplayScreen.Height; y++)
            {
                int offset = y > 100 ? Water((y - 101 - phase + 16) % 16) : 0;
                AssertEqual(unchecked((ushort)(baseScroll + offset)), ScrollShapeLines.Scroll((captured ?? throw new InvalidOperationException("Verification requires captured.")).Horizontal, y),
                    "Water BG3 phase subtraction and signed addition");
            }
            for (int cameraPhase = 0; cameraPhase < 16; cameraPhase++)
            {
                ushort cameraY = (ushort)((baseScroll & 0xfff0) | cameraPhase);
                // The described waves (#1468), evaluated on each gameplay line.
                ushort[] bg2 = ScrollShapeLines.GameplayScrolls(GameplayEffectCapture.BuildWaterBackgroundHorizontalScrolls(water,
                    new Position16<BackgroundPixels>(baseScroll, cameraY)) ?? throw new InvalidOperationException("Verification requires GameplayEffectCapture.BuildWaterBg2HorizontalScrolls(water, new Position16<Bg2Pixel...."));
                var heat = water with { Type = RoomFxType.Lava, LavaAcidBg2WavePhase = phase };
                ushort[] heatScroll = ScrollShapeLines.GameplayScrolls(GameplayEffectCapture.BuildLavaAcidBackgroundVerticalScrolls(heat, cameraY) ?? throw new InvalidOperationException("Verification requires GameplayEffectCapture.BuildLavaAcidBg2VerticalScrolls(heat, cameraY)."));
                for (int line = 0; line < bg2.Length; line++)
                {
                    int y = line + DisplayScreen.HudHeight;
                    int waterOffset = y > 100 ? Water((phase + cameraPhase + y - 101) & 15) : 0;
                    AssertEqual(unchecked((ushort)(baseScroll + waterOffset)), bg2[line],
                        "Water BG2 phase addition, surface boundary and signed wrap");
                    AssertEqual(unchecked((ushort)(cameraY + Heat((phase + cameraPhase + line) & 15))),
                        (heatScroll)[line], "Vertical heat HDMA phase and signed wrap");
                }
            }
        }
    }
}
