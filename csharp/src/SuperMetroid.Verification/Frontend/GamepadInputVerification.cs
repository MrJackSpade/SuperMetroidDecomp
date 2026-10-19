using SuperMetroid.Core.Input;

namespace SuperMetroid.Verification.Frontend;

/// <summary>Checks WinMM gamepad axes, POV priority, and both generic face-button layouts.</summary>
internal static class GamepadInputVerification
{
/// <summary>Checks the SNES USB adapter by printed button label and other pads by physical position.</summary>
internal static void VerifyGenericGamepadInput()
{
    UsbGamepadIdentifier snesAdapter =
        WindowsGamepadCodes.SnesUsbAdapter0079_0011;
    AssertEqual(
        GenericGamepadFaceButtonLayout.SnesUsbAdapter0079_0011,
        WindowsGamepadCodes.ResolveFaceButtonLayout(
            snesAdapter.ManufacturerId,
            snesAdapter.ProductId),
        "known Windows VID/PID selects printed-label layout");
    AssertEqual(
        GenericGamepadFaceButtonLayout.Positional,
        WindowsGamepadCodes.ResolveFaceButtonLayout(0x045e, 0x028e),
        "ordinary Xbox-compatible VID/PID retains positional layout");
    AssertEqual(
        (uint)0xff,
        (uint)JoystickPositionQueryFlags.ReturnAll,
        "WinMM all-fields request retains every actual query flag");

    AssertEqual(
        SnesButton.None,
        GenericGamepadInput.Map(new GenericGamepadSnapshot(
            Stick: new(0, 0),
            PointOfView: GenericGamepadInput.CenteredPointOfView,
            Buttons: 0)),
        "centered gamepad is neutral");

    AssertEqual(
        SnesButton.Left | SnesButton.Down,
        GenericGamepadInput.Map(new GenericGamepadSnapshot(
            Stick: new(-20000, 20000),
            PointOfView: GenericGamepadInput.CenteredPointOfView,
            Buttons: 0)),
        "gamepad analogue axes cross the dead zone");

    // POV 4500 is the exact up/right diagonal boundary. The hat owns both axes even if a
    // drifting stick reports the opposite direction, avoiding an impossible four-way word.
    AssertEqual(
        SnesButton.Up | SnesButton.Right,
        GenericGamepadInput.Map(new GenericGamepadSnapshot(
            Stick: new(-20000, 20000),
            PointOfView: 4500,
            Buttons: 0)),
        "gamepad POV diagonal overrides analogue drift");

    // Check face buttons individually. Testing only all four at once proves the union of
    // the output bits but cannot detect a permutation—the exact bug where this SNES USB
    // adapter made its physical Y fire and its physical X dash.
    VerifyGamepadButton(0, SnesButton.B, "positional south maps to SNES B");
    VerifyGamepadButton(1, SnesButton.A, "positional east maps to SNES A");
    VerifyGamepadButton(2, SnesButton.Y, "positional west maps to SNES Y");
    VerifyGamepadButton(3, SnesButton.X, "positional north maps to SNES X");
    VerifyGamepadButton(4, SnesButton.L, "left shoulder maps to SNES L");
    VerifyGamepadButton(5, SnesButton.R, "right shoulder maps to SNES R");

    VerifyGamepadButton(
        0,
        SnesButton.X,
        "0079:0011 printed X maps to SNES X",
        GenericGamepadFaceButtonLayout.SnesUsbAdapter0079_0011);
    VerifyGamepadButton(
        1,
        SnesButton.A,
        "0079:0011 printed A maps to SNES A",
        GenericGamepadFaceButtonLayout.SnesUsbAdapter0079_0011);
    VerifyGamepadButton(
        2,
        SnesButton.B,
        "0079:0011 printed B maps to SNES B",
        GenericGamepadFaceButtonLayout.SnesUsbAdapter0079_0011);
    VerifyGamepadButton(
        3,
        SnesButton.Y,
        "0079:0011 printed Y maps to SNES Y",
        GenericGamepadFaceButtonLayout.SnesUsbAdapter0079_0011);

    AssertEqual(
        SnesButton.Select | SnesButton.Start,
        GenericGamepadInput.Map(new GenericGamepadSnapshot(
            Stick: new(0, 0),
            PointOfView: GenericGamepadInput.CenteredPointOfView,
            Buttons: (1u << 8) | (1u << 9))),
        "ten-button generic pad Select and Start aliases");

    AssertThrows<ArgumentOutOfRangeException>(() => new UsbGamepadIdentifier(-1, 0), "a USB vendor identifier is never negative");
    AssertThrows<ArgumentOutOfRangeException>(() => new UsbGamepadIdentifier(0, ushort.MaxValue + 1),
        "a USB product identifier never exceeds its sixteen-bit range");

    VerifyRepeatAndHeldInputDelays();

    Console.WriteLine(
        "  Gamepad: axes, POV, positional/0079:0011 face layouts, shoulders, and menus agree.");
}

/// <summary>
/// The controller repeat delays and the held-input reload are frame counts that are only reloaded into countdowns: a
/// delay of zero still lets the countdown wrap below zero exactly as before, and a delay past the countdown's range is
/// rejected where it is set.
/// </summary>
private static void VerifyRepeatAndHeldInputDelays()
{
    var controller = new ControllerInputState();
    controller.Latch(SnesButton.Left);
    controller.Latch(SnesButton.Left);
    AssertEqual(ushort.MaxValue, controller.RepeatTimer.RawValue, "a zero repeat delay wraps the repeat countdown below zero");
    AssertEqual(SnesButton.None, controller.NewlyPressedWithRepeat, "a wrapped repeat countdown does not pulse");
    AssertThrows<ArgumentOutOfRangeException>(() => controller.InitialRepeatDelay = ushort.MaxValue + 1,
        "a repeat delay past the countdown's range is rejected");
    AssertThrows<ArgumentOutOfRangeException>(() => new ControllerInputState(subsequentRepeatDelay: -1),
        "a negative repeat delay is rejected");

    var system = new SuperMetroid.Core.Progression.Bank80SystemState();
    system.UpdateHeldInput(0, SnesButton.Left, SnesButton.None);
    AssertEqual(SnesButton.None, system.TimedHeldInput, "a held-input change restarts the delay");
    system.UpdateHeldInput(0, SnesButton.Left, SnesButton.None);
    AssertEqual(SnesButton.Left, system.TimedHeldInput, "a zero reload activates on the first stable update by wrapping below zero");
    AssertEqual(0, system.TimedHeldInputTimer.RawValue, "the expired held-input countdown is pinned to zero");
    AssertThrows<ArgumentOutOfRangeException>(() => system.UpdateHeldInput(ushort.MaxValue + 1, SnesButton.Left, SnesButton.None),
        "a held-input reload past the countdown's range is rejected");
}

/// <summary>Checks one physical button independently so face-button permutations remain visible.</summary>
static void VerifyGamepadButton(
    int zeroBasedButtonIndex,
    SnesButton expected,
    string message,
    GenericGamepadFaceButtonLayout layout = GenericGamepadFaceButtonLayout.Positional)
{
    AssertEqual(
        expected,
        GenericGamepadInput.Map(
            new GenericGamepadSnapshot(
                Stick: new(0, 0),
                PointOfView: GenericGamepadInput.CenteredPointOfView,
                Buttons: 1u << zeroBasedButtonIndex),
            faceButtonLayout: layout),
        message);
}
}
