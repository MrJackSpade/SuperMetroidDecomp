namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>What a signalling cue of the Alcoon's animations does: take a stride, end a walk cycle, or start walking.</summary>
internal enum AlcoonCueKind
{
    /// <summary>Takes a stride within the walk cycle; a wall turns the Alcoon round to the other walk.</summary>
    Move,
    /// <summary>Starts walking a random number of walk cycles, continuing at the walk for the Alcoon's facing.</summary>
    StartWalking,
    /// <summary>Takes the walk cycle's last stride and counts the cycle walked; a wall turns the Alcoon round to the other walk.</summary>
    CompleteWalkCycle,
}
