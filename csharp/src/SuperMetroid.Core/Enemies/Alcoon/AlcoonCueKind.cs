namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>What a cue of the Alcoon's programs does: one per kind of step its lists use.</summary>
internal enum AlcoonCueKind
{
    /// <summary>Spits a fireball downward and plays the fire sound.</summary>
    FireDownward,
    /// <summary>Spits a fireball straight ahead and plays the fire sound.</summary>
    FireHorizontally,
    /// <summary>Spits a fireball upward and plays the fire sound.</summary>
    FireUpward,
    /// <summary>Moves without counting a step; a wall turns the Alcoon round to the other walk.</summary>
    Move,
    /// <summary>Starts walking a random number of steps, continuing at the walk for the Alcoon's facing.</summary>
    StartWalking,
    /// <summary>Counts down a step and moves; a wall turns the Alcoon round to the other walk.</summary>
    Step,
}
