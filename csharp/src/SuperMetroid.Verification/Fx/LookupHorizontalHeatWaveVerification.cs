using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks horizontal lava/acid wave samples and their renderer phase arithmetic.</summary>
internal static class LookupHorizontalHeatWaveVerification
{
    /// <summary>Compares 16 retail offsets, bounds rejection, camera-phase addition, and 16-bit scroll wrap.</summary>
    internal static void VerifyHorizontalHeatWave(SuperMetroidAddressSpace rom)
    {
        short Original(int index) => unchecked((short)ReadVerificationWord(rom, 0x88b589 + 2 * index));
        for (int index = 0; index < 16; index++)
            AssertEqual(Original(index), RoomFxRomData.LavaAcid.HorizontalWaveDisplacement(index),
                "Original horizontal heat wave including unequal zero gaps");
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RoomFxRomData.LavaAcid.HorizontalWaveDisplacement(invalid),
                "Horizontal wave retains original span bounds");

        for (int phase = 0; phase < 16; phase++)
        for (int cameraPhase = 0; cameraPhase < 16; cameraPhase++)
        foreach (ushort baseScroll in new ushort[] { 0, ushort.MaxValue })
        {
            var fx = new RoomLayer3FxRenderSnapshot(RoomFxType.Lava,
                LayerBlendingConfiguration.WaterSubtractive, new Position16<Bg3Pixels>(baseScroll, 0),
                Surface: new(0, 100), LiquidOptions: RoomLiquidOptions.WaterPhysicsDisabledOrHorizontalHeatWave)
                { LavaAcidBg2WavePhase = phase };
            ushort[] scrolls = (GameplayEffectCapture.BuildLavaAcidBg2HorizontalScrolls(fx,
                new Position16<Bg2Pixels>(baseScroll, (ushort)cameraPhase)) ?? throw new InvalidOperationException("Verification requires GameplayEffectCapture.BuildLavaAcidBg2HorizontalScrolls(fx, new Position16<Bg2Pixel...."));
            for (int line = 0; line < scrolls.Length; line++)
                AssertEqual(unchecked((ushort)(baseScroll + Original((phase + cameraPhase + line) & 15))),
                    scrolls[line], "Production horizontal wave phase, camera nibble, signed addition and wrap");
        }
    }
}
