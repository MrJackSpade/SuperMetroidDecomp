namespace SuperMetroid.Desktop.Input;

/// <summary>Actual bit mask accepted by WinMM's JOYINFOEX query.</summary>
[Flags]
internal enum JoystickPositionQueryFlags : uint
{
    /// <summary><c>JOY_RETURNX</c>: requests valid X-axis data in <c>JOYINFOEX.dwXpos</c>, subsequently normalized by the desktop adapter for horizontal input.</summary>
    ReturnX = 0x00000001,
    /// <summary><c>JOY_RETURNY</c>: requests valid Y-axis data in <c>JOYINFOEX.dwYpos</c>, subsequently normalized by the desktop adapter for vertical input.</summary>
    ReturnY = 0x00000002,
    /// <summary><c>JOY_RETURNZ</c>: requests valid third-axis data in <c>JOYINFOEX.dwZpos</c>; this axis is not mapped to SNES controls.</summary>
    ReturnZ = 0x00000004,
    /// <summary><c>JOY_RETURNR</c>: requests valid rudder or fourth-axis data in <c>JOYINFOEX.dwRpos</c>; this axis is not mapped to SNES controls.</summary>
    ReturnR = 0x00000008,
    /// <summary><c>JOY_RETURNU</c>: requests valid fifth-axis data in <c>JOYINFOEX.dwUpos</c>; this axis is not mapped to SNES controls.</summary>
    ReturnU = 0x00000010,
    /// <summary><c>JOY_RETURNV</c>: requests valid sixth-axis data in <c>JOYINFOEX.dwVpos</c>; this axis is not mapped to SNES controls.</summary>
    ReturnV = 0x00000020,
    /// <summary><c>JOY_RETURNPOV</c>: requests the directional hat in <c>JOYINFOEX.dwPOV</c>; the adapter uses the centered sentinel when capabilities report no hat.</summary>
    ReturnPointOfView = 0x00000040,
    /// <summary><c>JOY_RETURNBUTTONS</c>: requests <c>JOYINFOEX.dwButtons</c>, where bit zero represents button one and multiple pressed buttons combine as a bitmask.</summary>
    ReturnButtons = 0x00000080,
    /// <summary><c>JOY_RETURNALL</c> (<c>$FF</c>): combines all six axis, POV, and button requests used for each desktop poll; does not request raw, centered, or continuous-POV modifiers.</summary>
    ReturnAll = ReturnX | ReturnY | ReturnZ | ReturnR | ReturnU | ReturnV |
        ReturnPointOfView | ReturnButtons,
}
