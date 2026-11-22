namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Samus's stand at the elevator's door (#2545, #2562), which reaches the elevator at the start of the next update's
/// enemies: every gameplay update runs the enemies before her collision, so the elevator leaves an update after she stands
/// at its door.
/// </summary>
/// <remarks>
/// Evidence: the game's collision with the elevator's door block records the stand, and the elevator's AI, which runs
/// before Samus's collision in each update, reads and clears that record on its next run.
/// Lifetime: the room's, beside its door contacts.
/// </remarks>
public sealed partial class ElevatorStandAtDoor
{
    /// <summary>Whether a stand waits for the elevator.</summary>
    public bool IsWaiting { get; private set; }

    /// <summary>Samus's collision stands at the elevator's door.</summary>
    internal void Stand() => IsWaiting = true;

    /// <summary>The next update's enemies take the stand: whether she stood, which the stand then has reached.</summary>
    internal bool Take()
    {
        bool stood = IsWaiting;
        IsWaiting = false;
        return stood;
    }
}
