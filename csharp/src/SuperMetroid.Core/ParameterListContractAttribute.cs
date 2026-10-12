namespace SuperMetroid.Core;

/// <summary>
/// Exempts one member from the parameter-list ceiling (#633) because its signature is an immutable
/// external contract: a P/Invoke, serialization or framework shape the repository does not own.
/// The reason names that contract; it is never a convenience exemption.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Delegate, Inherited = false)]
public sealed class ParameterListContractAttribute : Attribute
{
    /// <summary>Records which external contract fixes the signature.</summary>
    public ParameterListContractAttribute(string reason) => Reason = reason;

    /// <summary>The external contract that fixes the signature.</summary>
    public string Reason { get; }
}
