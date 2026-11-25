namespace Engine.Core;

/// <summary>
/// The terms an update of play is played on: what may follow from it. Play in its own hands is played on the usual
/// terms, on which everything may. A mode that plays play beneath itself, as a pause screen does while it darkens
/// the picture, says which of these it holds off for as long as it covers play.
/// </summary>
public sealed record PlayTerms
{
    /// <summary>The usual terms: play in its own hands.</summary>
    public static PlayTerms Usual { get; } = new();

    /// <summary>Whether a player whose energy is gone may begin to die.</summary>
    public bool PlayerMayDie { get; init; } = true;

    /// <summary>Whether a player whose energy is gone may be saved by what they are, as by a reserve.</summary>
    public bool PlayerMayBeSaved { get; init; } = true;

    /// <summary>Whether the pause may be asked for.</summary>
    public bool MayPause { get; init; } = true;

    /// <summary>Whether a session service may take play over.</summary>
    public bool ServicesMayTakeOver { get; init; } = true;

    /// <summary>Whether the update counts toward the play time.</summary>
    public bool CountsPlayTime { get; init; } = true;
}
