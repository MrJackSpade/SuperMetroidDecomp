using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Fx;

/// <summary>
/// Checks water's surface drift and rain's wind and fall through the production effect overlay step (#2141): each frame's
/// overlay scroll is the whole pixels the texture has travelled before that frame's velocity, kept within the overlay
/// pictures' repeat, and every frame shows the picture the signed 8.8 running sum it replaced showed.
/// </summary>
internal static class RoomFxTextureDriftVerification
{
    /// <summary>Steps water and rain past several repeats of their pictures and compares every frame's overlay scroll.</summary>
    internal static void VerifyRoomFxTextureDrift()
    {
        // 1100 quarter-pixel steps carry water's drift past one whole repeat and past where the 8.8 sum changed sign.
        RoomFxOverlayState water = Load(RoomFxType.Water, RoomEffectBlendDefinitions.OverlayBrightens,
            new RoomWater(LiquidTide.None, SurfaceDrift: true, BackgroundWave: false, IgnoresSurfaceUntilEnabled: false), randomNumber: 0);
        int travelledWater = 0;
        for (int frame = 0; frame < 1100; frame++)
        {
            water.Step(new ArtworkFrameSelections(), Frame());
            AssertShows(travelledWater, water.Scroll.X, $"water drift frame {frame}");
            travelledWater += WaterWaveDefinitions.SurfaceDriftSpeed.InTwoHundredFiftySixthsOfPixel;
        }
        AssertTrue(travelledWater / 256 > TextureDrift.RepeatPixels, "water's drift travels past a whole repeat");

        RoomFxOverlayState stillWater = Load(RoomFxType.Water, RoomEffectBlendDefinitions.OverlayBrightens,
            new RoomWater(LiquidTide.None, SurfaceDrift: false, BackgroundWave: false, IgnoresSurfaceUntilEnabled: false), randomNumber: 0);
        for (int frame = 0; frame < 600; frame++)
        {
            stillWater.Step(new ArtworkFrameSelections(), Frame());
            AssertEqual((ushort)0, stillWater.Scroll.X, $"water without its drift option stays put, frame {frame}");
        }

        // Each of the four wind speeds the random number selects, through enough six-pixel falls to pass several repeats.
        for (int selection = 0; selection < 4; selection++)
        {
            RoomFxOverlayState rain = Load(RoomFxType.Rain, RoomEffectBlendDefinitions.OverlayBrightens,
                liquid: null, randomNumber: selection * 4);
            int travelledX = 0;
            int travelledY = 0;
            for (int frame = 0; frame < 200; frame++)
            {
                rain.Step(new ArtworkFrameSelections(), Frame());
                AssertShows(travelledX, rain.Scroll.X, $"rain wind {selection} frame {frame} across");
                AssertShows(travelledY, rain.Scroll.Y, $"rain wind {selection} frame {frame} down");
                travelledX += AtmosphereDrift.RainHorizontalVelocity(AtmosphereDrift.RainWindChosenBy(selection * 4)).InTwoHundredFiftySixthsOfPixel;
                travelledY -= AtmosphereDriftDefinitions.RainFallSpeed.InTwoHundredFiftySixthsOfPixel;
            }
            AssertTrue(-travelledY / 256 > 2 * TextureDrift.RepeatPixels, "rain's fall travels past several repeats");
        }

        // With the camera at the room's corner the scroll is the drift alone: the whole pixels travelled, within one
        // repeat. A picture whose size divides the repeat shows at that scroll what it showed at the signed whole pixels
        // of the 8.8 running sum the drift replaced, which continued from its other end past either end of its range.
        static void AssertShows(int travelled, ushort scroll, string context)
        {
            int whole = travelled >> 8;
            AssertEqual((whole % TextureDrift.RepeatPixels + TextureDrift.RepeatPixels) % TextureDrift.RepeatPixels, (int)scroll, context);
            int replacedSum = unchecked((short)travelled);
            AssertEqual(0, (scroll - (replacedSum >> 8)) % TextureDrift.RepeatPixels, $"{context}: the replaced sum's picture");
        }

        static RoomFxOverlayFrame Frame() =>
            new() { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false };
    }

    /// <summary>A room-FX owner loaded from a synthetic record of <paramref name="type"/>.</summary>
    private static RoomFxOverlayState Load(RoomFxType type, RoomEffectBlend blend, RoomLiquid? liquid,
        int randomNumber)
    {
        RoomFxOverlayState state;
        var record = new RoomEffectDefinition(LiquidStartLevel: 100, LiquidTargetLevel: 100,
            LiquidLevelSpeed: default, LiquidMoveDelay: 0, Type: type, VisorAnimation: RoomVisorAnimation.Steady, EffectBlend: blend,
            Liquid: liquid, ColourEffects: [], BlockAnimations: [],
            Tint: RoomEffectTint.None);
        state = RoomFxOverlayState.ForDefinition(record, checked((ushort)randomNumber));
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
