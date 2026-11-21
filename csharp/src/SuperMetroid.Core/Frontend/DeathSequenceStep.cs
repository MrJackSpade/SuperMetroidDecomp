namespace SuperMetroid.Core.Frontend;

/// <summary>Which step of Samus's death (<see cref="Progression.SuperMetroidGameState.DeathSequence"/>) runs.</summary>
internal enum DeathSequenceStep
{
    /// <summary>One final gameplay pass with the game clock held, which then prepares Samus and her colours for death.</summary>
    FinalGameplayPass,
    /// <summary>The surroundings fade to black while Samus's suit keeps its colours, then the death music is queued.</summary>
    BlackingOutSurroundings,
    /// <summary>
    /// Samus's fatal pose is drawn until the queued death music has been processed, which begins her death animation: the
    /// pre-flash hold, the flashing and the suit explosion with its white-out, which her own death sequence
    /// (<see cref="Samus.Death.SamusDeathSequencePhase"/>) steps through.
    /// </summary>
    SamusDying,
    /// <summary>The finished death display fades to black before the game-over menu; also where both time-up endings finish.</summary>
    FinalBlackOut,
}
