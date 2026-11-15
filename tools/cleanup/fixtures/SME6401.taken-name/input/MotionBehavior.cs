namespace Fixture;

/// <summary>A behaviour that moves by the motions <typeparamref name="TMotion"/> names.</summary>
/// <typeparam name="TMotion">The motions.</typeparam>
public abstract class Behavior<TMotion> : Behavior
    where TMotion : struct, System.Enum
{
}
