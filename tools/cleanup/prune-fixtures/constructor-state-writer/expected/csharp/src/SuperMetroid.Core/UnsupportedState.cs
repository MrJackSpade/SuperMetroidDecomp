namespace SuperMetroid.Core;

/// <summary>Gameplay reached a state the game does not support; only checks create it, and the host reports it.</summary>
public sealed class UnsupportedState : InvalidOperationException
{
    /// <summary>The state's stable identity.</summary>
    public string Identity { get; }

    /// <summary>Creates the failure for an unsupported state.</summary>
    /// <param name="identity">The state's stable identity.</param>
    /// <param name="message">What happened.</param>
    public UnsupportedState(string identity, string message)
        : base(message)
    {
        Identity = identity;
    }
}
