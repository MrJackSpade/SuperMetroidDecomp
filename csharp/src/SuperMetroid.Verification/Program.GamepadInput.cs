using SuperMetroid.Core.Input;

internal static partial class Program
{
static void VerifyGenericGamepadInput()
{
    AssertEqual(
        SnesButton.None,
        GenericGamepadInput.Map(new GenericGamepadSnapshot(
            HorizontalAxis: 0,
            VerticalAxis: 0,
            PointOfView: GenericGamepadInput.CenteredPointOfView,
            Buttons: 0)),
        "centered gamepad is neutral");

    AssertEqual(
        SnesButton.Left | SnesButton.Down,
        GenericGamepadInput.Map(new GenericGamepadSnapshot(
            HorizontalAxis: -20000,
            VerticalAxis: 20000,
            PointOfView: GenericGamepadInput.CenteredPointOfView,
            Buttons: 0)),
        "gamepad analogue axes cross the dead zone");

    // POV 4500 is the exact up/right diagonal boundary. The hat owns both axes even if a
    // drifting stick reports the opposite direction, avoiding an impossible four-way word.
    AssertEqual(
        SnesButton.Up | SnesButton.Right,
        GenericGamepadInput.Map(new GenericGamepadSnapshot(
            HorizontalAxis: -20000,
            VerticalAxis: 20000,
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
            HorizontalAxis: 0,
            VerticalAxis: 0,
            PointOfView: GenericGamepadInput.CenteredPointOfView,
            Buttons: (1u << 8) | (1u << 9))),
        "ten-button generic pad Select and Start aliases");

    Console.WriteLine(
        "  Gamepad: axes, POV, positional/0079:0011 face layouts, shoulders, and menus agree.");
}

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
                HorizontalAxis: 0,
                VerticalAxis: 0,
                PointOfView: GenericGamepadInput.CenteredPointOfView,
                Buttons: 1u << zeroBasedButtonIndex),
            faceButtonLayout: layout),
        message);
}
}
