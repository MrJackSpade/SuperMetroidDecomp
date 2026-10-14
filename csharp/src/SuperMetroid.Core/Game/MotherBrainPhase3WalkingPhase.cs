namespace SuperMetroid.Core.Game;

/// <summary>Reachable third-phase walking function pointers at `$A9:C26A-$C326`.</summary>
public enum MotherBrainPhase3WalkingPhase
{
    /// <summary>No phase-three horizontal walking function is active.</summary>
    Inactive,
    /// <summary>Attempts a small forward step while respecting Samus spacing and arena bounds.</summary>
    TryToInchForward,
    /// <summary>Moves rapidly away from Samus until the quick-retreat condition completes.</summary>
    RetreatQuickly,
    /// <summary>Moves away at the slower retreat speed used to re-establish attack spacing.</summary>
    RetreatSlowly,
}
