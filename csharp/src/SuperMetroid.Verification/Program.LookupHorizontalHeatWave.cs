using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyHorizontalHeatWave(SuperMetroidAddressSpace rom)
    {
        short Original(int index) => unchecked((short)ReadVerificationWord(rom, 0x88b589 + 2 * index));
        for (int index = 0; index < 16; index++)
            AssertEqual(Original(index), RoomFxRomData.LavaAcid.HorizontalWaveDisplacement(index),
                "Original horizontal heat wave including unequal zero gaps");
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => RoomFxRomData.LavaAcid.HorizontalWaveDisplacement(invalid),
                "Horizontal wave retains original span bounds");

        for (int phase = 0; phase < 16; phase++)
        for (int cameraPhase = 0; cameraPhase < 16; cameraPhase++)
        foreach (ushort baseScroll in new ushort[] { 0, ushort.MaxValue })
        {
            var fx = new RoomLayer3FxRenderSnapshot(RoomFxType.Lava,
                LayerBlendingConfiguration.WaterSubtractive, baseScroll, 0,
                CurrentYPosition: 100, LiquidOptions: RoomFxRomData.LavaAcid.HorizontalBg2WaveOption)
                { LavaAcidBg2WavePhase = phase };
            ushort[] scrolls = SnesGameplayFrameRenderer.BuildLavaAcidBg2HorizontalScrolls(fx,
                baseScroll, (ushort)cameraPhase)!;
            for (int line = 0; line < scrolls.Length; line++)
                AssertEqual(unchecked((ushort)(baseScroll + Original((phase + cameraPhase + line) & 15))),
                    scrolls[line], "Production horizontal wave phase, camera nibble, signed addition and wrap");
        }
    }
}
