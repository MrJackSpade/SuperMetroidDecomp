namespace SuperMetroid.Desktop.Input;

/// <summary>Digital controls XInput reports in <c>XINPUT_GAMEPAD.wButtons</c>; any combination can be held at once.</summary>
[Flags]
internal enum XInputButtons
{
    /// <summary>No digital control is held.</summary>
    None = 0,
    /// <summary>D-pad up.</summary>
    DPadUp = 1 << 0,
    /// <summary>D-pad down.</summary>
    DPadDown = 1 << 1,
    /// <summary>D-pad left.</summary>
    DPadLeft = 1 << 2,
    /// <summary>D-pad right.</summary>
    DPadRight = 1 << 3,
    /// <summary>The Start (Menu) button.</summary>
    Start = 1 << 4,
    /// <summary>The Back (View) button.</summary>
    Back = 1 << 5,
    /// <summary>Left thumbstick pressed in.</summary>
    LeftThumb = 1 << 6,
    /// <summary>Right thumbstick pressed in.</summary>
    RightThumb = 1 << 7,
    /// <summary>Left bumper.</summary>
    LeftShoulder = 1 << 8,
    /// <summary>Right bumper.</summary>
    RightShoulder = 1 << 9,
    /// <summary>The south face button, labelled A on Xbox pads.</summary>
    South = 1 << 12,
    /// <summary>The east face button, labelled B on Xbox pads.</summary>
    East = 1 << 13,
    /// <summary>The west face button, labelled X on Xbox pads.</summary>
    West = 1 << 14,
    /// <summary>The north face button, labelled Y on Xbox pads.</summary>
    North = 1 << 15,
}
