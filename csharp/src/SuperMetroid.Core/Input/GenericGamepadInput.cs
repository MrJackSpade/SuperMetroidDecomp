namespace SuperMetroid.Core.Input;

/// <summary>
/// Platform-neutral snapshot of the controls exposed by a conventional USB gamepad.
/// </summary>
/// <remarks>
/// Windows reports joystick axes in device-specific unsigned ranges and POV hats in
/// hundredths of a degree. The desktop adapter normalizes those axes to signed 16-bit
/// values before constructing this record. Keeping the mapping itself in Core makes the
/// exact host-to-SNES bit conversion deterministic and independently testable without a
/// physical controller or a dependency on Windows native APIs.
/// </remarks>
public readonly record struct GenericGamepadSnapshot(
    short HorizontalAxis,
    short VerticalAxis,
    uint PointOfView,
    uint Buttons);

/// <summary>
/// Identifies how a host driver numbers the four physical face buttons before they are
/// converted to the SNES controller word.
/// </summary>
/// <remarks>
/// This is deliberately a host-input concern, not an in-game control binding. Super
/// Metroid still receives literal SNES X/A/B/Y bits and applies its configurable action
/// bindings afterward. Some generic SNES USB adapters enumerate buttons by their printed
/// labels instead of by the conventional DirectInput diamond positions.
/// </remarks>
public enum GenericGamepadFaceButtonLayout
{
    /// <summary>WinMM buttons 1-4 are south, east, west, north: SNES B, A, Y, X.</summary>
    Positional,

    /// <summary>WinMM buttons 1-4 are printed X, A, B, Y on VID 0079 / PID 0011 pads.</summary>
    SnesUsbAdapter0079_0011,
}

/// <summary>Converts a conventional physical gamepad layout into one SNES controller word.</summary>
public static class GenericGamepadInput
{
    /// <summary>POV value returned by Windows while a directional hat is centered.</summary>
    public const uint CenteredPointOfView = 0xffff;

    /// <summary>
    /// Signed-axis magnitude required before an analogue direction is considered held.
    /// </summary>
    /// <remarks>
    /// One quarter of the full half-axis range leaves enough room for inexpensive generic
    /// pads whose nominal center jitters while still making deliberate movement immediate.
    /// </remarks>
    public const int DefaultAxisDeadZone = 8192;

    /// <summary>
    /// Maps the common DirectInput/USB physical layout to Super Metroid's SNES controls.
    /// </summary>
    /// <remarks>
    /// Face buttons are mapped by position rather than the letters printed by a modern
    /// controller: south=B/dash, east=A/jump, west=Y/cancel, and north=X/fire. This matches
    /// the original SNES diamond and remains sensible on pads labelled A/B/X/Y differently.
    /// Button bits seven/eight are the usual Back/Start pair. Many low-cost ten-button USB
    /// pads instead publish Select/Start as bits nine/ten, so both non-overlapping pairs are
    /// accepted. The extra aliases do not alter recordings: only the resulting SNES word is
    /// written to disk.
    /// </remarks>
    public static SnesButton Map(
        GenericGamepadSnapshot snapshot,
        int axisDeadZone = DefaultAxisDeadZone,
        GenericGamepadFaceButtonLayout faceButtonLayout =
            GenericGamepadFaceButtonLayout.Positional)
    {
        if (axisDeadZone is < 0 or > short.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(axisDeadZone));

        bool up = false;
        bool right = false;
        bool down = false;
        bool left = false;

        // A centered POV is $FFFF. Every active value is an angle clockwise from north;
        // accepting both ends of each 90-degree quadrant preserves diagonals exactly.
        if (snapshot.PointOfView != CenteredPointOfView)
        {
            uint angle = snapshot.PointOfView % 36000;
            up = angle is >= 31500 or <= 4500;
            right = angle is >= 4500 and <= 13500;
            down = angle is >= 13500 and <= 22500;
            left = angle is >= 22500 and <= 31500;
        }

        // Prefer the digital hat on each axis when it is active. This prevents a slightly
        // drifting analogue stick from cancelling the deliberate opposite D-pad direction.
        if (!left && !right)
        {
            left = snapshot.HorizontalAxis < -axisDeadZone;
            right = snapshot.HorizontalAxis > axisDeadZone;
        }
        if (!up && !down)
        {
            up = snapshot.VerticalAxis < -axisDeadZone;
            down = snapshot.VerticalAxis > axisDeadZone;
        }

        SnesButton result = SnesButton.None;
        if (left) result |= SnesButton.Left;
        if (right) result |= SnesButton.Right;
        if (up) result |= SnesButton.Up;
        if (down) result |= SnesButton.Down;

        // WinMM button one occupies bit zero. Standard DirectInput pads number the diamond
        // by position (south/east/west/north), but the explicitly supported 0079:0011 SNES
        // adapter reports the printed labels X/A/B/Y. Keeping the two translations here
        // ensures the core receives the same authentic SNES word from either device; the
        // cartridge's separate options bindings still decide what those SNES buttons do.
        (SnesButton button0, SnesButton button1, SnesButton button2, SnesButton button3) =
            faceButtonLayout switch
            {
                GenericGamepadFaceButtonLayout.Positional =>
                    (SnesButton.B, SnesButton.A, SnesButton.Y, SnesButton.X),
                GenericGamepadFaceButtonLayout.SnesUsbAdapter0079_0011 =>
                    (SnesButton.X, SnesButton.A, SnesButton.B, SnesButton.Y),
                _ => throw new ArgumentOutOfRangeException(nameof(faceButtonLayout)),
            };
        if (IsPressed(snapshot.Buttons, 0)) result |= button0;
        if (IsPressed(snapshot.Buttons, 1)) result |= button1;
        if (IsPressed(snapshot.Buttons, 2)) result |= button2;
        if (IsPressed(snapshot.Buttons, 3)) result |= button3;
        if (IsPressed(snapshot.Buttons, 4)) result |= SnesButton.L;
        if (IsPressed(snapshot.Buttons, 5)) result |= SnesButton.R;

        // Eight-button pads normally use 7/8; generic ten-button SNES-style adapters often
        // use 9/10. Supporting both makes either device usable without a calibration dialog.
        if (IsPressed(snapshot.Buttons, 6) || IsPressed(snapshot.Buttons, 8))
            result |= SnesButton.Select;
        if (IsPressed(snapshot.Buttons, 7) || IsPressed(snapshot.Buttons, 9))
            result |= SnesButton.Start;

        return result;
    }

    private static bool IsPressed(uint buttons, int zeroBasedButtonIndex) =>
        (buttons & (1u << zeroBasedButtonIndex)) != 0;
}
