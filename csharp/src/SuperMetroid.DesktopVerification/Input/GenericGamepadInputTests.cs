using SuperMetroid.Core.Input;
using SuperMetroid.Desktop.Input;

namespace SuperMetroid.DesktopVerification.Input;

/// <summary>Checks WinMM gamepad axes, POV priority, and both generic face-button layouts.</summary>
internal static class GenericGamepadInputTests
{
/// <summary>XInput's documented <c>XINPUT_GAMEPAD_TRIGGER_THRESHOLD</c>: the trigger travel at which a trigger still counts as released.</summary>
private const int XInputTriggerThreshold = 30;

/// <summary>Checks the SNES USB adapter by printed button label and other pads by physical position.</summary>
internal static void Verify()
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
        ControllerButton.None,
        GenericGamepadInput.Map(new GenericGamepadSnapshot(
            Stick: new(0, 0),
            PointOfView: GenericGamepadInput.CenteredPointOfView,
            Buttons: 0)),
        "centered gamepad is neutral");

    AssertEqual(
        ControllerButton.Left | ControllerButton.Down,
        GenericGamepadInput.Map(new GenericGamepadSnapshot(
            Stick: new(-20000, 20000),
            PointOfView: GenericGamepadInput.CenteredPointOfView,
            Buttons: 0)),
        "gamepad analogue axes cross the dead zone");

    // POV 4500 is the exact up/right diagonal boundary. The hat owns both axes even if a
    // drifting stick reports the opposite direction, avoiding an impossible four-way word.
    AssertEqual(
        ControllerButton.Up | ControllerButton.Right,
        GenericGamepadInput.Map(new GenericGamepadSnapshot(
            Stick: new(-20000, 20000),
            PointOfView: 4500,
            Buttons: 0)),
        "gamepad POV diagonal overrides analogue drift");

    // Check face buttons individually. Testing only all four at once proves the union of
    // the output bits but cannot detect a permutation—the exact bug where this SNES USB
    // adapter made its physical Y fire and its physical X dash.
    VerifyGamepadButton(0, ControllerButton.B, "positional south maps to SNES B");
    VerifyGamepadButton(1, ControllerButton.A, "positional east maps to SNES A");
    VerifyGamepadButton(2, ControllerButton.Y, "positional west maps to SNES Y");
    VerifyGamepadButton(3, ControllerButton.X, "positional north maps to SNES X");
    VerifyGamepadButton(4, ControllerButton.L, "left shoulder maps to SNES L");
    VerifyGamepadButton(5, ControllerButton.R, "right shoulder maps to SNES R");

    VerifyGamepadButton(
        0,
        ControllerButton.X,
        "0079:0011 printed X maps to SNES X",
        GenericGamepadFaceButtonLayout.SnesUsbAdapter0079_0011);
    VerifyGamepadButton(
        1,
        ControllerButton.A,
        "0079:0011 printed A maps to SNES A",
        GenericGamepadFaceButtonLayout.SnesUsbAdapter0079_0011);
    VerifyGamepadButton(
        2,
        ControllerButton.B,
        "0079:0011 printed B maps to SNES B",
        GenericGamepadFaceButtonLayout.SnesUsbAdapter0079_0011);
    VerifyGamepadButton(
        3,
        ControllerButton.Y,
        "0079:0011 printed Y maps to SNES Y",
        GenericGamepadFaceButtonLayout.SnesUsbAdapter0079_0011);

    AssertEqual(
        ControllerButton.Select | ControllerButton.Start,
        GenericGamepadInput.Map(new GenericGamepadSnapshot(
            Stick: new(0, 0),
            PointOfView: GenericGamepadInput.CenteredPointOfView,
            Buttons: (1u << 8) | (1u << 9))),
        "ten-button generic pad Select and Start aliases");

    AssertThrows<ArgumentOutOfRangeException>(() => _ = new UsbGamepadIdentifier(-1, 0), "a USB vendor identifier is never negative");
    AssertThrows<ArgumentOutOfRangeException>(() => _ = new UsbGamepadIdentifier(0, ushort.MaxValue + 1),
        "a USB product identifier never exceeds its sixteen-bit range");

    VerifyXInputGamepad();

    Console.WriteLine(
        "  Gamepad: axes, POV, positional/0079:0011 face layouts, shoulders, menus and XInput agree.");
}

/// <summary>Checks each XInput control individually against the SNES button it must produce.</summary>
private static void VerifyXInputGamepad()
{
    VerifyXInputButton(XInputButtons.South, ControllerButton.B, "XInput south (Xbox A) maps to SNES B");
    VerifyXInputButton(XInputButtons.East, ControllerButton.A, "XInput east (Xbox B) maps to SNES A");
    VerifyXInputButton(XInputButtons.West, ControllerButton.Y, "XInput west (Xbox X) maps to SNES Y");
    VerifyXInputButton(XInputButtons.North, ControllerButton.X, "XInput north (Xbox Y) maps to SNES X");
    VerifyXInputButton(XInputButtons.LeftShoulder, ControllerButton.L, "XInput left bumper maps to SNES L");
    VerifyXInputButton(XInputButtons.RightShoulder, ControllerButton.R, "XInput right bumper maps to SNES R");
    VerifyXInputButton(XInputButtons.Back, ControllerButton.Select, "XInput Back maps to SNES Select");
    VerifyXInputButton(XInputButtons.Start, ControllerButton.Start, "XInput Start maps to SNES Start");
    VerifyXInputButton(XInputButtons.DPadUp, ControllerButton.Up, "XInput D-pad up maps to SNES Up");
    VerifyXInputButton(XInputButtons.DPadDown, ControllerButton.Down, "XInput D-pad down maps to SNES Down");
    VerifyXInputButton(XInputButtons.DPadLeft, ControllerButton.Left, "XInput D-pad left maps to SNES Left");
    VerifyXInputButton(XInputButtons.DPadRight, ControllerButton.Right, "XInput D-pad right maps to SNES Right");
    VerifyXInputButton(XInputButtons.DPadUp | XInputButtons.DPadLeft, ControllerButton.Up | ControllerButton.Left,
        "XInput D-pad diagonal maps to both SNES directions");
    VerifyXInputButton(XInputButtons.None, ControllerButton.None, "an idle XInput controller is neutral");

    AssertEqual(
        ControllerButton.L | ControllerButton.R,
        GenericGamepadInput.Map(new XInputGamepadState(XInputButtons.None, 255, 255, new(0, 0)).ToSnapshot()),
        "pulled XInput triggers map to SNES L and R");
    AssertEqual(
        ControllerButton.None,
        GenericGamepadInput.Map(new XInputGamepadState(
            XInputButtons.None, XInputTriggerThreshold, XInputTriggerThreshold, new(0, 0)).ToSnapshot()),
        "XInput triggers resting at the threshold stay released");

    // XInput's stick Y points up; the SNES word must still read up as Up.
    AssertEqual(
        ControllerButton.Up | ControllerButton.Right,
        GenericGamepadInput.Map(new XInputGamepadState(XInputButtons.None, 0, 0, new(20000, 20000)).ToSnapshot()),
        "XInput stick up-right maps to SNES Up and Right");
    AssertEqual(
        ControllerButton.Down | ControllerButton.Left,
        GenericGamepadInput.Map(new XInputGamepadState(
            XInputButtons.None, 0, 0, new(short.MinValue, short.MinValue)).ToSnapshot()),
        "XInput stick fully down-left maps to SNES Down and Left");
}

/// <summary>Checks one XInput digital control so face-button permutations remain visible.</summary>
private static void VerifyXInputButton(XInputButtons held, ControllerButton expected, string message) =>
    AssertEqual(
        expected,
        GenericGamepadInput.Map(new XInputGamepadState(held, 0, 0, new(0, 0)).ToSnapshot()),
        message);

/// <summary>Checks one physical button independently so face-button permutations remain visible.</summary>
private static void VerifyGamepadButton(
    int zeroBasedButtonIndex,
    ControllerButton expected,
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

/// <summary>Fails when <paramref name="actual"/> differs from <paramref name="expected"/>.</summary>
private static void AssertEqual<T>(T expected, T actual, string message)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidDataException($"{message}: expected {expected}, got {actual}.");
}

/// <summary>Fails unless <paramref name="action"/> throws exactly <typeparamref name="TException"/>.</summary>
private static void AssertThrows<TException>(Action action, string message) where TException : Exception
{
    try
    {
        action();
    }
    catch (TException error) when (error.GetType() == typeof(TException))
    {
        return;
    }
    throw new InvalidDataException($"{message}: expected {typeof(TException).Name}.");
}
}
