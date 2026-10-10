using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    /// <summary>Checks native water and heat wave tables and validates their captured scroll projections across all phases, camera low nibbles, and 16-bit wraparound.</summary>
    /// <param name="rom">Cartridge address space containing the original water and heat displacement tables.</param>
    private static void VerifyMirroredLiquidWave(SuperMetroidAddressSpace rom)
    {
        short Water(int index) => unchecked((short)ReadVerificationWord(rom, 0x88c46e + 2 * index));
        short Heat(int index) => unchecked((short)ReadVerificationWord(rom, 0x88b60a + 2 * index));
        for (int index = 0; index < 16; index++)
        {
            AssertEqual(Water(index), RoomFxRomData.Water.WaveDisplacement(index), "Original water displacement");
            AssertEqual(Heat(index), RoomFxRomData.LavaAcid.VerticalWaveDisplacement(index), "Original heat displacement");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => RoomFxRomData.Water.WaveDisplacement(invalid),
                "Water retains original sixteen-word bounds");
            AssertThrows<IndexOutOfRangeException>(() => RoomFxRomData.LavaAcid.VerticalWaveDisplacement(invalid),
                "Heat alias retains original sixteen-word bounds");
        }

        // Confirm actual projection consumers over every circular phase and camera
        // low nibble, with signed-displacement overflow at both ushort boundaries.
        for (int phase = 0; phase < 16; phase++)
        foreach (ushort baseScroll in new ushort[] { 0, ushort.MaxValue })
        {
            var water = new RoomLayer3FxRenderSnapshot(RoomFxType.Water,
                LayerBlendingConfiguration.WaterSubtractive, baseScroll, 0,
                CurrentYPosition: 100, LiquidOptions: 2, WaterBg3WavePhase: phase,
                WaterBg2WavePhase: phase, WaterSurfaceScreenY: 100);
            var captured = SnesGameplayFrameRenderer.CaptureRoomLayer3Fx(water)!;
            for (int y = SnesGameplayFrameRenderer.HudHeight; y < SnesGameplayFrameRenderer.Height; y++)
            {
                int offset = y > 100 ? Water((y - 101 - phase + 16) % 16) : 0;
                AssertEqual(unchecked((ushort)(baseScroll + offset)), captured.Scrolls[y].X,
                    "Water BG3 phase subtraction and signed addition");
            }
            for (int cameraPhase = 0; cameraPhase < 16; cameraPhase++)
            {
                ushort cameraY = (ushort)((baseScroll & 0xfff0) | cameraPhase);
                ushort[] bg2 = SnesGameplayFrameRenderer.BuildWaterBg2HorizontalScrolls(water,
                    baseScroll, cameraY)!;
                var heat = water with { Type = RoomFxType.Lava, LavaAcidBg2WavePhase = phase };
                ushort[] heatScroll = SnesGameplayFrameRenderer.BuildLavaAcidBg2VerticalScrolls(heat, cameraY)!;
                for (int line = 0; line < bg2.Length; line++)
                {
                    int y = line + SnesGameplayFrameRenderer.HudHeight;
                    int waterOffset = y > 100 ? Water((phase + cameraPhase + y - 101) & 15) : 0;
                    AssertEqual(unchecked((ushort)(baseScroll + waterOffset)), bg2[line],
                        "Water BG2 phase addition, surface boundary and signed wrap");
                    AssertEqual(unchecked((ushort)(cameraY + Heat((phase + cameraPhase + line) & 15))),
                        heatScroll[line], "Vertical heat HDMA phase and signed wrap");
                }
            }
        }
    }
}
