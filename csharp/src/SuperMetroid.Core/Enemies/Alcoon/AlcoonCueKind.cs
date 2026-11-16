namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>What a signalling cue of the Alcoon's animations does: move, count a step of its walk, or start walking.</summary>
internal enum AlcoonCueKind
{
    /// <summary>Moves without counting a step; a wall turns the Alcoon round to the other walk.</summary>
    Move,
    /// <summary>Starts walking a random number of steps, continuing at the walk for the Alcoon's facing.</summary>
    StartWalking,
    /// <summary>Counts down a step and moves; a wall turns the Alcoon round to the other walk.</summary>
    Step,
}
