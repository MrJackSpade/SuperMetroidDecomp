namespace SuperMetroid.Core.Input;

/// <summary>Converts a conventional physical gamepad layout into one SNES controller word.</summary>
public static class GenericGamepadInput
{
    /// <summary>POV value returned by Windows while a directional hat is centered.</summary>
    public const uint CenteredPointOfView = WindowsGamepadCodes.CenteredPointOfView;

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
            left = snapshot.Stick.X < -axisDeadZone;
            right = snapshot.Stick.X > axisDeadZone;
        }
        if (!up && !down)
        {
            up = snapshot.Stick.Y < -axisDeadZone;
            down = snapshot.Stick.Y > axisDeadZone;
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
        // The SNES buttons host face buttons 0 to 3 report.
        SnesButton[] faceButtons =
            faceButtonLayout switch
            {
                GenericGamepadFaceButtonLayout.Positional =>
                    [SnesButton.B, SnesButton.A, SnesButton.Y, SnesButton.X],
                GenericGamepadFaceButtonLayout.SnesUsbAdapter0079_0011 =>
                    [SnesButton.X, SnesButton.A, SnesButton.B, SnesButton.Y],
                _ => throw new ArgumentOutOfRangeException(nameof(faceButtonLayout)),
            };
        for (int faceButton = 0; faceButton < faceButtons.Length; faceButton++)
        {
            if (IsPressed(snapshot.Buttons, faceButton)) result |= faceButtons[faceButton];
        }
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

    /// <summary>Tests a host button number against the snapshot's zero-based bit mask.</summary>
    private static bool IsPressed(uint buttons, int zeroBasedButtonIndex) =>
        (buttons & (1u << zeroBasedButtonIndex)) != 0;
}
