namespace SuperMetroid.Core.Samus.Movement;

/// <summary>How Samus's base horizontal speed changes this frame.</summary>
public enum SamusAccelerationMode
{
    /// <summary>Ordinary forward acceleration; selected by momentum cancellation and no-dash aerial initialization.</summary>
    Accelerating,

    /// <summary>
    /// A turn that keeps the old momentum: the speed decelerates, the displacement runs against the pose's facing, and
    /// the camera treats Samus as turning against her momentum.
    /// </summary>
    Turning,

    /// <summary>
    /// Retained-momentum deceleration, selected by the jump and fall initializers when extra dash speed remains; the airborne
    /// deceleration-disallowed path tests only the turning bit, so it keeps accelerating there.
    /// </summary>
    Decelerating,
}
