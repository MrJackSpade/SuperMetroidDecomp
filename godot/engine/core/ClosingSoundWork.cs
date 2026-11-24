namespace Engine.Core;

/// <summary>What of the sounds' own work closes an update, as the mode that played it says.</summary>
/// <param name="MusicTurn">Whether the music waiting gets a turn as the update closes, before the sounds'.</param>
/// <param name="SoundTurns">How many turns the sounds waiting get.</param>
public readonly record struct ClosingSoundWork(bool MusicTurn, int SoundTurns)
{
    /// <summary>What closes an update unless its mode says otherwise: one turn of the sounds.</summary>
    public static ClosingSoundWork Usual { get; } = new(false, 1);

    /// <summary>Nothing: neither the music nor the sounds get a turn.</summary>
    public static ClosingSoundWork None { get; } = new(false, 0);
}
