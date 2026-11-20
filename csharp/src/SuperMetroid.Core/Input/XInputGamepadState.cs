namespace SuperMetroid.Core.Input;

/// <summary>The controls of one XInput controller (Xbox pads and pads in X-input mode) read in one poll.</summary>
/// <param name="Buttons">Digital controls held.</param>
/// <param name="LeftTrigger">Left trigger travel, 0 released to 255 fully pulled.</param>
/// <param name="RightTrigger">Right trigger travel, 0 released to 255 fully pulled.</param>
/// <param name="LeftStick">Left thumbstick deflection.</param>
public readonly record struct XInputGamepadState(
    XInputButtons Buttons,
    int LeftTrigger,
    int RightTrigger,
    Offset16<XInputStick> LeftStick)
{
    /// <summary>Trigger travel above which a trigger counts as held (XInput's <c>XINPUT_GAMEPAD_TRIGGER_THRESHOLD</c>).</summary>
    public const int TriggerThreshold = 30;

    /// <summary>Left trigger travel, 0 through 255: the range XInput reports.</summary>
    public int LeftTrigger { get; } = Ensure.BetweenInclusive(0, byte.MaxValue, LeftTrigger);

    /// <summary>Right trigger travel, 0 through 255: the range XInput reports.</summary>
    public int RightTrigger { get; } = Ensure.BetweenInclusive(0, byte.MaxValue, RightTrigger);

    /// <summary>
    /// Expresses the controller as the generic gamepad the shared mapper reads: face buttons by
    /// position, bumpers and triggers as the shoulders, Back/Start as Select/Start, the D-pad as
    /// a hat and the left stick as the analogue axes.
    /// </summary>
    public GenericGamepadSnapshot ToSnapshot()
    {
        uint buttons = 0;
        if (Buttons.HasFlag(XInputButtons.South)) buttons |= 1u << 0;
        if (Buttons.HasFlag(XInputButtons.East)) buttons |= 1u << 1;
        if (Buttons.HasFlag(XInputButtons.West)) buttons |= 1u << 2;
        if (Buttons.HasFlag(XInputButtons.North)) buttons |= 1u << 3;
        if (Buttons.HasFlag(XInputButtons.LeftShoulder) || LeftTrigger > TriggerThreshold) buttons |= 1u << 4;
        if (Buttons.HasFlag(XInputButtons.RightShoulder) || RightTrigger > TriggerThreshold) buttons |= 1u << 5;
        if (Buttons.HasFlag(XInputButtons.Back)) buttons |= 1u << 6;
        if (Buttons.HasFlag(XInputButtons.Start)) buttons |= 1u << 7;

        // XInput's Y axis points up and the generic stick's points down. Negating the most
        // negative reading would overflow, so a full downward deflection saturates instead.
        short downwardY = LeftStick.Y == short.MinValue ? short.MaxValue : (short)-LeftStick.Y;
        return new GenericGamepadSnapshot(
            Stick: new(LeftStick.X, downwardY),
            PointOfView: DPadPointOfView(Buttons),
            Buttons: buttons);
    }

    /// <summary>The hat angle, in hundredths of a degree clockwise from north, of the D-pad directions held.</summary>
    private static uint DPadPointOfView(XInputButtons buttons)
    {
        int right = (buttons.HasFlag(XInputButtons.DPadRight) ? 1 : 0) - (buttons.HasFlag(XInputButtons.DPadLeft) ? 1 : 0);
        int down = (buttons.HasFlag(XInputButtons.DPadDown) ? 1 : 0) - (buttons.HasFlag(XInputButtons.DPadUp) ? 1 : 0);
        return DPadAngles[((down + 1) * 3) + right + 1];
    }

    /// <summary>Hat angles by D-pad direction, rows up/level/down, columns left/level/right.</summary>
    private static readonly uint[] DPadAngles =
    [
        31500, 0, 4500,
        27000, GenericGamepadInput.CenteredPointOfView, 9000,
        22500, 18000, 13500,
    ];
}
