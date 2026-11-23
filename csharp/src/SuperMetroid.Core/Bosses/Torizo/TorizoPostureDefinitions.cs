namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// How far a Torizo facing right moves as it stands up through each pose (#1891, #2411); one facing left moves as far the
/// other way, and sitting down moves it back by as much as standing up moved it.
/// </summary>
internal static partial class TorizoPostureDefinitions
{
    /// <summary>From its seat to rising from it.</summary>
    internal static Offset16<RoomPixels> SeatedToRising { get; } = new(9, 0);

    /// <summary>From rising from its seat to crouched.</summary>
    internal static Offset16<RoomPixels> RisingToCrouched { get; } = new(6, -6);

    /// <summary>From crouched to half risen.</summary>
    internal static Offset16<RoomPixels> CrouchedToHalfRisen { get; } = new(7, -6);

    /// <summary>From half risen to nearly upright.</summary>
    internal static Offset16<RoomPixels> HalfRisenToNearlyUpright { get; } = new(-5, -7);

    /// <summary>From nearly upright to upright, stepping.</summary>
    internal static Offset16<RoomPixels> NearlyUprightToStepping { get; } = new(16, 0);

    /// <summary>From upright, stepping, to its wide stance.</summary>
    internal static Offset16<RoomPixels> SteppingToWideStance { get; } = new(7, 0);
}
