namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// What one eye-door draw shows (#1994): the eye in each of its looks, the body's middle and bottom in each pose of their
/// pulse, or the cleared opening the destroyed eye leaves.
/// </summary>
internal enum EyeDoorLook
{
    /// <summary>The cleared opening the destroyed eye leaves.</summary>
    Clear,
    /// <summary>The eye closed.</summary>
    EyeClosed,
    /// <summary>The eye half open.</summary>
    EyeHalfOpen,
    /// <summary>The eye open and watching, the one look a shot can hit.</summary>
    EyeOpen,
    /// <summary>The eye flashing as it flinches from a hit.</summary>
    EyeFlinching,
    /// <summary>The open eye drawn solid between its flinches, which no shot can hit.</summary>
    EyeOpenSolid,
    /// <summary>The body's middle resting.</summary>
    MiddleResting,
    /// <summary>The body's middle swelling.</summary>
    MiddleSwelling,
    /// <summary>The body's middle swollen.</summary>
    MiddleSwollen,
    /// <summary>The body's bottom resting.</summary>
    BottomResting,
    /// <summary>The body's bottom swelling.</summary>
    BottomSwelling,
    /// <summary>The body's bottom swollen.</summary>
    BottomSwollen,
}
