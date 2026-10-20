using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;

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
        RoomLayer3FxState water = Load(RoomFxType.Water, LayerBlendingConfiguration.LiquidOrFogAdditive,
            RoomLiquidOptions.SurfaceDrift, randomNumber: 0);
        ushort expectedWater = 0;
        for (int frame = 0; frame < 1100; frame++)
        {
            water.Step(BgArtworkFixtures.For(new SnesVram()), Frame());
            AssertEqual(WholePixels(expectedWater), water.Scroll.X, $"water drift frame {frame}");
            expectedWater = unchecked((ushort)(expectedWater + RoomFxRomData.Water.HorizontalSubscrollVelocity));
        }

        RoomLayer3FxState stillWater = Load(RoomFxType.Water, LayerBlendingConfiguration.LiquidOrFogAdditive,
            RoomLiquidOptions.None, randomNumber: 0);
        for (int frame = 0; frame < 600; frame++)
        {
            stillWater.Step(BgArtworkFixtures.For(new SnesVram()), Frame());
            AssertEqual((ushort)0, stillWater.Scroll.X, $"water without its drift option stays put, frame {frame}");
        }

        // Each of the four wind speeds the random number selects, through enough six-pixel falls to wrap.
        for (int selection = 0; selection < 4; selection++)
        {
            RoomLayer3FxState rain = Load(RoomFxType.Rain, LayerBlendingConfiguration.Rain,
                RoomLiquidOptions.None, randomNumber: selection * 4);
            ushort expectedX = 0;
            ushort expectedY = 0;
            for (int frame = 0; frame < 200; frame++)
            {
                rain.Step(BgArtworkFixtures.For(new SnesVram()), Frame());
                AssertEqual(new Position16<Bg3Pixels>(WholePixels(expectedX), WholePixels(expectedY)), rain.Scroll,
                    $"rain wind {selection} frame {frame}");
                expectedX = unchecked((ushort)(expectedX + RoomFxRomData.Rain.HorizontalVelocity(selection)));
                expectedY = unchecked((ushort)(expectedY - RoomFxRomData.Rain.VerticalVelocity));
            }
        }

        // An accumulator's whole pixels: its signed high byte, sign-extended into the scroll word.
        static ushort WholePixels(ushort accumulator) => unchecked((ushort)(sbyte)SnesLittleEndian.HighByte(accumulator));

        static RoomLayer3FxFrame Frame() =>
            new() { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, MainGameLoopCarry = false };
    }

    /// <summary>A room-FX owner loaded from a synthetic record of <paramref name="type"/> with the installed presentation.</summary>
    private static RoomLayer3FxState Load(RoomFxType type, LayerBlendingConfiguration blend, RoomLiquidOptions options,
        int randomNumber)
    {
        var presentation = RetailPresentationFixture();
        var stateSession = new RoomFxSession{
            Layer3Tilemaps = presentation.RoomFxLayer3Tilemaps,
            PaletteBlendColors = presentation.RoomFxPaletteBlends,
            AnimatedTileArtwork = presentation.RoomFxAnimatedTiles,
        };
        RoomLayer3FxState state;
        var record = new RoomFxRecordDefinition(Door: null, Terminates: false, BaseYPosition: 100, TargetYPosition: 100,
            PackedYVelocity: 0, Timer: 0, Type: (byte)type, DefaultLayerBlend: (byte)blend, Layer3LayerBlend: (byte)blend,
            LiquidOptions: checked((byte)options.ToNativeWord()), PaletteFxBitset: 0, AnimatedTileBitset: 0, PaletteBlend: 0);
        state = RoomLayer3FxState.ForDefinition(stateSession, new SnesCgram(), record, checked((ushort)randomNumber));
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
