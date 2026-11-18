namespace SuperMetroid.Core;

/// <summary>
/// Gameplay reached a behavior the model does not cover. The failure's stable identity names that behavior, apart from
/// where it occurred, so the failure is the same bug in every room it occurs in.
/// </summary>
public sealed class UnmodeledBehaviorException : InvalidOperationException
{
    /// <summary>Gets the stable identity of the behavior the game does not cover.</summary>
    public string BehaviorIdentity { get; }

    /// <summary>Creates the failure for an uncovered behavior, with a stable diagnostic identity.</summary>
    public UnmodeledBehaviorException(string behaviorIdentity, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        BehaviorIdentity = behaviorIdentity;
    }
}
