using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Fx;

/// <summary>
/// Checks water's surface drift and rain's wind and fall through the production layer-3 step: each frame's BG3 scroll
/// is the signed whole pixels of an 8.8 accumulator that wraps at sixteen bits, sampled before that frame's velocity.
/// </summary>
internal static class RoomFxTextureDriftVerification
{
    /// <summary>Steps water and rain past their accumulators' sixteen-bit wrap and compares every frame's BG3 scroll.</summary>
    internal static void VerifyRoomFxTextureDrift()
    {
        // 1100 quarter-pixel steps carry water's accumulator through its sign change and past its wrap.
        RoomFxOverlayState water = Load(RoomFxType.Water, RoomEffectBlendDefinitions.OverlayBrightens,
            new RoomWater(LiquidTide.None, SurfaceDrift: true, BackgroundWave: false, IgnoresSurfaceUntilEnabled: false), randomNumber: 0);
        ushort expectedWater = 0;
        for (int frame = 0; frame < 1100; frame++)
        {
            water.Step(new ArtworkFrameSelections(), Frame());
            AssertEqual(WholePixels(expectedWater), water.Scroll.X, $"water drift frame {frame}");
            expectedWater = unchecked((ushort)(expectedWater + WaterWaveDefinitions.HorizontalSubscrollVelocity));
        }

        RoomFxOverlayState stillWater = Load(RoomFxType.Water, RoomEffectBlendDefinitions.OverlayBrightens,
            new RoomWater(LiquidTide.None, SurfaceDrift: false, BackgroundWave: false, IgnoresSurfaceUntilEnabled: false), randomNumber: 0);
        for (int frame = 0; frame < 600; frame++)
        {
            stillWater.Step(new ArtworkFrameSelections(), Frame());
            AssertEqual((ushort)0, stillWater.Scroll.X, $"water without its drift option stays put, frame {frame}");
        }

        // Each of the four wind speeds the random number selects, through enough six-pixel falls to wrap.
        for (int selection = 0; selection < 4; selection++)
        {
            RoomFxOverlayState rain = Load(RoomFxType.Rain, RoomEffectBlendDefinitions.OverlayBrightens,
                liquid: null, randomNumber: selection * 4);
            ushort expectedX = 0;
            ushort expectedY = 0;
            for (int frame = 0; frame < 200; frame++)
            {
                rain.Step(new ArtworkFrameSelections(), Frame());
                AssertEqual(new Position16<OverlayPixels>(WholePixels(expectedX), WholePixels(expectedY)), rain.Scroll,
                    $"rain wind {selection} frame {frame}");
                expectedX = unchecked((ushort)(expectedX + AtmosphereDrift.RainHorizontalVelocity(AtmosphereDrift.RainWindChosenBy(selection * 4))));
                expectedY = unchecked((ushort)(expectedY - AtmosphereDriftDefinitions.RainVerticalVelocity));
            }
        }

        // An accumulator's whole pixels: its signed high byte, sign-extended into the scroll word.
        static ushort WholePixels(ushort accumulator) => unchecked((ushort)(sbyte)LittleEndianBytes.HighByte(accumulator));

        static RoomFxOverlayFrame Frame() =>
            new() { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.None };
    }

    /// <summary>A room-FX owner loaded from a synthetic record of <paramref name="type"/>.</summary>
    private static RoomFxOverlayState Load(RoomFxType type, RoomEffectBlend blend, RoomLiquid? liquid,
        int randomNumber)
    {
        var stateSession = new RoomFxSession();
        RoomFxOverlayState state;
        var record = new RoomFxRecordDefinition(BaseYPosition: 100, TargetYPosition: 100,
            YVelocity: default, Timer: 0, Type: type, VisorAnimation: RoomVisorAnimation.Steady, EffectBlend: blend,
            Liquid: liquid, PaletteEffects: [], AnimatedTiles: [],
            PaletteBlend: RoomFxPaletteBlend.None);
        state = RoomFxOverlayState.ForDefinition(stateSession, record, checked((ushort)randomNumber));
        AssertEqual(type, state.Type, $"synthetic {type} record loads");
        return state;
    }

    /// <summary>Runs the room-FX texture drift check.</summary>
    [VerificationCommand("--room-fx-texture-drift")]
    internal static int RoomFxTextureDriftCommand()
    {
        Suite(nameof(VerifyRoomFxTextureDrift), () => VerifyRoomFxTextureDrift());
        return 0;
    }
}
