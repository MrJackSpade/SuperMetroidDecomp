namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Which walk a <see cref="TorizoCue.Walk"/> cue is. One walking step of a Torizo, read from its walking table.</summary>
internal enum TorizoWalkKind
{
    /// <summary>Takes a Bomb Torizo walking step, turning at a wall (<c>Instruction_Torizo_BombTorizoWalkingMovement_Normal_IndexInY</c>, $AA:C470).</summary>
    WalkBombTorizo,
    /// <summary>Takes a faceless Bomb Torizo walking step, turning at a wall (<c>Instruction_Torizo_BTWalkingMovement_Faceless_IndexInY</c>, $AA:C4E5).</summary>
    WalkFacelessBombTorizo,
    /// <summary>Takes a Golden Torizo walking step, turning at a wall (<c>Instruction_GoldenTorizo_WalkingMovement_IndexInY</c>, $AA:D54D).</summary>
    WalkGoldenTorizo,
}
