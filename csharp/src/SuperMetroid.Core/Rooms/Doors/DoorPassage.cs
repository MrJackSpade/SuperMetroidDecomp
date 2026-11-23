namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Samus's passage through a door, from the door transition's start to the last step of the destination's fade-in. The
/// door transition starts and finishes it; the runtime builds one and hands it to everything that holds still while
/// Samus passes: the elevator, the Zebetites, the Etecoons, the face blocks' glow and Ridley's opening wait, Samus's
/// input handling and collision radius, the room's colour effects and the camera's scrolling.
/// </summary>
/// <remarks>
/// The passage outlives the rooms it joins: it holds through the destination's load, and no enemy's setup reads it.
/// </remarks>
internal sealed partial class DoorPassage
{
    /// <summary>
    /// Whether Samus is passing through a door: set when the door transition starts, cleared on the last step of the
    /// destination's fade-in, after that step's enemies have run, so movement resumes on the following update.
    /// </summary>
    public bool IsUnderWay { get; private set; }

    /// <summary>Starts the passage as the door transition starts.</summary>
    internal void Start() => IsUnderWay = true;

    /// <summary>Ends the passage on the destination fade-in's last step.</summary>
    internal void Finish() => IsUnderWay = false;
}
