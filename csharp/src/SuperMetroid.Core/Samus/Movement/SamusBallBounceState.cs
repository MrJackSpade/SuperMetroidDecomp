namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// How far a landing Morph Ball or Spring Ball has bounced, and which of the two bounced. A Spring Ball landing takes
/// either family's first rebound to its own second; a Morph Ball landing after a Spring Ball rebound comes to rest.
/// </summary>
public enum SamusBallBounceState
{
    /// <summary>Not bouncing.</summary>
    None,

    /// <summary>Morph Ball first rebound.</summary>
    FirstRebound,

    /// <summary>Morph Ball second rebound.</summary>
    SecondRebound,

    /// <summary>Spring Ball first rebound.</summary>
    SpringBallFirstRebound,

    /// <summary>Spring Ball second rebound.</summary>
    SpringBallSecondRebound,
}
