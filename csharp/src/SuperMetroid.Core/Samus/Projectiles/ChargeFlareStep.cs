namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A charge-flare component's next picture and the wait it holds it for.</summary>
/// <param name="Phase">The picture.</param>
/// <param name="Hold">The wait: the picture shows for it and one update more.</param>
internal readonly record struct ChargeFlareStep(int Phase, WrappingCountdown Hold);
