namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// How far a landing ball has bounced, and which of the two balls bounced (#2560): two independent facts. A Spring Ball
/// landing takes either ball's first rebound to its own second; a Morph Ball landing after a Spring Ball rebound comes to
/// rest. A ball that is not bouncing has no bounce.
/// </summary>
/// <param name="Ball">The ball that bounced.</param>
/// <param name="Rebound">The rebound it has reached.</param>
public readonly record struct SamusBallBounce(SamusBouncingBall Ball, SamusBallRebound Rebound)
{
    /// <summary>The Morph Ball on its first rebound.</summary>
    public static SamusBallBounce MorphBallFirst { get; } = new(SamusBouncingBall.MorphBall, SamusBallRebound.First);

    /// <summary>The Morph Ball on its second rebound.</summary>
    public static SamusBallBounce MorphBallSecond { get; } = new(SamusBouncingBall.MorphBall, SamusBallRebound.Second);

    /// <summary>The Spring Ball on its first rebound.</summary>
    public static SamusBallBounce SpringBallFirst { get; } = new(SamusBouncingBall.SpringBall, SamusBallRebound.First);

    /// <summary>The Spring Ball on its second rebound.</summary>
    public static SamusBallBounce SpringBallSecond { get; } = new(SamusBouncingBall.SpringBall, SamusBallRebound.Second);
}
