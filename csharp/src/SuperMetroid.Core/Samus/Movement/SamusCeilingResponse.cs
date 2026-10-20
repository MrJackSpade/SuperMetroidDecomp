namespace SuperMetroid.Core.Samus.Movement;

/// <summary>When Samus's vertical mover applies the response to hitting a ceiling.</summary>
internal enum SamusCeilingResponse
{
    /// <summary>The mover responds at once.</summary>
    Immediate,

    /// <summary>The caller responds after its own handling.</summary>
    Deferred,
}
