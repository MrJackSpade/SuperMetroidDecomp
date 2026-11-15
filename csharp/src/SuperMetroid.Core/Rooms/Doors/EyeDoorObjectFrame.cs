namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The eye door's room-object draws; the animation's facing and component pick each draw.</summary>
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
    /// <summary>The body's or bottom's resting pose, the first of its pulse.</summary>
    PartResting,
    /// <summary>The body's or bottom's swelling pose, the middle of its pulse.</summary>
    PartSwelling,
    /// <summary>The body's or bottom's swollen pose, the height of its pulse.</summary>
    PartSwollen,
}
