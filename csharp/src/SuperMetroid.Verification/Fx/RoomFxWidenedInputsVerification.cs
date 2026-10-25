using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Fx;

using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the room-FX and palette-FX inputs that hold a copy of a cartridge word as a number: each accepts the whole
/// word range and rejects anything outside it instead of wrapping.
/// </summary>
internal static class RoomFxWidenedInputsVerification
{
    /// <summary>Checks the range contract of every widened room-FX and palette-FX input.</summary>
    internal static void VerifyRoomFxWidenedInputs()
    {
        AssertEqual(ushort.MaxValue, Conditions(ushort.MaxValue).SamusY, "palette-FX Samus Y keeps the largest word");
        AssertThrows<ArgumentOutOfRangeException>(() => Conditions(ushort.MaxValue + 1), "palette-FX Samus Y above the word");
        AssertThrows<ArgumentOutOfRangeException>(() => Conditions(-1), "negative palette-FX Samus Y");

        AssertEqual(ushort.MaxValue, Frame(ushort.MaxValue).FirefleaDarknessLevel, "Fireflea darkness keeps the largest word");
        AssertEqual(0, new RoomLayer3FxFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, MainGameLoopCarry = false }.FirefleaDarknessLevel,
            "Fireflea darkness defaults to none");
        AssertThrows<ArgumentOutOfRangeException>(() => Frame(ushort.MaxValue + 1), "Fireflea darkness above the word");
        AssertThrows<ArgumentOutOfRangeException>(() => Frame(-1), "negative Fireflea darkness");

        AssertEqual(ushort.MaxValue, LoadSource(ushort.MaxValue).RandomNumber, "room-FX load random number keeps the largest word");
        AssertThrows<ArgumentOutOfRangeException>(() => LoadSource(ushort.MaxValue + 1), "room-FX load random number above the word");
        AssertThrows<ArgumentOutOfRangeException>(() => LoadSource(-1), "negative room-FX load random number");

        AssertEqual(ushort.MaxValue, FrameRandom(ushort.MaxValue).RandomNumber, "layer-3 frame random number keeps the largest word");
        AssertEqual(0, Frame(0).RandomNumber, "layer-3 frame random number defaults to zero");
        AssertThrows<ArgumentOutOfRangeException>(() => FrameRandom(ushort.MaxValue + 1), "layer-3 frame random number above the word");
        AssertThrows<ArgumentOutOfRangeException>(() => FrameRandom(-1), "negative layer-3 frame random number");

        AssertEqual(ushort.MaxValue, (Conditions(0) with { FrameCounter = ushort.MaxValue }).FrameCounter,
            "palette-FX frame counter keeps the largest word");
        AssertEqual(0, Conditions(0).FrameCounter, "palette-FX frame counter defaults to zero");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = Conditions(0) with { FrameCounter = ushort.MaxValue + 1 },
            "palette-FX frame counter above the word");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = Conditions(0) with { FrameCounter = -1 }, "negative palette-FX frame counter");

        var request = new RoomFxEarthquakeRequest(RoomEarthquakeDefinitions.RisingLiquidType, RoomEarthquakeDefinitions.RisingLiquidTimerBits);
        AssertEqual(RoomEarthquakeDefinitions.RisingLiquidType, request.Type, "earthquake request keeps its type");
        AssertEqual(RoomEarthquakeDefinitions.RisingLiquidTimerBits, request.TimerBits, "earthquake request keeps its timer bits");
        AssertThrows<ArgumentOutOfRangeException>(() => new RoomFxEarthquakeRequest(ushort.MaxValue + 1, 0), "earthquake request type above the word");
        AssertThrows<ArgumentOutOfRangeException>(() => new RoomFxEarthquakeRequest(0, -1), "negative earthquake request timer bits");

        static PaletteFxConditions Conditions(int samusY) =>
            new() { SamusY = samusY, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false };
        static RoomLayer3FxFrame Frame(int darkness) =>
            new() { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, MainGameLoopCarry = false, FirefleaDarknessLevel = darkness };
        static RoomLayer3FxFrame FrameRandom(int randomNumber) =>
            new() { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, MainGameLoopCarry = false, RandomNumber = randomNumber };
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
