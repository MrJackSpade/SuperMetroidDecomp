namespace SuperMetroid.Core;

/// <summary>
/// Justifies, at its one site, a pair of methods whose names differ only by opposing direction tokens and whose bodies
/// mirror each other, but which are a deliberate, behaviourally distinct asymmetry rather than one behaviour written
/// twice (SME6418). The reason names what the two sides do differently.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class MirroredAsymmetryAttribute : Attribute
{
    /// <summary>Marks a deliberate asymmetry; SME6418 reads the reason from the attribute argument.</summary>
    /// <param name="reason">What the two directions do differently.</param>
    public MirroredAsymmetryAttribute(string reason) => ArgumentException.ThrowIfNullOrWhiteSpace(reason);
}
