using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Fx;

using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the room-FX and palette-FX inputs that hold a copy of a game value as a number (#1627): a random number accepts
/// every value of the shared random sequence, the frame counter every count of the long frame counter, and each rejects a
/// value its source never holds instead of wrapping; Samus's height is her position's.
/// </summary>
internal static class RoomFxWidenedInputsVerification
{
    /// <summary>Checks the range contract of every widened room-FX and palette-FX input.</summary>
    internal static void VerifyRoomFxWidenedInputs()
    {
        // Samus's height is a copy of her position's, which her position holds to the room's coordinates (#1627).
        AssertEqual(ushort.MaxValue, Conditions(ushort.MaxValue).SamusY, "palette-FX Samus Y keeps the room's lowest line");
        AssertEqual(0, Conditions(0).SamusY, "palette-FX Samus Y keeps the room's top line");

        AssertEqual(6, Frame(6).FirefleaDarknessStep, "Fireflea darkness keeps its last step");
        AssertEqual(0, new RoomFxOverlayFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.None }.FirefleaDarknessStep,
            "Fireflea darkness defaults to none");
        AssertThrows<ArgumentOutOfRangeException>(() => Frame(7), "Fireflea darkness past its last step");
        AssertThrows<ArgumentOutOfRangeException>(() => Frame(-1), "negative Fireflea darkness");

        const int largestRandom = SuperMetroid.Core.Compatibility.SharedRandomState.LargestValue;
        AssertEqual(largestRandom, LoadSource(largestRandom).RandomNumber, "room-FX load random number keeps the shared random's largest value");
        AssertThrows<ArgumentOutOfRangeException>(() => LoadSource(largestRandom + 1), "room-FX load random number the shared random never holds");
        AssertThrows<ArgumentOutOfRangeException>(() => LoadSource(-1), "negative room-FX load random number");

        AssertEqual(largestRandom, FrameRandom(largestRandom).RandomNumber, "layer-3 frame random number keeps the shared random's largest value");
        AssertEqual(0, Frame(0).RandomNumber, "layer-3 frame random number defaults to zero");
        AssertThrows<ArgumentOutOfRangeException>(() => FrameRandom(largestRandom + 1), "layer-3 frame random number the shared random never holds");
        AssertThrows<ArgumentOutOfRangeException>(() => FrameRandom(-1), "negative layer-3 frame random number");

        const int lastFrame = SuperMetroid.Core.Compatibility.FrameClock.CounterPeriod - 1;
        AssertEqual(lastFrame, (Conditions(0) with { FrameCounter = lastFrame }).FrameCounter,
            "palette-FX frame counter keeps the frame counter's last count");
        AssertEqual(0, Conditions(0).FrameCounter, "palette-FX frame counter defaults to zero");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = Conditions(0) with { FrameCounter = lastFrame + 1 },
            "palette-FX frame counter past the frame counter's period");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = Conditions(0) with { FrameCounter = -1 }, "negative palette-FX frame counter");

        // A named liquid surface or target is a line the liquid's signed readings find within the room (#1627).
        const int deepest = CoordinateAxis.LastNonNegative;
        AssertTrue(CoordinateAxis.AsSigned(deepest) >= 0 && CoordinateAxis.AsSigned(deepest + 1) < 0,
            "the deepest named line is the last a signed reading finds at or below the room's top");
        var liquid = new RoomFxOverlayState(new RoomFxSession());
        liquid.SetLiquidMotion(baseYPosition: deepest, targetYPosition: deepest);
        AssertEqual(deepest, (int)(liquid.BaseY ?? throw new InvalidOperationException("Verification failed: the named surface was dropped.")).Whole,
            "a named surface keeps the deepest line a signed reading finds within the room");
        AssertEqual<int?>(deepest, liquid.TargetSurface, "a named target keeps the deepest line a signed reading finds within the room");
        liquid.SetLiquidMotion(baseYPosition: 0, targetYPosition: 0);
        AssertEqual(0, (int)(liquid.BaseY ?? throw new InvalidOperationException("Verification failed: the named surface was dropped.")).Whole,
            "a named surface keeps the room's top line");
        AssertThrows<ArgumentOutOfRangeException>(() => liquid.SetLiquidMotion(baseYPosition: deepest + 1),
            "a named surface past the lines a signed reading finds within the room");
        AssertThrows<ArgumentOutOfRangeException>(() => liquid.SetLiquidMotion(targetYPosition: deepest + 1),
            "a named target past the lines a signed reading finds within the room");
        AssertThrows<ArgumentOutOfRangeException>(() => liquid.SetLiquidMotion(baseYPosition: -1), "a named surface above the room's top");
        AssertThrows<ArgumentOutOfRangeException>(() => liquid.SetLiquidMotion(targetYPosition: -1), "a named target above the room's top");

        static PaletteFxConditions Conditions(int samusY) =>
            new() { SamusY = samusY, EquippedItems = 0, AreaMiniBossDefeated = false };
        static RoomFxOverlayFrame Frame(int darkness) =>
            new() { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.None, FirefleaDarknessStep = darkness };
        static RoomFxOverlayFrame FrameRandom(int randomNumber) =>
            new() { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.None, RandomNumber = randomNumber };
        static RoomFxLoadSource LoadSource(int randomNumber) =>
            new() { Fx = null, Door = null, RandomNumber = randomNumber, Room = null, SuppliedDefinition = null };
    }

    /// <summary>Dispatches the widened room-FX input checks.</summary>
    [VerificationCommand("--room-fx-widened-inputs")]
    internal static int RoomFxWidenedInputsCommand()
    {
        Suite(nameof(VerifyRoomFxWidenedInputs), () => VerifyRoomFxWidenedInputs());
        return 0;
    }
}
