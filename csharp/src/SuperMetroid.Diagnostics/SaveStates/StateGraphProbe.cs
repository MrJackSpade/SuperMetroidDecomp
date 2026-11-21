namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// A registration that accepts values of a type the framework makes rather than names (#2602): a frozen collection's
/// specialization, the read-only list the compiler makes for a collection expression, or a framework singleton.
/// </summary>
/// <param name="type">The handling of the values it accepts.</param>
internal abstract class StateGraphProbe(StateGraphType type)
{
    /// <summary>The handling of the values it accepts.</summary>
    internal StateGraphType Type { get; } = type;

    /// <summary>Whether the probe accepts <paramref name="value"/>.</summary>
    internal abstract bool Accepts(object value);

    /// <summary>Whether this probe's handling is to be preferred to <paramref name="other"/>, which accepts the same value.</summary>
    internal virtual bool MoreSpecificThan(StateGraphType other) => false;
}
