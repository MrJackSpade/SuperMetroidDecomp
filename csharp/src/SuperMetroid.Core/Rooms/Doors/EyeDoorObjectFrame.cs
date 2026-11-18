namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The eye door's eye's draws, the eye's facing picking each (the body and bottom draw their poses,
/// <see cref="EyeDoorPartPose"/>).
/// </summary>
internal enum EyeDoorObjectFrame
{
    /// <summary>The closed eye.</summary>
    ClosedEye,
    /// <summary>The half-open eye.</summary>
    HalfOpenEye,
    /// <summary>The open eye.</summary>
    OpenEye,
    /// <summary>The eye's first flinch pose.</summary>
    FlinchingEye,
    /// <summary>The open eye drawn solid between its flinches, which no shot can hit.</summary>
    OpenSolidEye,
    /// <summary>The cleared opening the eye leaves.</summary>
    OpeningClear,
    /// <summary>The blue cap the opening flashes to.</summary>
    BlueCap,
}
