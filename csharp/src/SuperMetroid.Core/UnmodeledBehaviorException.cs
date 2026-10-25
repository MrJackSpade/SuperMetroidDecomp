namespace SuperMetroid.Core;

/// <summary>
/// Gameplay reached a behavior the port does not model. The failure's stable identity names that behavior, apart from
/// where it occurred, so the failure is the same bug in every room it occurs in.
/// </summary>
public sealed class UnmodeledBehaviorException : InvalidOperationException
{
    /// <summary>Gets the stable identity of the behavior that is not modeled.</summary>
    public string DispatchIdentity { get; }

    /// <summary>Creates an unmodeled-behavior failure with a stable diagnostic identity.</summary>
    public UnmodeledBehaviorException(string dispatchIdentity, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        DispatchIdentity = dispatchIdentity;
    }
}
