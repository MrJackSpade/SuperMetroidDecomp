namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What an eye door's eye is doing (#1554).</summary>
internal enum EyeDoorEyePhase
{
    /// <summary>The eye is closed and looks for Samus every four updates; one just placed looks at its door first.</summary>
    Closed,
    /// <summary>Samus came near: the eye opens and starts taking shots.</summary>
    Opening,
    /// <summary>The open eye watches Samus and shoots at her.</summary>
    Watching,
    /// <summary>Samus stands right in front of the open eye, which blinks.</summary>
    Blinking,
    /// <summary>A hit short of the threshold makes the eye flinch.</summary>
    Flinching,
    /// <summary>The hit that reached the threshold destroyed the eye, which flashes into its blue cap.</summary>
    Destroyed,
}
