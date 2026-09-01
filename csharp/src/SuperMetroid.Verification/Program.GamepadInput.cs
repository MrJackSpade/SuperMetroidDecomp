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

    AssertEqual(
        SnesButton.B | SnesButton.A | SnesButton.Y | SnesButton.X |
        SnesButton.L | SnesButton.R,
        GenericGamepadInput.Map(new GenericGamepadSnapshot(
            HorizontalAxis: 0,
            VerticalAxis: 0,
            PointOfView: GenericGamepadInput.CenteredPointOfView,
            Buttons: 0x003f)),
        "gamepad face and shoulder positions map to SNES bits");

    AssertEqual(
        SnesButton.Select | SnesButton.Start,
        GenericGamepadInput.Map(new GenericGamepadSnapshot(
            HorizontalAxis: 0,
            VerticalAxis: 0,
            PointOfView: GenericGamepadInput.CenteredPointOfView,
            Buttons: (1u << 8) | (1u << 9))),
        "ten-button generic pad Select and Start aliases");

    Console.WriteLine(
        "  Gamepad: axes, dead zone, POV priority, face buttons, shoulders, and menu aliases agree.");
}
}
