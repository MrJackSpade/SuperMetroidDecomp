namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The eye door's PLM draws; the animation's facing and component pick each bank-$84 draw list.</summary>
internal enum EyeDoorPlmFrame
{
    /// <summary>The closed eye.</summary>
    Eye0,
    /// <summary>The half-open eye.</summary>
    Eye1,
    /// <summary>The open eye.</summary>
    Eye2,
    /// <summary>The first flinch pose.</summary>
    Eye3,
    /// <summary>The second flinch pose.</summary>
    Eye4,
    /// <summary>The cleared opening the eye leaves.</summary>
    OpeningClear,
    /// <summary>The blue cap the opening flashes to.</summary>
    BlueCap,
    /// <summary>A component's first pose.</summary>
    Pose0,
    /// <summary>A component's second pose.</summary>
    Pose1,
    /// <summary>A component's third pose.</summary>
    Pose2,
}
