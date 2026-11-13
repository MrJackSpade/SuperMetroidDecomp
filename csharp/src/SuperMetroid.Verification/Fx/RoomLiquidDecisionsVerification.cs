using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Verification.Fx;

/// <summary>
/// Checks the room liquid the import reads from a record's options byte (#1695) against the decisions the byte's bits make,
/// read here from the cartridge's bit layout independently of the import's codec: water's drift, background wave and
/// whether Samus ignores its surface, lava's and acid's heat haze, with the vertical haze winning, and every liquid's tide,
/// with the small tide winning. Effects without a liquid read none of the byte.
/// </summary>
internal static class RoomLiquidDecisionsVerification
{
    /// <summary>The options-byte bits no record may set: neither the import nor any liquid names them.</summary>
    private const int UnnamedBits = 0x30;

    /// <summary>
    /// Decodes every options byte for every effect: a byte setting an unnamed bit is refused, every other byte decodes to
    /// the liquid whose facts its bits select, and that liquid's own byte decodes back to it.
    /// </summary>
    internal static void VerifyRoomLiquidCodec()
    {
        foreach (RoomFxType type in Enum.GetValues<RoomFxType>())
        {
            for (int value = 0; value <= byte.MaxValue; value++)
            {
                byte options = (byte)value;
                string context = $"{type} options ${options:X2}";
                if ((options & UnnamedBits) != 0)
                {
                    AssertThrows<InvalidDataException>(() => RoomFxRecordCodec.Liquid(type, options), $"{context} is refused");
                    continue;
                }
                RoomLiquid? liquid = RoomFxRecordCodec.Liquid(type, options);
                AssertLiquidFacts(type, options, liquid, context);
                AssertEqual(liquid, RoomFxRecordCodec.Liquid(type, RoomFxRecordCodec.LiquidOptionsByte(liquid)),
                    $"{context} liquid's own byte reads back to it");
            }
        }
    }

    /// <summary>
    /// Checks a record's liquid against its cartridge options byte <paramref name="options"/>: its facts are the ones the
    /// byte's bits select, and the readers of the liquid (Samus's water physics, the tide, the background waves and heat
    /// hazes the display describes) decide as those bits do.
    /// </summary>
    internal static void AssertRecordLiquidDecisions(RoomFxRecordDefinition record, byte options, string context)
    {
        AssertLiquidFacts(record.Type, options, record.Liquid, context);
        if (record.Liquid is null)
            return;
        bool Bit(ushort option) => (options & option) != 0;
        bool water = RoomFxTypes.UsesWater(record.Type);
        bool lavaOrAcid = record.Type is RoomFxType.Lava or RoomFxType.Acid;
        bool vertical = Bit(RoomFxNativeSources.VerticalBg2WaveOption);
        bool bitTwo = Bit(RoomFxNativeSources.WaterPhysicsDisabledOrHorizontalHeatWaveOption);

        RoomFxOverlayState fx = RoomFxOverlayState.ForDefinition(new RoomFxSession(), record, randomNumber: 0);
        var samus = new SamusLiquidPhysicsState(new BreathBubbleAndLavaSprayWaits(), new SamusParticles());
        samus.ConfigureFor(fx);
        AssertEqual(water && bitTwo, samus.IgnoresWaterSurface, $"{context} Samus ignores the water's surface");

        RoomFxOverlayRenderSnapshot snapshot = fx.CaptureForDisplay()
            ?? throw new InvalidOperationException($"{context} liquid publishes no display snapshot.");
        AssertEqual(record.Type == RoomFxType.Water && vertical,
            GameplayEffectCapture.BuildWaterBackgroundHorizontalScrolls(snapshot with { WaterSurfaceScreenY = 100 }, default) is not null,
            $"{context} water background wave");
        AssertEqual(lavaOrAcid && !vertical && bitTwo,
            GameplayEffectCapture.BuildLavaAcidBackgroundHorizontalScrolls(snapshot, default) is not null,
            $"{context} horizontal heat haze");
        AssertEqual(lavaOrAcid && vertical,
            GameplayEffectCapture.BuildLavaAcidBackgroundVerticalScrolls(snapshot, 0) is not null,
            $"{context} vertical heat haze");

        int scale = Bit(RoomFxNativeSources.SmallTideOption) ? LiquidTideDefinitions.SmallTideScale
            : Bit(RoomFxNativeSources.LargeTideOption) ? LiquidTideDefinitions.LargeTideScale
            : 0;
        fx.TidePhase = RotationAngle.Zero;
        fx.StepLiquidTide();
        int sample = Trigonometry.SignedSine((RotationAngle.Zero - RotationAngle.QuarterTurn).ToTrigonometryAngle());
        AssertEqual(SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(sample * scale), fx.TideOffset, $"{context} tide reach");
    }

    /// <summary>Checks that <paramref name="liquid"/> holds the facts <paramref name="options"/>' bits select for an effect of <paramref name="type"/>.</summary>
    private static void AssertLiquidFacts(RoomFxType type, byte options, RoomLiquid? liquid, string context)
    {
        bool Bit(ushort option) => (options & option) != 0;
        LiquidTide tide = Bit(RoomFxNativeSources.SmallTideOption) ? LiquidTide.Small
            : Bit(RoomFxNativeSources.LargeTideOption) ? LiquidTide.Large
            : LiquidTide.None;
        bool vertical = Bit(RoomFxNativeSources.VerticalBg2WaveOption);
        bool bitTwo = Bit(RoomFxNativeSources.WaterPhysicsDisabledOrHorizontalHeatWaveOption);
        if (RoomFxTypes.UsesWater(type))
        {
            AssertEqual(new RoomWater(tide, Bit(RoomFxNativeSources.SurfaceDriftOption), vertical, bitTwo), liquid, $"{context} water");
            return;
        }
        if (type is RoomFxType.Lava or RoomFxType.Acid)
        {
            HeatHazeDirection haze = vertical ? HeatHazeDirection.Vertical : bitTwo ? HeatHazeDirection.Horizontal : HeatHazeDirection.None;
            AssertEqual(new RoomLavaOrAcid(tide, haze), liquid, $"{context} lava or acid");
            return;
        }
        AssertTrue(liquid is null, $"{context} has no liquid");
    }
}
