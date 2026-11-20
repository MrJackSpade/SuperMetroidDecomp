namespace SuperMetroid.Core;

/// <summary>
/// Gameplay reached a state the game does not support. The failure's stable identity names that behavior, apart from
/// where it occurred, so the failure is the same bug in every room it occurs in.
/// </summary>
public sealed class UnsupportedGameStateException : InvalidOperationException
{
    /// <summary>Gets the stable identity of the state the game does not support.</summary>
    public string BehaviorIdentity { get; }

    /// <summary>Creates the failure for an unsupported state, with a stable diagnostic identity.</summary>
    public UnsupportedGameStateException(string behaviorIdentity, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        BehaviorIdentity = behaviorIdentity;
    }
}
