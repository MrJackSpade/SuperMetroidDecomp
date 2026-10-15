namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>What a cue of the Alcoon's programs does: one per bank-$A8 instruction its lists use.</summary>
internal enum AlcoonCueKind
{
    /// <summary>Spits a fireball downward and plays the fire sound (<c>SpawnFireballDownward</c>, $A8:DF39).</summary>
    FireDownward,
    /// <summary>Spits a fireball straight ahead and plays the fire sound (<c>SpawnFireballHorizontally</c>, $A8:DF33).</summary>
    FireHorizontally,
    /// <summary>Spits a fireball upward and plays the fire sound (<c>SpawnFireballUpward</c>, $A8:DF1C).</summary>
    FireUpward,
    /// <summary>Moves without counting a step; a wall turns the Alcoon round to the other walk (<c>MoveHorizontallyTurnIfWallCollision</c>, $A8:DF71).</summary>
    Move,
    /// <summary>Starts walking a random number of steps, continuing at the walk for the Alcoon's facing (<c>StartWalking</c>, $A8:DF3F).</summary>
    StartWalking,
    /// <summary>Counts down a step and moves; a wall turns the Alcoon round to the other walk (<c>DecrementStepCounterMoveHorizontally</c>, $A8:DF63).</summary>
    Step,
}
